using System;
using System.Collections.Generic;
using System.Linq;
using System.Device.I2c;
using static Rinsen.IoT.OneWire.DS2482Channel;

namespace Rinsen.IoT.OneWire
{
    public abstract class DS2482 : IDisposable
    {
        private static readonly Dictionary<byte, Type> _oneWireDeviceTypes = new Dictionary<byte, Type>();

        protected readonly IList<DS2482Channel> Channels = new List<DS2482Channel>();
        private bool _disposeI2cDevice;
        private readonly List<IOneWireDevice> _oneWireDevices = new List<IOneWireDevice>();
        private bool _initialized = false;

        public I2cDevice I2cDevice { get; }

        static DS2482()
        {
            AddDeviceType<DS18S20>(0x10);
            AddDeviceType<DS18B20>(0x28);
        }

        public DS2482(I2cDevice i2cDevice, bool disposeI2cDevice)
        {
            I2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(I2cDevice));
            _disposeI2cDevice = disposeI2cDevice;
        }

        public abstract bool IsCorrectChannelSelected(OneWireChannel channel);

        public abstract void SetSelectedChannel(OneWireChannel channel);

        public static void AddDeviceType<T>(byte familyCode) where T : IOneWireDevice
        {
            _oneWireDeviceTypes.Add(familyCode, typeof(T));
        }

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

        public bool OneWireReset()
        {
            return Channels[0].OneWireReset();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (I2cDevice != null && !_disposeI2cDevice)
                    I2cDevice.Dispose();
            }
        }
    }
}
