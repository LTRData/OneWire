using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire;

public abstract class DS18X20Base : IOneWireDevice, ITempSensor, ISensorId
{
    public DS2482Channel DS2482Channel { get; private set; } = null!;

    public byte[] OneWireAddress { get; private set; } = null!;

    byte[] ISensorId.SensorId => OneWireAddress;

    public void Initialize(DS2482Channel ds2482Channel, byte[] oneWireAddress)
    {
        DS2482Channel = ds2482Channel;
        OneWireAddress = oneWireAddress;
    }

    public async Task<double?> GetTemperatureAsync()
    {
        var scratchpad = ArrayPool<byte>.Shared.Rent(9);

        try
        {

            var result = await GetTemperatureScratchpadAsync(scratchpad.AsMemory(0, 9)).ConfigureAwait(false);

            if (!result)
            {
                return null;
            }

            return GetTemp_Read(scratchpad.AsSpan(0, 9));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(scratchpad);
        }
    }

    protected abstract double GetTemp_Read(ReadOnlySpan<byte> scratchpad);

    protected async Task<bool> GetTemperatureScratchpadAsync(Memory<byte> scratchpad)
    {
        ResetOneWireAndMatchDeviceRomAddress();

        DS2482Channel.EnableStrongPullup();

        await StartTemperatureConversionAsync().ConfigureAwait(false);

        ResetOneWireAndMatchDeviceRomAddress();

        return ReadScratchpad(scratchpad.Span);
    }

    Task StartTemperatureConversionAsync()
    {
        DS2482Channel.OneWireWriteByte(FunctionCommand.CONVERT_T);

        return Task.Delay(TimeSpan.FromSeconds(1));
    }

    bool ReadScratchpad(Span<byte> scratchpadData)
    {
        if (scratchpadData.Length != 9)
        {
            throw new ArgumentException("Buffer needs to be 9 bytes", nameof(scratchpadData));
        }

        DS2482Channel.OneWireWriteByte(FunctionCommand.READ_SCRATCHPAD);

        for (int i = 0; i < scratchpadData.Length; i++)
        {
            scratchpadData[i] = DS2482Channel.OneWireReadByte();
        }

        // Connection failed
#if NET8_0_OR_GREATER
        return scratchpadData.ContainsAnyExcept((byte)255);
#else
        foreach (var b in scratchpadData)
        {
            if (b != 255)
            {
                return true;
            }
        }

        return false;
#endif
    }

    void ResetOneWireAndMatchDeviceRomAddress()
    {
        DS2482Channel.OneWireReset();

        DS2482Channel.OneWireWriteByte(RomCommand.MATCH);

        foreach (var item in OneWireAddress)
        {
            DS2482Channel.OneWireWriteByte(item);
        }
    }

    protected class RomCommand
    {
        public const byte SEARCH = 0xF0;
        public const byte READ = 0x33;
        public const byte MATCH = 0x55;
        public const byte SKIP = 0xCC;
        public const byte ALARM_SEARCH = 0xEC;
    }

    protected class FunctionCommand
    {
        public const byte CONVERT_T = 0x44;
        public const byte WRITE_SCRATCHPAD = 0x4E;
        public const byte READ_SCRATCHPAD = 0xBE;
        public const byte COPY_SCRATCHPAD = 0x48;
        public const byte RECALL_E = 0xB8;
        public const byte READ_POWER_SUPPLY = 0xB4;
    }
}
