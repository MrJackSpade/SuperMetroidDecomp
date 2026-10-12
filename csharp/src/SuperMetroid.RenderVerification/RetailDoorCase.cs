using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Spatial;

/// <summary>One retail door and the destination room its transition reaches (#633).</summary>
internal readonly record struct RetailDoorCase
{
    /// <summary>Door.</summary>
    public required ushort Door { get; init; }

    /// <summary>Destination.</summary>
    public required ushort Destination { get; init; }
}
