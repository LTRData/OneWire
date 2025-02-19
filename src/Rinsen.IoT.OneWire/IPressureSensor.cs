using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire;

public interface IPressureSensor : ISensorId
{
    Task<double?> GetPressureAsync();
}
