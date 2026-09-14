# OneWire

1-Wire access through DS2482-100 and DS2482-800 I2C bridges, using `System.Device.Gpio`.

This is the LTRData fork of [Rinsen/OneWire](https://github.com/Rinsen/OneWire), originally developed by Fredrik Rinsén. The current library is packaged as [LTRData.Rinsen.IoT.OneWire](https://www.nuget.org/packages/LTRData.Rinsen.IoT.OneWire); its namespace remains `Rinsen.IoT.OneWire`.

## Package and hardware

```sh
dotnet add package LTRData.Rinsen.IoT.OneWire
```

The current project targets **.NET 8, 9 and 10**. Earlier versions and the retained Windows IoT Core samples used a different framework/API setup.

| Component | Support |
| --- | --- |
| DS2482-100 | One 1-Wire channel through an I2C bridge. |
| DS2482-800 | Eight 1-Wire channels through an I2C bridge. |
| DS18B20 | Temperature sensor, family code `0x28`. |
| DS18S20 | Temperature sensor, family code `0x10`, including extended-resolution calculation. |
| Other family codes | Discovered as `UndefinedOneWireDevice` unless a custom implementation is registered. |

The library requires a working `System.Device.I2c.I2cDevice` provider, an accessible I2C bus and the bridge/sensors wired appropriately. On a Linux-based Raspberry Pi, enable I2C and give the application access to the relevant `/dev/i2c-*` device. See the [.NET IoT I2C setup guide](https://github.com/dotnet/iot/blob/main/Documentation/raspi-i2c.md).

This accesses 1-Wire through the DS2482 bridge; it does not read Linux `w1` sysfs devices or drive a GPIO pin directly. Framework compatibility alone does not provide an I2C controller on a Windows PC or another host.

## Read temperatures

With a DS2482-100 on bus 1 at address `0x18` (AD0 and AD1 low):

```csharp
using System;
using Rinsen.IoT.OneWire;

using var bridge = DS2482DeviceFactory.CreateDS2482_100(
    busId: 1, address: 0x18);

foreach (var sensor in bridge.GetDevices<DS18B20>())
{
    var temperature = await sensor.GetTemperatureAsync();

    Console.WriteLine(temperature is double value
        ? $"{Convert.ToHexString(sensor.OneWireAddress)}: {value:F2} °C"
        : $"{Convert.ToHexString(sensor.OneWireAddress)}: no valid reading");
}
```

Bridge creation is synchronous. Both sensor classes use `GetTemperatureAsync()`, returning a nullable Celsius value. A reading includes a one-second conversion wait; invalid scratchpad data or CRC returns `null`, while I2C failures can throw.

Use `CreateDS2482_800(...)` for an eight-channel bridge and `GetDevices<DS18S20>()` for DS18S20 sensors. `GetDevices<T>()` searches all bridge channels on first use and caches the discovered devices. `GetAllDevices()` also includes unrecognized devices.

Reuse the bridge for repeated measurements, and await each operation before starting the next one on that bridge. The conversion delay does not make simultaneous bus operations safe.

## Addresses and ownership

The Boolean factory overloads select **I2C bus 1** and derive the address from the AD pins:

- `CreateDS2482_100(ad0, ad1)`: `0x18 + AD0 + 2*AD1`.
- `CreateDS2482_800(ad0, ad1, ad2)`: `0x18 + AD0 + 2*AD1 + 4*AD2`.

`true` means the pin is high. For another bus or an explicit address, use the `(int busId, int address)` overload. Multiple bridges can share a bus when configured with different addresses.

Factories that create their own `I2cDevice` transfer its lifetime to the returned bridge; disposing the bridge disposes that I2C device. The overload accepting an existing `I2cDevice` leaves its disposal to the caller.

Bridge initialization errors may be wrapped in `DS2482100DeviceNotFoundException` or `DS2482800DeviceNotFoundException`. Check the bus, address, permissions, wiring and inner exception; an error does not uniquely identify an addressing problem.

## Adding device types

Before the first discovery, register a family code with `DS2482.AddDeviceType<MyDevice>(familyCode)`.

The type must implement [IOneWireDevice](https://github.com/LTRData/OneWire/blob/master/src/Rinsen.IoT.OneWire/IOneWireDevice.cs) and have a public parameterless constructor. Discovery creates the instance and calls `Initialize(DS2482Channel, byte[])` with its channel and ROM address. Registrations are shared across bridge instances, and duplicate family-code registrations are rejected.

See [DS18B20](https://github.com/LTRData/OneWire/blob/master/src/Rinsen.IoT.OneWire/DS18B20.cs) and its [DS18X20Base implementation](https://github.com/LTRData/OneWire/blob/master/src/Rinsen.IoT.OneWire/DS1820Base.cs) for an example.

## Building and historical projects

Use the .NET 10 SDK to build the library directly:

```sh
dotnet build src/Rinsen.IoT.OneWire/Rinsen.IoT.OneWire.csproj -c Debug -f net10.0
```

Release builds also generate the NuGet package; `LocalNuGetPath` controls the package output directory.

The [sample directories](https://github.com/LTRData/OneWire/tree/master/sample) retain older UWP/Windows IoT Core headed and background applications. They use earlier factory/API patterns and are historical references rather than current quick-start projects. The [test project](https://github.com/LTRData/OneWire/tree/master/tests/Rinsen.IoT.OneWire.Tests) still targets `net7.0-windows10.0.17763.0` and has not been aligned with the current library targets and conversion API. Consequently, building the library project directly avoids the outdated sample/test dependencies in the solution.

## License and attribution

The project retains Fredrik Rinsén's [MIT license](https://github.com/LTRData/OneWire/blob/master/LICENSE). I2C access is provided by the [.NET IoT libraries](https://github.com/dotnet/iot). The bridge/channel source also credits Maxim's sample implementation.
