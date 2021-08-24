using Rinsen.IoT.OneWire.Abstractions;
using System;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    public class DS18B20 : IOneWireDevice
    {
        private OneWireMaster _oneWireMaster;

        public byte[] OneWireAddress { get; private set; }

        public string OneWireAddressString { get { return BitConverter.ToString(OneWireAddress); } }

        public void Initialize(OneWireMaster oneWireMaster, byte[] oneWireAddress)
        {
            _oneWireMaster = oneWireMaster;
            OneWireAddress = oneWireAddress;
        }

        public async Task<double?> GetTemperatureAsync()
        {
            byte[] scratchpad = await GetTemperatureScratchpadAsync();

            if (scratchpad == null)
            {
                return null;
            }

            return GetTemp_Read(scratchpad);
        }
        
        internal virtual double? GetTemp_Read(byte[] scratchpad)
        {
            if (scratchpad[Scratchpad.TemperatureLSB] == 0x50 && scratchpad[Scratchpad.TemperatureMSB] == 0x05)
            {
                return null;
            }

            var raw = unchecked((short)(scratchpad[Scratchpad.TemperatureLSB] | (scratchpad[Scratchpad.TemperatureMSB] << 8)));

            return raw / 16d;
        }

        protected async Task<byte[]> GetTemperatureScratchpadAsync()
        {
            ResetOneWireAndMatchDeviceRomAddress();
            await StartTemperatureConversionAsync();

            ResetOneWireAndMatchDeviceRomAddress();

            var scratchpad = ReadScratchpad();
            return scratchpad;
        }

        Task StartTemperatureConversionAsync()
        {
            _oneWireMaster.WriteByte(FunctionCommand.CONVERT_T);

            return Task.Delay(TimeSpan.FromSeconds(1));
        }

        byte[] ReadScratchpad()
        {
            _oneWireMaster.WriteByte(FunctionCommand.READ_SCRATCHPAD);

            var scratchpadData = new byte[9];

            var validBytesFound = false;

            for (int i = 0; i < scratchpadData.Length; i++)
            {
                scratchpadData[i] = _oneWireMaster.ReadByte();
                if (scratchpadData[i] != 255)
                {
                    validBytesFound = true;
                }
            }

            if (!validBytesFound)
            {
                return null;
            }

            return scratchpadData;
        }

        void ResetOneWireAndMatchDeviceRomAddress()
        {
            _oneWireMaster.Reset();

            _oneWireMaster.WriteByte(RomCommand.MATCH);

            foreach (var item in OneWireAddress)
            {
                _oneWireMaster.WriteByte(item);
            }
        }

        class Scratchpad
        {
            public const int TemperatureLSB = 0;

            public const int TemperatureMSB = 1;

            public const int ThRegisterOrUserByte1 = 2;

            public const int TlRegisterOrUserByte2 = 3;

            public const int ConfigurationRegister = 4;

            public const int Reserved = 5;

            public const int Reserved2 = 6;

            public const int Reserved3 = 7;

            public const int CRC = 8;

        }

        public class RomCommand
        {
            public const byte SEARCH = 0xF0;
            public const byte READ = 0x33;
            public const byte MATCH = 0x55;
            public const byte SKIP = 0xCC;
            public const byte ALARM_SEARCH = 0xEC;
        }

        public class FunctionCommand
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
