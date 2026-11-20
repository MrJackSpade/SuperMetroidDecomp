using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The glass of Mother Brain's tank (#1554). Its first update shows the cleared glass once Mother Brain is dead, or the
/// shattered glass once it was destroyed on an earlier visit, and it then leaves the room on its second update.
/// Otherwise it arms its hit count and shows its first stage. From its second update on, before its timing runs, a
/// Missile or Super Missile that struck its block counts a hit (any strike is then read); Mother Brain's head shots also
/// count hits, whenever they land. Each stage is redrawn on every update until the glass has taken the stage's hits,
/// then breaks into the next on that update, throwing its shards around the next image where it has a burst. After the
/// last stage the shattered glass shows, the destroyed event is set and the glass leaves the room.
/// </summary>
/// <param name="owner">The room's glass owner, which throws the shards and holds the boss and event state.</param>
[RoomLifetime]
internal sealed class MotherBrainGlassObject(MotherBrainGlassObjects owner) : RoomObjectBehaviour, IShootableRoomObject
{
    /// <summary>The room's glass owner.</summary>
    private readonly MotherBrainGlassObjects _owner = Ensure.NotNull(owner);

    /// <summary>Where the glass is in its course.</summary>
    private MotherBrainGlassPhase Phase { get; set; } = new MotherBrainGlassPhase.Starting();

    /// <summary>The stage the glass is at (#2315): its first until it shows it, which its first update does.</summary>
    private MotherBrainGlassStage Stage { get; set; } = MotherBrainGlassStageDefinitions.First;

    /// <summary>The hits the glass has taken, from missiles on its block and Mother Brain's head shots.</summary>
    private int Hits { get; set; }

    /// <summary>Whether strikes on the glass's block count hits: from its first update, once it shows its first stage.</summary>
    private bool HitCountArmed { get; set; }

    /// <summary>How long the glass holds before its next step: its first update, then each image's own updates.</summary>
    private readonly BossCountdown _holdBeforeNextStep = BossCountdown.LastingOneUpdate();

    /// <summary>
    /// A Missile or Super Missile that strikes the glass's block counts a hit as it lands once the glass counts hits (#2562):
    /// its count is read only at its updates, which strikes do not reach before its first, so counting at the strike
    /// counts every strike the game's update counts.
    /// </summary>
    public void Strike(ResidentObjectProjectileHit hit)
    {
        if (HitCountArmed && hit.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
            Hits++;
    }

    /// <summary>Mother Brain's head shot counts a hit on the glass at once.</summary>
    internal void CountHeadShot() => Hits++;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (_holdBeforeNextStep.StepKeepsRunning())
            return;
        if (Phase is MotherBrainGlassPhase.Starting)
        {
            if (_owner.IsBossDead())
                Show(place, context, MotherBrainGlassStageDefinitions.ClearedImage, 1, new MotherBrainGlassPhase.Resolved());
            else if (_owner.IsGlassDestroyed())
                Show(place, context, MotherBrainGlassStageDefinitions.ShatteredImage, 1, new MotherBrainGlassPhase.Resolved());
            else
            {
                HitCountArmed = true;
                ShowStage(place, context, Stage);
            }
        }
        else if (Phase is MotherBrainGlassPhase.Holding)
        {
            if (Hits < Stage.BreaksAtHits)
                ShowStage(place, context, Stage);
            // A stage that throws no shards breaks straight into the next; the last stage throws its shards.
            else if (Stage is MotherBrainGlassStage.Cracks cracks)
                ShowStage(place, context, cracks.Next);
            else if (Stage is MotherBrainGlassStage.Bursts bursts)
                Burst(place, context, bursts);
        }
        else if (Phase is MotherBrainGlassPhase.Bursting { Stage: var burst })
        {
            _owner.ThrowShards(place, burst.Burst.Second, context.Outcome);
            if (burst is MotherBrainGlassStage.Bursts.Into into)
                ShowStage(place, context, into.Next);
            else
                Show(place, context, MotherBrainGlassStageDefinitions.ShatteredImage, MotherBrainGlassStageDefinitions.FinalShatterDuration,
                    new MotherBrainGlassPhase.Shattering());
        }
        else if (Phase is MotherBrainGlassPhase.Shattering)
        {
            _owner.SetGlassDestroyed();
            context.RoomObjects.DeleteObject(place);
        }
        else if (Phase is MotherBrainGlassPhase.Resolved)
            context.RoomObjects.DeleteObject(place);
    }

    /// <summary>
    /// Breaks <paramref name="stage"/>: throws its first shards and shows the image after it (the next stage's, or the
    /// shattered glass after the last) for the burst's frames, its second shards thrown once that image has shown.
    /// </summary>
    private void Burst(RoomObjectPlace place, RoomObjectStepContext context, MotherBrainGlassStage.Bursts stage)
    {
        _owner.ThrowShards(place, stage.Burst.First, context.Outcome);
        MotherBrainGlassDraw followingImage = stage is MotherBrainGlassStage.Bursts.Into into
            ? into.Next.Image
            : MotherBrainGlassStageDefinitions.ShatteredImage;
        Show(place, context, followingImage, MotherBrainGlassStageDefinitions.BurstFrameDuration, new MotherBrainGlassPhase.Bursting(stage));
    }

    /// <summary>Shows <paramref name="stage"/> for one update and holds it.</summary>
    private void ShowStage(RoomObjectPlace place, RoomObjectStepContext context, MotherBrainGlassStage stage)
    {
        Stage = stage;
        Show(place, context, stage.Image, 1, new MotherBrainGlassPhase.Holding());
    }

    /// <summary>Draws <paramref name="image"/> and holds it for <paramref name="updates"/> in <paramref name="phase"/>.</summary>
    private void Show(RoomObjectPlace place, RoomObjectStepContext context, MotherBrainGlassDraw image, int updates, MotherBrainGlassPhase phase)
    {
        context.Draw(place, MotherBrainGlassDraws.Family, image);
        _holdBeforeNextStep.LastFor(updates);
        Phase = phase;
    }
}
