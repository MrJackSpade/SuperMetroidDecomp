namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Typed projection of Dead Torizo's bank-$A9 extended WRAM fields.</summary>
public sealed class DeadTorizoEnemyState
{
    /// <summary>Captures the corpse-rotting program's native work-table and callback identities for this actor.</summary>
    internal DeadTorizoEnemyState(RoomEnemySlot slot, CorpseRottingProgram rotting)
    {
        Slot = slot;
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
    }

    /// <summary>Physical enemy slot zero, owning the corpse's position, collision/instruction words, bank-$A9 dispatcher in VariableA, and signed 8.8 velocities in Variables B and C.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>Bank-$7E offset $9000 of the mutable four-byte (signed pixel Y, delay) entries initialized for the 96-pixel corpse.</summary>
    public ushort TablePointer { get; }
    /// <summary>Native generic transfer-pointer field, zero in configuration $A9:DD58; Torizo instead uses the dedicated phase-selected transfer queues at $A9:D4CF.</summary>
    public ushort VramTablePointer { get; }
    /// <summary>Native non-destructive row-copy callback $A9:E38B, copying the eligible corpse pixel-row bitplanes downward while retaining their source.</summary>
    public ushort CopyFunction { get; }
    /// <summary>Native destructive row-move callback $A9:E272, copying the eligible corpse pixel-row bitplanes downward and clearing their source.</summary>
    public ushort MoveFunction { get; }
    /// <summary>Bank-$A9 tile-row byte-offset table $E226; the legacy Rotation name describes addressing of 8-pixel tile rows, not an angular transform.</summary>
    public ushort RotationTablePointer { get; }
    /// <summary>Native completed-entry hook $A9:D5BD, requesting dust at whole room Y 188 and X 272..303 from the current RNG seed, plus a periodic library-two $10 sound.</summary>
    public ushort FinishFunction { get; }
    /// <summary>Ninety-six staggered rotting entries, matching the native corpse height $0060 in pixel rows.</summary>
    public ushort EntryCount { get; }
    /// <summary>Native height-minus-one value, 95: a moved row completes when its next pixel Y reaches this boundary, which also identifies the final entry.</summary>
    public ushort YLimit { get; }
    /// <summary>Native height-minus-two value, 94: this entry and the final entry use destructive moves even during their last three delay ticks.</summary>
    public ushort LateMoveEntryIndex { get; }
    /// <summary>Native inter-tile byte adjustment $0134, applied when source pixel Y modulo eight is 6 or 7 so the two-pixel downward destination wraps into the next tile row.</summary>
    public ushort WrapOffset { get; }
    /// <summary>Wrapping phase word incremented on every main-AI transfer-building call, even while intact or finished; its low bit alternates six corpse tile rows and one sand strip per queue.</summary>
    public ushort VramTransferPhase { get; internal set; }
    /// <summary>Native $7E:7808 contact-delay counter, incremented in $A9:D3C8 until sixteen calls have elapsed before falling through into rotting; direct shot/touch triggering bypasses this delay.</summary>
    public ushort PreRotDelayCounter { get; internal set; }
    /// <summary>Native $7E:7804 sand-heap pixel-row selector, initialized to 15 and copied in descending order through 1; zero means no more heap lines remain.</summary>
    public ushort SandLineCounter { get; internal set; }
    /// <summary>Native $7E:7806 divider incremented only by rotting calls; reaching fifteen resets it to zero and copies the next nonzero sand line.</summary>
    public ushort SandFrameCounter { get; internal set; }
    /// <summary>Host diagnostic count of shared corpse-rotting scheduler calls since initialization, including the call that completes its final entry.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Host diagnostic count of completed row entries, incremented before their dust callback, including the final entry.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Host diagnostic count of dust effects requested by completed entries, one request per completed-entry hook invocation.</summary>
    public uint DustSpawnCount { get; internal set; }
    /// <summary>Host diagnostic count of sand lines copied into the live $7E:9500 heap surface, incremented once for each descending nonzero line selector.</summary>
    public uint SandLineCopyCount { get; internal set; }
    /// <summary>Most recently completed zero-based row-entry index, or $FFFF before any completion; retained as host diagnostic state.</summary>
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;
}
