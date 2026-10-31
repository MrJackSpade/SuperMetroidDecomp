namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Selects the schedule of Phantoon's casual flames.</summary>
public static class PhantoonCasualFlames
{
    /// <summary>Selects the schedule of pattern <paramref name="index"/>.</summary>
    /// <param name="index">Selector 0..3, indexing the pattern table.</param>
    /// <returns>An immutable view of the selected count, cooldown, and interval words.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside 0..3.</exception>
    public static PhantoonCasualFlameSchedule Pattern(int index) => (uint)index < (uint)PhantoonCasualFlameDefinitions.Schedules.Length
        ? new PhantoonCasualFlameSchedule(PhantoonCasualFlameDefinitions.Schedules[index])
        : throw new ArgumentOutOfRangeException(nameof(index));
}
