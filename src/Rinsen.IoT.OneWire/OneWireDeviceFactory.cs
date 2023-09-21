using System.Collections.Generic;

namespace Rinsen.IoT.OneWire
{
    public static class OneWireDeviceFactory<T>
    {
        public static IEnumerable<T> GetDevices(DS2482 ds2482) => ds2482.GetDevices<T>();
    }
}
