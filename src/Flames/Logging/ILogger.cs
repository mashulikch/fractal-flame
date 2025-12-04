namespace Flames.Logging;

/// <summary>
/// Простейший интерфейс логгера.
/// Позволяет писать информационные сообщения, предупреждения и ошибки
/// </summary>
public interface ILogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}