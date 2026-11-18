namespace SuperMetroid.Core.Progression;

/// <summary>
/// One reading of the game clock (<see cref="GameTimeState"/>), as a save keeps it (#2500): frames, seconds and minutes
/// 0 through 59 and hours 0 through 99 (#1692). A reading is checked once, when it is made, so none lies outside the
/// clock's domain; the default reading is the clock at 0:00:00.00.
/// </summary>
public readonly record struct GameClockReading
{
    /// <summary>Updates in one second, and the rollover point of frames, seconds and minutes.</summary>
    internal const int UnitsPerNextUnit = 60;

    /// <summary>The largest hour the clock shows; reaching any later hour saturates every component instead.</summary>
    internal const int MaximumHours = 99;

    /// <summary>The reading <paramref name="hours"/>:<paramref name="minutes"/>:<paramref name="seconds"/>.<paramref name="frames"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A component lies outside the clock's domain (<see cref="IsReading"/>).</exception>
    public GameClockReading(int frames, int seconds, int minutes, int hours)
    {
        Frames = Ensure.BetweenInclusive(0, UnitsPerNextUnit - 1, frames);
        Seconds = Ensure.BetweenInclusive(0, UnitsPerNextUnit - 1, seconds);
        Minutes = Ensure.BetweenInclusive(0, UnitsPerNextUnit - 1, minutes);
        Hours = Ensure.BetweenInclusive(0, MaximumHours, hours);
    }

    /// <summary>The sub-second counter: 0 through 59.</summary>
    public int Frames { get; }

    /// <summary>The seconds component: 0 through 59.</summary>
    public int Seconds { get; }

    /// <summary>The minutes component: 0 through 59.</summary>
    public int Minutes { get; }

    /// <summary>The hours component: 0 through 99.</summary>
    public int Hours { get; }

    /// <summary>
    /// Whether the four components are a reading of the clock: frames, seconds and minutes 0 through 59, hours 0 through 99.
    /// A save codec checks a stored clock with this before making its reading, to reject the stored save in its own terms.
    /// </summary>
    public static bool IsReading(int frames, int seconds, int minutes, int hours) =>
        IsUnitReading(frames) && IsUnitReading(seconds) && IsUnitReading(minutes) && hours is >= 0 and <= MaximumHours;

    /// <summary>Whether <paramref name="value"/> is a frames, seconds or minutes reading: 0 through 59.</summary>
    private static bool IsUnitReading(int value) => value is >= 0 and < UnitsPerNextUnit;

    /// <inheritdoc />
    public override string ToString() => $"{Hours}:{Minutes:D2}:{Seconds:D2}.{Frames:D2}";
}
