using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's corpse rotting away after her final death: her corpse's own rot schedule and the corpse's own rows as
/// the rot has left them, which her corpse's frame is drawn whole through (#1539).
/// </summary>
public sealed class MotherBrainCorpseRottingState
{
    /// <summary>The corpse's visible pixel rows, each of which the rot schedule drops.</summary>
    internal const int RowCount = MotherBrainCorpseArtworkDefinitions.PixelRowCount;

    /// <summary>The corpse's own rot schedule, from before its first rot update.</summary>
    internal CorpseRotSchedule Schedule { get; } = new(RowCount);

    /// <summary>The corpse's rows as its rot has left them.</summary>
    private readonly RottingCorpseRows rows = new(RowCount);

    /// <summary>The corpse's rows as its rot has left them so far (#1539).</summary>
    internal CorpseRotRows Shown => rows.Shown;

    /// <summary>Runs one rot update of Mother Brain's corpse.</summary>
    public MotherBrainCorpseRottingStepResult Step(
        Position16<RoomPixels> brainPosition,
        MotherBrainRandomSample randomNumberSeed,
        int enemyPassCount)
    {
        var dustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);

        bool stillRotting = Schedule.Step(
            rows.MoveRow,
            rowFallen: _ =>
            {
                // The rotting step samples the already-existing global seed; it does not advance
                // RNG. One dust projectile is emitted for every fallen row.
                dustRequests.Add(new MotherBrainCorpseDustRequest(
                    Position: brainPosition + MotherBrainLateFightDefinitions.CorpseDustFromBrain +
                        new Offset16<RoomPixels>(randomNumberSeed.Read(MotherBrainRandomDefinitions.CorpseDustOffset), 0),
                    Animation: DustExplosionAnimation.CorpseDustCloud,
                    Sound: MotherBrainCadenceDefinitions.CorpseDustSound.IsDue(enemyPassCount) ? EnemySound.LiquidDamage : null));
            });

        return new MotherBrainCorpseRottingStepResult(StillRotting: stillRotting, DustRequests: dustRequests);
    }
}
