using System;

namespace Rinsen.IoT.OneWire;

public static class ExtensionMethods
{
    public static bool GetBit(this byte b, int bitNumber) => (b & (1 << bitNumber)) != 0;

    public static string FormatIdString(this ISensorId device) => BitConverter.ToString(device.SensorId);
}
