namespace Flames.Config;

/// <summary>
/// Проверка корректности конфигурации перед запуском генерации
/// </summary>
public static class ConfigValidator
{
    public static void Validate(AppConfig config)
    {
        if (config.Size.Width <= 0)
        {
            throw new ConfigException("Width must be > 0");
        }

        if (config.Size.Height <= 0)
        {
            throw new ConfigException("Height must be > 0");
        }

        if (config.IterationCount <= 0)
        {
            throw new ConfigException("Iteration count must be > 0");
        }

        if (config.Threads <= 0)
        {
            throw new ConfigException("Threads must be > 0");
        }

        if (string.IsNullOrWhiteSpace(config.OutputPath))
        {
            throw new ConfigException("Output path must not be empty");
        }

        if (config.Functions == null || config.Functions.Count == 0)
        {
            throw new ConfigException("At least one function must be specified");
        }

        foreach (var f in config.Functions)
        {
            if (string.IsNullOrWhiteSpace(f.Name))
            {
                throw new ConfigException("Function name must not be empty");
            }

            if (f.Weight <= 0)
            {
                throw new ConfigException($"Function '{f.Name}' weight must be > 0");
            }
        }

        if (config.AffineParams == null || config.AffineParams.Count == 0)
        {
            throw new ConfigException("At least one set of affine params must be specified");
        }

        if (config.Gamma <= 0)
        {
            throw new ConfigException("Gamma must be > 0");
        }

        if (config.SymmetryLevel < 1)
        {
            throw new ConfigException("Symmetry level must be >= 1");
        }
    }
}