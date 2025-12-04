using System;
using Flames.Variations;
using Xunit;

namespace Flames.Tests.Variations;

public class VariationsTests
{
    private const double Eps = 1e-9;

    [Fact]
    public void Swirl_ZeroPoint_RemainsZero()
    {
        // Arrange
        var v = new SwirlVariation();

        // Act
        v.Transform(0.0, 0.0, out double nx, out double ny);

        // Assert
        Assert.InRange(nx, -Eps, Eps);
        Assert.InRange(ny, -Eps, Eps);
    }

    [Fact]
    public void Horseshoe_UsesSameFormulaAsImplementation()
    {
        // Arrange
        var v = new HorseshoeVariation();
        double x = 1.0;
        double y = 0.0;

        // Act
        v.Transform(x, y, out double nx, out double ny);

        // Expected: ровно та же формула, что и в твоей реализации:
        // r = 2 / (sqrt(x^2 + y^2) + 1)
        double r = 2.0 / (Math.Sqrt(x * x + y * y) + 1.0);
        double expectedX = r * x;
        double expectedY = r * y;

        // Assert
        Assert.InRange(nx, expectedX - Eps, expectedX + Eps);
        Assert.InRange(ny, expectedY - Eps, expectedY + Eps);
    }

    [Fact]
    public void Bubble_ScalesPointByExpectedFactor()
    {
        // Arrange
        var v = new BubbleVariation();
        double x = 1.0;
        double y = 0.0;

        // Act
        v.Transform(x, y, out double nx, out double ny);

        // Assert: считаем по той же формуле
        double r2 = 6.0 * (x * x + y * y);
        double factor = 50.0 / (r2 + 59.0);
        double expectedX = factor * x;
        double expectedY = factor * y;

        Assert.InRange(nx, expectedX - Eps, expectedX + Eps);
        Assert.InRange(ny, expectedY - Eps, expectedY + Eps);
    }

    [Fact]
    public void Exponential_YZero_UsesExpXMinusOne()
    {
        // Arrange
        var v = new ExponentialVariation();
        double x = 1.0;
        double y = 0.0;

        // Act
        v.Transform(x, y, out double nx, out double ny);

        // Assert: при y=0 угол = 0, cos=1, sin=0
        double ex = Math.Exp(x - 1.0); // exp(0) = 1
        double expectedX = ex;
        double expectedY = 0.0;

        Assert.InRange(nx, expectedX - Eps, expectedX + Eps);
        Assert.InRange(ny, expectedY - Eps, expectedY + Eps);
    }

    [Fact]
    public void Handkerchief_ZeroPoint_IsFinite()
    {
        // Arrange
        var v = new Handkerchief();

        // Act
        v.Transform(0.0, 0.0, out double nx, out double ny);

        // Assert
        Assert.False(double.IsNaN(nx));
        Assert.False(double.IsNaN(ny));
        Assert.InRange(nx, -Eps, Eps);
        Assert.InRange(ny, -Eps, Eps);
    }

    [Fact]
    public void Spherical_ProducesFiniteValues()
    {
        // Arrange
        var v = new SphericalVariation();

        // Act
        v.Transform(0.3, -0.7, out double nx, out double ny);

        // Assert
        Assert.False(double.IsNaN(nx));
        Assert.False(double.IsNaN(ny));
        Assert.False(double.IsInfinity(nx));
        Assert.False(double.IsInfinity(ny));
    }
}
