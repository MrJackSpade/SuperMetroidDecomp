using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The three non-sidehopper dead-monster graphics families.</summary>
public enum DeadTourianCorpseSpecies
{
    /// <summary>Three 3-by-2-tile Zoomer corpse variants; native wait/pre-rot/rotting functions are $A9:DA69/$DA94/$DAD0.</summary>
    Zoomer,
    /// <summary>Two 3-by-2-tile Ripper corpse variants; native wait/pre-rot/rotting functions are $A9:DA73/$DA99/$DAE6.</summary>
    Ripper,
    /// <summary>Three 2-by-4-tile Skree corpse variants; native wait/pre-rot/rotting functions are $A9:DA6E/$DA9E/$DAFC.</summary>
    Skree,
}

/// <summary>Typed extended state for one dead Zoomer, Ripper, or Skree.</summary>
public sealed class DeadTourianCorpseEnemyState
{
    /// <summary>Initializes a new dead tourian corpse enemy state instance.</summary>
    internal DeadTourianCorpseEnemyState(RoomEnemySlot slot, DeadTourianCorpseSpecies species, int variantIndex, ushort configurationPointer, CorpseRottingProgram rotting, DeadTourianBehavior.DeadTourianCorpseProfile profile, DeadTourianBehavior.DeadTourianCorpseVariant variant)
    {
        Slot = slot;
        Species = species;
        VariantIndex = variantIndex;
        ConfigurationPointer = configurationPointer;
        TablePointer = rotting.TablePointer;
        VramTablePointer = rotting.VramTablePointer;
        CopyFunction = rotting.CopyFunction;
        MoveFunction = rotting.MoveFunction;
        RotationTablePointer = rotting.RotationTablePointer;
        FinishFunction = rotting.FinishFunction;
        EntryCount = rotting.EntryCount;
        YLimit = rotting.YLimit;
        LateMoveEntryIndex = rotting.LateMoveEntryIndex;
        WrapOffset = rotting.WrapOffset;
        Profile = profile;
        Variant = variant;
    }

    /// <summary>Owning room slot, whose variable A holds the bank-$A9 wait, pre-rot, rotting, or completed no-operation function; state is registered for this slot until room reset.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>Corpse family selected by the enemy header, determining native callbacks and the tiled artwork layout.</summary>
    public DeadTourianCorpseSpecies Species { get; }
    /// <summary>Zero-based variant within the species, obtained by dividing the even native parameter-1 table-byte offset by two; Rippers allow 0–1, Zoomers and Skrees 0–2.</summary>
    public int VariantIndex { get; }
    /// <summary>Bank-$A9 pointer to the selected 16-byte corpse initialization record in the eight-record catalog beginning at $DD88.</summary>
    public ushort ConfigurationPointer { get; }
    /// <summary>Bank-$7E WRAM pointer to mutable four-byte rotting entries, each containing a signed pixel-row Y offset and an update-delay word; initialized with descending Y and staggered delays.</summary>
    public ushort TablePointer { get; }
    /// <summary>Bank-$A9 pointer to the terminated tile-data DMA list queued after every rotting call, including the call that finishes the corpse.</summary>
    public ushort VramTablePointer { get; }
    /// <summary>Bank-$A9 identity of the variant's nondestructive pixel-row copy routine, used by the shared scheduler during the final delay updates.</summary>
    public ushort CopyFunction { get; }
    /// <summary>Bank-$A9 identity of the variant's destructive pixel-row move routine, which copies the row downward and clears its source bitplanes.</summary>
    public ushort MoveFunction { get; }
    /// <summary>Bank-$A9 pointer to the species' tile-row byte-offset table at $E24C/$E252/$E258, used to locate 4bpp pixel rows in the corpse work buffer.</summary>
    public ushort RotationTablePointer { get; }
    /// <summary>$A9:DC08, CorpseRotEntryFinishedHook_Normal: row-completion callback that spawns dust below the corpse and periodically queues the crumble sound.</summary>
    public ushort FinishFunction { get; }
    /// <summary>Number of four-byte rotting entries and native sprite height in pixels: 16 for Zoomer/Ripper, 32 for Skree.</summary>
    public ushort EntryCount { get; }
    /// <summary>Exclusive pixel-row Y limit and final entry index, equal to sprite height minus one; reaching it completes an entry.</summary>
    public ushort YLimit { get; }
    /// <summary>First entry index that uses destructive moves even during the last delay updates, equal to sprite height minus two.</summary>
    public ushort LateMoveEntryIndex { get; }
    /// <summary>Byte displacement added when rows 6 or 7 move across an 8-pixel tile boundary: 84 for three-column Zoomer/Ripper blocks, 52 for two-column Skree blocks.</summary>
    public ushort WrapOffset { get; }
    /// <summary>Native Corpse.preRotDelayTimer in slot variable B; increments once per pre-rot AI call after solid Samus collision, switching to rotting at 16. Touch/shot callbacks enter rotting directly.</summary>
    public ushort PreRotDelayCounter
    {
        get => Slot.VariableB;
        internal set => Slot.VariableB = value;
    }
    /// <summary>Port diagnostic count of shared rotting-scheduler calls for this corpse; the final completion call is included.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Port diagnostic count of row-table completion callbacks, including the final entry before the AI becomes a no-operation.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Port diagnostic count of dust spawn requests from completed rows; each uses animation 10 at corpse Y plus 16 pixels and a native RNG-masked horizontal offset.</summary>
    public uint DustSpawnCount { get; internal set; }
    /// <summary>Zero-based index of the most recently completed rotting-table entry, or $FFFF before any entry finishes.</summary>
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;

    // Callback profiles remain assembly-internal. Public debugger state exposes only the
    // stable ROM-derived words above; actor code keeps typed access without parallel maps.
    /// <summary>Gets profile.</summary>
    internal DeadTourianBehavior.DeadTourianCorpseProfile Profile { get; }
    /// <summary>Gets variant.</summary>
    internal DeadTourianBehavior.DeadTourianCorpseVariant Variant { get; }
}
