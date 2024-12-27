using System.Runtime.InteropServices;
using MemoryDispatcher.Memory;

namespace MemoryDispatcher.Processes;

public class Process(int id)
{
#pragma warning disable CA2211
    public static int KillChance;
#pragma warning restore CA2211

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
#pragma warning disable CA1806
        pthread_create(out _, IntPtr.Zero, Run, IntPtr.Zero);
#pragma warning restore CA1806
    }

    [DllImport("libc.so.6")]
    private static extern int pthread_create(out IntPtr thread, IntPtr attr, ThreadStartDelegate startRoutine, IntPtr arg);

    private IntPtr Run(IntPtr arg)
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

            if (_addressesInfo.Count > 1 && _random.Next(0, 100) < 20)
            {
                var addressForFree = _addressesInfo[_random.Next(_addressesInfo.Count)];
                _logger.Log($"Free [Address:{addressForFree.Address.Pointer}] of [Process:{Id}]", Logger.OsCallColor);
                Os.RequireFree(this, addressForFree.Address);
                _addressesInfo.Remove(addressForFree);
            }

            if (_random.Next(0, 100) < KillChance)
            {
                _logger.Log($"Killing [Process:{Id}]", Logger.OsCallColor);
                Os.Kill(this);
                return IntPtr.Zero;
            }

            Thread.Sleep(1000 / Os.SimulationSpeedScale);
        }
    }

    private delegate IntPtr ThreadStartDelegate(IntPtr arg);

    private class AddressInfo(VirtualAddress address, int size, int offset)
    {
        public VirtualAddress Address { get; } = address;
        public int Size { get; } = size;
        public int Offset { get; set; } = offset;
    }
}