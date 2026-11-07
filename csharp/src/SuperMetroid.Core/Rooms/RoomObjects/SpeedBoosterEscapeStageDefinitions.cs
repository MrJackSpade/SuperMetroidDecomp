using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Compiled stage definitions for 's Speed Booster escape controller.</summary>
internal static class SpeedBoosterEscapeStageDefinitions
{
    /// <summary>Byte width of each live target-X, maximum-FX-Y, velocity record.</summary>
    internal const int RecordByteCount = 6;

    /// <summary>Byte offset of the terminal disabled target-X sentinel.</summary>
    internal const int TerminatorOffset = 18;

    /// <summary>Eastern checkpoint action, reached first while escaping left.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition EasternCheckpoint = new(1835, 447, SignedEightEight.FromSignedRaw(-176));

    /// <summary>Central checkpoint action, reached after the eastern rise.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition CentralCheckpoint = new(1290, 359, SignedEightEight.FromSignedRaw(-224));

    /// <summary>Western checkpoint action, the final rise before completion.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition WesternCheckpoint = new(580, 256, SignedEightEight.FromSignedRaw(-224));

    /// <summary>
    /// The stage each record of the escape stage table selects, in table order; the last record (the terminal disabled
    /// target-X sentinel) completes the event and selects none.
    /// </summary>
    internal static readonly ImmutableArray<SpeedBoosterEscapeStageDefinition?> Stages =
        [EasternCheckpoint, CentralCheckpoint, WesternCheckpoint, null];
}
