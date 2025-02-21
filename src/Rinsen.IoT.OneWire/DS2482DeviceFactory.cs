﻿using System;
using System.Device.I2c;

namespace Rinsen.IoT.OneWire;

public static class DS2482DeviceFactory
{
    public static DS2482_100 CreateDS2482_100(bool ad0, bool ad1)
    {
        byte address = 0x18;
        if (ad0)
        {
            address |= 1 << 0;
        }
        
        if (ad1)
        {
            address |= 1 << 1;
        }

        return CreateDS2482_100(1, address);
    }

    public static DS2482_100 CreateDS2482_100(int busId, int address) => PrivateCreateDs2482_100(GetI2cDevice(busId, address), disposeI2cDevice: true);

    public static DS2482_100 CreateDS2482_100(I2cDevice i2cDevice) => PrivateCreateDs2482_100(i2cDevice, disposeI2cDevice: false);

    private static DS2482_100 PrivateCreateDs2482_100(I2cDevice i2cDevice, bool disposeI2cDevice)
    {
        var ds2482_100 = new DS2482_100(i2cDevice, disposeI2cDevice);

        try
        {
            ds2482_100.OneWireReset();
        }
        catch (Exception e)
        {
            throw new DS2482100DeviceNotFoundException("No DS2482-100 detected, check that AD0 and AD1 is correct in ctor and that the physical connection to the DS2482-100 one wire bridge is correct.", e);
        }

        return ds2482_100;
    }

    public static DS2482_800 CreateDS2482_800(bool ad0, bool ad1, bool ad2)
    {
        byte address = 0x18;
        
        if (ad0)
        {
            address |= 1 << 0;
        }

        if (ad1)
        {
            address |= 1 << 1;
        }
        
        if (ad2)
        {
            address |= 1 << 2;
        }

        return CreateDS2482_800(1, address);
    }

    public static DS2482_800 CreateDS2482_800(int busId, int address) => PrivateCreateDS2482_800(GetI2cDevice(busId, address), disposeI2cDevice: true);

    public static DS2482_800 CreateDS2482_800(I2cDevice i2cDevice) => PrivateCreateDS2482_800(i2cDevice, false);

    private static DS2482_800 PrivateCreateDS2482_800(I2cDevice i2cDevice, bool disposeI2cDevice)
    {
        var ds2482_800 = new DS2482_800(i2cDevice, disposeI2cDevice);

        try
        {
            ds2482_800.OneWireReset();
        }
        catch (Exception e)
        {
            throw new DS2482800DeviceNotFoundException("No DS2482-800 detected, check that AD0, AD1 and AD2 is correct in ctor and that the physical connection to the DS2482-800 one wire bridge is correct.", e);
        }

        return ds2482_800;
    }

    private static I2cDevice GetI2cDevice(int busId, int address) => I2cDevice.Create(settings: new(busId, address));
}
