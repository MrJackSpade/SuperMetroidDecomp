namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Dead Torizo's corpse state: its function and velocity, rot table and schedule, transfer half, contact delay and
/// sand-heap progress. The function and velocity are values the cartridge keeps in the enemy's common record, emptied
/// with it when the corpse is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class DeadTorizoEnemyState
{
    /// <summary>Captures the corpse's rot schedule for this actor.</summary>
    internal DeadTorizoEnemyState(RoomEnemySlot slot, CorpseRottingProgram rotting)
    {
        Slot = slot;
        YLimit = rotting.YLimit;
        LateMoveEntryIndex = rotting.LateMoveEntryIndex;
        WrapOffset = rotting.WrapOffset;
    }

    /// <summary>Physical enemy slot zero, owning the corpse's position and collision/instruction words.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>The corpse's ninety-six (signed pixel Y, delay) entries in the shared rotting heap, one per corpse pixel row.</summary>
    internal CorpseRotTable RotTable { get; init; }
    /// <summary>Corpse height less one, 95: a moved row completes when its next pixel Y reaches this boundary, which also identifies the final entry.</summary>
    public int YLimit { get; }
    /// <summary>Corpse height less two, 94: this entry and the final entry use destructive moves even during their last three delay calls.</summary>
    public int LateMoveEntryIndex { get; }
    /// <summary>Byte displacement, 308, applied when source pixel Y modulo eight is 6 or 7 so the two-pixel downward destination lands in the next tile row.</summary>
    public int WrapOffset { get; }

    /// <summary>
    /// Whether this call's corpse uploads use the odd half: body rows 6-11 and the first sand strip. Every main-AI call
    /// flips it before building transfers, even while the corpse is intact or finished. The cartridge increments a phase
    /// word and selects by its low bit, which alternates identically across the word's wrap from 65535 to 0.
    /// </summary>
    public bool UploadsOddTransferHalf { get; internal set; }
    /// <summary>Contact-delay calls counted before rotting, 0..16: the sixteenth enters rotting and the corpse never returns to the delay. Direct shot or touch triggering bypasses it.</summary>
    public int PreRotDelayCounter { get; internal set; }
    /// <summary>Next sand-heap pixel row to copy, starting at 15 and copied in descending order through 1; zero means no more heap rows remain.</summary>
    public int SandLineCounter { get; internal set; }
    /// <summary>Rotting calls since the last sand row, 0..14; reaching fifteen resets it to zero and copies the next nonzero sand row.</summary>
    public int SandFrameCounter { get; internal set; }
    /// <summary>Host diagnostic count of shared corpse-rotting scheduler calls since initialization, including the call that completes its final entry.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Host diagnostic count of completed row entries, incremented before their dust callback, including the final entry.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Host diagnostic count of dust effects requested by completed entries, one request per completed-entry hook invocation.</summary>
    public uint DustSpawnCount { get; internal set; }
    /// <summary>Host diagnostic count of sand rows copied into the live heap surface, incremented once for each descending nonzero row selector.</summary>
    public uint SandLineCopyCount { get; internal set; }

    /// <summary>Moves past the sand row just copied to the row above it; a corpse with no sand row left has nothing to consume.</summary>
    internal void ConsumeSandLine()
    {
        if (SandLineCounter <= 0)
            throw new InvalidOperationException("Dead Torizo has no sand row left to copy.");
        SandLineCounter--;
    }

    /// <summary>The bank-$A9 corpse function; none once a kill has emptied the record.</summary>
    internal DeadTorizoFunction? Function { get; set; }

    /// <summary>Signed 8.8 velocity per update: X, and Y, which rotting increases by one 256th each update.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        Velocity = default;
    }
}
