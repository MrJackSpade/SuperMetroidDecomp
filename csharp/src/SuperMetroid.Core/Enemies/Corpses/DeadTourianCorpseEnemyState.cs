using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Typed corpse state for one dead Zoomer, Ripper, or Skree.</summary>
public sealed class DeadTourianCorpseEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DeadTourianProgram> Playback { get; set; }

    /// <summary>Captures the selected corpse variant's callback profile and its own rot schedule for one slot, and starts its own rows.</summary>
    internal DeadTourianCorpseEnemyState(RoomEnemySlot slot, DeadTourianCorpseSpecies species, int variantIndex, DeadTourianBehavior.DeadTourianCorpseProfile profile, CorpseRotSchedule rot)
    {
        Slot = slot;
        Rot = Ensure.NotNull(rot);
        Rows = new RottingCorpseRows(rot.Rows.Count);
        Species = species;
        VariantIndex = variantIndex;
        Profile = profile;
    }

    /// <summary>Owning room slot; state is registered for this slot until room reset.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>The phase main AI runs.</summary>
    public DeadTourianCorpseFunction? Function { get; internal set; }
    /// <summary>Corpse family selected by the enemy definition, determining callbacks and the tiled artwork layout.</summary>
    public DeadTourianCorpseSpecies Species { get; }
    /// <summary>Zero-based variant within the species, its place in the species' corpses; Rippers have 0–1, Zoomers and Skrees 0–2.</summary>
    public int VariantIndex { get; }
    /// <summary>The corpse's own rot schedule: the fall of each of its pixel rows, 16 for Zoomer and Ripper, 32 for Skree.</summary>
    internal CorpseRotSchedule Rot { get; }
    /// <summary>The corpse's own pixel rows as its rot calls have left them, which its frames are drawn through (#1539).</summary>
    internal RottingCorpseRows Rows { get; }
    /// <summary>
    /// Pre-rot AI calls counted after solid Samus collision, from zero; the sixteenth switches to rotting and the corpse
    /// never returns to the delay, so the count stops at sixteen. Touch/shot callbacks enter rotting directly.
    /// </summary>
    public int PreRotDelayCounter { get; internal set; }
    /// <summary>Diagnostic count of shared rotting-scheduler calls for this corpse; the final completion call is included.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Diagnostic count of row-table completion callbacks, including the final entry before the AI becomes a no-operation.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Diagnostic count of dust spawn requests from completed rows; each uses animation 10 at corpse Y plus 16 pixels and a RNG-masked horizontal offset.</summary>
    public uint DustSpawnCount { get; internal set; }

    /// <summary>Assembly-internal callback profile and tile-row layout of this corpse's species.</summary>
    internal DeadTourianBehavior.DeadTourianCorpseProfile Profile { get; }

    /// <summary>Counts one pre-rot AI call.</summary>
    internal void CountPreRotDelayCall() => PreRotDelayCounter++;
}
