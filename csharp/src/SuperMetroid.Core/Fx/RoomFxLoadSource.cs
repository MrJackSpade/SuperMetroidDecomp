using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>Where a room's layer-3 FX comes from: its FX header and entry door, RNG, room, and an optional supplied definition (#633).</summary>
internal sealed record RoomFxLoadSource
{
    /// <summary>The room's FX header, or null for a load without one.</summary>
    public required RoomFxDefinition? Fx { get; init; }

    /// <summary>The door the room is entered through, or null for a load that matches only default records.</summary>
    public required DoorDefinition? Door { get; init; }

    /// <summary>
    /// The shared random number when the room's FX loads; rain reads two of its bits to choose its drift. A copy of
    /// the generator's word (0 through 65535) that nothing does arithmetic on, so values outside it are rejected.
    /// </summary>
    public required int RandomNumber { get; init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>The room being entered, or null for a load outside a room.</summary>
    public required RoomHeaderDefinition? Room { get; init; }

    /// <summary>Supplied definition.</summary>
    public required RoomFxRecordDefinition? SuppliedDefinition { get; init; }
}
