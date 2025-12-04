using System;
using Flames.Config;
using Flames.Core;
using Flames.Logging;
using Flames.Variations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Flames.Tests.Core;

public class FlamesGeneratorMoreTests
{
    private sealed class NullLogger : ILogger
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message) { }
    }

    private AppConfig CreateSimpleConfig()
    {
        return new AppConfig
        {
            Size = new SizeConfig { Width = 120, Height = 80 },
            IterationCount = 20_000,
            Threads = 1,
            OutputPath = "test.png",
            GammaCorrection = false,
            SymmetryLevel = 1,
            Functions = new()
            {
                new FunctionConfig { Name = "swirl",      Weight = 1.0 },
                new FunctionConfig { Name = "horseshoe",  Weight = 0.7 }
            },
            AffineParams = new()
            {
                new AffineParams { A = 0.5, B = 0.1, C = -0.2, D = 0.0, E = 0.5, F = 0.0 },
                new AffineParams { A = 0.3, B = -0.2, C = 0.1, D = 0.2, E = 0.4, F = 0.1 }
            }
        };
    }

    [Fact]
    public void Generate_ImageHasNonBlackPixels()
    {
        // Arrange
        var config = CreateSimpleConfig();
        var logger = new NullLogger();
        var variations = VariationFactory.Create(config);
        var generator = new FlamesGenerator(config, variations, logger);

        // Act
        using Image<Rgba32> image = generator.Generate();

        // Assert

        int nonBlack = 0;
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var c = image[x, y];
                if (c.R != 0 || c.G != 0 || c.B != 0)
                {
                    nonBlack++;
                }
            }
        }

        Assert.True(nonBlack > 0);
    }

    [Fact]
    public void Generate_WithGammaCorrection_DoesNotThrow()
    {
        // Arrange
        var config = CreateSimpleConfig();
        config.GammaCorrection = true;
        config.Gamma = 2.2;

        var logger = new NullLogger();
        var variations = VariationFactory.Create(config);
        var generator = new FlamesGenerator(config, variations, logger);

        // Act & Assert
        using Image<Rgba32> image = generator.Generate();

        Assert.Equal(config.Size.Width, image.Width);
        Assert.Equal(config.Size.Height, image.Height);
    }
}
