using System;
using System.IO;
using System.Text.Json;
using Flames.Logging;

namespace Flames.Config;

public static class ConfigLoader
{
    /// <summary>
    /// Главный метод загрузки конфигурации
    /// </summary>
    public static AppConfig Load(string[] args, ILogger logger)
    {
        // Начинаем с конфигурации по умолчанию
        var config = new AppConfig();

        string? configPath = FindConfigPath(args);
        if (configPath != null)
        {
            logger.Info($"Loading config from '{configPath}'...");
            try
            {
                var json = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Десериализация JSON в AppConfig
                var jsonConfig = JsonSerializer.Deserialize<AppConfig>(json, options);
                if (jsonConfig == null)
                {
                    throw new ConfigException("Config file is empty or invalid JSON");
                }

                // Заменяем дефолтную конфигурацию тем, что пришло из файла
                config = jsonConfig;
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is JsonException ||
                ex is ConfigException)
            {
                // Оборачиваем любую ошибку в ConfigException с понятным описанием
                throw new ConfigException($"Failed to read config '{configPath}': {ex.Message}");
            }
        }

        // Теперь применяем аргументы командной строки поверх уже загруженной конфигурации
        CliParser.ApplyArgs(args, config, logger);

        // Если функций нет — назначаем дефолтную функцию
        if (config.Functions == null || config.Functions.Count == 0)
        {
            logger.Warn("No functions specified, using default: swirl:1.0");
            config.Functions = new()
            {
                new FunctionConfig { Name = "swirl", Weight = 1.0 }
            };
        }

        // Если affine-параметров нет — используем единичное преобразование
        if (config.AffineParams == null || config.AffineParams.Count == 0)
        {
            logger.Warn("No affine params specified, using default identity transform.");
            config.AffineParams = new List<AffineParams>
            {
                new AffineParams()
            };
        }

        return config;
    }

    private static string? FindConfigPath(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--config")
            {
                return args[i + 1];
            }
        }
        return null;
    }
}