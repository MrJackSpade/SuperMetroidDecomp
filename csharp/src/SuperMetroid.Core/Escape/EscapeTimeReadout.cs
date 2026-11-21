namespace SuperMetroid.Core.Escape;

/// <summary>The minutes, seconds and centiseconds the escape timer shows for a remaining time.</summary>
public readonly partial record struct EscapeTimeReadout
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="minutes">Whole minutes remaining, 0 through 99.</param>
    /// <param name="seconds">Whole seconds past the minutes, 0 through 59.</param>
    /// <param name="centiseconds">Centiseconds past the seconds, 0 through 99.</param>
    internal EscapeTimeReadout(int minutes, int seconds, int centiseconds)
    {
        Minutes = minutes;
        Seconds = seconds;
        Centiseconds = centiseconds;
    }

    /// <summary>Whole minutes remaining, 0 through 99.</summary>
    public int Minutes { get; init; }

    /// <summary>Whole seconds past the minutes, 0 through 59.</summary>
    public int Seconds { get; init; }

    /// <summary>Centiseconds past the seconds, 0 through 99.</summary>
    public int Centiseconds { get; init; }

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
