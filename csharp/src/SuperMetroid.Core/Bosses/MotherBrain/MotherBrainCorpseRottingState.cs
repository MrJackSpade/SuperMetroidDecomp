using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's corpse rotting away after her final death: her corpse's own rot schedule, the rot calls made and the
/// rows that have fallen.
/// </summary>
/// <remarks>
/// The retail routine delays each visible pixel row by a different amount and moves the corpse's pixel rows down, showing
/// every row after each step but the last. The extraction plays those moves and pictures every step (#1392); this keeps
/// the schedule, whose fallen rows raise dust, and the steps taken, which select the pictures shown (#1406).
/// </remarks>
public sealed class MotherBrainCorpseRottingState
{
    /// <summary>The corpse's visible pixel rows, each of which the rot schedule drops.</summary>
    public const int EntryCount = MotherBrainCorpseArtworkDefinitions.PixelRowCount;

    /// <summary>The corpse's own rot schedule, from before its first rot call.</summary>
    internal CorpseRotSchedule Schedule { get; } = new(EntryCount);

    /// <summary>Number of rot calls made.</summary>
    public uint ProcessCallCount { get; private set; }

    /// <summary>Number of rows that have fallen so far.</summary>
    public uint FinishedEntryCount { get; private set; }

    /// <summary>Runs one rot call of Mother Brain's corpse.</summary>
    public MotherBrainCorpseRottingStepResult Step(
        Position16<RoomPixels> brainPosition,
        MotherBrainRandomSample randomNumberSeed,
        int mainEnemyExecutionCounter)
    {
        ProcessCallCount++;
        var dustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);

        bool stillRotting = Schedule.Step(
            rowFallen: _ =>
            {
                // `$B223` samples the already-existing global seed; it does not advance
                // RNG. One dust projectile is emitted for every fallen row.
                FinishedEntryCount++;
                dustRequests.Add(new MotherBrainCorpseDustRequest(
                    Position: brainPosition + new Offset16<RoomPixels>(
                        checked((short)(randomNumberSeed.LowBits(5) - 16)),
                        16),
                    Animation: MiscDustAnimation.CorpseDustCloud,
                    SoundEffectQueued: (mainEnemyExecutionCounter & 7) == 0,
                    SoundEffect: Library2Sound.LiquidDamage));
            });

        // The last row's fall returns before the corpse's rows are shown again, so the
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
