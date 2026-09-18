namespace SuperMetroid.Core.Rooms;

/// <summary>One direction-selected fallback door-closing PLM definition.</summary>
internal readonly record struct DoorClosingPlmDefinition(
    ushort Header,
    ushort InitialInstructionList);

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
    /// <summary>$8F:E68A-$8F:E6A1, the twelve direction-selected fallback headers.</summary>
    internal const int HeaderTableAddress = 0x8fe68a;

    /// <summary>Number of entries in the retail door-closing header table.</summary>
    public const int DirectionCount = 12;

    /// <summary>$84:C4CF, closing program selected by header $84:C8BE.</summary>
    internal const ushort BlueFacingRightInstructionList = 0xc4cf;

    /// <summary>$84:C49E, closing program selected by header $84:C8BA.</summary>
    internal const ushort BlueFacingLeftInstructionList = 0xc49e;

    /// <summary>$84:C531, closing program selected by header $84:C8C6.</summary>
    internal const ushort BlueFacingDownInstructionList = 0xc531;

    /// <summary>$84:C500, closing program selected by header $84:C8C2.</summary>
    internal const ushort BlueFacingUpInstructionList = 0xc500;

    private static readonly DoorClosingPlmDefinition[] DefinitionsByDirection =
    [
        default,
        default,
        default,
        default,
        new(RoomPlmHeaders.BlueDoorClosingFacingRight, BlueFacingRightInstructionList),
        new(RoomPlmHeaders.BlueDoorClosingFacingLeft, BlueFacingLeftInstructionList),
        new(RoomPlmHeaders.BlueDoorClosingFacingDown, BlueFacingDownInstructionList),
        new(RoomPlmHeaders.BlueDoorClosingFacingUp, BlueFacingUpInstructionList),
        new(RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing),
        new(RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing),
        new(RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing),
        new(RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing),
    ];

    /// <summary>Returns the complete fallback definition selected by a door direction.</summary>
    internal static DoorClosingPlmDefinition GetDefinition(byte direction)
    {
        if (direction >= DefinitionsByDirection.Length)
        {
            throw new InvalidDataException(
                $"Door direction ${direction:X2} indexes beyond the " +
                $"{DefinitionsByDirection.Length}-entry retail closing-PLM table.");
        }

        return DefinitionsByDirection[direction];
    }

    /// <summary>Returns the exact bank-$84 header selected by a retail door direction.</summary>
    public static ushort GetHeader(byte direction) => GetDefinition(direction).Header;
}
