using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Typed corpse state for one dead Zoomer, Ripper, or Skree.</summary>
public sealed class DeadTourianCorpseEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DeadTourianSegment> Playback { get; set; }

    /// <summary>Captures the selected corpse variant's reaction profile and its own rot schedule for one slot, and starts its own rows.</summary>
    internal DeadTourianCorpseEnemyState(RoomEnemySlot slot, DeadTourianBehavior.DeadTourianCorpseProfile profile, CorpseRotSchedule rot)
    {
        Slot = slot;
        Rot = Ensure.NotNull(rot);
        Rows = new RottingCorpseRows(rot.Rows.Count);
        Profile = profile;
    }

    /// <summary>Owning room slot; state is registered for this slot until room reset.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>The phase main AI runs.</summary>
    public DeadTourianCorpseFunction? Function { get; internal set; }
    /// <summary>The corpse's own rot schedule: the fall of each of its pixel rows, 16 for Zoomer and Ripper, 32 for Skree.</summary>
    internal CorpseRotSchedule Rot { get; }
    /// <summary>The corpse's own pixel rows as its rot calls have left them, which its frames are drawn through (#1539).</summary>
    internal RottingCorpseRows Rows { get; }
    /// <summary>
    /// Pre-rot AI calls counted after solid Samus collision, from zero; the sixteenth switches to rotting and the corpse
    /// never returns to the delay, so the count stops at sixteen. Touch/shot reactions enter rotting directly.
    /// </summary>
    public int PreRotDelayCounter { get; internal set; }

    /// <summary>Assembly-internal reaction profile and tile-row layout of this corpse's species.</summary>
    internal DeadTourianBehavior.DeadTourianCorpseProfile Profile { get; }

    /// <summary>Counts one pre-rot AI call.</summary>
    internal void CountPreRotDelayCall() => PreRotDelayCounter++;
}
