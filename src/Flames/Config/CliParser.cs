using System;
using System.Collections.Generic;
using System.Globalization;
using Flames.Logging;

namespace Flames.Config;

public static class CliParser
{
    public static void ApplyArgs(string[] args, AppConfig config, ILogger logger)
    {
        var inv = CultureInfo.InvariantCulture;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            // Флаг --config обрабатывается в ConfigLoader, здесь только пропускаем путь к файлу
            if (arg == "--config")
            {
                i++; // пропускаем значение пути к файлу
                continue;
            }

            switch (arg)
            {
                case "-w":
                case "--width":
                    config.Size.Width = ReadInt(args, ref i, "width"); 
                    break;

                case "-h":
                case "--height":
                    config.Size.Height = ReadInt(args, ref i, "height");
                    break;

                case "--seed":
                    config.Seed = ReadDouble(args, ref i, "seed");
                    break;

                case "-i":
                case "--iteration-count":
                    config.IterationCount = ReadInt(args, ref i, "iteration-count");
                    break;

                case "-o":
                case "--output-path":
                    config.OutputPath = ReadString(args, ref i, "output-path");
                    break;

                case "-t":
                case "--threads":
                    config.Threads = ReadInt(args, ref i, "threads");
                    break;

                case "-ap":
                case "--affine-params":
                {
                    // Конфигурация аффинных преобразований:
                    // формат: a,b,c,d,e,f/a,b,c,d,e,f/...
                    string value = ReadString(args, ref i, "affine-params");
                    config.AffineParams = ParseAffineParamsList(value, inv);
                    break;
                }

                case "-f":
                case "--functions":
                {
                    string value = ReadString(args, ref i, "functions");
                    config.Functions = ParseFunctions(value, inv);
                    break;
                }

                case "-g":
                case "--gamma-correction":
                {
                    // Флаг гамма-коррекции. Можно просто указать -g (true) или -g true / -g false
                    bool value = true;
                    if (i + 1 < args.Length &&
                        (string.Equals(args[i + 1], "true", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(args[i + 1], "false", StringComparison.OrdinalIgnoreCase)))
                    {
                        i++;
                        value = bool.Parse(args[i]);
                    }

                    config.GammaCorrection = value;
                    break;
                }

                case "--gamma":
                    config.Gamma = ReadDouble(args, ref i, "gamma");
                    break;

                case "-s":
                case "--symmetry-level":
                    config.SymmetryLevel = ReadInt(args, ref i, "symmetry-level");
                    break;

                default: // Неизвестный аргумент — не падаем, просто предупреждаем
                    logger.Warn($"Unknown argument '{arg}' ignored.");
                    break;
            }
        }
    }

    private static int ReadInt(string[] args, ref int index, string name)
    {
        if (index + 1 >= args.Length)
        {
            throw new ConfigException($"Missing value for '{name}'");
        }

        index++;
        if (!int.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            throw new ConfigException($"Invalid integer value for '{name}': '{args[index]}'");
        }

        return value;
    }

    private static double ReadDouble(string[] args, ref int index, string name)
    {
        if (index + 1 >= args.Length)
        {
            throw new ConfigException($"Missing value for '{name}'");
        }

        index++;
        if (!double.TryParse(args[index], NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new ConfigException($"Invalid double value for '{name}': '{args[index]}'");
        }

        return value;
    }

    private static string ReadString(string[] args, ref int index, string name)
    {
        if (index + 1 >= args.Length)
        {
            throw new ConfigException($"Missing value for '{name}'");
        }

        index++;
        return args[index];
    }

    private static List<AffineParams> ParseAffineParamsList(string raw, CultureInfo inv)
    {
        var result = new List<AffineParams>();

        // Разделение по "/" — отдельные преобразования
        var transforms = raw.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var t in transforms)
        {
            // Каждое преобразование: 6 чисел через запятую
            var parts = t.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 6)
            {
                throw new ConfigException("Each affine transform must have 6 comma-separated values: a,b,c,d,e,f");
            }

            double ParseDouble(int idx)
            {
                if (!double.TryParse(parts[idx], NumberStyles.Float, inv, out double value))
                {
                    throw new ConfigException($"Invalid affine parameter '{parts[idx]}' at position {idx + 1}");
                }

                return value;
            }

            result.Add(new AffineParams
            {
                A = ParseDouble(0),
                B = ParseDouble(1),
                C = ParseDouble(2),
                D = ParseDouble(3),
                E = ParseDouble(4),
                F = ParseDouble(5)
            });
        }

        if (result.Count == 0)
        {
            throw new ConfigException("At least one affine transform must be specified");
        }

        return result;
    }

    private static List<FunctionConfig> ParseFunctions(string raw, CultureInfo inv)
    {
        var list = new List<FunctionConfig>();
        var items = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var item in items)
        {
            var parts = item.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            string name = parts[0];
            double weight = 1.0;

            if (parts.Length > 1)
            {
                if (!double.TryParse(parts[1], NumberStyles.Float, inv, out weight))
                {
                    throw new ConfigException($"Invalid weight for function '{name}': '{parts[1]}'");
                }
            }

            list.Add(new FunctionConfig
            {
                Name = name,
                Weight = weight
            });
        }

        return list;
    }
}
