using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Where a room's layer-3 FX comes from: its FX and door pointers, RNG, room header, and an optional supplied definition (#633).</summary>
internal sealed record RoomFxLoadSource
{
    /// <summary>Fx pointer.</summary>
    public required ushort FxPointer { get; init; }

    /// <summary>Door pointer.</summary>
    public required ushort DoorPointer { get; init; }

    /// <summary>Random number.</summary>
    public required ushort RandomNumber { get; init; }

    /// <summary>Room header pointer.</summary>
    public required ushort RoomHeaderPointer { get; init; }

    /// <summary>Supplied definition.</summary>
    public required RoomFxRecordDefinition? SuppliedDefinition { get; init; }
}
