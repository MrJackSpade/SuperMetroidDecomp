namespace SuperMetroid.Core.Fx;

/// <summary>Where a room's layer-3 FX comes from: its FX and door pointers, RNG, room header, and an optional supplied definition (#633).</summary>
internal sealed record RoomFxLoadSource
{
    /// <summary>Fx pointer.</summary>
    public required ushort FxPointer { get; init; }

    /// <summary>Door pointer.</summary>
    public required ushort DoorPointer { get; init; }

    /// <summary>
    /// The shared random number when the room's FX loads; rain reads two of its bits to choose its drift. A copy of
    /// the generator's word (0 through 65535) that nothing does arithmetic on, so values outside it are rejected.
    /// </summary>
    public required int RandomNumber { get; init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>Room header pointer.</summary>
    public required ushort RoomHeaderPointer { get; init; }

    /// <summary>Supplied definition.</summary>
    public required RoomFxRecordDefinition? SuppliedDefinition { get; init; }
}
