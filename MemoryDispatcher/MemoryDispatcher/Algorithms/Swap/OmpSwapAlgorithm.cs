using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Algorithms.Swap;

public class OmpSwapAlgorithm : ISwapAlgorithm
{
    private readonly Logger _logger = Logger.ForContext<ISwapAlgorithm>();

    public MemoryPage ChooseMemoryPage(List<MemoryPage> memoryPages)
    {
        var memoryPagesMaxId = memoryPages.Max(memoryPage => memoryPage.VirtualAddress.Pointer);
        var memoryPage = memoryPages.First(memoryPage => memoryPage.VirtualAddress.Pointer == memoryPagesMaxId);
        _logger.Log($"Chosen [MemoryPage:{memoryPage.VirtualAddress.Pointer}]");
        return memoryPage;
    }
}