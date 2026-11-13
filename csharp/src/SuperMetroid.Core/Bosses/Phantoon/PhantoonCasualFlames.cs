namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Selects the schedule of Phantoon's casual flames.</summary>
public static class PhantoonCasualFlames
{
    /// <summary>Selects the schedule of pattern <paramref name="index"/>.</summary>
    /// <param name="index">The flame pattern, 0 through 3.</param>
    /// <returns>The selected burst's intervals and cooldown.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside 0..3.</exception>
    public static PhantoonCasualFlameSchedule Pattern(int index) =>
        Ensure.Element(PhantoonCasualFlameDefinitions.Schedules, index);
}
