namespace MemoryDispatcher.Processes;

public class Process
{
    private static int AutoIncrementIdCounter => _idCounter++;
    private static int _idCounter;

    public int Id { get; }
    
    private Process(int id)
    {
        Id = id;
    }
    
    public static Process Create()
    {
        var process = new Process(AutoIncrementIdCounter);
        Logger.Log($"Process with id {process.Id} created");
        return process;
    }

    public void Run()
    {
        Os.RequireMemory(this);
    }
}