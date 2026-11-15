namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The burst of casual flames Phantoon's mouth spits while he runs his figure-eight (#1933): its pattern, the wait before
/// the next flame and the flames left in it.
/// </summary>
public sealed class PhantoonCasualFlameBurst
{
    /// <summary>The burst's pattern, zero through three.</summary>
    public int Pattern { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The updates before the burst's next flame, or before a new burst is chosen once one has ended.</summary>
    public int Hold { get; internal set; }

    /// <summary>
    /// The flames left in the burst being spat, zero or more; none when no burst is under way, so the next expiry picks a
    /// new burst (#1627). A burst that reaches its last flame ends and has none.
    /// </summary>
    public int? Remaining
    {
        get;
        internal set => field = value is { } count ? Ensure.AtLeastZero(count) : null;
    }
}
