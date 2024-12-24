using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Algorithms.Allocation;

public interface IAllocationAlgorithm
{
    MemoryPage? TryChooseMemoryPage(List<MemoryPage> memoryPages);
}