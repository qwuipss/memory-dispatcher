namespace MemoryDispatcher.Memory;

public class MemoryPage(int pointer, int processId)
{
    public int ProcessId { get; set; } = processId;

    public VirtualAddress VirtualAddress { get; } = new(pointer);
}