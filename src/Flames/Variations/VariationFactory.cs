using System.Collections.Generic;
using System.Drawing;
using Flames.Config;

namespace Flames.Variations;

/// <summary>
/// Создаёт список VariationDefinition на основании конфигурации:
/// маппит имя функции на конкретную реализацию и назначает ей цвет
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
            var color = PickColorForIndex(index);
            list.Add(new VariationDefinition(variation, f.Weight, color));
            index++;
        }

        return list.ToArray();
    }

    /// <summary>
    /// Возвращает объект вариации по имени (с учётом алиасов).
    /// Если имя неизвестно — бросает ConfigException
    /// </summary>
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

    private static Color PickColorForIndex(int index)
    {
        Color[] palette =
        {
            Color.FromArgb(255, 100, 100, 240),
            Color.FromArgb(255, 200, 200, 10),
            Color.FromArgb(255,255, 10, 10),
            Color.FromArgb(255, 200, 160, 240),
            Color.FromArgb(255, 0, 100, 120)
        };

        return palette[index % palette.Length];
    }
}