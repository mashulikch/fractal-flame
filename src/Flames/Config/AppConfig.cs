using System.Collections.Generic;
using System.Text.Json.Serialization;
//здесь я решила написать много комментов, чтобы и мне было понятно, и ревьюить тоже удобно

namespace Flames.Config;

/// <summary>
/// Размер итогового изображения
/// </summary>
public sealed class SizeConfig
{
    /// <summary>
    /// Ширина изображения в пикселях (по умолчанию 1920)
    /// </summary>
    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;

    /// <summary>
    /// Высота изображения в пикселях (по умолчанию 1080)
    /// </summary>
    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;
}

/// <summary>
/// Конфигурация одной функции/вариации (название + вес)
/// </summary>
public sealed class FunctionConfig
{
    /// <summary>
    /// Название функции, например "swirl", "horseshoe" и т.п
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "swirl";

    /// <summary>
    /// Вес функции (вероятность выбора при генерации)
    /// </summary>
    [JsonPropertyName("weight")]
    public double Weight { get; set; } = 1.0;
}

/// <summary>
/// Набор параметров аффинного преобразования:
/// (x', y') = (a * x + b * y + c, d * x + e * y + f)
/// </summary>
public sealed class AffineParams
{
    /// <summary>
    /// Масштаб/вращение X
    /// </summary>
    [JsonPropertyName("a")]
    public double A { get; set; } = 1.0;

    /// <summary>
    /// Смешивание X от Y
    /// </summary>
    [JsonPropertyName("b")]
    public double B { get; set; } = 0.0;

    /// <summary>
    /// Сдвиг по X
    /// </summary>
    [JsonPropertyName("c")]
    public double C { get; set; } = 0.0;

    /// <summary>
    /// Смешивание Y от X
    /// </summary>
    [JsonPropertyName("d")]
    public double D { get; set; } = 0.0;

    /// <summary>
    /// Масштаб/вращение Y
    /// </summary>
    [JsonPropertyName("e")]
    public double E { get; set; } = 1.0;

    /// <summary>
    /// Сдвиг по Y
    /// </summary>
    [JsonPropertyName("f")]
    public double F { get; set; } = 0.0;
}

/// <summary>
/// Общая конфигурация приложения (CLI + JSON + дефолты)
/// </summary>
public sealed class AppConfig
{
    /// <summary>
    /// Размер изображения (width/height)
    /// </summary>
    [JsonPropertyName("size")]
    public SizeConfig Size { get; set; } = new();

    /// <summary>
    /// Количество итераций генерации
    /// </summary>
    [JsonPropertyName("iteration_count")]
    public int IterationCount { get; set; } = 2500;

    /// <summary>
    /// Путь до выходного PNG-файла
    /// </summary>
    [JsonPropertyName("output_path")]
    public string OutputPath { get; set; } = "result.png";

    /// <summary>
    /// Количество потоков для генерации
    /// </summary>
    [JsonPropertyName("threads")]
    public int Threads { get; set; } = 1;

    /// <summary>
    /// Seed для генератора случайных чисел
    /// </summary>
    [JsonPropertyName("seed")]
    public double Seed { get; set; } = 5.1234;

    /// <summary>
    /// Список функций/вариаций с весами
    /// </summary>
    [JsonPropertyName("functions")]
    public List<FunctionConfig> Functions { get; set; } = new();

    /// <summary>
    /// Список аффинных преобразований
    /// </summary>
    [JsonPropertyName("affine_params")]
    public List<AffineParams> AffineParams { get; set; } = new();

    /// <summary>
    /// Флаг включения гамма-коррекции
    /// </summary>
    [JsonPropertyName("gamma_correction")]
    public bool GammaCorrection { get; set; } = false;

    /// <summary>
    /// Значение гаммы для гамма-коррекции
    /// </summary>
    [JsonPropertyName("gamma")]
    public double Gamma { get; set; } = 2.2;

    /// <summary>
    /// Уровень симметрии (сколько раз поворачиваем каждую точку)
    /// </summary>
    [JsonPropertyName("symmetry_level")]
    public int SymmetryLevel { get; set; } = 1;
}

/// <summary>
/// Специальное исключение для ошибок конфигурации
/// </summary>
public sealed class ConfigException : System.Exception
{
    public ConfigException(string message) : base(message) { }
}
