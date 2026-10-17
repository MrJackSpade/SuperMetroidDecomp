namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Typed corpse state for one dead Zoomer, Ripper, or Skree. The function and the pre-rot count are values the
/// cartridge keeps in the enemy's common record, so they are emptied with that record when the corpse's slot is
/// cleared (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class DeadTourianCorpseEnemyState
{
    /// <summary>Captures the selected corpse variant's rot schedule, callback profile and artwork for one slot.</summary>
    internal DeadTourianCorpseEnemyState(RoomEnemySlot slot, DeadTourianCorpseSpecies species, int variantIndex, CorpseRottingProgram rotting, DeadTourianBehavior.DeadTourianCorpseProfile profile, DeadTourianBehavior.DeadTourianCorpseVariant variant)
    {
        Slot = slot;
        Species = species;
        VariantIndex = variantIndex;
        YLimit = rotting.YLimit;
        LateMoveEntryIndex = rotting.LateMoveEntryIndex;
        WrapOffset = rotting.WrapOffset;
        Profile = profile;
        Variant = variant;
    }

    /// <summary>Owning room slot; state is registered for this slot until room reset.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>The phase main AI runs; none once the record has been emptied.</summary>
    public DeadTourianCorpseFunction? Function { get; internal set; }
    /// <summary>Corpse family selected by the enemy header, determining native callbacks and the tiled artwork layout.</summary>
    public DeadTourianCorpseSpecies Species { get; }
    /// <summary>Zero-based variant within the species, obtained by dividing the even native parameter-1 table-byte offset by two; Rippers allow 0–1, Zoomers and Skrees 0–2.</summary>
    public int VariantIndex { get; }
    /// <summary>The corpse's (signed pixel Y, delay) entries in the shared rotting heap, one per corpse pixel row: 16 for Zoomer and Ripper, 32 for Skree.</summary>
    internal CorpseRotTable RotTable { get; init; }
    /// <summary>The corpse tile uploads queued after every rotting call, including the call that finishes the corpse.</summary>
    internal DeadMonsterRottingDefinitions.TransferRows VramTransfers { get; init; }
    /// <summary>Exclusive pixel-row Y limit and final entry index, equal to sprite height minus one; reaching it completes an entry.</summary>
    public int YLimit { get; }
    /// <summary>First entry index that uses destructive moves even during the last delay updates, equal to sprite height minus two.</summary>
    public int LateMoveEntryIndex { get; }
    /// <summary>Byte displacement added when rows 6 or 7 move across an 8-pixel tile boundary: 84 for three-column Zoomer/Ripper blocks, 52 for two-column Skree blocks.</summary>
    public int WrapOffset { get; }
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
    /// <summary>Assembly-internal variant artwork and behavior paired with this corpse's configuration.</summary>
    internal DeadTourianBehavior.DeadTourianCorpseVariant Variant { get; }

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
