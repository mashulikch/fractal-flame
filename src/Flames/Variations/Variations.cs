using System;

namespace Flames.Variations;

/// <summary>
/// Класс вариации "Swirl".
/// Преобразование с использованием квадрата радиуса и синуса/косинуса.
/// Даёт "завихрения"
/// </summary>
public sealed class SwirlVariation : IVariation
{
    public string Name => "swirl";

    public void Transform(double x, double y, out double newX, out double newY)
    {
        double r2 = x * x + y * y;
        double sin = r2 * Math.Sin(x + r2);
        double cos = r2 * Math.Cos(y - r2);

        newX = x * sin - y * cos;
        newY = x * cos + y * sin;

    }
}

/// <summary>
/// Вариация "Handkerchief".
/// Использует радиус и синус/косинус для построения сложного узора
/// </summary>
public sealed class Handkerchief : IVariation
{
    public string Name => "handk";

    public void Transform(double x, double y, out double newX, out double newY)
    {
        double r2 = Math.Sqrt(x * x + y * y);
        double sin = r2 * Math.Sin(x + r2);
        double cos = r2 * Math.Cos(y - r2);

        newX = sin;
        newY = cos;
    }
}

/// <summary>
/// Вариация "Horseshoe".
/// Одна из классических вариаций, даёт "подковообразные" структуры
/// </summary>
public sealed class HorseshoeVariation : IVariation
{
    public string Name => "horseshoe";

    public void Transform(double x, double y, out double newX, out double newY)
    {
        double r = 2 / (Math.Sqrt(x * x + y * y) + 1);
        newX = r * x;
        newY = r * y;
    }
}

/// <summary>
/// Вариация "Spherical".
/// Проецирует пространство на сферу (1 / (x^2 + y^2)) (решила поиграться с константами)
/// </summary>
public sealed class SphericalVariation : IVariation
{
    public string Name => "spherical";

    public void Transform(double x, double y, out double newX, out double newY)
    {
        double r2 = x * x + y * y + 1e-6;
        double factor = 5.0 / (53 * r2);
        newX = x * factor;
        newY = y * factor;
    }
}

/// <summary>
/// Вариация "Exponential".
/// Использует экспоненту и тригонометрические функции
/// </summary>
public sealed class ExponentialVariation : IVariation
{
    public string Name => "exp";

    public void Transform(double x, double y, out double newX, out double newY)
    {
        newX = Math.Exp(x - 1) * Math.Cos(Math.PI * y);
        newY = Math.Exp(x - 1) * Math.Sin(Math.PI * y);
    }
}

/// <summary>
/// Вариация "Bubble".
/// Даёт эффект "пузырей" с уменьшающейся плотностью (тут тоже)
/// </summary>
public sealed class BubbleVariation : IVariation
{
    public string Name => "bubble";

    public void Transform(double x, double y, out double newX, out double newY)
    {
        double r2 = 6 * (x * x + y * y);
        double factor = 50.0 / (r2 + 59.0);
        newX = factor * x;
        newY = factor * y;
    }

}
