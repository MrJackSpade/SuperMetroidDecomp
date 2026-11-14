using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>Where a room's effect overlay FX comes from: its effect selection and entry door, RNG, room, and an optional supplied definition (#633).</summary>
internal sealed record RoomFxLoadSource
{
    /// <summary>The room's effect selection, or null for a load without one.</summary>
    public required RoomFxSelection? Fx { get; init; }

    /// <summary>The door the room is entered through, or null for a load that matches only default effects.</summary>
    public required DoorDefinition? Door { get; init; }

    /// <summary>
    /// The shared random number when the room's FX loads; rain reads two of its bits to choose its drift. A copy of
    /// the shared random sequence's value (0 through <see cref="SharedRandomState.LargestValue"/>, the values that
    /// sequence wraps through) that nothing does arithmetic on, so a value it never holds is rejected (#1627).
    /// </summary>
    public required int RandomNumber { get; init => field = Ensure.BetweenInclusive(0, SharedRandomState.LargestValue, value); }

    /// <summary>The room being entered, or null for a load outside a room.</summary>
    public required RoomDefinition? Room { get; init; }

    /// <summary>Supplied definition.</summary>
    public required RoomEffectDefinition? SuppliedDefinition { get; init; }
}
