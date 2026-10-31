namespace SuperMetroid.Core.Samus.Poses;

/// <summary>How the Samus animation programs in <see cref="SamusAnimationDelayDefinitions"/> are written.</summary>
/// <remarks>
/// The frame delays are authored animation cadence: the chosen timing of each drawn pose sequence (turn, landing,
/// breathing, spin, appearance), which follows from no physical gameplay quantity. Fitting or reciting them in arithmetic
/// would re-encode the same choreography, so long uniform runs are written as repeats.
/// </remarks>
internal static class SamusAnimationDelayPrograms
{
    /// <summary><paramref name="count"/> frames, each held for <paramref name="ticks"/> ticks.</summary>
    internal static IEnumerable<int> Repeat(int ticks, int count) => Enumerable.Repeat(ticks, count);
}
