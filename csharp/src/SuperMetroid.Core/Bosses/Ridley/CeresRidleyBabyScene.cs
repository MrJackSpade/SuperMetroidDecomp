using SuperMetroid.Core.Bosses.Artwork;
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
    /// <summary>Offsets the private Baby/door drawing hook during a room quake.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves the door and Mode-7 wall enemy records beside Ridley.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyBabyScene(EnemyRoomBinding room, RoomEarthquake earthquake, EnemyAudioQueue audio, EnemySlotTable slotTable, EnemyPresentation presentation, RidleyEncounter ridleyEncounter, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _room = room;
        _earthquake = earthquake;
        _audio = audio;
        _slotTable = slotTable;
        _presentation = presentation;
        _ridleyEncounter = ridleyEncounter;
    }

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
                state.BabyVerticalVelocity = 0;
                state.BabyFunction = CeresBabyFunction.Dropped;
                goto case CeresBabyFunction.Dropped;

            case CeresBabyFunction.Dropped:
                state.BabyVerticalVelocity += 8;
                // The falling speed only grows from zero and the fall ends at Y=192, so it is always a
                // nonnegative 8.8 velocity well inside the signed range the integrator accepts.
                state.BabyPosition = state.BabyPosition.WithY(
                    state.BabyPosition.Y.AddEightEightToHighFraction(Ensure.BetweenInclusive(0, ushort.MaxValue, state.BabyVerticalVelocity)));
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

    /// <summary>Ports DrawRidleyTail $A6:DB2A and DrawRidleyWings $A6:DAD8.</summary>
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
            // The tip's sixteen poses split the byte angle into sixteenths.
            int tipIndex = WordWrap.LowByte(
                tip.Angle + state.TailSegments[5].Angle + 8) / 16;
            EnemyFrameId tipFrame = RidleySupplementalVisualDefinitions.TailTipFrames.Of(
                tipIndex);
            DrawRidleyWorldSpritemap(
                sprites,
                new SpriteSource(SpriteSourceKind.RidleyTail, 6),
                state.SpritemapColouring,
                tip,
                tipFrame,
                camera);

            // $A6:DB59-$DBBF walks back from segment five to the base after emitting the
            // tip. Earlier draws win equal-priority overlap, so reversing this visually
            // changes the curl even though every individual piece is otherwise correct.
            for (int index = 5; index >= 0; index--)
                DrawRidleyWorldSpritemap(
                    sprites,
                    new SpriteSource(SpriteSourceKind.RidleyTail, index),
                    state.SpritemapColouring,
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
            new(CoordinateAxis.Wrap(slot.Position.X.Whole - camera.X), CoordinateAxis.Wrap(slot.Position.Y.Whole - camera.Y)), state.SpritemapColouring);
    }

    /// <summary>
    /// Replays the direct branch to <c>$A6:A2E3</c> taken while Ridley's movement and
    /// animation word is zero. This is deliberately separate from the deferred hook below:
    /// the cartridge executes this path inside Ridley's enemy-main call, before the ordinary
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
            _slotTable.Slots[0].DefinitionId != EnemyDefinitionId.RidleyCeres ||
            _ridleyEncounter.Ridley.MovementAnimationEnabled)
            return;

        DrawCeresRidleyBabyAndDoor(sprites, camera);
    }

    /// <summary>
    /// Replays the enemy-graphics-drawn hook installed by <c>$A6:A2E5</c>. The cartridge
    /// installs it only after Ridley's animation word becomes nonzero; it then emits the
    /// same private Baby instruction list after the ordinary enemy layer loop.
    /// </summary>
    public void DrawCeresRidleyPostEnemyHook(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        _room.EnsureLoaded();
        if (_ridleyEncounter.Ridley is null ||
            _slotTable.Slots[0].DefinitionId != EnemyDefinitionId.RidleyCeres ||
            !_ridleyEncounter.Ridley.MovementAnimationEnabled)
            return;

        DrawCeresRidleyBabyAndDoor(sprites, camera);
    }

    /// <summary>Publishes the private Baby frame and hidden door overlay with the native secondary quake offset.</summary>
    private void DrawCeresRidleyBabyAndDoor(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> camera)
    {
        RidleyEnemyState state = _ridleyEncounter.Ridley
            ?? throw new InvalidOperationException("Ceres Ridley Baby draw requires active state.");

        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            if (AdvanceCeresBabyDrawInstruction(state) is { } babyFrame)
            {
                _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.CeresBaby), babyFrame,
                    state.BabyPosition.Whole.ToScreen(camera), SharedSpriteColouring.Common);
            }
        }

        // Enemy one is the ordinary Ridley-room door. Its $F69F instruction hands its
        // drawing to Ridley's scene when the common enemy pass intentionally hides the actor;
        // $A6:A2FA then draws this fixed overlay after the Baby. This is a native cross-slot
        // draw hook, not a replacement for the door actor's own instruction interpreter.
        RoomEnemySlot door = _slotTable.Slots[1];
        if (door.Entity is IDrawnByCeresRidleyScene { HandsDrawingToRidley: true })
        {
            // The private Ceres hook does not call WriteEnemyOAM and therefore owns a
            // second, cartridge-authored quake adjustment at `$A6:A2F2-$A314`. Its `Y`
            // index is the low two bits of the earthquake timer, but the table contains
            // words and the assembly omits an ASL. Preserve that retail byte-index read:
            // bytes 00,00,FC,FF become signed X offsets 0,0,-4,-1.
            int quakeXOffset = CeresDoorQuake.XOffset(_earthquake.EarthquakeTimer.RemainingFrames);
            _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.CeresDoorOverlay), CeresDoorFrameDefinitions.RidleyPrivateOverlayFrame,
                new(CoordinateAxis.Wrap(door.Position.X.Whole - camera.X + quakeXOffset), CoordinateAxis.Wrap(door.Position.Y.Whole - camera.Y)),
                new EncounterSpriteColouring(CeresDoorColourDefinitions.Inactive));
        }
    }

    /// <summary>
    /// Runs the Baby's typed cutscene program one draw update and returns its current spritemap. $A6:DBE7 counts the
    /// elapsed updates up and advances once they reach the pose's duration; the next pose starts at one.
    /// </summary>
    internal EnemyFrameId? AdvanceCeresBabyDrawInstruction(RidleyEnemyState state)
    {
        if (state.BabyPoseDuration != 0 && state.BabyPoseDuration != state.BabyInstructionTimer)
        {
            state.BabyInstructionTimer++;
            return CeresBabyFrame(state.BabyCurrentPose);
        }

        AnimationProgramCursor<CeresBabyProgram> cursor = state.BabyProgram;
        WrappingCountdown counter = WrappingCountdown.Start(0);
        AnimationProgramAdvance<CeresBabyPose> advance = CeresBabyAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, _) => ApplyCeresBabyCue(state, cue));
        state.BabyProgram = cursor;
        if (advance is not AnimationProgramAdvance<CeresBabyPose>.Show shown)
            throw new InvalidOperationException($"The Ceres Baby's program ended a draw update with {advance}.");
        state.BabyPoseDuration = shown.Duration;
        state.BabyInstructionTimer = 1;
        state.BabyCurrentPose = shown.Frame;
        return CeresBabyFrame(state.BabyCurrentPose);
    }

    /// <summary>The frame the draw shows for <paramref name="pose"/>, or none before the Baby's first pose.</summary>
    private static EnemyFrameId? CeresBabyFrame(CeresBabyPose? pose) =>
        pose is { } shown ? CeresBabyFrameDefinitions.ShownFrames.Of(shown) : null;

    /// <summary>Carries out one cue of the Baby's program, returning the program a branch continues at, or null to run on.</summary>
    private CeresBabyProgram? ApplyCeresBabyCue(RidleyEnemyState state, CeresBabyCue cue)
    {
        switch (cue)
        {
            case CeresBabyCue.CryOrGoTo cryOrGoTo:
                // $A6:BFC9: while the baby is falling ($880C nonzero) it always cries and
                // continues. Otherwise it samples the live RNG word's low bit without
                // calling GenerateRandomNumber; a set bit jumps silently ($A6:BFD5).
                if (state.BabyVerticalVelocity == 0 && (_room.Random.Read() & 1) != 0)
                    return cryOrGoTo.Target;
                _audio.QueueEnemySound(Library3Sound.CeresBabyMetroidCry, priority: SoundRequestPriority.Normal);
                return null;
            case CeresBabyCue.UpdateColors updateColors:
                if (_bossArtwork.CeresRidleyColors is { } babyColors)
                    babyColors.ApplyBaby(_presentation.Colours, updateColors.Shade);
                else throw new InvalidOperationException(
                    "Ceres Ridley requires installed Baby Metroid colors.");
                return null;
            case CeresBabyCue.GoToIfMoving goToIfMoving:
                return state.BabyVerticalVelocity != 0 ? goToIfMoving.Target : null;
            default:
                throw new InvalidOperationException($"Undefined Ceres Baby cue {cue}.");
        }
    }

    /// <summary>Publishes a Ridley component's world frame at its screen position for the current camera.</summary>
    private void DrawRidleyWorldSpritemap(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        SpriteColouring colouring,
        RidleyTailSegment segment,
        EnemyFrameId frame,
        Position16<RoomPixels> camera)
    {
        Position16<ScreenPixels> screen = segment.Position.ToScreen(camera);
        _presentation.PublishEncounterFrame(sprites, source, frame, screen, colouring);
    }
}
