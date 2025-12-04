namespace Flames.Variations;

/// <summary>
/// Описание вариации: функция, её вес и базовый цвет (RGB в диапазоне 0..1).
/// </summary>
public sealed class VariationDefinition
{
    public IVariation Variation { get; }
    public double Weight { get; }
    public string Name => Variation.Name;

    public double ColorR { get; }
    public double ColorG { get; }
    public double ColorB { get; }

    public VariationDefinition(
        IVariation variation,
        double weight,
        double colorR,
        double colorG,
        double colorB)
    {
        Variation = variation;
        Weight = weight;

        ColorR = colorR;
        ColorG = colorG;
        ColorB = colorB;
    }
}