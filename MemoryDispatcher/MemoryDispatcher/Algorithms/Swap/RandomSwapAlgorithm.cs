using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Algorithms.Swap;

public class RandomSwapAlgorithm : ISwapAlgorithm
{
    private readonly Logger _logger = Logger.ForContext<ISwapAlgorithm>();

    public MemoryPage ChooseMemoryPage(List<MemoryPage> memoryPages)
    {
        var memoryPage = memoryPages[Random.Shared.Next(memoryPages.Count)];
        _logger.Log($"Chosen [MemoryPage:{memoryPage.VirtualAddress.Pointer}]");
        return memoryPage;
    }
}