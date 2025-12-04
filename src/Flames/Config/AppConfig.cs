using System.Collections.Generic;
using System.Text.Json.Serialization;
//здесь я решила написать много комментов, чтобы и мне было понятно, и ревьюить тоже удобно
namespace Flames.Config;

/// <summary>
/// Размер итогового изображения
/// </summary>
public sealed class SizeConfig
{
    //Ширина изображения в пикселях (по умолчанию 1920)
    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;

    //Высота изображения в пикселях (по умолчанию 1080)
    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;
}

/// <summary>
/// Конфигурация одной функции/вариации (название + вес)
/// </summary>
public sealed class FunctionConfig
{
    //Название функции, например "swirl", "horseshoe" и т.п
    [JsonPropertyName("name")]
    public string Name { get; set; } = "swirl";

    //Вес функции (вероятность выбора при генерации)
    [JsonPropertyName("weight")]
    public double Weight { get; set; } = 1.0;
}

/// <summary>
/// Набор параметров аффинного преобразования:
/// (x', y') = (a * x + b * y + c, d * x + e * y + f)
/// </summary>
public sealed class AffineParams
{
    //Масштаб/вращение X
    [JsonPropertyName("a")]
    public double A { get; set; } = 1.0;

    //Смешивание X от Y
    [JsonPropertyName("b")]
    public double B { get; set; } = 0.0;

    //Сдвиг по X
    [JsonPropertyName("c")]
    public double C { get; set; } = 0.0;

    //Смешивание Y от X
    [JsonPropertyName("d")]
    public double D { get; set; } = 0.0;

    //Масштаб/вращение Y
    [JsonPropertyName("e")]
    public double E { get; set; } = 1.0;

    //Сдвиг по Y
    [JsonPropertyName("f")]
    public double F { get; set; } = 0.0;
}

/// <summary>
/// Общая конфигурация приложения (CLI + JSON + дефолты)
/// </summary>
public sealed class AppConfig
{
    //Размер изображения (width/height)
    [JsonPropertyName("size")]
    public SizeConfig Size { get; set; } = new();

    //Количество итераций генерации
    [JsonPropertyName("iteration_count")]
    public int IterationCount { get; set; } = 2500;

    //Путь до выходного PNG-файла
    [JsonPropertyName("output_path")]
    public string OutputPath { get; set; } = "result.png";

    //Количество потоков для генерации
    [JsonPropertyName("threads")]
    public int Threads { get; set; } = 1;

    //Seed для генератора случайных чисел
    [JsonPropertyName("seed")]
    public double Seed { get; set; } = 5.1234;

    //Список функций/вариаций с весами
    [JsonPropertyName("functions")]
    public List<FunctionConfig> Functions { get; set; } = new();

    //Список аффинных преобразований
    [JsonPropertyName("affine_params")]
    public List<AffineParams> AffineParams { get; set; } = new();

    //Флаг включения гамма-коррекции
    [JsonPropertyName("gamma_correction")]
    public bool GammaCorrection { get; set; } = false;

    // Значение гаммы для гамма-коррекции
    [JsonPropertyName("gamma")]
    public double Gamma { get; set; } = 2.2;

    //Уровень симметрии (сколько раз поворачиваем каждую точку)
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
