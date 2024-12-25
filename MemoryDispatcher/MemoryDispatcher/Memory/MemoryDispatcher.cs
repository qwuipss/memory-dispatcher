using MemoryDispatcher.Algorithms.Allocation;
using MemoryDispatcher.Processes;

namespace MemoryDispatcher.Memory;

public class MemoryDispatcher
{
    private readonly IAllocationAlgorithm _allocationAlgorithm;
    private readonly Logger _logger;
    private readonly MemoryMap _memoryMap;

    private readonly List<MemoryPage> _memoryPages;
    private readonly int _pageSize;
    private readonly Swap _swap;
    private int _virtualAddressPointer;

    public MemoryDispatcher(int pageSize, int pagesCount, IAllocationAlgorithm allocationAlgorithm, Swap swap, MemoryMap memoryMap)
    {
        _logger = Logger.ForContext<MemoryDispatcher>();

        _pageSize = pageSize;

        _memoryPages = Enumerable.Range(0, pagesCount)
            .Select(_ => new MemoryPage(AutoIncrementVirtualAddressPointer, 0))
            .ToList();

        _allocationAlgorithm = allocationAlgorithm;
        _swap = swap;
        _memoryMap = memoryMap;

        _logger.Log(
            $"Memory dispatcher initialized. [PageSize:{pageSize}], [PagesCount:{pagesCount}]. [AllocationAlgorithm:{_allocationAlgorithm.GetType().Name}]",
            ConsoleColor.Cyan);
    }

    private int AutoIncrementVirtualAddressPointer => ++_virtualAddressPointer;

    public void Write(Process process, VirtualAddress address, int addressOffset, byte[] data, int dataOffset, int dataWriteLength)
    {
        lock (_memoryPages)
        {
            if (_memoryMap.TryGetBuffer(address.Pointer, out var buffer))
            {
                _logger.Log($"[MemoryPage:{address.Pointer}] found in MemoryMap. Start writing data", Logger.MemoryWritingReadingColor);

                for (var i = 0; i < dataWriteLength; i++) buffer![addressOffset + i] = data[dataOffset + i];
            }
            else
            {
                _logger.Log($"[MemoryPage:{address.Pointer}] not found in MemoryMap");
                _logger.Log($"Allocating MemoryPage for [Process:{process.Id}]", Logger.AllocatingColor);

                var memoryPage = _allocationAlgorithm.TryChooseMemoryPage(_memoryPages);
                if (memoryPage is null)
                {
                    // ReSharper disable once InconsistentlySynchronizedField
                    var unloadedMemoryPage = _swap.Unload(_memoryPages, virtualAddressPointer => _memoryMap[virtualAddressPointer]);
                    _memoryMap.Remove(unloadedMemoryPage.VirtualAddress.Pointer);

                    _logger.Log($"[MemoryPage:{unloadedMemoryPage.VirtualAddress.Pointer}] of [Process:{process.Id}] removed from MemoryMap",
                        Logger.RemovingColor);

                    var loadedMemoryPage = _swap.Load(address, process, out buffer);
                    memoryPage = loadedMemoryPage;
                }
                else
                {
                    _logger.Log($"Allocated existed [MemoryPage:{memoryPage.VirtualAddress.Pointer}] for [Process:{process.Id}]",
                        Logger.AllocatingColor);

                    var loadedMemoryPage = _swap.Load(address, process, out buffer);

                    _memoryPages.Remove(memoryPage);

                    memoryPage = loadedMemoryPage;
                }

                _memoryPages.Add(memoryPage);

                _logger.Log($"Allocated new [MemoryPage:{memoryPage.VirtualAddress.Pointer}] for [Process:{process.Id}]",
                    Logger.AllocatingColor);

                _memoryMap[memoryPage.VirtualAddress.Pointer] = buffer;

                _logger.Log($"[MemoryPage:{memoryPage.VirtualAddress.Pointer}] of [Process:{process.Id}] added to MemoryMap",
                    Logger.AllocatingColor);
            }
        }
    }

    public (VirtualAddress Address, int Size) Allocate(Process process)
    {
        lock (_memoryPages)
        {
            //check 
            _logger.Log($"Allocating MemoryPage for [Process:{process.Id}]", Logger.AllocatingColor);

            var memoryPage = _allocationAlgorithm.TryChooseMemoryPage(_memoryPages);
            if (memoryPage is not null)
            {
                memoryPage.ProcessId = process.Id;
                _memoryMap[memoryPage.VirtualAddress.Pointer] = new byte[_pageSize];

                _logger.Log($"Allocated existed [MemoryPage:{memoryPage.VirtualAddress.Pointer}] for [Process:{process.Id}]", Logger.AllocatingColor);

                return (memoryPage.VirtualAddress, _pageSize);
            }

            // ReSharper disable once InconsistentlySynchronizedField
            var unloadedMemoryPage = _swap.Unload(_memoryPages, virtualAddressPointer => _memoryMap[virtualAddressPointer]);
            _memoryMap.Remove(unloadedMemoryPage.VirtualAddress.Pointer);

            _logger.Log($"[MemoryPage:{unloadedMemoryPage.VirtualAddress.Pointer}] of [Process:{process.Id}] removed from MemoryMap",
                Logger.RemovingColor);

            var newMemoryPage = new MemoryPage(AutoIncrementVirtualAddressPointer, process.Id);
            _memoryPages.Add(newMemoryPage);

            _logger.Log($"Allocated new [MemoryPage:{newMemoryPage.VirtualAddress.Pointer}] for [Process:{process.Id}]", Logger.AllocatingColor);

            _memoryMap[newMemoryPage.VirtualAddress.Pointer] = new byte[_pageSize];

            _logger.Log($"[MemoryPage:{newMemoryPage.VirtualAddress.Pointer}] of [Process:{process.Id}] added to MemoryMap",
                Logger.AllocatingColor);

            return (newMemoryPage.VirtualAddress, _pageSize);
        }
    }

    public void FreeAll(Process process)
    {
        lock (_memoryPages)
        {
            foreach (var memoryPage in _memoryPages.Where(memoryPage => memoryPage.ProcessId == process.Id)) FreeMemoryPage(memoryPage);

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
        _logger.Log($"[MemoryPage:{memoryPage.VirtualAddress.Pointer}] of [Process:{memoryPage.ProcessId}] removed from MemoryMap]");
    }
}