using System.Collections.Concurrent;

namespace MemoryDispatcher.Memory;

public class MemoryMap
{
    private readonly Dictionary<int, byte[]> _map = new();

    public byte[] this[int key]
    {
        get => _map[key];
        set => _map[key] = value;
    }

    public void Remove(int key)
    {
        _map.Remove(key, out _);
    }   
}