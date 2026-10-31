namespace SuperMetroid.Core.Audio;

/// <summary>Helpers of <see cref="SoundRequestCheckTimer"/> that only verification reads.</summary>
internal static class SoundRequestCheckTimerConstants
{
    /// <summary>A timer holding <paramref name="phase"/>, which must lie in [0, <see cref="SoundRequestCheckTimer.Period"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside one period.</exception>
    public static SoundRequestCheckTimer FromPhase(int phase) =>
        new(Ensure.BetweenInclusive(0, SoundRequestCheckTimer.Period - 1, phase));
}
