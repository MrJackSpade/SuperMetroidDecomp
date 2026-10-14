using System.ComponentModel;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Spatial;

/// <summary>Retail room headers shared by slow-consumer audio and pause-capture checks.</summary>
internal static class SlowConsumerFixture
{
    /// <summary>Retail $8F:A3AE room header: room-local gameplay/pause fixture, matching portable capture tests.</summary>
    internal const ushort AlphaPowerBombRoom = 0xa3ae;
    /// <summary>Retail $8F:CEFB tube room: bounded Maridia liquid/pause coverage.</summary>
    internal const ushort MaridiaTubeRoom = 0xcefb;
}
