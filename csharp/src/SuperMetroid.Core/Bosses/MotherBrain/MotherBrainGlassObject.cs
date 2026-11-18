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
    internal MotherBrainGlassPhase Phase { get; private set; } = MotherBrainGlassPhase.Starting;

    /// <summary>The stage the glass is at (#2315), or null before it shows its first.</summary>
    internal ListStep<MotherBrainGlassStage>? Stage { get; private set; }

    /// <summary>The hits the glass has taken, from missiles on its block and Mother Brain's head shots.</summary>
    internal int Hits { get; private set; }

    /// <summary>Whether strikes on the glass's block count hits: from its first update, once it shows its first stage.</summary>
    internal bool HitCountArmed { get; private set; }

    /// <summary>The strike on the glass's block its hit count has not yet read, or null for none.</summary>
    internal ResidentObjectProjectileHit? PendingHit { get; private set; }

    /// <summary>How long the glass holds before its next step: its first update, then each image's own updates.</summary>
    private readonly BossCountdown _holdBeforeNextStep = HoldFor(1);

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => PendingHit = hit;

    /// <summary>Mother Brain's head shot counts a hit on the glass at once.</summary>
    internal void CountHeadShot() => Hits++;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (HitCountArmed)
        {
            if (PendingHit?.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
                Hits++;
            PendingHit = null;
        }
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
                ShowStage(place, context, ListStep<MotherBrainGlassStage>.FirstOf(MotherBrainGlassStageDefinitions.Stages));
                return;
            case MotherBrainGlassPhase.Holding when Hits < ShownStage.Entry.BreaksAtHits:
                ShowStage(place, context, ShownStage);
                return;
            case MotherBrainGlassPhase.Holding when ShownStage.Entry.Burst is { } burst:
                _owner.SpawnShards(place, burst.First);
                Show(place, context, ShownStage.Following?.Entry.Image ?? MotherBrainGlassStageDefinitions.ShatteredImage,
                    MotherBrainGlassStageDefinitions.BurstFrameDuration, MotherBrainGlassPhase.Bursting);
                return;
            case MotherBrainGlassPhase.Holding:
                ShowStage(place, context, ShownStage.Following ?? throw new InvalidOperationException(
                    "Mother Brain's last glass stage broke without throwing its shards."));
                return;
            case MotherBrainGlassPhase.Bursting:
                _owner.SpawnShards(place, (ShownStage.Entry.Burst ?? throw new InvalidOperationException(
                    $"Mother Brain's glass burst at {ShownStage}, which throws no shards.")).Second);
                if (ShownStage.Following is { } next)
                    ShowStage(place, context, next);
                else
                    Show(place, context, MotherBrainGlassStageDefinitions.ShatteredImage, MotherBrainGlassStageDefinitions.FinalShatterDuration,
                        MotherBrainGlassPhase.Shattering);
                return;
            case MotherBrainGlassPhase.Shattering:
                _owner.SetGlassDestroyed();
                context.RoomObjects.DeleteObject(place);
                return;
            case MotherBrainGlassPhase.Resolved:
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MotherBrainGlassPhase)} {Phase}.");
        }
    }

    /// <summary>The stage the glass shows.</summary>
    /// <exception cref="InvalidOperationException">The glass has shown no stage yet.</exception>
    private ListStep<MotherBrainGlassStage> ShownStage => Stage ??
        throw new InvalidOperationException($"Mother Brain's glass is {Phase} before showing its first stage.");

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

    /// <summary>A hold that lasts <paramref name="updates"/> updates.</summary>
    private static BossCountdown HoldFor(int updates)
    {
        var hold = new BossCountdown();
        hold.LastFor(updates);
        return hold;
    }
}
