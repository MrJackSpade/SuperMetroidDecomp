using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>One physical lava stage used by the Speed Booster escape controller.</summary>
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

    /// <summary>The four valid record offsets into $84:B876: three checkpoint actions and the terminator.</summary>
    private enum StageOffset : ushort
    {
        /// <summary>$84:B876: eastern checkpoint action, reached first while escaping left.</summary>
        EasternCheckpoint = 0,
        /// <summary>$84:B87C: central checkpoint action, reached after the eastern rise.</summary>
        CentralCheckpoint = 6,
        /// <summary>$84:B882: western checkpoint action, the final rise before completion.</summary>
        WesternCheckpoint = 12,
        /// <summary>$84:B888: the terminal $8000 target-X sentinel that completes the event.</summary>
        Terminator = TerminatorOffset,
    }

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

        return ClosedNativeWords.Decode<StageOffset>(tableByteOffset, "Speed Booster escape stage offset") switch
        {
            StageOffset.EasternCheckpoint => new(0x072b, 0x01bf, 0xff50),
            StageOffset.CentralCheckpoint => new(0x050a, 0x0167, 0xff20),
            StageOffset.WesternCheckpoint => new(0x0244, 0x0100, 0xff20),
            StageOffset.Terminator => null,
            _ => throw new InvalidOperationException($"Undefined {nameof(StageOffset)} {tableByteOffset:X4}."),
        };
    }
}
