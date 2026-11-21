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
    private MotherBrainGlassPhase Phase { get; set; } = MotherBrainGlassPhase.Starting;

    /// <summary>The stage the glass is at (#2315): its first until it shows it, which its first update does.</summary>
    private ListStep<MotherBrainGlassStage> Stage { get; set; } = ListStep<MotherBrainGlassStage>.FirstOf(MotherBrainGlassStageDefinitions.Stages);

    /// <summary>
    /// The shards the burst under way throws once the next image has shown (#2471): its stage's second group, which the
    /// stage's break leaves here as it throws the first; none before the glass first bursts.
    /// </summary>
    private IReadOnlyList<MotherBrainGlassShard> _burstSecondShards = [];

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
        switch (Phase)
        {
            case MotherBrainGlassPhase.Starting when _owner.IsBossDead():
                Show(place, context, MotherBrainGlassStageDefinitions.ClearedImage, 1, MotherBrainGlassPhase.Resolved);
                return;
            case MotherBrainGlassPhase.Starting when _owner.IsGlassDestroyed():
                Show(place, context, MotherBrainGlassStageDefinitions.ShatteredImage, 1, MotherBrainGlassPhase.Resolved);
                return;
            case MotherBrainGlassPhase.Starting:
                HitCountArmed = true;
                ShowStage(place, context, Stage);
                return;
            case MotherBrainGlassPhase.Holding when Hits < Stage.Entry.BreaksAtHits:
                ShowStage(place, context, Stage);
                return;
            case MotherBrainGlassPhase.Holding when Stage.Entry.Burst is { } burst:
                _owner.ThrowShards(place, burst.First, context.Outcome);
                _burstSecondShards = burst.Second;
                Show(place, context, Stage.Following?.Entry.Image ?? MotherBrainGlassStageDefinitions.ShatteredImage,
                    MotherBrainGlassStageDefinitions.BurstFrameDuration, MotherBrainGlassPhase.Bursting);
                return;
            // A stage that throws no shards breaks straight into the next; the last stage throws its shards.
            case MotherBrainGlassPhase.Holding when Stage.Following is { } following:
                ShowStage(place, context, following);
                return;
            case MotherBrainGlassPhase.Bursting:
                _owner.ThrowShards(place, _burstSecondShards, context.Outcome);
                if (Stage.Following is { } next)
                    ShowStage(place, context, next);
                else
                    Show(place, context, MotherBrainGlassStageDefinitions.ShatteredImage, MotherBrainGlassStageDefinitions.FinalShatterDuration,
                        MotherBrainGlassPhase.Shattering);
                return;
            case MotherBrainGlassPhase.Shattering:
                _owner.SetGlassDestroyed();
                LeaveRoom(place, context);
                return;
            case MotherBrainGlassPhase.Resolved:
                LeaveRoom(place, context);
                return;
            case MotherBrainGlassPhase.Holding:
            default:
                throw new InvalidOperationException($"Mother Brain's glass has no step for {Phase} at {Stage}.");
        }
    }

    /// <summary>Shows <paramref name="stage"/> for one update and holds it.</summary>
    private void ShowStage(RoomObjectPlace place, RoomObjectStepContext context, ListStep<MotherBrainGlassStage> stage)
    {
        Stage = stage;
        Show(place, context, stage.Entry.Image, 1, MotherBrainGlassPhase.Holding);
    }

    /// <summary>Draws <paramref name="image"/> and holds it for <paramref name="updates"/> in <paramref name="phase"/>.</summary>
    private void Show(RoomObjectPlace place, RoomObjectStepContext context, MotherBrainGlassDraw image, int updates, MotherBrainGlassPhase phase)
    {
        context.Draw(place, MotherBrainGlassDraws.Family, image);
        _holdBeforeNextStep.LastFor(updates);
        Phase = phase;
    }

    /// <summary>The glass leaves the room: its owner forgets its place before the place is freed.</summary>
    private void LeaveRoom(RoomObjectPlace place, RoomObjectStepContext context)
    {
        _owner.ForgetGlass(place);
        context.RoomObjects.DeleteObject(place);
    }
}
