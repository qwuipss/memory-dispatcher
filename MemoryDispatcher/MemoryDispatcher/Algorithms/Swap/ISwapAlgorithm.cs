using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Algorithms.Swap;

public interface ISwapAlgorithm
{
    MemoryPage ChooseMemoryPage(List<MemoryPage> memoryPages);
}