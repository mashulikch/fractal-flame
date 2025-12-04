using System;
using System.IO;
using Flames.Imaging;
using Flames.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Flames.Tests.Imaging;

public class ImageSaverTests
{
    private sealed class NullLogger : ILogger
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message) { }
    }

    [Fact]
    public void SavePng_CreatesNonEmptyPngFile()
    {
        // Arrange
        string dir = Path.Combine(Path.GetTempPath(), "ff_imagesaver_tests");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, $"test_{Guid.NewGuid():N}.png");

        using var image = new Image<Rgba32>(10, 10);

        image[2, 2] = new Rgba32(255, 255, 255);
        image[3, 3] = new Rgba32(255, 0, 0);

        var logger = new NullLogger();

        // Act
        ImageSaver.SavePng(image, path, logger);

        // Assert
        Assert.True(File.Exists(path));
        var length = new FileInfo(path).Length;
        Assert.True(length > 0);

        using (var fs = File.OpenRead(path))
        {
            byte[] sig = new byte[8];
            int read = fs.Read(sig, 0, 8);
            Assert.Equal(8, read);
            byte[] expected = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Assert.Equal(expected, sig);
        }
    }
}