using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Baby Metroid's container in Ceres Ridley's room, and the sprites and door drawn around the fight.</summary>
[RoomLifetime]
internal sealed class CeresRidleyBabyScene
{
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Offsets the scene's own Baby and door drawing during a room quake.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
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

    /// <summary>The gravity the dropped Baby's fall gains each update.</summary>
    private static readonly SignedEightEight CeresBabyFallGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(8);

    /// <summary>Keeps the Baby at Ridley's arm anchor, falls independently, then follows the grasp point on retrieval.</summary>
    internal static void TickCeresBaby(RoomEnemySlot slot, RidleyEnemyState state)
    {
        switch (state.BabyFunction)
        {
            case CeresBabyFunction.CarriedInArms:
                // Before the fake retreat, the Baby remains at Ridley's original left-hand
                // anchor. This function is also why the retrieval target begins coherent
                // even though the Baby is not a separate RoomEnemySlot.
                state.BabyPosition = state.BabyPosition.WithWhole(slot.Position.Whole + CeresBabyArmsAnchor);
                return;

            case CeresBabyFunction.Drop:
                state.BabyPosition = state.BabyPosition.WithFractionY(0);
                state.BabyVerticalVelocity = SignedEightEight.Zero;
                state.BabyFunction = CeresBabyFunction.Dropped;
                goto case CeresBabyFunction.Dropped;

            case CeresBabyFunction.Dropped:
                // The falling speed only grows from zero and the fall ends at Y=192, so it stays a few pixels an
                // update.
                state.BabyVerticalVelocity += CeresBabyFallGravity;
                state.BabyPosition = state.BabyPosition.WithY(state.BabyPosition.Y.MovedBy(state.BabyVerticalVelocity));
                if (CoordinateAxis.Difference(state.BabyPosition.Y.Whole, 192) >= 0)
                {
                    state.BabyPosition = state.BabyPosition.WithWholeY(192);
                    state.BabyFunction = CeresBabyFunction.Idle;
                }
                return;

            case CeresBabyFunction.CarriedInFeet:
                // Once the hand rectangles overlap, the Baby follows the grasp point for
                // the real retreat and disappears with Ridley at the status-one handoff.
                state.BabyPosition = state.BabyPosition.WithWhole(slot.Position.Whole + CeresBabyFeetAnchor);
                return;

            case CeresBabyFunction.Idle:
                return;

            case CeresBabyFunction.None:
                throw new InvalidOperationException("The Ceres Baby Metroid has no behavior function.");

            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresBabyFunction)} {state.BabyFunction}.");
        }
    }

    /// <summary>The Ceres Baby's anchor in Ridley's arms before the fake retreat.</summary>
    internal static Offset16<RoomPixels> CeresBabyArmsAnchor => new(-16, 22);

    /// <summary>The Ceres Baby's anchor in Ridley's feet once retrieved, also the grasp point the retrieval compares.</summary>
    internal static Offset16<RoomPixels> CeresBabyFeetAnchor => new(14, 66);

    /// <summary>Draws Ridley's tail and wings.</summary>
    internal void DrawRidleySupplementalSprites(
        SpriteFrameBuffer sprites,
        RoomEnemySlot slot,
        Position16<RoomPixels> camera)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(slot);
        if ((slot.DefinitionId == EnemyDefinitionId.RidleyCeres && _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped) ||
            !state.MovementAnimationEnabled)
            return;

        if (state.TailSegments.Length == 7)
        {
            RidleyTailSegment tip = state.TailSegments[6];
            // The tip's pose follows its direction turned by the link before it.
            EnemyFrameId tipFrame = RidleySupplementalVisualDefinitions.TailTipFrames.At(
                tip.Angle.Direction.TurnedBy(state.TailSegments[5].Angle.Direction));
            DrawRidleyWorldFrame(
                sprites,
                new SpriteSource(SpriteSourceKind.RidleyTail, 6),
                state.TailAndWingsShading,
                tip,
                tipFrame,
                camera);

            // The tail draw walks back from segment five to the base after emitting the
            // tip. Earlier draws win equal-priority overlap, so reversing this visually
            // changes the curl even though every individual piece is otherwise correct.
            for (int index = 5; index >= 0; index--)
                DrawRidleyWorldFrame(
                    sprites,
                    new SpriteSource(SpriteSourceKind.RidleyTail, index),
                    state.TailAndWingsShading,
                    state.TailSegments[index],
                    RidleySupplementalVisualDefinitions.SegmentFrames.Of(index),
                    camera);
        }

        if (state.FacingDirection == RidleyFacing.Turning)
            return;
        int wingFrameIndex = (state.FacingDirection == RidleyFacing.Left ? 0 : 10) + state.WingFrame;
        EnemyFrameId wingFrame = RidleySupplementalVisualDefinitions.WingFrames.Of(
            wingFrameIndex);
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.RidleyWings), wingFrame,
            new(CoordinateAxis.Wrap(slot.Position.X.Whole - camera.X), CoordinateAxis.Wrap(slot.Position.Y.Whole - camera.Y)), state.TailAndWingsShading);
    }

    /// <summary>
    /// Replays the direct branch to the Baby and door draw taken while Ridley's movement and
    /// animation are off. This is deliberately separate from the later draw below:
    /// the game runs this path inside Ridley's main AI, before the ordinary
    /// drawing queues, so the Baby appears with the room and before Ridley's composite.
    /// </summary>
    public void DrawCeresRidleyImmediateBabyAndDoor(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        _room.EnsureLoaded();
        // The shared Ridley state also exists in Lower Norfair. Only Ceres
        // Ridley's initializer installs this private Baby/door draw path.
        if (_ridleyEncounter.Ridley is null ||
            _ridleyEncounter.Parts.Body.DefinitionId != EnemyDefinitionId.RidleyCeres ||
            _ridleyEncounter.Ridley.MovementAnimationEnabled)
            return;

        DrawCeresRidleyBabyAndDoor(sprites, camera);
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
        _room.EnsureLoaded();
        if (_ridleyEncounter.Ridley is null ||
            _ridleyEncounter.Parts.Body.DefinitionId != EnemyDefinitionId.RidleyCeres ||
            !_ridleyEncounter.Ridley.MovementAnimationEnabled)
            return;

        DrawCeresRidleyBabyAndDoor(sprites, camera);
    }

    /// <summary>Publishes the private Baby frame and hidden door overlay with the secondary quake offset.</summary>
    private void DrawCeresRidleyBabyAndDoor(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera)
    {
        RidleyEnemyState state = _ridleyEncounter.Ridley
            ?? throw new InvalidOperationException("Ceres Ridley Baby draw requires active state.");

        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            if (AdvanceCeresBabyDrawAnimation(state) is { } babyFrame)
            {
                _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.CeresBaby), babyFrame,
                    state.BabyPosition.Whole.ToScreen(camera), EnemyShades.HurtFlashLook);
            }
        }

        // The room's door hands its drawing to Ridley's scene in its hiding step, when the common
        // enemy pass intentionally hides the actor; the Baby and door draw then draws this fixed overlay
        // after the Baby. This draws the door for it; it does not replace the door actor's own animation.
        RoomEnemySlot door = _ridleyEncounter.Parts.CeresRoomDoor;
        if (door.Entity is IDrawnByCeresRidleyScene { HandsDrawingToRidley: true })
        {
            // The scene draws the door itself rather than through the common enemy drawing, so it applies the door's
            // own earthquake shake, which moves only the drawn door.
            int quakeXOffset = CeresDoorQuake.XOffset(_earthquake.RemainingFrames);
            _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.CeresDoorOverlay), CeresDoorFrameDefinitions.RidleyPrivateOverlayFrame,
                new(CoordinateAxis.Wrap(door.Position.X.Whole - camera.X + quakeXOffset), CoordinateAxis.Wrap(door.Position.Y.Whole - camera.Y)),
                // The overlay shows its own standard colours; the door actor's own looks never reach it (#1512).
                PictureShading.Standard);
        }
    }

    /// <summary>
    /// Runs the Baby's typed cutscene animation one draw update and returns its current frame. The Baby's draw counts the
    /// elapsed updates up and advances once they reach the pose's duration; the next pose starts at one.
    /// </summary>
    internal EnemyFrameId? AdvanceCeresBabyDrawAnimation(RidleyEnemyState state)
    {
        if (state.BabyPoseDuration != 0 && state.BabyPoseDuration != state.BabyFrameHold)
        {
            state.BabyFrameHold++;
            return CeresBabyFrame(state.BabyCurrentPose);
        }

        Animation<CeresBabySegment> animation = state.BabyPlayback.Animation;
        AnimationPlace place = state.BabyPlayback.Place;
        // The Baby's animations count no repeats, so nothing reads or writes this counter.
        WrappingCountdown counter = WrappingCountdown.Start(0);
        AnimationShown<CeresBabySegment, CeresBabyPose> outcome = CeresBabyAnimations.Segments.PlayToNextFrame(ref animation, ref place, ref counter,
            cue => ApplyCeresBabyCue(state, cue) is { } target
                ? new AnimationContinuation<CeresBabySegment>(CeresBabyAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.BabyPlayback = new AnimationPlayback<CeresBabySegment>(animation, place);
        if (outcome is not AnimationShown<CeresBabySegment, CeresBabyPose>.Frame shown)
            throw new InvalidOperationException($"The Ceres Baby's animation ended a draw update with {outcome}.");
        state.BabyPoseDuration = shown.Duration;
        state.BabyFrameHold = 1;
        state.BabyCurrentPose = shown.Shown;
        return CeresBabyFrame(state.BabyCurrentPose);
    }

    /// <summary>The frame the draw shows for <paramref name="pose"/>, or none before the Baby's first pose.</summary>
    private static EnemyFrameId? CeresBabyFrame(CeresBabyPose? pose) =>
        pose is { } shown ? CeresBabyFrameDefinitions.ShownFrames.Of(shown) : null;

    /// <summary>Carries out one cue of the Baby's animation, returning the segment whose animation a branch continues in, or null to play on.</summary>
    private CeresBabySegment? ApplyCeresBabyCue(RidleyEnemyState state, CeresBabyCue cue)
    {
        switch (cue)
        {
            case CeresBabyCue.CryOrGoTo cryOrGoTo:
                // While the baby is falling (its falling flag set) it always cries and
                // continues. Otherwise it reads the current random number without
                // drawing a new one; an odd number makes it jump silently.
                if (state.BabyVerticalVelocity.IsZero && RidleyRandomDefinitions.CeresBabySilentJump.IsSetIn(_room.Random.Read()))
                    return cryOrGoTo.Target;
                _audio.QueueEnemySound(RoomSound.CeresBabyMetroidCry, priority: SoundRequestPriority.Normal);
                return null;
            case CeresBabyCue.UpdateColors updateColors:
                state.Colours.ShowBaby(updateColors.Shade);
                return null;
            case CeresBabyCue.GoToIfMoving goToIfMoving:
                return !state.BabyVerticalVelocity.IsZero ? goToIfMoving.Target : null;
            default:
                throw new InvalidOperationException($"Undefined Ceres Baby cue {cue}.");
        }
    }

    /// <summary>Publishes a Ridley component's world frame at its screen position for the current camera.</summary>
    private void DrawRidleyWorldFrame(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        PictureShading colouring,
        RidleyTailSegment segment,
        EnemyFrameId frame,
        Position16<RoomPixels> camera)
    {
        Position16<ScreenPixels> screen = segment.Position.ToScreen(camera);
        _presentation.PublishEncounterFrame(sprites, source, frame, screen, colouring);
    }
}
