using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Algorithms.Allocation;

public class RmpAllocationAlgorithm : IAllocationAlgorithm
{
    private readonly Logger _logger = Logger.ForContext<IAllocationAlgorithm>();

    public MemoryPage? TryChooseMemoryPage(List<MemoryPage> memoryPages)
    {
        var attempts = 5;
        MemoryPage? memoryPage;

        while (attempts > 0)
        {
            memoryPage = memoryPages[Random.Shared.Next(memoryPages.Count)];
            if (memoryPage.ProcessId is 0)
            {
                _logger.Log($"Chosen [MemoryPage:{memoryPage.VirtualAddress.Pointer}]");
                return memoryPage;
            }

            attempts--;
        }

        // ReSharper disable once VariableHidesOuterVariable
        memoryPage = memoryPages.FirstOrDefault(memoryPage => memoryPage.ProcessId is 0);
        _logger.Log(memoryPage is null ? "MemoryPage was not chosen" : $"Chosen [MemoryPage:{memoryPage.VirtualAddress.Pointer}]");
        return memoryPage;
    }
}