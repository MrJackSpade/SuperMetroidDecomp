namespace SuperMetroid.Core.Escape;

/// <summary>The minutes, seconds and centiseconds the escape timer shows for a remaining time.</summary>
/// <param name="Minutes">Whole minutes remaining, 0 through 99.</param>
/// <param name="Seconds">Whole seconds past the minutes, 0 through 59.</param>
/// <param name="Centiseconds">Centiseconds past the seconds, 0 through 99.</param>
public readonly record struct EscapeTimeReadout(int Minutes, int Seconds, int Centiseconds)
{
    /// <summary>The readout of <paramref name="remainingCentiseconds"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The time is negative or longer than the timer shows.</exception>
    public static EscapeTimeReadout Of(int remainingCentiseconds)
    {
        int remaining = Ensure.BetweenInclusive(0, EscapeTimerDefinitions.LongestTime, remainingCentiseconds);
        return new(
            remaining / EscapeTimerDefinitions.CentisecondsPerMinute,
            remaining / EscapeTimerCadenceDefinitions.CentisecondsPerSecond % EscapeTimerDefinitions.SecondsPerMinute,
            remaining % EscapeTimerCadenceDefinitions.CentisecondsPerSecond);
    }
}
