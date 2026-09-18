namespace SuperMetroid.Core.Rooms;

/// <summary>One physical lava stage used by the Speed Booster escape controller.</summary>
internal readonly record struct SpeedBoosterEscapeStageDefinition(
    ushort TargetSamusX,
    ushort MaximumFxY,
    ushort PackedYVelocity);

/// <summary>Compiled stage definitions for bank-$84's Speed Booster escape controller.</summary>
internal static class SpeedBoosterEscapeStageDefinitions
{
    /// <summary>
    /// Native table $84:B876-$B889: three six-byte physical records followed by $8000.
    /// </summary>
    internal const int TableAddress = 0x84b876;

    /// <summary>Native byte width of each live target-X, maximum-FX-Y, velocity record.</summary>
    internal const ushort RecordByteCount = 6;

    /// <summary>Native byte offset of the terminal $8000 target-X sentinel.</summary>
    internal const ushort TerminatorOffset = 18;

    /// <summary>Native terminal target-X word that marks event $15.</summary>
    internal const ushort Terminator = 0x8000;

    private static readonly SpeedBoosterEscapeStageDefinition[] Entries =
    [
        new(0x072b, 0x01bf, 0xff50),
        new(0x050a, 0x0167, 0xff20),
        new(0x0244, 0x0100, 0xff20),
    ];

    /// <summary>
    /// Resolves a native PLM timer offset. A null result is the authored terminal row.
    /// </summary>
    internal static SpeedBoosterEscapeStageDefinition? Resolve(ushort tableByteOffset)
    {
        if (tableByteOffset > TerminatorOffset || tableByteOffset % RecordByteCount != 0)
        {
            throw new InvalidDataException(
                $"Speed Booster escape PLM timer ${tableByteOffset:X4} is not a valid " +
                "offset into table $84:B876.");
        }

        return tableByteOffset == TerminatorOffset
            ? null
            : Entries[tableByteOffset / RecordByteCount];
    }
}
