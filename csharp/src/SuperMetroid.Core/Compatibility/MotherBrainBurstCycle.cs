namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where Mother Brain's next dust cloud or explosion bursts, and on which update (#1872). Her ascent's dust, her death
/// explosions and the escape door's dust each show their own list of positions from its first to its last and then
/// again (#2315), the cycle stepping back one place a burst and counting it back from the list's end; but the three step
/// one place between them, so each effect starts where the one before it left off: her first death explosions burst at
/// the place before the one the ascent's last dust cloud took, and the
/// door's first dust at the place before the last death explosions'. The death explosions and the door's dust also count
/// down one interval between bursts, so the door's first dust bursts once the interval the last death explosions left
/// runs out. Her death restarts the place and the interval before her fade-out explosions and again once the door's dust
/// ends.
/// </summary>
/// <remarks>
/// Consulted whenever one of the three effects may burst: on each step of her ascent, on every update of her death
/// explosions and on every update the escape door explodes. The place chooses only where a burst shows; the interval
/// decides on which update each burst draws its random numbers. One cycle per encounter, created by her body's setup,
/// held by her encounter state and given to the phase-three sequence attached to it. Evidence: the original keeps one counter for the ascent dust's,
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
    internal int Place { get; set; }

    /// <summary>The update a burst is due on, after its interval's updates have passed.</summary>
    private const int DueUpdate = 1;

    /// <summary>
    /// The wait for the next burst: the interval's updates left and the update the burst is due on, over on that update.
    /// A cycle not yet run is due at once.
    /// </summary>
    internal readonly UpdateCountdown burstWait = UpdateCountdown.Lasting(DueUpdate);

    /// <summary>
    /// Steps to the bursting effect's next burst and returns its entry of <paramref name="shown"/>, the effect's list in
    /// the order its bursts show it (#2315): the cycle steps to the place before the last, continuing from the last place
    /// once it passes the first, and counts that place back from the list's end, so a whole run of the cycle shows the list
    /// from its first entry to its last.
    /// </summary>
    /// <param name="shown">The bursting effect's entries, in the order its bursts show them.</param>
    /// <returns>The entry the burst takes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The place the cycle continues from lies past the effect's list.</exception>
    public T NextOf<T>(IReadOnlyList<T> shown)
    {
        int positions = Ensure.NotNull(shown).Count;
        Ensure.BetweenInclusive(1, int.MaxValue, positions);
        Place = Place - 1 < 0 ? positions - 1 : Place - 1;
        return Ensure.Element(shown, positions - 1 - Place);
    }

    /// <summary>
    /// Counts one update off the interval. True when it runs out: a burst is due now, and the interval starts again at
    /// <paramref name="interval"/>.
    /// </summary>
    /// <param name="interval">The updates the bursting effect waits after this burst before the next is due.</param>
    /// <returns>Whether a burst is due on this update.</returns>
    public bool IntervalRunsOut(int interval)
    {
        if (!burstWait.Count())
            return false;
        burstWait.Restart(interval + DueUpdate);
        return true;
    }

    /// <summary>Restarts the cycle: the next burst is due at once and takes its list's last place.</summary>
    public void Restart()
    {
        Place = 0;
        burstWait.Restart(DueUpdate);
    }
}
