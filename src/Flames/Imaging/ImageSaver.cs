using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Flames.Logging;

namespace Flames.Imaging;

/// <summary>
/// Класс для сохранения Bitmap в PNG-файл
/// </summary>
public static class ImageSaver
{
    public static void SavePng(Bitmap bitmap, string outputPath, ILogger logger)
    {
        try
        {
            string directory = Path.GetDirectoryName(outputPath) ?? ".";
            if (!string.IsNullOrEmpty(directory) && directory != ".")
            {
                Directory.CreateDirectory(directory);
            }
            
            bitmap.Save(outputPath, ImageFormat.Png);
            logger.Info($"Image saved to '{outputPath}'.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save image '{outputPath}': {ex.Message}", ex);
        }
    }
}