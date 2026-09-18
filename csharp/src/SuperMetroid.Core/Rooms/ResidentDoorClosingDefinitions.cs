namespace SuperMetroid.Core.Rooms;

/// <summary>One resident door header and the secondary list used while entering its room.</summary>
internal readonly record struct ResidentDoorClosingDefinition(
    ushort Header,
    ushort ClosingInstructionList);

/// <summary>
/// Fixed secondary-list metadata read by <c>$82:E8EB Spawn_Door_Closing_PLM</c> from
/// each resident grey or coloured bank-$84 door header.
/// </summary>
/// <remarks>
/// These are dispatcher identities, not the animation programs themselves. The selected
/// lists remain cartridge-backed because their timer, sound, draw, and branch operations
/// are interpreted in sequence. Compiling only the header-to-list relationship removes a
/// fixed executable-header read without pretending that the mixed programs are immutable
/// engine lookup records.
/// </remarks>
public static class ResidentDoorClosingDefinitions
{
    /// <summary>Number of retail resident grey and coloured door headers.</summary>
    public const int Count = 17;

    /// <summary>$84:BA4C, Bomb Torizo's Bomb-gated right-facing closing program.</summary>
    internal const ushort BombTorizoGreyDoor = 0xba4c;

    /// <summary>$84:BE59, ordinary grey door facing left closing program.</summary>
    internal const ushort GreyFacingLeft = 0xbe59;

    /// <summary>$84:BEC2, ordinary grey door facing right closing program.</summary>
    internal const ushort GreyFacingRight = 0xbec2;

    /// <summary>$84:BF2B, ordinary grey door facing up closing program.</summary>
    internal const ushort GreyFacingUp = 0xbf2b;

    /// <summary>$84:BF94, ordinary grey door facing down closing program.</summary>
    internal const ushort GreyFacingDown = 0xbf94;

    /// <summary>$84:BFFD, yellow door facing left closing program.</summary>
    internal const ushort YellowFacingLeft = 0xbffd;

    /// <summary>$84:C060, yellow door facing right closing program.</summary>
    internal const ushort YellowFacingRight = 0xc060;

    /// <summary>$84:C0C3, yellow door facing up closing program.</summary>
    internal const ushort YellowFacingUp = 0xc0c3;

    /// <summary>$84:C122, yellow door facing down closing program.</summary>
    internal const ushort YellowFacingDown = 0xc122;

    /// <summary>$84:C185, green door facing left closing program.</summary>
    internal const ushort GreenFacingLeft = 0xc185;

    /// <summary>$84:C1E4, green door facing right closing program.</summary>
    internal const ushort GreenFacingRight = 0xc1e4;

    /// <summary>$84:C243, green door facing up closing program.</summary>
    internal const ushort GreenFacingUp = 0xc243;

    /// <summary>$84:C2A2, green door facing down closing program.</summary>
    internal const ushort GreenFacingDown = 0xc2a2;

    /// <summary>$84:C301, red door facing left closing program.</summary>
    internal const ushort RedFacingLeft = 0xc301;

    /// <summary>$84:C363, red door facing right closing program.</summary>
    internal const ushort RedFacingRight = 0xc363;

    /// <summary>$84:C3C5, red door facing up closing program.</summary>
    internal const ushort RedFacingUp = 0xc3c5;

    /// <summary>$84:C427, red door facing down closing program.</summary>
    internal const ushort RedFacingDown = 0xc427;

    private static readonly ResidentDoorClosingDefinition[] Definitions =
    [
        new(RoomPlmHeaders.BombTorizoGreyDoor, BombTorizoGreyDoor),
        new(RoomPlmHeaders.GreyDoorFacingLeft, GreyFacingLeft),
        new(RoomPlmHeaders.GreyDoorFacingRight, GreyFacingRight),
        new(RoomPlmHeaders.GreyDoorFacingUp, GreyFacingUp),
        new(RoomPlmHeaders.GreyDoorFacingDown, GreyFacingDown),
        new(RoomPlmHeaders.YellowDoorFacingLeft, YellowFacingLeft),
        new(RoomPlmHeaders.YellowDoorFacingRight, YellowFacingRight),
        new(RoomPlmHeaders.YellowDoorFacingUp, YellowFacingUp),
        new(RoomPlmHeaders.YellowDoorFacingDown, YellowFacingDown),
        new(RoomPlmHeaders.GreenDoorFacingLeft, GreenFacingLeft),
        new(RoomPlmHeaders.GreenDoorFacingRight, GreenFacingRight),
        new(RoomPlmHeaders.GreenDoorFacingUp, GreenFacingUp),
        new(RoomPlmHeaders.GreenDoorFacingDown, GreenFacingDown),
        new(RoomPlmHeaders.RedDoorFacingLeft, RedFacingLeft),
        new(RoomPlmHeaders.RedDoorFacingRight, RedFacingRight),
        new(RoomPlmHeaders.RedDoorFacingUp, RedFacingUp),
        new(RoomPlmHeaders.RedDoorFacingDown, RedFacingDown),
    ];

    /// <summary>Returns all retail resident-door closing definitions in header order.</summary>
    internal static ReadOnlySpan<ResidentDoorClosingDefinition> All => Definitions;

    /// <summary>Resolves a resident grey or coloured door header to its second list.</summary>
    internal static ushort Resolve(ushort header)
    {
        foreach (ResidentDoorClosingDefinition definition in Definitions)
        {
            if (definition.Header == header)
                return definition.ClosingInstructionList;
        }

        throw new InvalidDataException(
            $"Resident door header $84:{header:X4} has no compiled closing definition.");
    }
}
