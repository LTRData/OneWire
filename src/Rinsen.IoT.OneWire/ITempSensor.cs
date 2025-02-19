using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire;

public interface ITempSensor : ISensorId
{
    Task<double?> GetTemperatureAsync();
}
