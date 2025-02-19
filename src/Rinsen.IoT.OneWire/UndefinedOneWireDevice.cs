using System;

namespace Rinsen.IoT.OneWire;

public class UndefinedOneWireDevice(DS2482Channel ds2482Channel, byte[] oneWireAddress) : IOneWireDevice
{
    public DS2482Channel DS2482Channel { get; } = ds2482Channel;

    public byte[] OneWireAddress { get; } = oneWireAddress;

    public string OneWireAddressString { get { return BitConverter.ToString(OneWireAddress); } }

    public void Initialize(DS2482Channel ds2482, byte[] oneWireAddress) => throw new NotImplementedException();
}
