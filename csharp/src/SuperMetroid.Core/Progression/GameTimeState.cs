namespace SuperMetroid.Core.Progression;

/// <summary>
/// The gameplay clock the game keeps and saves in each slot. It counts accepted gameplay updates at the clock's assumed
/// rate, independent of wall-clock refresh rate, and saturates at its largest reading (<see cref="GameClockReading"/>).
/// </summary>
/// <remarks>
/// The clock is one saturating count of accepted updates (#2543), its reading (<see cref="GameClockReading"/>); the frames,
/// seconds, minutes and hours it shows and a save keeps are derived from that count, so play never leaves the clock's
/// domain (#1692), and the clock loads only a reading.
/// </remarks>
public sealed class GameTimeState
{
    /// <summary>
    /// The longest play time, in whole minutes, the clock reaches in play: 99 hours and 59 minutes, where it saturates
    /// (#1627).
    /// </summary>
    public const int LongestPlayMinutes = GameClockReading.MaximumUpdates / (GameClockReading.UnitsPerNextUnit * GameClockReading.UnitsPerNextUnit);

    /// <summary>The clock's current reading, which a save keeps.</summary>
    public GameClockReading Reading { get; private set; }

    /// <summary>The sub-second counter, advanced once per accepted gameplay update and wrapping each second.</summary>
    public int Frames => Reading.Frames;

    /// <summary>The minutes component, within the hour.</summary>
    public int Minutes => Reading.Minutes;

    /// <summary>The hours component, up to <see cref="GameClockReading.MaximumHours"/>.</summary>
    public int Hours => Reading.Hours;

    /// <summary>Counts one accepted gameplay update; the clock stays at its largest reading once it reaches it.</summary>
    public void Step() => Reading = Reading.Advanced();

    /// <summary>Restores the clock to <paramref name="reading"/>, the clock a save slot holds.</summary>
    public void Load(GameClockReading reading) => Reading = reading;
}
