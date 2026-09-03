namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge table semantics for <c>$8F:E68A Door_Closing_PLMs</c>.
/// </summary>
/// <remarks>
/// The low two bits retain physical travel direction. Values zero through three are
/// non-closing doors, four through seven request a blue cap, and eight through eleven
/// request the Mother Brain escape gate. Keeping this mapping outside the runtime logic
/// makes the four duplicated native table entries explicit and prevents range arithmetic
/// from accidentally treating an invalid direction byte as a valid header pointer.
/// </remarks>
public static class DoorClosingPlmRomData
{
    /// <summary>Number of entries in the retail door-closing header table.</summary>
    public const int DirectionCount = 12;

    private static readonly ushort[] HeaderByDirection =
    [
        0,
        0,
        0,
        0,
        RoomPlmHeaders.BlueDoorClosingFacingRight,
        RoomPlmHeaders.BlueDoorClosingFacingLeft,
        RoomPlmHeaders.BlueDoorClosingFacingDown,
        RoomPlmHeaders.BlueDoorClosingFacingUp,
        RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
        RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
        RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
        RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
    ];

    /// <summary>Returns the exact bank-$84 header selected by a retail door direction.</summary>
    public static ushort GetHeader(byte direction)
    {
        if (direction >= HeaderByDirection.Length)
        {
            throw new InvalidDataException(
                $"Door direction ${direction:X2} indexes beyond the " +
                $"{HeaderByDirection.Length}-entry retail closing-PLM table.");
        }

        return HeaderByDirection[direction];
    }
}
