using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state of one half of Tourian's Zebetite barrier.</summary>
public sealed class ZebetiteEnemyState
{
    /// <summary>
    /// Creates the state for one Zebetite half from its spawn parameters, its pairing, showing <paramref name="shown"/>. It
    /// has no linked half until a pair links its halves.
    /// </summary>
    /// <param name="half">Which half the placement names.</param>
    /// <param name="shown">What the half shows: its barrier's picture, or nothing for a barrier every Zebetite's fall has destroyed.</param>
    internal ZebetiteEnemyState(ZebetiteHalf half, ZebetiteShown shown)
    {
        Shown = Ensure.NotNull(shown);
        Half = half;
        // A primary half first spawns its linked half, which itself waits out the door transition.
        Action = Half == ZebetiteHalf.Linked ? ZebetiteAction.WaitForDoorTransition : ZebetiteAction.SpawnLinkedHalf;
    }

    /// <summary>The action this half runs each update, independent of the other half's.</summary>
    public ZebetiteAction Action { get; internal set; }

    /// <summary>
    /// Barrier generation 0 through <see cref="PersistentProgress.ZebetiteCount"/>: how many Zebetites had been destroyed
    /// when this one spawned (<see cref="PersistentProgress.ZebetitesDestroyed"/>), advanced as it falls; the count means
    /// every barrier is destroyed.
    /// </summary>
    public int Generation
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, PersistentProgress.ZebetiteCount, value);
    }

    /// <summary>The generation's form: a split pair's primary half requests a linked second half.</summary>
    public ZebetiteBarrierForm Form { get; internal set; }

    /// <summary>
    /// What this half shows (#2250): a standing barrier's picture, chosen by its form and health at its setup and then by
    /// each active update; nothing for a barrier set up once every Zebetite has fallen, which its setup deletes.
    /// </summary>
    internal ZebetiteShown Shown { get; set; }

    /// <summary>
    /// The other half of a linked pair, to which a shot's health and flash are copied; null for a single barrier, which
    /// shares them as <see cref="Compatibility.ZebetiteShotShare"/> describes. The half names the enemy in that place
    /// in the room, whatever occupies it later.
    /// </summary>
    internal RoomEnemyPlace? LinkedHalf { get; set; }

    /// <summary>
    /// Which half this is: the linked bottom half skips the primary's colour cycling and dies without advancing the
    /// Zebetite count or respawning.
    /// </summary>
    internal ZebetiteHalf Half { get; }
}
