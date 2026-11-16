namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where Mother Brain's next dust cloud or explosion bursts, and on which update (#1872). Her ascent's dust, her death
/// explosions and the escape door's dust each step back through their own list of positions, a step before the first
/// continuing from the last; but the three step one place between them, so each effect starts where the one before it
/// left off: her first death explosions burst at the place before the one the ascent's last dust cloud took, and the
/// door's first dust at the place before the last death explosions'. The death explosions and the door's dust also count
/// down one interval between bursts, so the door's first dust bursts once the interval the last death explosions left
/// runs out. Her death restarts the place and the interval before her fade-out explosions and again once the door's dust
/// ends.
/// </summary>
/// <remarks>
/// Consulted whenever one of the three effects may burst: on each step of her ascent, on every update of her death
/// explosions and on every update the escape door explodes. The place chooses only where a burst shows; the interval
/// decides on which update each burst draws its random numbers. One cycle per encounter, owned by her encounter state and
/// shared with the phase-three sequence attached to it. Evidence: the original keeps one counter for the ascent dust's,
/// the death explosions' and the door dust's places and one for the death explosions' and the door dust's interval,
/// clearing them only at the two points of her death this type's restart stands for; the full-movie replays compare
/// every enemy projectile's position, these bursts among them, so the place each starts from is observable to them: with
/// the death explosions starting a cycle of their own, the 13% replay diverges at her death on an explosion's position.
/// </remarks>
[CompatibilityService("Mother Brain's ascent dust, death explosions and escape-door dust take their positions from one place each continues from where the effect before left it, and the door's dust continues the death explosions' interval")]
public sealed class MotherBrainBurstCycle
{
    /// <summary>
    /// The place the last burst took in its effect's list of positions; the next burst takes the place before it. It may
    /// hold a place past the end of the next effect's list or below zero, as the encounter's earlier steps leave it.
    /// </summary>
    public int Place { get; private set; }

    /// <summary>Updates left of the interval before the next burst is due; a burst is due once it runs below zero.</summary>
    public int Interval { get; private set; }

    /// <summary>
    /// Steps to the place before the last, continuing from the last of <paramref name="positions"/> places once it passes
    /// the first, and returns it.
    /// </summary>
    /// <param name="positions">How many positions the bursting effect's list holds.</param>
    /// <returns>The place the burst takes.</returns>
    public int NextPlace(int positions)
    {
        Ensure.BetweenInclusive(1, int.MaxValue, positions);
        Place = Place - 1 < 0 ? positions - 1 : Place - 1;
        return Place;
    }

    /// <summary>
    /// Counts one update off the interval. True when it runs out: a burst is due now, and the interval starts again at
    /// <paramref name="interval"/>.
    /// </summary>
    /// <param name="interval">The updates the bursting effect waits after this burst before the next is due.</param>
    /// <returns>Whether a burst is due on this update.</returns>
    public bool IntervalRunsOut(int interval)
    {
        if (--Interval >= 0)
            return false;
        Interval = interval;
        return true;
    }

    /// <summary>Restarts the cycle: the next burst is due at once and takes its list's last place.</summary>
    public void Restart()
    {
        Place = 0;
        Interval = 0;
    }
}
