using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Typed corpse state for one dead Zoomer, Ripper, or Skree.</summary>
public sealed class DeadTourianCorpseEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DeadTourianSegment> Playback { get; set; }

    /// <summary>Captures the corpse's species and its own rot schedule, and starts its own rows and its first phase.</summary>
    internal DeadTourianCorpseEnemyState(DeadTourianCorpseSpecies species, CorpseRotSchedule rot, DeadTourianCorpsePhase phase)
    {
        Phase = phase;
        Rot = Ensure.NotNull(rot);
        Rows = new RottingCorpseRows(rot.Rows.Count);
        Species = species;
    }

    /// <summary>The phase main AI runs.</summary>
    public DeadTourianCorpsePhase Phase { get; internal set; }
    /// <summary>The corpse's own rot schedule: the fall of each of its pixel rows, 16 for Zoomer and Ripper, 32 for Skree.</summary>
    internal CorpseRotSchedule Rot { get; }
    /// <summary>The corpse's own pixel rows as its rot has left them, which its frames are drawn through (#1539).</summary>
    internal RottingCorpseRows Rows { get; }
    /// <summary>
    /// Pre-rot AI updates counted after solid Samus collision, from zero; the sixteenth switches to rotting and the corpse
    /// never returns to the delay, so the count stops at sixteen. Touch/shot reactions enter rotting directly.
    /// </summary>
    public int PreRotDelayCounter { get; internal set; }

    /// <summary>The species whose corpse it is.</summary>
    internal DeadTourianCorpseSpecies Species { get; }

    /// <summary>Counts one update of the wait before the rot.</summary>
    internal void CountPreRotDelayUpdate() => PreRotDelayCounter++;
}
