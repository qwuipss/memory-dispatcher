using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Algorithms.Swap;

public class LfuSwapAlgorithm : ISwapAlgorithm // LeastFrequentlyUsed
{
    public MemoryPage ChooseMemoryPage(List<MemoryPage> memoryPages)
    {
        var memoryPagesMaxId = memoryPages.Max(memoryPage => memoryPage.VirtualAddress.Pointer);
        var memoryPage = memoryPages.First(memoryPage => memoryPage.VirtualAddress.Pointer == memoryPagesMaxId);
        return memoryPage;
    }
}