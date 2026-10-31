using System.Collections.Immutable;
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Fixed hidden-wall rumble schedule from <c>$A4:98CE-$A4:9905</c>.</summary>
internal static class CrocomireRumbleDefinitions
{
    /// <summary>
    /// The last oscillation's timing as authored. The rumble ends at the next step, before either value is used.
    /// </summary>
    private const int EndMarker = 32896;

    /// <summary>
    /// $A4:98FA: selected final repeat counter3 in the shake/sound performance.
    /// This counts completed oscillations, not elapsed frames. The consumer adds
    /// one final traversal before loading the next phase.
    /// </summary>
    private const ushort FinalDecayHold = 3;

    /// <summary>
    /// The rumble's targets in the order it visits them: seven oscillations, each a return target followed by a
    /// negative target and the timing the next oscillation loads. Stepping past the last target ends the rumble.
    /// </summary>
    /// <remarks>
    /// Negative extrema double after each two-cycle buildup, reaching amplitude 4 at phase 4, then halve each phase
    /// through phase 6. Cooldown rises by four from eight, saturates at sixteen for two cycles, then halves; the final
    /// selected repeat count belongs to the authored shake cadence. The six live approach deltas rise from one to two
    /// for the middle pair, then return to one; the seventh phase carries the end marker.
    /// </remarks>
    internal static ImmutableArray<CrocomireRumbleDefinition> All { get; } =
    [
        new(0, 0, 0),
        new(-1, 8, 1),
        new(1, 0, 0),
        new(-1, 12, 1),
        new(1, 0, 0),
        new(-2, 16, 2),
        new(2, 0, 0),
        new(-2, 16, 2),
        new(2, 0, 0),
        new(-4, 8, 1),
        new(1, 0, 0),
        new(-2, FinalDecayHold, 1),
        new(1, 0, 0),
        new(-1, EndMarker, EndMarker),
    ];
}
