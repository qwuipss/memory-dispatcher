using MemoryDispatcher.Algorithms.Allocation;
using MemoryDispatcher.Algorithms.Swap;
using MemoryDispatcher.Memory;
using MemoryDispatcher.Processes;

namespace MemoryDispatcher;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    private static void Main()
    {
        const string swapPath = "/home/ivan/projects/cs/memory-dispatcher/swap/";
        ISwapAlgorithm swapAlgorithm = new OmpSwapAlgorithm();
        var swap = new Swap(swapAlgorithm, swapPath);

        var memoryMap = new MemoryMap();

        const int pageSize = 128;
        const int pagesCount = 64;
        IAllocationAlgorithm allocationAlgorithm = new FfmpAllocationAlgorithm();
        var memoryDispatcher = new Memory.MemoryDispatcher(pageSize, pagesCount, allocationAlgorithm, swap, memoryMap);

        Process.KillChance = 5;

        const int processesCount = 20;
        Os.Initialize(memoryDispatcher, processesCount);
        Os.SimulationSpeedScale = 5;
        Os.Start();
    }
}