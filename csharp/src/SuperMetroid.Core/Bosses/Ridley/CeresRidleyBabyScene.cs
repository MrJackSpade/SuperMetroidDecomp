using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Baby Metroid Ceres Ridley carries, its fall and its draw, and the door of his room drawn around the fight.</summary>
[RoomLifetime]
internal sealed class CeresRidleyBabyScene
{
    /// <summary>Provides room randomness.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Offsets the scene's own Baby and door drawing during a room quake.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues the Baby's cries.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Publishes the Baby's and the door overlay's frames.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The room's view of Ridley: Ceres Ridley as he attached himself, and the door of his room.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyBabyScene(EnemyRoomBinding room, RoomEarthquake earthquake, EnemyAudioQueue audio, EnemyPresentation presentation, RidleyEncounter ridleyEncounter)
    {
        _room = room;
        _earthquake = earthquake;
        _audio = audio;
        _presentation = presentation;
        _ridleyEncounter = ridleyEncounter;
    }

    /// <summary>Keeps the Baby at Ridley's arm anchor, falls independently, then follows the grasp point on retrieval.</summary>
    /// <param name="place">Ridley's body, which carries the Baby.</param>
    /// <param name="baby">The Baby.</param>
    internal static void TickCeresBaby(RoomEnemyPlace place, CeresBaby baby)
    {
        switch (baby.Action)
        {
            case CeresBabyAction.CarriedInArms:
                // Until the fake retreat, Ridley carries the Baby at his left-hand anchor, so it is where he holds it
                // when he lets it fall.
                baby.Position = baby.Position.WithWhole(place.Position.Whole + CeresRidleyArenaDefinitions.BabyArmsAnchor);
                return;

            case CeresBabyAction.Drop:
                baby.Position = baby.Position.WithFractionY(PixelFraction.None);
                baby.VerticalVelocity = SignedEightEight.Zero;
                baby.Action = CeresBabyAction.Dropped;
                goto case CeresBabyAction.Dropped;

            case CeresBabyAction.Dropped:
                // The falling speed only grows from zero and the fall ends at the Baby's floor, so it stays a few pixels an
                // update.
                baby.VerticalVelocity += CeresRidleyArenaDefinitions.BabyFallGravity;
                baby.Position = baby.Position.WithY(baby.Position.Y.MovedBy(baby.VerticalVelocity));
                if (baby.Position.Y.Whole >= CeresRidleyArenaDefinitions.BabyFloorY)
                {
                    baby.Position = baby.Position.WithWholeY(CeresRidleyArenaDefinitions.BabyFloorY);
                    baby.Action = CeresBabyAction.Idle;
                }
                return;

            case CeresBabyAction.CarriedInFeet:
                // Once the hand rectangles overlap, the Baby follows the grasp point for
                // the real retreat and disappears with Ridley at the escape handoff.
                baby.Position = baby.Position.WithWhole(place.Position.Whole + CeresRidleyArenaDefinitions.BabyFeetAnchor);
                return;

            case CeresBabyAction.Idle:
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresBabyAction)} {baby.Action}.");
        }
    }

    /// <summary>
    /// Draws the Baby and the door while Ridley's movement and animation are off: as Ridley updates, before the
    /// other enemies are drawn, so the Baby appears with the room and ahead of Ridley's own picture.
    /// </summary>
    public void DrawCeresRidleyImmediateBabyAndDoor(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        // Only Ceres Ridley attaches this Baby and door draw to the room.
        if (_ridleyEncounter.CeresRidley is not { } ceres || ceres.Ridley.MovementAnimationEnabled)
            return;

        DrawCeresRidleyBabyAndDoor(sprites, ceres.Ridley, camera);
    }

    /// <summary>
    /// Draws the Baby and door over every enemy once Ceres Ridley's reveal has started his
    /// animation: the same private Baby animation, after the ordinary enemy layer loop.
    /// </summary>
    public void DrawCeresRidleyBabyAndDoorOverEnemies(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        if (_ridleyEncounter.CeresRidley is not { } ceres || !ceres.Ridley.MovementAnimationEnabled)
            return;

        DrawCeresRidleyBabyAndDoor(sprites, ceres.Ridley, camera);
    }

    /// <summary>Publishes the private Baby frame and hidden door overlay with the secondary quake offset.</summary>
    private void DrawCeresRidleyBabyAndDoor(
        SpriteFrameBuffer sprites,
        CeresRidleyState ridley,
        Position16<RoomPixels> camera)
    {
        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            if (AdvanceCeresBabyDrawAnimation(ridley) is { } babyFrame)
            {
                _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.CeresBaby), babyFrame,
                    ridley.Baby.Position.Whole.RelativeToCamera(camera), EnemyShades.HurtFlashLook);
            }
        }

        // The room's door hands its drawing to Ridley's scene in its hiding step, when the common
        // enemy pass intentionally hides the actor; the Baby and door draw then draws this fixed overlay
        // after the Baby. This draws the door for it; it does not replace the door actor's own animation.
        if (_ridleyEncounter.CeresRoomDoor is { Entity: IDrawnByCeresRidleyScene { Drawing: CeresRidleyRoomDoorDrawing.ByRidleysScene } } door)
        {
            // The scene draws the door itself rather than through the common enemy drawing, so it applies the door's
            // own earthquake shake, which moves only the drawn door.
            int quakeXOffset = CeresDoorQuake.XOffset(_earthquake.RemainingFrames);
            _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.CeresDoorOverlay), CeresDoorFrameDefinitions.RidleyRoomDoorDrawnByRidley,
                (door.Position.Whole + new Offset16<RoomPixels>(quakeXOffset, 0)).RelativeToCamera(camera),
                // The overlay shows its own standard colours; the door actor's own looks never reach it (#1512).
                PictureShading.Standard);
        }
    }

    /// <summary>
    /// Runs the Baby's typed cutscene animation one draw update on the Baby's own clock (#2228) and returns its current
    /// frame: the pose it shows holds for its duration, counted off one each draw update, and the next pose is then shown.
    /// </summary>
    /// <param name="ridley">Ceres Ridley's encounter, which holds the Baby and the colours its cues show.</param>
    internal EnemyFrameId? AdvanceCeresBabyDrawAnimation(CeresRidleyState ridley)
    {
        CeresBaby baby = ridley.Baby;
        if (!baby.Clock.Expire())
            return CeresBabyFrame(baby.ShownPose);

        Animation<CeresBabySegment> animation = baby.Playback.Animation;
        AnimationPlace place = baby.Playback.Place;
        AnimationShown<CeresBabySegment, CeresBabyPose> outcome = CeresBabyAnimations.Segments.PlayToNextFrame(ref animation, ref place,
            cue => ApplyCeresBabyCue(ridley, cue) is { } target
                ? new AnimationContinuation<CeresBabySegment>(CeresBabyAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        baby.Playback = new AnimationPlayback<CeresBabySegment>(animation, place);
        if (outcome is not AnimationShown<CeresBabySegment, CeresBabyPose>.Frame shown)
            throw new InvalidOperationException($"The Ceres Baby's animation ended a draw update with {outcome}.");
        baby.Clock.Show(shown.Duration);
        baby.ShownPose = shown.Shown;
        return CeresBabyFrame(baby.ShownPose);
    }

    /// <summary>The frame the draw shows for <paramref name="pose"/>, or none before the Baby's first pose.</summary>
    private static EnemyFrameId? CeresBabyFrame(CeresBabyPose? pose) =>
        pose is { } shown ? CeresBabyFrameDefinitions.ShownFrames.Of(shown) : null;

    /// <summary>The animation the Baby starts while it moves, where it reacts to its motion (#2167).</summary>
    internal const CeresBabySegment MovingAnimation = CeresBabySegment.ExpressiveLoop;

    /// <summary>The animation the Baby starts when it rests silently instead of crying (#2167).</summary>
    internal const CeresBabySegment SilentAnimation = CeresBabySegment.IdleSquish;

    /// <summary>Carries out one cue of the Baby's animation, returning the segment whose animation its owner starts, or null to play on.</summary>
    private CeresBabySegment? ApplyCeresBabyCue(CeresRidleyState ridley, CeresBabyCue cue)
    {
        switch (cue)
        {
            case CeresBabyCue.Cries:
                // While the Baby is falling it always cries and plays on. Otherwise the current random number, read without
                // drawing a new one, makes it jump silently one time in two.
                if (ridley.Baby.VerticalVelocity.IsZero && RidleyRandomDefinitions.CeresBabySilentJump.IsTakenBy(_room.Random.Read()))
                    return SilentAnimation;
                _audio.QueueEnemySound(RoomSound.CeresBabyMetroidCry, priority: SoundRequestPriority.Normal);
                return null;
            case CeresBabyCue.UpdateColors updateColors:
                ridley.Colours.ShowBaby(updateColors.Shade);
                return null;
            case CeresBabyCue.ReactsToMotion:
                return !ridley.Baby.VerticalVelocity.IsZero ? MovingAnimation : null;
            default:
                throw new InvalidOperationException($"Undefined Ceres Baby cue {cue}.");
        }
    }
}
