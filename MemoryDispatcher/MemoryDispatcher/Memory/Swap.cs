using MemoryDispatcher.Algorithms.Swap;
using MemoryDispatcher.Processes;

namespace MemoryDispatcher.Memory;

public class Swap
{
    public static int Invokes;

    private readonly Logger _logger;
    private readonly ISwapAlgorithm _swapAlgorithm;
    private readonly string _swapPath;

    public Swap(ISwapAlgorithm swapAlgorithm, string swapPath)
    {
        _logger = Logger.ForContext<Swap>();
        _swapAlgorithm = swapAlgorithm;
        _swapPath = swapPath;

        _logger.Log($"Swap initialized. [SwapAlgorithm:{_swapAlgorithm.GetType().Name}]", Logger.InitializationColor);
    }

    public MemoryPage Load(VirtualAddress virtualAddress, Process process, out byte[] buffer)
    {
        Invokes++;

        var swapFilePath = GetMemoryPageSwapFilePath(process, virtualAddress);

        _logger.Log($"Start reading [MemoryPage:{virtualAddress.Pointer}] from [SwapFile:{swapFilePath}]", Logger.DiskWritingReadingColor);

        using var fileStream = File.OpenRead(swapFilePath);
        using var streamReader = new StreamReader(fileStream);

        var stringBuffer = streamReader.ReadLine();
        buffer = stringBuffer!.Split().Select(byte.Parse).ToArray();

        _logger.Log($"[MemoryPage:{virtualAddress.Pointer}] was read from [SwapFile:{swapFilePath}]", Logger.DiskWritingReadingColor);

        _logger.Log($"Deleting [SwapFile:{swapFilePath}] of [Process:{process.Id}]", Logger.RemovingColor);

        File.Delete(swapFilePath);

        var memoryPage = new MemoryPage(virtualAddress.Pointer, process.Id);

        return memoryPage;
    }

    public MemoryPage Unload(List<MemoryPage> memoryPages, Func<int, byte[]> bufferLocator)
    {
        Invokes++;

        var memoryPage = _swapAlgorithm.ChooseMemoryPage(memoryPages);
        memoryPages.Remove(memoryPage);

        _logger.Log($"[MemoryPage:{memoryPage.VirtualAddress.Pointer}] of [Process:{memoryPage.ProcessId}] removed from MemoryPages]",
            Logger.RemovingColor);

        var swapFilePath = GetMemoryPageSwapFilePath(memoryPage);

        _logger.Log($"Start writing [MemoryPage:{memoryPage.VirtualAddress.Pointer}] to [SwapFile:{swapFilePath}]", Logger.DiskWritingReadingColor);

        using var fileStream = OpenSwapFile(memoryPage);
        using var streamWriter = new StreamWriter(fileStream);

        var buffer = bufferLocator(memoryPage.VirtualAddress.Pointer);
        streamWriter.WriteLine(string.Join(' ', buffer));

        _logger.Log($"[MemoryPage:{memoryPage.VirtualAddress.Pointer}] was written to [SwapFile:{swapFilePath}]", Logger.DiskWritingReadingColor);

        return memoryPage;
    }

    public void FreeAll(Process process)
    {
        Invokes++;

        var processSwapDir = GetProcessSwapDirPath(process);
        if (Directory.Exists(processSwapDir))
        {
            _logger.Log($"Deleting [SwapDir:{processSwapDir}] of [Process:{process.Id}]", Logger.RemovingColor);
            Directory.Delete(processSwapDir, true);
        }
        else
        {
            _logger.Log($"[SwapDir:{processSwapDir}] of [Process:{process.Id}] not found");
        }
    }

    public void Free(Process process, VirtualAddress virtualAddress)
    {
        Invokes++;

        var processSwapFilePath = GetMemoryPageSwapFilePath(process, virtualAddress);
        _logger.Log($"Deleting [SwapFile:{processSwapFilePath}] of [Process:{process.Id}]", Logger.RemovingColor);
        File.Delete(processSwapFilePath);
    }

    private string GetMemoryPageSwapFilePath(MemoryPage memoryPage)
    {
        return Path.Join(_swapPath, $"{memoryPage.ProcessId}/bfr-{memoryPage.VirtualAddress.Pointer}");
    }

    private string GetMemoryPageSwapFilePath(Process process, VirtualAddress virtualAddress)
    {
        return Path.Join(GetProcessSwapDirPath(process), $"/bfr-{virtualAddress.Pointer}");
    }

    private string GetMemoryPageSwapFilePath(int processId, VirtualAddress virtualAddress)
    {
        return Path.Join(GetProcessSwapDirPath(processId), $"/bfr-{virtualAddress.Pointer}");
    }

    private string GetProcessSwapDirPath(Process process)
    {
        return GetProcessSwapDirPath(process.Id);
    }

    private string GetProcessSwapDirPath(int processId)
    {
        return Path.Join(_swapPath, $"/{processId}");
    }

    private FileStream OpenSwapFile(MemoryPage memoryPage)
    {
        var processSwapDir = GetProcessSwapDirPath(memoryPage.ProcessId);

        string processSwapFilePath;

        if (!Directory.Exists(processSwapDir))
        {
            _logger.Log($"Creating [SwapDir:{processSwapDir}] for [Process:{memoryPage.ProcessId}]", Logger.DiskWritingReadingColor);

            Directory.CreateDirectory(processSwapDir);

            processSwapFilePath = GetMemoryPageSwapFilePath(memoryPage.ProcessId, memoryPage.VirtualAddress);

            return File.Create(processSwapFilePath);
        }

        processSwapFilePath = GetMemoryPageSwapFilePath(memoryPage.ProcessId, memoryPage.VirtualAddress);

        if (File.Exists(processSwapFilePath)) return File.OpenWrite(processSwapFilePath);

        _logger.Log($"Creating [SwapFile:{processSwapFilePath}] for [Process:{memoryPage.ProcessId}]", Logger.DiskWritingReadingColor);

        return File.Create(processSwapFilePath);
    }
}