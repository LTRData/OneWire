﻿using System.Device.I2c;
using static Rinsen.IoT.OneWire.DS2482Channel;

namespace Rinsen.IoT.OneWire;

//
// This implementation is based on Maxim sample implementation and has parts from ported C code
// source: http://www.maximintegrated.com/en/app-notes/index.mvp/id/187
//

public class DS2482_100 : DS2482
{
    public DS2482_100(I2cDevice i2cDevice, bool disposeI2cDevice)
        : base(i2cDevice, disposeI2cDevice)
        
    {
        Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO0));
    }

    public override bool IsCorrectChannelSelected(OneWireChannel channel) => true;

    public override void SetSelectedChannel(OneWireChannel channel)
    {        
    }
}
