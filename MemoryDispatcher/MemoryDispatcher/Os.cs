using MemoryDispatcher.Processes;

namespace MemoryDispatcher;

public class Os
{
    public const int SimulationSpeedScale = 10;

    private static Memory.MemoryDispatcher _memoryDispatcher;

    public static void Initialize(Memory.MemoryDispatcher memoryDispatcher)
    {
        _memoryDispatcher = memoryDispatcher;
        
        Logger.Log($"Os initialized", Logger.InitializationColor);
    }
    
    public static void RequireMemory(Process process)
    {
        _memoryDispatcher.Allocate(process);
    }
}