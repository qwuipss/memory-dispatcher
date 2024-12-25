using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Algorithms.Allocation;

public class FfmpAllocationAlgorithm : IAllocationAlgorithm
{
    private readonly Logger _logger = Logger.ForContext<IAllocationAlgorithm>();

    public MemoryPage? TryChooseMemoryPage(List<MemoryPage> memoryPages)
    {
        var memoryPage = memoryPages.FirstOrDefault(memoryPage => memoryPage.ProcessId is 0);
        _logger.Log(memoryPage is null ? "MemoryPage was not chosen" : $"Chosen [MemoryPage:{memoryPage.VirtualAddress.Pointer}]");
        return memoryPage;
    }
}