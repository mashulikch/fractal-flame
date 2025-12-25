using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Flames.Logging;

namespace Flames.Imaging;

/// <summary>
/// Сохранение итогового изображения в PNG.
/// Теперь использую ImageSharp, и работает на всех ОС
/// </summary>
public static class ImageSaver
{
    public static void SavePng(Image<Rgba32> image, string outputPath, ILogger logger)
    {
        try
        {
            string directory = Path.GetDirectoryName(outputPath) ?? ".";
            if (!string.IsNullOrEmpty(directory) && directory != ".")
            {
                Directory.CreateDirectory(directory);
            }

            image.Save(outputPath, new PngEncoder());
            logger.Info($"Image saved to '{outputPath}'.");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Failed to save image '{outputPath}': {ex.Message}", ex);
        }
    }
}