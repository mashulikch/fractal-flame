namespace Flames.Variations;

/// <summary>
/// Интерфейс для одной вариации (фрактального преобразования)
/// </summary>
public interface IVariation
{
    string Name { get; }

    /// <summary>
    /// Преобразует точку (x, y) в (newX, newY) по своей формуле
    /// </summary>
    void Transform(double x, double y, out double newX, out double newY);
}