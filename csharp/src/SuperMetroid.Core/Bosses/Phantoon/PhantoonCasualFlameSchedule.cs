namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One burst of Phantoon's casual flames: its flames in the order the mouth spits them, each with the wait before it, then
/// the cooldown after the last flame before the next burst is chosen.
/// </summary>
public sealed record PhantoonCasualFlameSchedule
{
    /// <summary>Mouth-scheduler updates waited after the burst's last flame before a new burst is chosen.</summary>
    public required int Cooldown { get; init; }

    /// <summary>The burst's flames, first flame first.</summary>
    public required IReadOnlyList<PhantoonCasualFlame> Flames { get; init; }
}
