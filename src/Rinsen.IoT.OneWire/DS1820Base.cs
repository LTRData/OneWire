using System;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    public abstract class DS18X20Base : IOneWireDevice, ITempSensor, ISensorId
    {
        public DS2482Channel DS2482Channel { get; private set; }

        public byte[] OneWireAddress { get; private set; }

        byte[] ISensorId.SensorId => OneWireAddress;

        public void Initialize(DS2482Channel ds2482Channel, byte[] oneWireAddress)
        {
            DS2482Channel = ds2482Channel;
            OneWireAddress = oneWireAddress;
        }

        public async Task<double?> GetTemperatureAsync()
        {
            var scratchpad = await GetTemperatureScratchpadAsync();

            if (scratchpad == null)
            {
                return null;
            }

            return GetTemp_Read(scratchpad);
        }

        protected abstract double GetTemp_Read(byte[] scratchpad);

        protected async Task<byte[]> GetTemperatureScratchpadAsync()
        {
            ResetOneWireAndMatchDeviceRomAddress();

            DS2482Channel.EnableStrongPullup();

            await StartTemperatureConversionAsync();

            ResetOneWireAndMatchDeviceRomAddress();

            return ReadScratchpad();
        }

        Task StartTemperatureConversionAsync()
        {
            DS2482Channel.OneWireWriteByte(FunctionCommand.CONVERT_T);

            return Task.Delay(TimeSpan.FromSeconds(1));
        }

        byte[] ReadScratchpad()
        {
            DS2482Channel.OneWireWriteByte(FunctionCommand.READ_SCRATCHPAD);

            var scratchpadData = new byte[9];

            for (int i = 0; i < scratchpadData.Length; i++)
            {
                scratchpadData[i] = DS2482Channel.OneWireReadByte();
            }

            // Connection failed

            if (Array.TrueForAll(scratchpadData, ((byte)255).Equals))
            {
                return null;
            }

            return scratchpadData;
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
}
