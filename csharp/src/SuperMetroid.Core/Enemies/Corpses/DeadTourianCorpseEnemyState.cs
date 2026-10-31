namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Typed corpse state for one dead Zoomer, Ripper, or Skree. The function and the pre-rot count are values the
/// cartridge keeps in the enemy's common record, so they are emptied with that record when the corpse's slot is
/// cleared (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class DeadTourianCorpseEnemyState
{
    /// <summary>Captures the selected corpse variant's callback profile and its own rot schedule for one slot.</summary>
    internal DeadTourianCorpseEnemyState(RoomEnemySlot slot, DeadTourianCorpseSpecies species, int variantIndex, DeadTourianBehavior.DeadTourianCorpseProfile profile, CorpseRotSchedule rot)
    {
        Slot = slot;
        Rot = Ensure.NotNull(rot);
        Species = species;
        VariantIndex = variantIndex;
        Profile = profile;
    }

    /// <summary>Owning room slot; state is registered for this slot until room reset.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>The phase main AI runs; none once the record has been emptied.</summary>
    public DeadTourianCorpseFunction? Function { get; internal set; }
    /// <summary>Corpse family selected by the enemy header, determining native callbacks and the tiled artwork layout.</summary>
    public DeadTourianCorpseSpecies Species { get; }
    /// <summary>Zero-based variant within the species, obtained by dividing the even native parameter-1 table-byte offset by two; Rippers allow 0–1, Zoomers and Skrees 0–2.</summary>
    public int VariantIndex { get; }
    /// <summary>The corpse's own rot schedule: the fall of each of its pixel rows, 16 for Zoomer and Ripper, 32 for Skree.</summary>
    internal CorpseRotSchedule Rot { get; }
    /// <summary>The kind of corpse whose staged characters this one rots, with every other corpse of the kind.</summary>
    internal RottingCorpseKind Flesh { get; init; }
    /// <summary>
    /// Pre-rot AI calls counted after solid Samus collision, from 0 through <see cref="LargestPreRotDelayCount"/>; the
    /// sixteenth switches to rotting and the corpse never returns to the delay. Touch/shot callbacks enter rotting directly.
    /// </summary>
    public int PreRotDelayCounter { get; internal set; }
    /// <summary>The largest pre-rot count; the next call counts zero.</summary>
    internal const int LargestPreRotDelayCount = 65535;
    /// <summary>Port diagnostic count of shared rotting-scheduler calls for this corpse; the final completion call is included.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Port diagnostic count of row-table completion callbacks, including the final entry before the AI becomes a no-operation.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Port diagnostic count of dust spawn requests from completed rows; each uses animation 10 at corpse Y plus 16 pixels and a native RNG-masked horizontal offset.</summary>
    public uint DustSpawnCount { get; internal set; }

    /// <summary>Assembly-internal callback profile and tile-row layout of this corpse's species.</summary>
    internal DeadTourianBehavior.DeadTourianCorpseProfile Profile { get; }

    /// <summary>Counts one pre-rot AI call, wrapping from <see cref="LargestPreRotDelayCount"/> to zero.</summary>
    internal void CountPreRotDelayCall() =>
        PreRotDelayCounter = PreRotDelayCounter == LargestPreRotDelayCount ? 0 : PreRotDelayCounter + 1;

    /// <summary>Empties the function and pre-rot count, when the corpse's common record is emptied.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        PreRotDelayCounter = 0;
    }
}
