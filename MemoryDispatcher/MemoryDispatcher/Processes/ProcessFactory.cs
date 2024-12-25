namespace MemoryDispatcher.Processes;

public class ProcessFactory
{
    private static int _idCounter;

    private static readonly Logger Logger = Logger.ForContext<Process>();
    private static int AutoIncrementIdCounter => ++_idCounter;

    public static Process Create()
    {
        var process = new Process(AutoIncrementIdCounter);
        Logger.Log($"Process with id {process.Id} created", Logger.InitializationColor);
        return process;
    }
}