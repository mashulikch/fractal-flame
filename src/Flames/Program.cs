using System;
using Flames.Config;
using Flames.Core;
using Flames.Imaging;
using Flames.Logging;
using Flames.Variations;
using SixLabors.ImageSharp;

namespace Flames;

public static class Program
{
    public static int Main(string[] args)
    {
        var logger = new ConsoleLogger();

        try
        {
            if (args.Any(a => a == "--help" || a == "-?"))
            {
                PrintHelp();
                return 0;
            }

            logger.Info("Fractal flame generator starting...");

            var config = ConfigLoader.Load(args, logger);
            ConfigValidator.Validate(config);

            logger.Info($"Image size: {config.Size.Width}x{config.Size.Height}");
            logger.Info($"Iterations: {config.IterationCount}");
            logger.Info($"Threads: {config.Threads}");
            logger.Info($"Output: {config.OutputPath}");
            logger.Info($"Gamma correction: {(config.GammaCorrection ? "on" : "off")} (gamma = {config.Gamma})");
            logger.Info($"Symmetry level: {config.SymmetryLevel}");

            var variations = VariationFactory.Create(config);
            logger.Info($"Variations: {string.Join(", ", variations.Select(v => v.Name))}");

            var generator = new FlamesGenerator(config, variations, logger);

            using var bitmap = generator.Generate();
            ImageSaver.SavePng(bitmap, config.OutputPath, logger);

            logger.Info("Done.");
            return 0;
        }
        catch (ConfigException ex)
        {
            logger.Error("Configuration error: " + ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            logger.Error("Fatal error: " + ex.Message);
            logger.Error(ex.StackTrace ?? string.Empty);
            return 1;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Fractal Flame Generator");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet <dll_path> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -w, --width <int>                 Image width (default: 1920)");
        Console.WriteLine("  -h, --height <int>                Image height (default: 1080)");
        Console.WriteLine("      --seed <double>               Random seed (default: 5.1234)");
        Console.WriteLine("  -i, --iteration-count <int>       Iteration count (default: 2500)");
        Console.WriteLine("  -o, --output-path <path>          Output PNG path (default: result.png)");
        Console.WriteLine("  -t, --threads <int>               Threads count (default: 1)");
        Console.WriteLine("  -ap, --affine-params <a,b,c,d,e,f>");
        Console.WriteLine("                                    Affine transform parameters");
        Console.WriteLine("  -f, --functions name:weight,...   Variations config, e.g. swirl:1.0,horseshoe:0.8");
        Console.WriteLine("      --config <file>               JSON config file");
        Console.WriteLine("  -g, --gamma-correction [bool]     Enable gamma correction (default: false)");
        Console.WriteLine("      --gamma <double>              Gamma value (default: 2.2)");
        Console.WriteLine("  -s, --symmetry-level <int>        Symmetry level N>=1 (default: 1)");
        Console.WriteLine("      --help, -?                    Show this help");
    }
}
