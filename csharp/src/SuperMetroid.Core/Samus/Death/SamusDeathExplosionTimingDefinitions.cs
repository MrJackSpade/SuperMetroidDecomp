namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// Compiled timing metadata for Samus's nine suit-explosion frames. Palette selectors
/// remain presentation data; this catalog owns only the fixed timer values interleaved
/// with those selectors by Samus's death sequence.
/// </summary>
/// <remarks>Issue #1165 retains the nine countdown choices under the authored
/// animation/nonsense exception. The death sequence loads the entry countdown and
/// immediately steps it; each step decrements before advancing the drawing index,
/// which selects one of nine fixed right/left drawings by that index at the
/// same captured position. The initial 21 countdown delays whiteout, the middle
/// 6/3/4/5/5/6/6 countdowns choose the drawing cadence, and the final 80 delays
/// completion. There is no distance, velocity or measured brightness input
/// whose units determine those dwell times. A numeric case list or fitted
/// curve would only recite these chosen animation delays; replacing them with
/// uniform or physically generated timing changes the cinematic. This is not
/// a size, complexity, performance or missing-provenance justification. The odd
/// palette selectors are separately calculated; this exception covers only
/// these nine durations and does not exempt adjacent control data.</remarks>
internal static class SamusDeathExplosionTimingDefinitions
{
    /// <summary>Number of authored suit-explosion timing records.</summary>
    public const int StepCount = 9;

    /// <summary>
    /// Nine authored dwell times, in updates: initial whiteout hold, seven drawing cadences, and final completion delay.
    /// </summary>
    internal static IReadOnlyList<int> Durations { get; } =
        [21, 6, 3, 4, 5, 5, 6, 6, 80];
}
