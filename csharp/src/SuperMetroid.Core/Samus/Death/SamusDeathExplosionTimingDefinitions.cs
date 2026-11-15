namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// Compiled timing metadata for Samus's nine suit-explosion frames. Shade selectors
/// remain presentation data; this catalog owns only the fixed timer values interleaved
/// with those selectors by Samus's death sequence.
/// </summary>
/// <remarks>The nine countdowns are authored animation timing. The death sequence loads the entry countdown and
/// immediately steps it; each step decrements before advancing to the next drawing,
/// one of nine fixed right/left drawings, at the
/// same captured position. The initial 21 countdown delays whiteout, the middle
/// 6/3/4/5/5/6/6 countdowns choose the drawing cadence, and the final 80 delays
/// completion. There is no distance, velocity or measured brightness input
/// whose units determine those dwell times; uniform or physically generated timing would change the
/// cinematic. The shades the explosion shows are worked out separately.</remarks>
internal static class SamusDeathExplosionTimingDefinitions
{
    /// <summary>The suit explosion's steps, each with its authored duration.</summary>
    public const int StepCount = 9;

    /// <summary>
    /// Nine authored dwell times, in updates: initial whiteout hold, seven drawing cadences, and final completion delay.
    /// </summary>
    internal static IReadOnlyList<int> Durations { get; } =
        [21, 6, 3, 4, 5, 5, 6, 6, 80];
}
