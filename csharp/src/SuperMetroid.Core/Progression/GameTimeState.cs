namespace SuperMetroid.Core.Progression;

/// <summary>
/// The gameplay clock the game keeps and saves in each slot. It counts accepted gameplay updates at an assumed
/// 60 per second, independent of wall-clock refresh rate, and saturates at 99:59:59.59.
/// </summary>
/// <remarks>
/// Each component is widened to <see cref="int"/> with the save field's domain, 0 through 65,535. Ordinary play keeps
/// frames, seconds and minutes inside 0..59 and hours inside 0..99, but a loaded save may hold any field value, and the
/// clock's wrap of each increment past 65,535 is load-bearing for such a value: 65,535 frames step to 0 without
/// a rollover. <see cref="IncrementWrapping"/> keeps that wrap explicitly.
/// </remarks>
public sealed class GameTimeState
{
    /// <summary>Updates in one second, and the rollover point of frames, seconds and minutes.</summary>
    private const int UnitsPerNextUnit = 60;

    /// <summary>The largest hour the clock shows; reaching any later hour saturates every component instead.</summary>
    private const int MaximumHours = 99;

    /// <summary>
    /// The longest play time, in whole minutes, the clock reaches in play: 99 hours and 59 minutes, where it saturates
    /// (#1627).
    /// </summary>
    public const int LongestPlayMinutes = MaximumHours * UnitsPerNextUnit + UnitsPerNextUnit - 1;

    /// <summary>
    /// The sub-second counter, advanced once per accepted gameplay update: 0 through 59 in play, any save-field value
    /// (0 through 65,535) after loading a save that holds one.
    /// </summary>
    public int Frames { get; private set; }

    /// <summary>The seconds component: 0 through 59 in play, any save-field value after loading a save that holds one.</summary>
    public int Seconds { get; private set; }

    /// <summary>The minutes component: 0 through 59 in play, any save-field value after loading a save that holds one.</summary>
    public int Minutes { get; private set; }

    /// <summary>The hours component: 0 through 99 in play, any save-field value after loading a save that holds one.</summary>
    public int Hours { get; private set; }

    /// <summary>Advances the four-component clock once: frames, then seconds, minutes and hours, saturating at 99:59:59.59.</summary>
    public void Step()
    {
        // The comparisons read 32,768 and above as negative, but every component stays inside the save-field range, and the
        // wrapping increment keeps a loaded corrupt value stepping exactly as the game's clock does.
        Frames = IncrementWrapping(Frames);
        if (Frames >= UnitsPerNextUnit)
        {
            Frames = 0;
            Seconds = IncrementWrapping(Seconds);
            if (Seconds >= UnitsPerNextUnit)
            {
                Seconds = 0;
                Minutes = IncrementWrapping(Minutes);
                if (Minutes >= UnitsPerNextUnit)
                {
                    Minutes = 0;
                    Hours = IncrementWrapping(Hours);
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

    /// <summary>Restores the clock a save slot holds, without normalizing it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A component is outside a save field's range, 0 through 65,535.</exception>
    public void Load(int frames, int seconds, int minutes, int hours)
    {
        Ensure.BetweenInclusive(0, SaveFieldValue.MaximumValue, frames);
        Ensure.BetweenInclusive(0, SaveFieldValue.MaximumValue, seconds);
        Ensure.BetweenInclusive(0, SaveFieldValue.MaximumValue, minutes);
        Ensure.BetweenInclusive(0, SaveFieldValue.MaximumValue, hours);
        Frames = frames;
        Seconds = seconds;
        Minutes = minutes;
        Hours = hours;
    }

    /// <summary>
    /// One clock-component increment: the next value, wrapping from the save field's largest value, 65,535, to zero as
    /// the clock's increment does.
    /// </summary>
    private static int IncrementWrapping(int component) => component == SaveFieldValue.MaximumValue ? 0 : component + 1;
}
