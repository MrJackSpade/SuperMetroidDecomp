using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's corpse rotting away after her final death: her corpse's own rot schedule, the rot calls made, the rows
/// that have fallen, and the corpse's rows as the rot has left them, which her corpse's frame is drawn through (#1531).
/// </summary>
/// <remarks>
/// The game delays each visible pixel row by a different amount and moves the corpse's pixel rows down, showing every row
/// after each step but the last.
/// </remarks>
public sealed class MotherBrainCorpseRottingState
{
    /// <summary>The corpse's visible pixel rows, each of which the rot schedule drops.</summary>
    public const int EntryCount = MotherBrainCorpseArtworkDefinitions.PixelRowCount;

    /// <summary>The corpse's own rot schedule, from before its first rot call.</summary>
    internal CorpseRotSchedule Schedule { get; } = new(EntryCount);

    /// <summary>The corpse's rows as its rot calls have left them.</summary>
    private readonly RottingCorpseRows rows = new(EntryCount);

    /// <summary>Number of rot calls made.</summary>
    public uint ProcessCallCount { get; private set; }

    /// <summary>Number of rows that have fallen so far.</summary>
    public uint FinishedEntryCount { get; private set; }

    /// <summary>
    /// The corpse's rows as the game last showed them (#1531): after each rot call but the last; none before the first.
    /// </summary>
    internal CorpseRotRows? Shown { get; private set; }

    /// <summary>Runs one rot call of Mother Brain's corpse.</summary>
    public MotherBrainCorpseRottingStepResult Step(
        Position16<RoomPixels> brainPosition,
        MotherBrainRandomSample randomNumberSeed,
        int mainEnemyExecutionCounter)
    {
        ProcessCallCount++;
        var dustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);

        bool stillRotting = Schedule.Step(
            rows.MoveRow,
            rowFallen: _ =>
            {
                // The rotting step samples the already-existing global seed; it does not advance
                // RNG. One dust projectile is emitted for every fallen row.
                FinishedEntryCount++;
                dustRequests.Add(new MotherBrainCorpseDustRequest(
                    Position: brainPosition + new Offset16<RoomPixels>(
                        randomNumberSeed.LowBits(5) - 16,
                        16),
                    Animation: MiscDustAnimation.CorpseDustCloud,
                    SoundEffectQueued: (mainEnemyExecutionCounter & 7) == 0,
                    SoundEffect: Library2Sound.LiquidDamage));
            });

        // The last row's fall returns before the corpse's rows are shown again, so the last step is never shown; every
        // still-active call shows all six rows as it left them.
        if (stillRotting)
            Shown = rows.Shown;
        return new MotherBrainCorpseRottingStepResult(StillRotting: stillRotting, DustRequests: dustRequests);
    }
}
