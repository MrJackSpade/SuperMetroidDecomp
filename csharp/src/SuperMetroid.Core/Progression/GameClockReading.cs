namespace SuperMetroid.Core.Progression;

/// <summary>
/// One reading of the game clock (<see cref="GameTimeState"/>, #2500, #2543): the gameplay updates it has counted, from
/// none through <see cref="MaximumUpdates"/>, where the clock saturates at 99:59:59.59. Its frames, seconds, minutes and
/// hours are derived from that count, frames, seconds and minutes 0 through 59 and hours 0 through 99 (#1692), and a save
/// keeps those four. The default reading is the clock at 0:00:00.00.
/// </summary>
public readonly partial record struct GameClockReading
{
    /// <summary>Updates in one second, and the rollover point of frames, seconds and minutes.</summary>
    internal const int UnitsPerNextUnit = 60;

    /// <summary>The largest hour the clock shows; reaching any later hour saturates every component instead.</summary>
    internal const int MaximumHours = 99;

    /// <summary>Updates in one minute.</summary>
    private const int UpdatesPerMinute = UnitsPerNextUnit * UnitsPerNextUnit;

    /// <summary>Updates in one hour.</summary>
    private const int UpdatesPerHour = UpdatesPerMinute * UnitsPerNextUnit;

    /// <summary>The most updates the clock counts: the reading 99:59:59.59, where it saturates.</summary>
    internal const int MaximumUpdates = (MaximumHours + 1) * UpdatesPerHour - 1;

    /// <summary>The reading after <paramref name="updates"/> counted updates.</summary>
    private GameClockReading(int updates) => Updates = updates;

    /// <summary>The reading <paramref name="hours"/>:<paramref name="minutes"/>:<paramref name="seconds"/>.<paramref name="frames"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A component lies outside the clock's domain (<see cref="IsReading"/>).</exception>
    public GameClockReading(int frames, int seconds, int minutes, int hours)
        : this(Ensure.BetweenInclusive(0, UnitsPerNextUnit - 1, frames)
            + Ensure.BetweenInclusive(0, UnitsPerNextUnit - 1, seconds) * UnitsPerNextUnit
            + Ensure.BetweenInclusive(0, UnitsPerNextUnit - 1, minutes) * UpdatesPerMinute
            + Ensure.BetweenInclusive(0, MaximumHours, hours) * UpdatesPerHour)
    {
    }

    /// <summary>The gameplay updates the clock has counted, from none through <see cref="MaximumUpdates"/>.</summary>
    internal int Updates { get; }

    /// <summary>The sub-second counter: 0 through 59.</summary>
    public int Frames => Updates % UnitsPerNextUnit;

    /// <summary>The seconds component: 0 through 59.</summary>
    public int Seconds => Updates / UnitsPerNextUnit % UnitsPerNextUnit;

    /// <summary>The minutes component: 0 through 59.</summary>
    public int Minutes => Updates / UpdatesPerMinute % UnitsPerNextUnit;

    /// <summary>The hours component: 0 through 99.</summary>
    public int Hours => Updates / UpdatesPerHour;

    /// <summary>The reading one counted update later; the saturated reading stays as it is.</summary>
    internal GameClockReading Advanced() => Updates < MaximumUpdates ? new GameClockReading(Updates + 1) : this;

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
