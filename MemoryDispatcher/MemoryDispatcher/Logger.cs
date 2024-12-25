namespace MemoryDispatcher;

public class Logger
{
    public const ConsoleColor InitializationColor = ConsoleColor.Cyan;
    public const ConsoleColor AllocatingColor = ConsoleColor.Green;
    public const ConsoleColor RemovingColor = ConsoleColor.Red;
    public const ConsoleColor DiskWritingReadingColor = ConsoleColor.Blue;
    public const ConsoleColor MemoryWritingReadingColor = ConsoleColor.Yellow;
    public const ConsoleColor OsCallColor = ConsoleColor.Magenta;

    private static readonly object Lock = new();

    private readonly string _context;

    private Logger(string context)
    {
        _context = context;
    }

    public static Logger ForContext<TContext>()
    {
        return new Logger(typeof(TContext).Name);
    }

    public void Log(string message, ConsoleColor color = ConsoleColor.Gray)
    {
        lock (Lock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffffff}] [{_context}] {message}");
        }
    }
}