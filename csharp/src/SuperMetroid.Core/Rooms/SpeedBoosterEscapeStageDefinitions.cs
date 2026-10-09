namespace SuperMetroid.Core.Rooms;

/// <summary>One physical lava stage used by the Speed Booster escape controller.</summary>
/// <param name="TargetSamusX">Unsigned Samus X checkpoint threshold that triggers this escape action.</param>
/// <param name="MaximumFxY">Unsigned maximum lava-effect Y coordinate associated with this checkpoint.</param>
/// <param name="PackedYVelocity">The native packed signed 8.8 upward velocity applied at this checkpoint.</param>
internal readonly record struct SpeedBoosterEscapeStageDefinition(
    ushort TargetSamusX,
    ushort MaximumFxY,
    ushort PackedYVelocity);

/// <summary>Compiled stage definitions for bank-$84's Speed Booster escape controller.</summary>
internal static class SpeedBoosterEscapeStageDefinitions
{

    /// <summary>Native byte width of each live target-X, maximum-FX-Y, velocity record.</summary>
    internal const ushort RecordByteCount = 6;

    /// <summary>Native byte offset of the terminal $8000 target-X sentinel.</summary>
    internal const ushort TerminatorOffset = 18;

    /// <summary>$84:B876: eastern checkpoint action, reached first while escaping left.</summary>
    private const ushort EasternCheckpoint = 0;

    /// <summary>$84:B87C: central checkpoint action, reached after the eastern rise.</summary>
    private const ushort CentralCheckpoint = 6;

    /// <summary>$84:B882: western checkpoint action, the final rise before completion.</summary>
    private const ushort WesternCheckpoint = 12;

    /// <summary>
    /// Selects the three position-triggered actions at $84:B846, in east-to-west order.
    /// Each case supplies an unsigned X threshold, unsigned maximum lava Y and packed
    /// signed 8.8 rise velocity for the supported NTSC revision. These are checkpoint
    /// actions, not samples to interpolate: the caller executes at most one per frame
    /// and advances its timer by six. Offset eighteen completes the event; every other
    /// ushort offset is invalid. No stage data or generated cache is stored.
    /// </summary>
    internal static SpeedBoosterEscapeStageDefinition? Resolve(ushort tableByteOffset)
    {
        if (tableByteOffset > TerminatorOffset || tableByteOffset % RecordByteCount != 0)
        {
            throw new InvalidDataException(
                $"Speed Booster escape PLM timer ${tableByteOffset:X4} is not a valid " +
                "offset into table $84:B876.");
        }

        return tableByteOffset switch
        {
            EasternCheckpoint => new(0x072b, 0x01bf, 0xff50),
            CentralCheckpoint => new(0x050a, 0x0167, 0xff20),
            WesternCheckpoint => new(0x0244, 0x0100, 0xff20),
            _ => null,
        };
    }
}
