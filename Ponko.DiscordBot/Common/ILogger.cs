using System.Threading.Channels;

namespace Ponko.DiscordBot.Common;

public enum LogType
{
    Debug,
    Warning,
    Error,
}

public interface ILogger
{
    void Log(string msg);
    void LogWarning(string msg);
    void LogError(string msg);
    void Log(string msg, LogType type)
    {
        if (type == LogType.Debug) Log(msg);
        else if (type == LogType.Warning) LogWarning(msg);
        else if (type == LogType.Error) LogError(msg);
    }
}

public class DefaultLogger : ILogger
{
    public bool Timestamp { get; set; } = true;

    private void Print(string msg)
    {
        if (Timestamp)
        {
            var now = DateTime.Now;
            msg = $"[{now.ToShortTimeString()}] {msg}";
        }
        else
        {
            msg = $"[] {msg}";
        }
        Console.WriteLine(msg);
    }

    public void Log(string msg)
    {
        Print(msg);
    }

    public void Log(string msg, ConsoleColor clr)
    {
        Console.ForegroundColor = clr;
        Print(msg);
        Console.ResetColor();
    }

    public void LogError(string msg)
    {
        Log(msg, ConsoleColor.Red);
    }

    public void LogWarning(string msg)
    {
        Log(msg, ConsoleColor.Yellow);
    }
}
