namespace SuperMetroid.Core.Progression;

/// <summary>
/// The gameplay clock the game keeps and saves in each slot. It counts accepted gameplay updates at an assumed
/// 60 per second, independent of wall-clock refresh rate, and saturates at 99:59:59.59.
/// </summary>
/// <remarks>
/// The clock holds its own domain: frames, seconds and minutes 0 through 59 and hours 0 through 99 (#1692). Play never
/// leaves it, since every component rolls over at its unit and the hours saturate, so a save holds only a reading of the
/// clock (<see cref="GameClockReading"/>), and the clock loads only a reading.
/// </remarks>
public sealed class GameTimeState
{
    /// <summary>Updates in one second, and the rollover point of frames, seconds and minutes.</summary>
    private const int UnitsPerNextUnit = GameClockReading.UnitsPerNextUnit;

    /// <summary>The largest hour the clock shows; reaching any later hour saturates every component instead.</summary>
    private const int MaximumHours = GameClockReading.MaximumHours;

    /// <summary>
    /// The longest play time, in whole minutes, the clock reaches in play: 99 hours and 59 minutes, where it saturates
    /// (#1627).
    /// </summary>
    public const int LongestPlayMinutes = MaximumHours * UnitsPerNextUnit + UnitsPerNextUnit - 1;

    /// <summary>The sub-second counter, advanced once per accepted gameplay update: 0 through 59.</summary>
    public int Frames { get; private set; }

    /// <summary>The seconds component: 0 through 59.</summary>
    public int Seconds { get; private set; }

    /// <summary>The minutes component: 0 through 59.</summary>
    public int Minutes { get; private set; }

    /// <summary>The hours component: 0 through 99.</summary>
    public int Hours { get; private set; }

    /// <summary>The clock's current reading, which a save keeps.</summary>
    public GameClockReading Reading => new(Frames, Seconds, Minutes, Hours);

    /// <summary>Advances the four-component clock once: frames, then seconds, minutes and hours, saturating at 99:59:59.59.</summary>
    public void Step()
    {
        Frames++;
        if (Frames == UnitsPerNextUnit)
        {
            Frames = 0;
            Seconds++;
            if (Seconds == UnitsPerNextUnit)
            {
                Seconds = 0;
                Minutes++;
                if (Minutes == UnitsPerNextUnit)
                {
                    Minutes = 0;
                    Hours++;
                }
            }
        }

        if (Hours > MaximumHours)
        {
            Frames = UnitsPerNextUnit - 1;
            Seconds = UnitsPerNextUnit - 1;
            Minutes = UnitsPerNextUnit - 1;
            Hours = MaximumHours;
        }
    }

    /// <summary>Restores the clock to <paramref name="reading"/>, the clock a save slot holds.</summary>
    public void Load(GameClockReading reading)
    {
        Frames = reading.Frames;
        Seconds = reading.Seconds;
        Minutes = reading.Minutes;
        Hours = reading.Hours;
    }
}
