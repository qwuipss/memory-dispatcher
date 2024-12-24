using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Algorithms.Allocation;

public class FirstFreePageAllocationAlgorithm : IAllocationAlgorithm
{
    public MemoryPage? TryChooseMemoryPage(List<MemoryPage> memoryPages)
    {
        var memoryPage = memoryPages.FirstOrDefault(memoryPage => memoryPage.ProcessId is 0);
        return memoryPage;
    }

    public MemoryPage ChooseMemoryPage(List<MemoryPage> memoryPages)
    {
        var memoryPage = memoryPages.First(memoryPage => memoryPage.ProcessId is 0);
        return memoryPage;
    }
}