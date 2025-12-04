using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using Flames.Config;
using Flames.Logging;
using Flames.Variations;

namespace Flames.Core;

/// <summary>
/// Локальный буфер для одного потока:
/// хранит число попаданий и суммарные цветовые компоненты для каждого пикселя.
/// Это позволяет избежать блокировок при записи из разных потоков
/// </summary>
internal sealed class WorkerBuffer
{
    public int[] Hits { get; }
    public float[] Red { get; }
    public float[] Green { get; }
    public float[] Blue { get; }

    public WorkerBuffer(int pixelCount)
    {
        Hits = new int[pixelCount];
        Red = new float[pixelCount];
        Green = new float[pixelCount];
        Blue = new float[pixelCount];
    }
}

/// <summary>
/// Основной генератор фрактального пламени.
/// Отвечает за запуск итераций (однопоточно/многопоточно),
/// накопление цветов и построение итогового Bitmap
/// </summary>
public sealed class FlamesGenerator
{
    private readonly AppConfig _config;
    private readonly VariationDefinition[] _variations;
    private readonly AffineTransform[] _affines;
    private readonly ILogger _logger;

    // Накопленные веса для выбора функции по весам
    private readonly double[] _cumulativeWeights;
    private readonly double _totalWeight;

    public FlamesGenerator(AppConfig config, VariationDefinition[] variations, ILogger logger)
    {
        _config = config;
        _variations = variations;
        _affines = BuildAffines(config);
        _logger = logger;

        _cumulativeWeights = BuildCumulativeWeights(variations);
        _totalWeight = _cumulativeWeights[^1];
    }
    
    //Создаёт массив аффинных преобразований из конфигурации
    private static AffineTransform[] BuildAffines(AppConfig config)
    {
        if (config.AffineParams == null || config.AffineParams.Count == 0)
        {
            throw new ConfigException("At least one affine transform must be specified");
        }

        var result = new AffineTransform[config.AffineParams.Count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new AffineTransform(config.AffineParams[i]);
        }

        return result;
    }

    // Точка входа: запускает генерацию и возвращает итоговый Bitmap
    public Bitmap Generate()
    {
        int width = _config.Size.Width;
        int height = _config.Size.Height;
        int pixelCount = width * height;

        // Центр и масштаб: перевод координат мира в координаты пикселей
        double centerX = width / 2.0;
        double centerY = height / 2.0;
        double scale = Math.Min(width, height) * 0.6;

        int threads = Math.Max(1, _config.Threads);
        int iterations = _config.IterationCount;

        _logger.Info(threads == 1
            ? "Running in single-threaded mode."
            : $"Running in multi-threaded mode with {threads} threads.");

        // Выделение отдельного буфера для каждого потока
        var workerBuffers = new WorkerBuffer[threads];
        for (int t = 0; t < threads; t++)
        {
            workerBuffers[t] = new WorkerBuffer(pixelCount);
        }

        // Счётчик выполненных итераций (для прогресса)
        long globalDone = 0;
        int lastPercentReported = 0;
        object progressLock = new();

        // Число итераций между обновлениями прогресса
        int progressChunk = Math.Max(1000, iterations / 100);

        // Делегат для обновления прогресса из разных потоков
        Action<long> progressUpdate = delta =>
        {
            long done = Interlocked.Add(ref globalDone, delta);
            if (iterations <= 0)
            {
                return;
            }

            int percent = (int)(done * 100 / iterations);
            if (percent > 100)
            {
                percent = 100;
            }

            lock (progressLock)
            {
                if (percent > lastPercentReported)
                {
                    lastPercentReported = percent;
                    _logger.Info($"Progress: {percent}%");
                }
            }
        };

        // Сколько итераций пропускаем в начале, чтобы "разогреть" траекторию
        int warmupIterations = 20;
        int symmetryLevel = Math.Max(1, _config.SymmetryLevel);
        double centerXLocal = centerX;
        double centerYLocal = centerY;
        double scaleLocal = scale;

        var tasks = new Task[threads];

        // Деление общего количества итераций между потоками
        int perThread = iterations / threads;
        int remainder = iterations % threads;

        for (int t = 0; t < threads; t++)
        {
            int threadIndex = t;
            int myIterations = perThread + (t < remainder ? 1 : 0);
            var buffer = workerBuffers[t];

            tasks[t] = Task.Run(() =>
            {
                var rng = CreateRandom(_config.Seed, threadIndex);
                
                // Запуск рабочеего цикла для потока
                RunWorker(
                    myIterations,
                    warmupIterations,
                    rng,
                    width,
                    height,
                    scaleLocal,
                    centerXLocal,
                    centerYLocal,
                    symmetryLevel,
                    buffer,
                    progressChunk,
                    progressUpdate);
            });
        }

        //Ожидание завершения всех потоков
        Task.WaitAll(tasks);

        //Объединие данных из всех workerBuffers в один общий
        var hitCount = new int[pixelCount];
        var red = new float[pixelCount];
        var green = new float[pixelCount];
        var blue = new float[pixelCount];

        foreach (var buf in workerBuffers)
        {
            var h = buf.Hits;
            var r = buf.Red;
            var g = buf.Green;
            var b = buf.Blue;

            for (int i = 0; i < pixelCount; i++)
            {
                hitCount[i] += h[i];
                red[i] += r[i];
                green[i] += g[i];
                blue[i] += b[i];
            }
        }

        _logger.Info("Iterations completed. Building bitmap...");

        // Нахождение максимума попаданий по пикселям для логарифмической нормализации
        int maxHits = 0;
        for (int i = 0; i < pixelCount; i++)
        {
            if (hitCount[i] > maxHits)
            {
                maxHits = hitCount[i];
            }
        }

        if (maxHits == 0)
        {
            _logger.Warn("No points landed on image. Result will be empty.");
        }
        
        // Создаём итоговый Bitmap, 24 бита RGB
        var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

        bool gammaCorrection = _config.GammaCorrection;
        double gamma = _config.Gamma <= 0 ? 2.2 : _config.Gamma;

        // Проход по пикселям и вычисление итогового цвета
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                int hits = hitCount[idx];

                // Фильтрация очень редких попаданий(картинка была менее "шумной")
                if (hits < 4)
                {
                    bitmap.SetPixel(x, y, Color.Black);
                    continue;
                }

                double density = Math.Log(hits + 1) / Math.Log(maxHits + 1.0);

                double r = (red[idx] / hits) * density;
                double g = (green[idx] / hits) * density;
                double b = (blue[idx] / hits) * density;

                r = Clamp01(r);
                g = Clamp01(g);
                b = Clamp01(b);

                if (gammaCorrection)
                {
                    double invGamma = 1.0 / gamma;
                    r = Math.Pow(r, invGamma);
                    g = Math.Pow(g, invGamma);
                    b = Math.Pow(b, invGamma);
                }

                int ir = (int)(r * 255.0);
                int ig = (int)(g * 255.0);
                int ib = (int)(b * 255.0);

                if (ir < 0) ir = 0;
                else if (ir > 255) ir = 255;

                if (ig < 0) ig = 0;
                else if (ig > 255) ig = 255;

                if (ib < 0) ib = 0;
                else if (ib > 255) ib = 255;

                bitmap.SetPixel(x, y, Color.FromArgb(ir, ig, ib));
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Основной рабочий цикл для одного потока:
    /// выполняет warmup, затем основное число итераций,
    /// обновляя локальный буфер и прогресс.
    /// </summary>
    private void RunWorker(
        int iterations,
        int warmupIterations,
        Random rng,
        int width,
        int height,
        double scale,
        double centerX,
        double centerY,
        int symmetryLevel,
        WorkerBuffer buffer,
        int progressChunk,
        Action<long> progressUpdate)
    {
        double x = rng.NextDouble() * 2.0 - 1.0;
        double y = rng.NextDouble() * 2.0 - 1.0;

        double colorR = 0.0;
        double colorG = 0.0;
        double colorB = 0.0;
        bool hasColor = false;

        int doneInChunk = 0;
        double angleStep = 2.0 * Math.PI / symmetryLevel;

        for (int i = 0; i < warmupIterations; i++)
        {
            int idxVar = PickVariationIndex(rng);
            var v = _variations[idxVar];

            var affine = _affines[rng.Next(_affines.Length)];
            affine.Transform(x, y, out double ax, out double ay);
            v.Variation.Transform(ax, ay, out double vx, out double vy);

            x = vx;
            y = vy;

            if (!hasColor)
            {
                colorR = v.ColorR;
                colorG = v.ColorG;
                colorB = v.ColorB;
                hasColor = true;
            }
            else
            {
                colorR = (colorR + v.ColorR) * 0.5;
                colorG = (colorG + v.ColorG) * 0.5;
                colorB = (colorB + v.ColorB) * 0.5;
            }
        }

        for (int i = 0; i < iterations; i++)
        {
            int idxVar = PickVariationIndex(rng);
            var v = _variations[idxVar];

            var affine = _affines[rng.Next(_affines.Length)];
            affine.Transform(x, y, out double ax, out double ay);
            v.Variation.Transform(ax, ay, out double vx, out double vy);

            x = vx;
            y = vy;

            if (!hasColor)
            {
                colorR = v.ColorR;
                colorG = v.ColorG;
                colorB = v.ColorB;
                hasColor = true;
            }
            else
            {
                colorR = (colorR + v.ColorR) * 0.5;
                colorG = (colorG + v.ColorG) * 0.5;
                colorB = (colorB + v.ColorB) * 0.5;
            }

            // Рисуем точку (и её повороты при симметрии)
            PlotPoint(
                x,
                y,
                colorR,
                colorG,
                colorB,
                width,
                height,
                scale,
                centerX,
                centerY,
                symmetryLevel,
                angleStep,
                buffer);

            doneInChunk++;
            if (doneInChunk >= progressChunk)
            {
                // Обновляем глобальный прогресс
                progressUpdate(doneInChunk);
                doneInChunk = 0;
            }
        }

        // Если после цикла остались незарегистрированные итерации — добавляем их в прогресс
        if (doneInChunk > 0)
        {
            progressUpdate(doneInChunk);
        }
    }

    /// <summary>
    /// Рисует одну логическую точку и её копии с поворотами (симметрия),
    /// записывая данные в локальный буфер потока.
    /// </summary>
    private void PlotPoint(
        double x,
        double y,
        double colorR,
        double colorG,
        double colorB,
        int width,
        int height,
        double scale,
        double centerX,
        double centerY,
        int symmetryLevel,
        double angleStep,
        WorkerBuffer buffer)
    {
        for (int k = 0; k < symmetryLevel; k++)
        {
            double xr = x;
            double yr = y;

            if (symmetryLevel > 1 && k > 0)
            {
                double angle = angleStep * k;
                double cos = Math.Cos(angle);
                double sin = Math.Sin(angle);

                double tx = x * cos - y * sin;
                double ty = x * sin + y * cos;
                xr = tx;
                yr = ty;
            }

            int px = (int)(xr * scale + centerX);
            int py = (int)(yr * scale + centerY);

            if (px < 0 || px >= width || py < 0 || py >= height)
            {
                continue;
            }

            int idx = py * width + px;

            buffer.Hits[idx]++;
            buffer.Red[idx] += (float)colorR;
            buffer.Green[idx] += (float)colorG;
            buffer.Blue[idx] += (float)colorB;
        }
    }

    /// <summary>
    /// Выбирает индекс вариации в соответствии с её весом.
    /// Использует массив накопленных сумм (_cumulativeWeights).
    /// </summary>
    private int PickVariationIndex(Random rng)
    {
        double value = rng.NextDouble() * _totalWeight;
        for (int i = 0; i < _cumulativeWeights.Length; i++)
        {
            if (value < _cumulativeWeights[i])
            {
                return i;
            }
        }
        return _cumulativeWeights.Length - 1;
    }

    /// <summary>
    /// Строит массив накопленных весов для быстрого выбора вариации
    /// </summary>
    private static double[] BuildCumulativeWeights(VariationDefinition[] variations)
    {
        double sum = 0.0;
        var arr = new double[variations.Length];
        for (int i = 0; i < variations.Length; i++)
        {
            sum += variations[i].Weight;
            arr[i] = sum;
        }

        if (sum <= 0)
        {
            throw new ConfigException("Total weight of functions must be > 0");
        }

        return arr;
    }

    /// <summary>
    /// Создаёт RNG для конкретного потока на основе общего seed и индекса
    /// </summary>
   private static Random CreateRandom(double seedBase, int index)
    {
        unchecked
        {
            int seed = seedBase.GetHashCode();
            seed = seed * 397 ^ index;
            return new Random(seed);
        }
    }

    private static double Clamp01(double v)
    {
        if (v < 0.0)
        {
            return 0.0;
        }

        if (v > 1.0)
        {
            return 1.0;
        }

        return v;
    }
}
