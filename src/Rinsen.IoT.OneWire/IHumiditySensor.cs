using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire;

public interface IHumiditySensor : ISensorId
{
    Task<double?> GetHumidityAsync();
}
