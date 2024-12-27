using MemoryDispatcher.Memory;
using MemoryDispatcher.Processes;

namespace MemoryDispatcher;

public class Os
{
#pragma warning disable CA2211
    public static int SimulationSpeedScale;
#pragma warning restore CA2211

    private static readonly object Lock = new();

    private static Memory.MemoryDispatcher _memoryDispatcher = null!;
    private static readonly Logger Logger = Logger.ForContext<Os>();

    // ReSharper disable once CollectionNeverQueried.Local
    private static readonly HashSet<Process> Processes = [];

    public static void Initialize(Memory.MemoryDispatcher memoryDispatcher, int processesCount)
    {
        _memoryDispatcher = memoryDispatcher;

        for (var i = 0; i < processesCount; i++) Processes.Add(CreateProcess());

        Logger.Log("Os initialized", Logger.InitializationColor);
    }

    public static void Start()
    {
        foreach (var process in Processes) process.Run();

        Console.ReadLine();
        Console.WriteLine(
            $"[SwapInvokes:{Swap.Invokes}]. [MemoryDispatcherInvokes:{Memory.MemoryDispatcher.Invokes}]. [Ratio:{Memory.MemoryDispatcher.Invokes / Swap.Invokes}]");
        Console.WriteLine();
    }

    public static (VirtualAddress Address, int Size) RequireMemory(Process process)
    {
        return _memoryDispatcher.Allocate(process);
    }

    public static void RequireWrite(Process process, VirtualAddress address, int addressOffset, byte[] data, int dataOffset, int dataWriteLength)
    {
        _memoryDispatcher.Write(process, address, addressOffset, data, dataOffset, dataWriteLength);
    }

    public static void RequireFree(Process process, VirtualAddress address)
    {
        _memoryDispatcher.Free(process, address);
    }

    public static void Kill(Process process)
    {
        lock (Lock)
        {
            _memoryDispatcher.FreeAll(process);
            Processes.Remove(process);

            var newProcess = CreateProcess();
            Processes.Add(newProcess);
            newProcess.Run();
        }
    }

    private static Process CreateProcess()
    {
        return ProcessFactory.Create();
    }
}