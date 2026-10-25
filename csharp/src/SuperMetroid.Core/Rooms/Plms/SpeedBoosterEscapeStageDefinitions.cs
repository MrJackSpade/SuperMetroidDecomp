using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Compiled stage definitions for bank-$84's Speed Booster escape controller.</summary>
internal static class SpeedBoosterEscapeStageDefinitions
{

    /// <summary>Native byte width of each live target-X, maximum-FX-Y, velocity record.</summary>
    internal const ushort RecordByteCount = 6;

    /// <summary>Native byte offset of the terminal $8000 target-X sentinel.</summary>
    internal const ushort TerminatorOffset = 18;

    /// <summary>$84:B876: eastern checkpoint action, reached first while escaping left.</summary>
    private static readonly SpeedBoosterEscapeStageDefinition EasternCheckpoint = new(0x072b, 0x01bf, SignedEightEight.FromSignedRaw(-176));

    /// <summary>$84:B87C: central checkpoint action, reached after the eastern rise.</summary>
    private static readonly SpeedBoosterEscapeStageDefinition CentralCheckpoint = new(0x050a, 0x0167, SignedEightEight.FromSignedRaw(-224));

    /// <summary>$84:B882: western checkpoint action, the final rise before completion.</summary>
    private static readonly SpeedBoosterEscapeStageDefinition WesternCheckpoint = new(0x0244, 0x0100, SignedEightEight.FromSignedRaw(-224));

    /// <summary>
    /// The stage each record of $84:B876 selects, in table order; the last record ($84:B888, the terminal $8000
    /// target-X sentinel) completes the event and selects none.
    /// </summary>
    private static readonly ImmutableArray<SpeedBoosterEscapeStageDefinition?> Stages =
        [EasternCheckpoint, CentralCheckpoint, WesternCheckpoint, null];

    /// <summary>
    /// Selects the three position-triggered actions at $84:B846, in east-to-west order.
    /// Each case supplies an unsigned X threshold, unsigned maximum lava Y and packed
    /// signed 8.8 rise velocity for the supported NTSC revision. These are checkpoint
    /// actions, not samples to interpolate: the caller executes at most one per frame
    /// and advances its timer by six. Offset eighteen completes the event; every other
    /// ushort offset is invalid. No stage data or generated cache is stored.
    /// </summary>
    internal static SpeedBoosterEscapeStageDefinition? Resolve(int tableByteOffset)
    {
        if ((uint)tableByteOffset > TerminatorOffset || tableByteOffset % RecordByteCount != 0)
        {
            throw new InvalidDataException(
                $"Speed Booster escape PLM timer ${tableByteOffset:X4} is not a valid " +
                "offset into table $84:B876.");
        }

        return Stages[tableByteOffset / RecordByteCount];
    }
}
