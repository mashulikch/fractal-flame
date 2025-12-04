using System.Drawing;

namespace Flames.Variations;

/// <summary>
/// Описание одной вариации для генератора:
/// - сама функция (IVariation)
/// - вес (вероятность выбора)
/// - базовый цвет, которым "подкрашиваются" точки этой функции
/// </summary>
public sealed class VariationDefinition
{
    public IVariation Variation { get; }
    public double Weight { get; }
    public string Name => Variation.Name;

    public double ColorR { get; }
    public double ColorG { get; }
    public double ColorB { get; }

    public VariationDefinition(IVariation variation, double weight, Color color)
    {
        Variation = variation;
        Weight = weight;

        // Конвертация цвета из [0..255] в [0..1]
        ColorR = color.R / 255.0;
        ColorG = color.G / 255.0;
        ColorB = color.B / 255.0;
    }
}