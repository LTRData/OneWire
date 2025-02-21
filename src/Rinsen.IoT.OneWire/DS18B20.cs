using System;

namespace Rinsen.IoT.OneWire;

public class DS18B20 : DS18X20Base
{
    protected override double GetTemp_Read(ReadOnlySpan<byte> scratchpad)
    {
        var raw = unchecked((short)(scratchpad[Scratchpad.TemperatureLSB] | (scratchpad[Scratchpad.TemperatureMSB] << 8)));

        return raw / 16d;
    }

    static class Scratchpad
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
}
