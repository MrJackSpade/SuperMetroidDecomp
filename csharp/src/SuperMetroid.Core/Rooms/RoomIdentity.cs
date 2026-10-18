namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Logical room identity formed by the cartridge's mutually exclusive area and per-area
/// room bytes. This deliberately does not contain a bank-$8F room-header pointer: pointers
/// identify storage, while this value identifies the room as understood by game logic.
/// </summary>
public readonly record struct RoomIdentity
{
    /// <summary>Creates a logical gameplay room key from a validated retail area and that area's native room byte.</summary>
    /// <param name="area">One of the seven retail world-area identities.</param>
    /// <param name="roomIndex">Native per-area room index, 0 through 255; values are intentionally not globally unique.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is outside the retail domain.</exception>
    public RoomIdentity(AreaId area, int roomIndex)
    {
        // Enum casts can manufacture arbitrary bytes. Validate at construction so a room
        // identity can safely cross table and dispatcher boundaries without another guard.
        _ = AreaIds.ToIndex(area);
        Area = area;
        RoomIndex = Ensure.BetweenInclusive(0, byte.MaxValue, roomIndex);
    }

    /// <summary>The room's validated retail world area.</summary>
    public AreaId Area { get; }

    /// <summary>The native room index, whose meaning is local to <see cref="Area"/>.</summary>
    public int RoomIndex { get; }

    /// <summary>Formats the native logical pair used in diagnostics and debugger watches.</summary>
    public override string ToString() => $"${(byte)Area:X2}/${RoomIndex:X2}";
}
