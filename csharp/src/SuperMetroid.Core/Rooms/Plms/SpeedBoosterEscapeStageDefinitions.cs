using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Compiled stage definitions for bank-$84's Speed Booster escape controller.</summary>
internal static class SpeedBoosterEscapeStageDefinitions
{
    /// <summary>Native byte width of each live target-X, maximum-FX-Y, velocity record.</summary>
    internal const int RecordByteCount = 6;

    /// <summary>Native byte offset of the terminal $8000 target-X sentinel.</summary>
    internal const int TerminatorOffset = 18;

    /// <summary>$84:B876: eastern checkpoint action, reached first while escaping left.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition EasternCheckpoint = new(1835, 447, SignedEightEight.FromSignedRaw(-176));

    /// <summary>$84:B87C: central checkpoint action, reached after the eastern rise.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition CentralCheckpoint = new(1290, 359, SignedEightEight.FromSignedRaw(-224));

    /// <summary>$84:B882: western checkpoint action, the final rise before completion.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition WesternCheckpoint = new(580, 256, SignedEightEight.FromSignedRaw(-224));

    /// <summary>
    /// The stage each record of $84:B876 selects, in table order; the last record ($84:B888, the terminal $8000
    /// target-X sentinel) completes the event and selects none.
    /// </summary>
    internal static readonly ImmutableArray<SpeedBoosterEscapeStageDefinition?> Stages =
        [EasternCheckpoint, CentralCheckpoint, WesternCheckpoint, null];
}
