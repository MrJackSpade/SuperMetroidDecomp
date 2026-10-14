/// <summary>The room a ROM-free comparison loads, and its name in diagnostics (#633).</summary>
internal readonly record struct RomFreeRoom
{
    /// <summary>Room pointer.</summary>
    public required ushort RoomPointer { get; init; }

    /// <summary>Room name.</summary>
    public required string RoomName { get; init; }
}
