using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state of one half of Tourian's Zebetite barrier.</summary>
public sealed class ZebetiteEnemyState
{
    /// <summary>
    /// Creates the state for one Zebetite half from its spawn parameters: its pairing. It has no linked half until a
    /// pair links its halves.
    /// </summary>
    internal ZebetiteEnemyState(RoomEnemyPopulationRecord population)
    {
        IsSecondaryHalf = population.SpawnSettingsOf<ZebetiteSpawnSettings>().Half == ZebetiteHalf.Linked;
    }

    /// <summary>The indirect main-AI entry, independent for each physical half.</summary>
    public ZebetiteAction? Action { get; internal set; }

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

    /// <summary>The generation's form, including whether it requests a linked second half.</summary>
    public ZebetiteGenerationFlags GenerationFlags { get; internal set; }

    /// <summary>This half's health animation and where it stands (#641, #1600).</summary>
    internal AnimationPlayback<ZebetiteSegment> Playback { get; set; } = ZebetiteAnimations.Starting(default);

    /// <summary>
    /// The other half of a linked pair, to which a shot's health and flash are copied; null for a single barrier, which
    /// shares them as <see cref="Compatibility.ZebetiteShotShare"/> describes. The half names the enemy in that place
    /// in the room, whatever occupies it later.
    /// </summary>
    internal RoomEnemySlot? LinkedHalf { get; set; }

    /// <summary>Whether this is the linked bottom half; it skips primary palette cycling and dies without advancing the Zebetite count or respawning.</summary>
    public bool IsSecondaryHalf { get; internal set; }
}
