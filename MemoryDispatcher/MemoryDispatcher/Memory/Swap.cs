using MemoryDispatcher.Algorithms.Swap;
using MemoryDispatcher.Processes;

namespace MemoryDispatcher.Memory;

public class Swap(ISwapAlgorithm swapAlgorithm, string swapPath)
{
    private readonly ISwapAlgorithm _swapAlgorithm = swapAlgorithm;

    private readonly string _swapPath = swapPath;

    public MemoryPage Unload(List<MemoryPage> memoryPages, Func<int, byte[]> bufferLocator)
    {
        var memoryPage = _swapAlgorithm.ChooseMemoryPage(memoryPages);
        memoryPages.Remove(memoryPage);

        var swapPath = GetMemoryPageSwapFilePath(memoryPage);

        using var textWriter = new StreamWriter(File.OpenWrite(swapPath));

        textWriter.WriteLine(memoryPage.ProcessId);
        textWriter.WriteLine(memoryPage.VirtualAddress.Pointer);

        var buffer = bufferLocator(memoryPage.VirtualAddress.Pointer);
        textWriter.WriteLine(string.Join(' ', buffer));

        return memoryPage;
    }

    public void FreeAll(Process process)
    {
        var swapPath = GetProcessSwapDirPath(process);
        Directory.Delete(swapPath, true);
    }

    public void Free(Process process, VirtualAddress virtualAddress)
    {
        var swapPath = GetMemoryPageSwapFilePath(process, virtualAddress);
        File.Delete(swapPath);
    }

    private string GetMemoryPageSwapFilePath(MemoryPage memoryPage)
    {
        return Path.Combine(_swapPath, $"{memoryPage.ProcessId}/bfr-{memoryPage.VirtualAddress.Pointer}");
    }

    private string GetMemoryPageSwapFilePath(Process process, VirtualAddress virtualAddress)
    {
        return Path.Combine(GetProcessSwapDirPath(process), $"/bfr-{virtualAddress.Pointer}");
    }

    private string GetProcessSwapDirPath(Process process)
    {
        return Path.Combine(_swapPath, $"/{process.Id}");
    }
}