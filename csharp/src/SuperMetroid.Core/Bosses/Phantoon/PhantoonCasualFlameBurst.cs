namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The burst of casual flames Phantoon's mouth spits while he runs his figure-eight (#1933): which burst it is, the wait
/// before its next flame and which flame that is.
/// </summary>
public sealed partial class PhantoonCasualFlameBurst
{
    /// <summary>
    /// The burst being spat, or the last one spat once it has ended: one of
    /// <see cref="PhantoonCasualFlameDefinitions.Burst"/> (#2258).
    /// </summary>
    public PhantoonCasualFlameSchedule Schedule { get; internal set => field = Ensure.NotNull(value); } = PhantoonCasualFlameDefinitions.SteadyFive;

    /// <summary>
    /// The wait before the burst's next flame, or before a new burst is chosen once one has ended (#2469): the mouth acts on
    /// the update it runs out on. A fight starts it run out, so its first update chooses a burst.
    /// </summary>
    public BossCountdown Hold { get; } = new();

    /// <summary>
    /// The burst's flame the mouth spits next; none when no burst is under way, so the next expiry picks a new burst
    /// (#1627). A burst that has spat its last flame ends and has none.
    /// </summary>
    public ListStep<PhantoonCasualFlame>? NextFlame { get; internal set; }
}
