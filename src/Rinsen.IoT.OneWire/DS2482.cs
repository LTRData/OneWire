using System;
using System.Collections.Generic;
using System.Linq;
using System.Device.I2c;
using static Rinsen.IoT.OneWire.DS2482Channel;

namespace Rinsen.IoT.OneWire;

public abstract class DS2482(I2cDevice i2cDevice, bool disposeI2cDevice) : IDisposable
{
    private static readonly Dictionary<byte, Type> _oneWireDeviceTypes = [];

    protected readonly List<DS2482Channel> Channels = [];
    private readonly List<IOneWireDevice> _oneWireDevices = [];
    private bool _initialized = false;

    public I2cDevice I2cDevice { get; } = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

    static DS2482()
    {
        AddDeviceType<DS18S20>(0x10);
        AddDeviceType<DS18B20>(0x28);
    }

    public abstract bool IsCorrectChannelSelected(OneWireChannel channel);

    public abstract void SetSelectedChannel(OneWireChannel channel);

    public static void AddDeviceType<T>(byte familyCode) where T : IOneWireDevice => _oneWireDeviceTypes.Add(familyCode, typeof(T));

    public IReadOnlyCollection<IOneWireDevice> GetAllDevices()
    {
        InitializeDevices();

        return _oneWireDevices;
    }

    private void InitializeDevices()
    {
        if (!_initialized)
        {
            foreach (var channel in Channels)
            {
                _oneWireDevices.AddRange(channel.GetConnectedOneWireDevices(_oneWireDeviceTypes));
            }

            _initialized = true;
        }
    }

    public IEnumerable<T> GetDevices<T>()
    {
        InitializeDevices();

        return _oneWireDevices.OfType<T>();
    }

    public bool OneWireReset() => Channels[0].OneWireReset();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (I2cDevice != null && disposeI2cDevice)
            {
                I2cDevice.Dispose();
            }
        }
    }
}
