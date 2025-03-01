﻿using System;

namespace Rinsen.IoT.OneWire;

public class DS18S20 : DS18X20Base
{
    protected override double GetTemp_Read(ReadOnlySpan<byte> scratchpad)
    {
        var raw = unchecked((short)(scratchpad[Scratchpad.TemperatureLSB] | (scratchpad[Scratchpad.TemperatureMSB] << 8)));

        var ftemp = raw / 2d;

        ftemp = ftemp - 0.25 + ((double)(scratchpad[Scratchpad.CountPerC] - scratchpad[Scratchpad.CountRemain]) / scratchpad[Scratchpad.CountPerC]);

        return ftemp;
    }

    static class Scratchpad
    {
        public const int TemperatureLSB = 0;

        public const int TemperatureMSB = 1;

        public const int ThRegisterOrUserByte1 = 2;

        public const int TlRegisterOrUserByte2 = 3;

        public const int Reserved = 4;

        public const int Reserved2 = 5;

        public const int CountRemain = 6;

        public const int CountPerC = 7;

        public const int CRC = 8;

    }
}
