using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;
using System.Reflection;
using SuperMetroid.Core.Runtime;

/// <summary>The room a ROM-free comparison loads, and its name in diagnostics (#633).</summary>
internal readonly record struct RomFreeRoom
{
    /// <summary>Room pointer.</summary>
    public required ushort RoomPointer { get; init; }

    /// <summary>Room name.</summary>
    public required string RoomName { get; init; }
}
