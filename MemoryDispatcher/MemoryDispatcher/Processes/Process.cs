using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Processes;

public class Process(int id)
{
    public static int KillChance;
    
    private static readonly byte[] Choices = Enumerable
        .Repeat(byte.MinValue, byte.MaxValue)
        .Select(_ => (byte)Random.Shared.Next(byte.MinValue, byte.MaxValue))
        .ToArray();

    private readonly List<AddressInfo> _addressesInfo = [];
    private readonly Logger _logger = Logger.ForContext<Process>();
    private readonly Random _random = new();

    public int Id { get; } = id;

    public void Run()
    {
        Task.Run(() =>
        {
            while (true)
            {
                var data = _random.GetItems(Choices, _random.Next(16, 64));

                _logger.Log($"[Process:{Id}] generated data [DataLength:{data.Length}]");

                var addressInfo = _addressesInfo.SingleOrDefault(addressInfo => addressInfo.Offset < addressInfo.Size);

                if (addressInfo == default)
                {
                    _logger.Log($"Address with free space was not found. RequireMemory for [Process:{Id}]", Logger.OsCallColor);

                    var allocatedAddress = Os.RequireMemory(this);
                    addressInfo = new AddressInfo(allocatedAddress.Address, allocatedAddress.Size, 0);

                    _logger.Log(
                        $"RequireWrite for [Process:{Id}] to [Address:{addressInfo.Address.Pointer}], [AddressOffset:{addressInfo.Offset}], " +
                        $"[DataWriteLength:{data.Length}], [DataOffset:{0}]", Logger.OsCallColor);
                    Os.RequireWrite(this, addressInfo.Address, addressInfo.Offset, data, 0, data.Length);

                    addressInfo.Offset += data.Length;

                    _addressesInfo.Add(addressInfo);

                    Thread.Sleep(1000 / Os.SimulationSpeedScale);
                    continue;
                }

                var addressFreeSpace = addressInfo.Size - addressInfo.Offset;

                if (addressFreeSpace >= data.Length)
                {
                    _logger.Log(
                        $"RequireWrite for [Process:{Id}] to [Address:{addressInfo.Address.Pointer}], [AddressOffset:{addressInfo.Offset}], " +
                        $"[DataWriteLength:{data.Length}], [DataOffset:{0}]", Logger.OsCallColor);

                    Os.RequireWrite(this, addressInfo.Address, addressInfo.Offset, data, 0, data.Length);
                    addressInfo.Offset += data.Length;
                }
                else
                {
                    _logger.Log(
                        $"RequireWrite for [Process:{Id}] to [Address:{addressInfo.Address.Pointer}], [AddressOffset:{addressInfo.Offset}], " +
                        $"[DataWriteLength:{addressFreeSpace}], [DataOffset:{0}]", Logger.OsCallColor);

                    Os.RequireWrite(this, addressInfo.Address, addressInfo.Offset, data, 0, addressFreeSpace);
                    addressInfo.Offset += addressFreeSpace;

                    _logger.Log($"[Address:{addressInfo.Address.Pointer}] insufficient space. RequireMemory for [Process:{Id}]", Logger.OsCallColor);

                    var allocatedAddress = Os.RequireMemory(this);
                    addressInfo = new AddressInfo(allocatedAddress.Address, allocatedAddress.Size, 0);

                    var remainDataLength = data.Length - addressFreeSpace;

                    _logger.Log(
                        $"RequireWrite for [Process:{Id}] to [Address:{addressInfo.Address.Pointer}], [AddressOffset:{addressInfo.Offset}], " +
                        $"[DataWriteLength:{remainDataLength}], [DataOffset:{addressFreeSpace}]", Logger.OsCallColor);

                    Os.RequireWrite(this, addressInfo.Address, addressInfo.Offset, data, addressFreeSpace, remainDataLength);

                    addressInfo.Offset += remainDataLength;

                    _addressesInfo.Add(addressInfo);
                }

                if (_random.Next(0, 100) < KillChance)
                {
                    Os.Kill(this);
                    return;
                }

                Thread.Sleep(1000 / Os.SimulationSpeedScale);
            }
        });
    }

    private class AddressInfo(VirtualAddress address, int size, int offset)
    {
        public VirtualAddress Address { get; } = address;
        public int Size { get; } = size;
        public int Offset { get; set; } = offset;
    }
}