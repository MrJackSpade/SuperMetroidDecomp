namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The way a Yapping Maw's curl phase moves its tongue (#2569).</summary>
internal static class YappingMawCurlPhases
{
    /// <summary>
    /// The change one curl update at <paramref name="speed"/> makes to the tongue's curl in <paramref name="phase"/>: out by
    /// the speed while extending, back by it while retracting.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is neither extending nor retracting.</exception>
    internal static SignedSixteenSixteen CurlChange(this YappingMawCurlPhase phase, SignedSixteenSixteen speed) => phase switch
    {
        YappingMawCurlPhase.Extending => speed,
        YappingMawCurlPhase.Retracting => -speed,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, $"Undefined {nameof(YappingMawCurlPhase)}."),
    };
}
