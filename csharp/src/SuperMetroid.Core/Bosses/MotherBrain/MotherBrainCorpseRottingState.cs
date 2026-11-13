using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's corpse rotting away after her final death: her corpse's own rot schedule and the corpse's own rows as
/// the rot has left them, which her corpse's frame is drawn whole through (#1539).
/// </summary>
public sealed class MotherBrainCorpseRottingState
{
    /// <summary>The corpse's visible pixel rows, each of which the rot schedule drops.</summary>
    public const int EntryCount = MotherBrainCorpseArtworkDefinitions.PixelRowCount;

    /// <summary>The corpse's own rot schedule, from before its first rot call.</summary>
    internal CorpseRotSchedule Schedule { get; } = new(EntryCount);

    /// <summary>The corpse's rows as its rot calls have left them.</summary>
    private readonly RottingCorpseRows rows = new(EntryCount);

    /// <summary>The corpse's rows as its rot calls so far have left them (#1539).</summary>
    internal CorpseRotRows Shown => rows.Shown;

    /// <summary>Runs one rot call of Mother Brain's corpse.</summary>
    public MotherBrainCorpseRottingStepResult Step(
        Position16<RoomPixels> brainPosition,
        MotherBrainRandomSample randomNumberSeed,
        int mainEnemyExecutionCounter)
    {
        var dustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);

        bool stillRotting = Schedule.Step(
            rows.MoveRow,
            rowFallen: _ =>
            {
                // The rotting step samples the already-existing global seed; it does not advance
                // RNG. One dust projectile is emitted for every fallen row.
                dustRequests.Add(new MotherBrainCorpseDustRequest(
                    Position: brainPosition + new Offset16<RoomPixels>(
                        randomNumberSeed.Read(MotherBrainRandomDefinitions.CorpseDustOffset) - 16,
                        16),
                    Animation: MiscDustAnimation.CorpseDustCloud,
                    SoundEffectQueued: MotherBrainCadenceDefinitions.CorpseDustSoundEveryEighthEnemyPass.IsDue(mainEnemyExecutionCounter),
                    SoundEffect: EnemySound.LiquidDamage));
            });

        return new MotherBrainCorpseRottingStepResult(StillRotting: stillRotting, DustRequests: dustRequests);
    }
}
