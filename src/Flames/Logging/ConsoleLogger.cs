using System;

namespace Flames.Logging;

/// <summary>
/// Реализация ILogger, которая пишет сообщения в консоль с разным цветом
/// </summary>
public sealed class ConsoleLogger : ILogger
{
    private readonly object _lock = new(); // Блокировка, чтобы несколько потоков не печатали вперемешку

    public void Info(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[INFO ] {DateTime.Now:HH:mm:ss} {message}");
            Console.ResetColor();
        }
    }

    public void Warn(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN ] {DateTime.Now:HH:mm:ss} {message}");
            Console.ResetColor();
        }
    }

    public void Error(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} {message}");
            Console.ResetColor();
        }
    }
}