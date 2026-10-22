using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's specialization of the shared corpse-rotting engine at
/// <c>$A9:DB12-$DCB6/$A9:E08B-$EBAB</c>.
/// </summary>
/// <remarks>
/// The retail routine treats every visible pixel row as a four-byte table entry, delays the rows by different amounts,
/// and moves 4bpp bitplane rows through a work-memory buffer that six transfers upload after each step. The extraction
/// plays those moves and pictures every step (#1392); this keeps the table, whose finished rows raise dust, and the
/// steps taken, which name the pictures.
/// </remarks>
public sealed class MotherBrainCorpseRottingState
{
    /// <summary>One rot entry per visible pixel row of the table at <see cref="EnemyStagingSurfaces.MotherBrainRotTable"/>.</summary>
    public const int EntryCount = EnemyStagingDefinitions.MotherBrainRotTableEntryCount;

    /// <summary>True after the head initialization equivalent at <c>$A9:8705</c>.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Number of calls made through the active rot processor.</summary>
    public uint ProcessCallCount { get; private set; }

    /// <summary>Number of row-finished hooks emitted so far.</summary>
    public uint FinishedEntryCount { get; private set; }

    /// <summary>Builds the native rot table in the rotting heap; the extraction stages the corpse frame the table rots.</summary>
    public void Initialize(EnemyStagingSurfaces scratch)
    {
        Ensure.NotNull(scratch);

        // `$DC40` starts at height-1 and writes four bytes per entry. Y decreases from the
        // corpse's bottom row to its top while the delay grows by two calls per entry.
        CorpseRottingTableProcessor.Initialize(scratch.MotherBrainRotTable);

        ProcessCallCount = 0;
        FinishedEntryCount = 0;
        IsInitialized = true;
    }

    /// <summary>Runs one exact call of <c>ProcessCorpseRotting</c> for Mother Brain.</summary>
    public MotherBrainCorpseRottingStepResult Step(
        EnemyStagingSurfaces scratch,
        Position16<RoomPixels> brainPosition,
        MotherBrainRandomSample randomNumberSeed,
        int mainEnemyExecutionCounter)
    {
        Ensure.NotNull(scratch);
        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "Mother Brain corpse rotting must be initialized from installed artwork first.");
        }

        ProcessCallCount++;
        var dustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);

        bool stillRotting = CorpseRottingTableProcessor.Step(
            scratch.MotherBrainRotTable,
            yLimit: EntryCount - 1,
            lateMoveEntryIndex: EntryCount - 2,
            entryFinished: entryIndex =>
            {
                // `$B223` samples the already-existing global seed; it does not advance
                // RNG. One dust projectile is emitted for every completed table entry.
                FinishedEntryCount++;
                dustRequests.Add(new MotherBrainCorpseDustRequest(
                    Position: brainPosition + new Offset16<RoomPixels>(
                        checked((short)(randomNumberSeed.LowBits(5) - 16)),
                        16),
                    Animation: MiscDustAnimation.CorpseDustCloud,
                    SoundEffectQueued: (mainEnemyExecutionCounter & 7) == 0,
                    SoundEffect: SoundEffectLibrary2Sounds.DeadMonsterDust));
            });

        // The final entry returns carry clear before the enemy-specific VRAM transfer
        // callback, whereas every still-active call queues the complete six-record list.
        if (!stillRotting)
        {
            return new MotherBrainCorpseRottingStepResult(
                StillRotting: false,
                VramTransfers: [],
                DustRequests: dustRequests);
        }

        // Carry set from `$DBDF` reaches `$B1DD`, which queues all six records every time.
        int step = checked((int)ProcessCallCount);
        return new MotherBrainCorpseRottingStepResult(
            StillRotting: true,
            [.. MotherBrainCorpseArtworkDefinitions.RotTransfers.Select(transfer => new MotherBrainCorpseRotUpload(transfer, step))],
            DustRequests: dustRequests);
    }
}
