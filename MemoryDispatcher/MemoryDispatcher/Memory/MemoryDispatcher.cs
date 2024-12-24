using MemoryDispatcher.Algorithms.Allocation;
using MemoryDispatcher.Processes;

namespace MemoryDispatcher.Memory;

public class MemoryDispatcher
{
    private readonly int _pageSize;

    private readonly List<MemoryPage> _memoryPages;

    private readonly IAllocationAlgorithm _allocationAlgorithm;
    private readonly Swap _swap;
    private readonly MemoryMap _memoryMap;

    private int AutoIncrementVirtualAddressPointer => _virtualAddressPointer++;
    private int _virtualAddressPointer;

    public MemoryDispatcher(int pageSize, int pagesCount, IAllocationAlgorithm allocationAlgorithm, Swap swap, MemoryMap memoryMap)
    {
        _pageSize = pageSize;

        _memoryPages = Enumerable.Range(0, pagesCount)
            .Select(_ => new MemoryPage(AutoIncrementVirtualAddressPointer, 0))
            .ToList();

        _allocationAlgorithm = allocationAlgorithm;
        _swap = swap;
        _memoryMap = memoryMap;

        Logger.Log(
            $"Memory dispatcher initialized. PageSize: [{pageSize}], PagesCount: [{pagesCount}]. AllocationAlgorithm: [{_allocationAlgorithm.GetType().Name}]",
            ConsoleColor.Cyan);
    }

    public void Write(Process process, VirtualAddress virtualAddress, byte[] data, int offset)
    {
    }

    public VirtualAddress Allocate(Process process)
    {
        lock (_memoryPages)
        {//check 
            Logger.Log($"Allocating MemoryPage for Process: [{process.Id}]", Logger.AllocatingColor);
            
            var memoryPage = _allocationAlgorithm.TryChooseMemoryPage(_memoryPages);
            if (memoryPage is not null)
            {
                memoryPage.ProcessId = process.Id;
                _memoryMap[memoryPage.VirtualAddress.Pointer] = new byte[_pageSize];
                
                Logger.Log($"Allocated MemoryPage: [{memoryPage.VirtualAddress.Pointer}] for Process: [{process.Id}]", Logger.AllocatingColor);
                
                return memoryPage.VirtualAddress;
            }

            var unloadedMemoryPage = _swap.Unload(_memoryPages, virtualAddressPointer => _memoryMap[virtualAddressPointer]);
            _memoryMap.Remove(unloadedMemoryPage.VirtualAddress.Pointer);

            var newMemoryPage = new MemoryPage(AutoIncrementVirtualAddressPointer, process.Id);
            _memoryPages.Add(newMemoryPage);
            _memoryMap[newMemoryPage.VirtualAddress.Pointer] = new byte[_pageSize];

            return newMemoryPage.VirtualAddress;
        }
    }

    public void FreeAll(Process process)
    {
        lock (_memoryPages)
        {
            foreach (var memoryPage in _memoryPages.Where(memoryPage => memoryPage.ProcessId == process.Id))
            {
                FreeMemoryPage(memoryPage);
            }

            _swap.FreeAll(process);
        }
    }

    public void Free(Process process, VirtualAddress virtualAddress)
    {
        // check
        lock (_memoryPages)
        {
            var memoryPage = _memoryPages.FirstOrDefault(memoryPage => memoryPage.VirtualAddress.Pointer == virtualAddress.Pointer);
            if (memoryPage is null)
            {
                _swap.Free(process, virtualAddress);
                return;
            }

            FreeMemoryPage(memoryPage);
        }
    }

    private void FreeMemoryPage(MemoryPage memoryPage)
    {
        memoryPage.ProcessId = 0;
        _memoryMap.Remove(memoryPage.VirtualAddress.Pointer);
    }
}