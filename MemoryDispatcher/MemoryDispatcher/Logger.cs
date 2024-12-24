namespace MemoryDispatcher;

public static class Logger
{
    public const ConsoleColor InitializationColor = ConsoleColor.Cyan;
    public const ConsoleColor AllocatingColor = ConsoleColor.Green;
    
    private static readonly object Lock = new();
    
    public static void Log(string message, ConsoleColor color = ConsoleColor.Gray)
    {
        lock (Lock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffffff}] {message}");
        }
    }
}