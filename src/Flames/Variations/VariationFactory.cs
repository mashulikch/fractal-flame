using System.Collections.Generic;
using Flames.Config;

namespace Flames.Variations;

/// <summary>
/// Фабрика вариаций: создаёт список VariationDefinition
/// на основе конфигурации и назначает им цвета.
/// </summary>
public static class VariationFactory
{
    /// <summary>
    /// Создаёт массив VariationDefinition из списка функций в AppConfig
    /// </summary>
    public static VariationDefinition[] Create(AppConfig config)
    {
        var list = new List<VariationDefinition>();

        int index = 0;
        foreach (var f in config.Functions)
        {
            // Создаём конкретную вариацию по имени
            var variation = CreateVariationByName(f.Name);
            var (r, g, b) = PickColorForIndex(index);

            list.Add(new VariationDefinition(variation,
                f.Weight,
                r,
                g,
                b));

            index++;
        }

        return list.ToArray();
    }

    private static IVariation CreateVariationByName(string nameRaw)
    {
        string name = nameRaw.Trim().ToLowerInvariant();

        return name switch
        {
            "swirl" => new SwirlVariation(),
            "horseshoe" => new HorseshoeVariation(),
            "spherical" or "sphere" => new SphericalVariation(),
            "exponential" or "exp" => new ExponentialVariation(),
            "handkerchief" or "handk" => new Handkerchief(),
            "bubble" => new BubbleVariation(),
            _ => throw new ConfigException($"Unknown variation function '{nameRaw}'")
        };
    }

    private static (double r, double g, double b) PickColorForIndex(int index)
    {
        var palette = new (double r, double g, double b)[]
        {
            (100.0/255.0, 100.0/255.0, 240.0/255.0),
            (200.0/255.0, 200.0/255.0, 10.0/255.0),
            (255.0/255.0, 10.0/255.0, 10.0/255.0),
            (200.0/255.0, 160.0/255.0, 240.0/255.0),
            (0.0/255.0,   100.0/255.0, 120.0/255.0),
        };

        return palette[index % palette.Length];
    }
}
