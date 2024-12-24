using MemoryDispatcher.Algorithms.Allocation;
using MemoryDispatcher.Algorithms.Swap;
using MemoryDispatcher.Memory;
using MemoryDispatcher.Processes;

namespace MemoryDispatcher;

class Program
{
    static void Main()
    {
        const string swapPath = "/home/ivan/projects/cs/memory-dispatcher/swap";
        var swapAlgorithm = new LfuSwapAlgorithm();
        var swap = new Swap(swapAlgorithm, swapPath);

        var memoryMap = new MemoryMap();

        const int pageSize = 128;
        const int pagesCount = 20;
        var allocationAlgorithm = new FirstFreePageAllocationAlgorithm();
        var memoryDispatcher = new Memory.MemoryDispatcher(pageSize, pagesCount, allocationAlgorithm, swap, memoryMap);

        Os.Initialize(memoryDispatcher);

        var process = Process.Create();
        process.Run();
    }
}