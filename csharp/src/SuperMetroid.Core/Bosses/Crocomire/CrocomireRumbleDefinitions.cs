using System.Collections.Immutable;
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Fixed hidden-wall rumble schedule from <c>$A4:98CA-$A4:9909</c>.</summary>
internal static class CrocomireRumbleDefinitions
{
    /// <summary>
    /// $A4:9902-9909: the words ending the hidden-wall waveform, also the last oscillation's timing words: the same
    /// both-bytes-128 word the terminator hands to the melt shake counter.
    /// </summary>
    private const ushort EndMarker = CrocomireBehavior.MeltShakeCounterAfterRumble;
    /// <summary>$A4:9867 installs this completed rumble cursor at the terminator.</summary>
    private const ushort CompletedCursor = 0x0080;

    /// <summary>$A4:98D0-9900: seven negative targets in the native waveform.</summary>
    internal const int PhaseCount = 7;

    /// <summary>The table offset of the first oscillation, after the two copied prefix targets.</summary>
    internal const int FirstPhaseOffset = 4;

    /// <summary>The bytes each oscillation occupies: a positive return target, then a negative target and its timing.</summary>
    internal const int PhaseBytes = 8;

    /// <summary>
    /// $A4:98FA: selected final repeat counter3 in the shake/sound performance.
    /// This counts completed oscillations, not elapsed frames. The consumer adds
    /// one final traversal before loading the next phase.
    /// </summary>
    private const ushort FinalDecayHold = 3;

    /// <summary>$A4:98CA: exact copied prefix word4, accepted as a target by the
    /// preserved restored-cursor domain. Native entry starts at byte offset4;
    /// this is compatibility content, not a claimed normal-entry amplitude.</summary>
    private const short CopiedPrefixTarget0 = 4;
    /// <summary>$A4:98CC: second exact prefix word1. Its original normal-entry
    /// purpose is not established; preserving it does not invent a physical rule.</summary>
    private const short CopiedPrefixTarget1 = 1;

    /// <summary>
    /// $A4:98CA-9909 stores one-word nonnegative targets and three-word negative
    /// targets. After the initial zero, seven oscillations each occupy eight bytes:
    /// one positive return target followed by the negative target and next timing.
    /// </summary>
    /// <remarks>
    /// Negative extrema double after each two-cycle buildup, reaching amplitude 4 at phase 4, then halve each phase
    /// through phase 6. Cooldown rises by four from eight, saturates at sixteen for two cycles, then halves; the final
    /// selected repeat count belongs to the authored shake cadence. The six live approach deltas rise from one to two
    /// for the middle pair, then return to one; the seventh phase carries the terminal marker.
    /// </remarks>
    internal static ImmutableArray<CrocomireRumbleDefinition> All { get; } =
    [
        new(CopiedPrefixTarget0, 2, 0, 0, false),
        new(CopiedPrefixTarget1, 4, 0, 0, false),
        new(0, 6, 0, 0, false),
        new(-1, 12, 8, 1, false),
        new(1, 14, 0, 0, false),
        new(-1, 20, 12, 1, false),
        new(1, 22, 0, 0, false),
        new(-2, 28, 16, 2, false),
        new(2, 30, 0, 0, false),
        new(-2, 36, 16, 2, false),
        new(2, 38, 0, 0, false),
        new(-4, 44, 8, 1, false),
        new(1, 46, 0, 0, false),
        new(-2, 52, FinalDecayHold, 1, false),
        new(1, 54, 0, 0, false),
        new(-1, 60, EndMarker, EndMarker, false),
        new(unchecked((short)EndMarker), CompletedCursor, 0, 0, true),
        new(unchecked((short)EndMarker), CompletedCursor, 0, 0, true),
    ];
}
