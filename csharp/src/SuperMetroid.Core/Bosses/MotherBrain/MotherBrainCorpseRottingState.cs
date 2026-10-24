using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's specialization of the shared corpse-rotting engine at
/// <c>$A9:DB12-$DCB6/$A9:E08B-$EBAB</c>.
/// </summary>
/// <remarks>
/// The retail routine treats every visible pixel row as a four-byte table entry, delays the rows by different amounts,
/// and moves the corpse's pixel rows down, showing every row after each step but the last. The extraction plays those
/// moves and pictures every step (#1392); this keeps the table, whose finished rows raise dust, and the steps taken,
/// which select the pictures shown (#1406).
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
                    SoundEffect: Library2Sound.LiquidDamage));
            });

        // The final entry returns carry clear before the corpse's rows are shown again, so the
        // last step is never shown; every still-active call shows all six rows as it left them.
        if (!stillRotting)
        {
            return new MotherBrainCorpseRottingStepResult(
                StillRotting: false,
                ShownRotStep: null,
                DustRequests: dustRequests);
        }

        // Carry set from `$DBDF` reaches `$B1DD`, which shows every row each time.
        return new MotherBrainCorpseRottingStepResult(
            StillRotting: true,
            ShownRotStep: checked((int)ProcessCallCount),
            DustRequests: dustRequests);
    }
}
