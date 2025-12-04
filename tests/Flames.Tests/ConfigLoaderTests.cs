using System;
using System.Collections.Generic;
using System.IO;
using Flames.Config;
using Flames.Logging;
using Xunit;

namespace Flames.Tests.Config;

public class ConfigLoaderTests
{
    private sealed class DummyLogger : ILogger
    {
        public readonly List<string> Infos = new();
        public readonly List<string> Warns = new();
        public readonly List<string> Errors = new();

        public void Info(string message) => Infos.Add(message);
        public void Warn(string message) => Warns.Add(message);
        public void Error(string message) => Errors.Add(message);
    }

    [Fact]
    public void Load_ShouldReadValuesFromJsonConfig()
    {
        // Arrange: создаём временный JSON файл
        string tempFile = Path.Combine(Path.GetTempPath(), $"ff_config_{Guid.NewGuid():N}.json");
        string json = """
        {
          "size": { "width": 123, "height": 456 },
          "iteration_count": 999,
          "threads": 4,
          "output_path": "from_json.png",
          "gamma": 2.5,
          "symmetry_level": 3,
          "functions": [
            { "name": "swirl", "weight": 1.0 }
          ],
          "affine_params": [
            { "a": 1.0, "b": 0.0, "c": 0.1, "d": 0.0, "e": 1.0, "f": 0.2 }
          ]
        }
        """;
        File.WriteAllText(tempFile, json);

        var args = new[] { "--config", tempFile };
        var logger = new DummyLogger();

        try
        {
            // Act
            var config = ConfigLoader.Load(args, logger);

            // Assert
            Assert.Equal(123, config.Size.Width);
            Assert.Equal(456, config.Size.Height);
            Assert.Equal(999, config.IterationCount);
            Assert.Equal(4, config.Threads);
            Assert.Equal("from_json.png", config.OutputPath);
            Assert.Equal(2.5, config.Gamma, 5);
            Assert.Equal(3, config.SymmetryLevel);
            Assert.Single(config.Functions);
            Assert.Single(config.AffineParams);
        }
        finally
        {
            // Удаляем временный файл
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_CliArgsOverrideJsonValues()
    {
        // Arrange: JSON с threads = 1
        string tempFile = Path.Combine(Path.GetTempPath(), $"ff_config_{Guid.NewGuid():N}.json");
        string json = """
        {
          "size": { "width": 100, "height": 100 },
          "threads": 1,
          "functions": [ { "name": "swirl", "weight": 1.0 } ],
          "affine_params": [ { "a": 1.0, "b": 0.0, "c": 0.0, "d": 0.0, "e": 1.0, "f": 0.0 } ]
        }
        """;
        File.WriteAllText(tempFile, json);

        // CLI должен переопределить threads и width
        var args = new[]
        {
            "--config", tempFile,
            "-t", "8",
            "-w", "640"
        };
        var logger = new DummyLogger();

        try
        {
            // Act
            var config = ConfigLoader.Load(args, logger);

            // Assert: значения из CLI перекрывают JSON
            Assert.Equal(640, config.Size.Width); 
            Assert.Equal(8, config.Threads);      
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
