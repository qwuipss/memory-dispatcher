namespace MemoryDispatcher.Memory;

public class MemoryMap
{
    private readonly Dictionary<int, byte[]> _map = new();

    public byte[] this[int key]
    {
        get => _map[key];
        set => _map[key] = value;
    }

    public bool TryGetBuffer(int virtualAddressPointer, out byte[]? buffer)
    {
        return _map.TryGetValue(virtualAddressPointer, out buffer);
    }

    public void Remove(int virtualAddressPointer)
    {
        _map.Remove(virtualAddressPointer, out _);
    }
}