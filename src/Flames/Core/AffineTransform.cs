using Flames.Config;

namespace Flames.Core;

/// <summary>
/// Обёртка над AffineParams для быстрого вычисления аффинного преобразования:
/// (x', y') = (a * x + b * y + c, d * x + e * y + f)
/// </summary>
public sealed class AffineTransform
{
    public double A { get; }
    public double B { get; }
    public double C { get; }
    public double D { get; }
    public double E { get; }
    public double F { get; }

    public AffineTransform(AffineParams p)
    {
        A = p.A;
        B = p.B;
        C = p.C;
        D = p.D;
        E = p.E;
        F = p.F;
    }

    /// <summary>
    /// Применяет аффинное преобразование к точке (x, y)
    /// </summary>
    public void Transform(double x, double y, out double newX, out double newY)
    {
        newX = A * x + B * y + C;
        newY = D * x + E * y + F;
    }
}