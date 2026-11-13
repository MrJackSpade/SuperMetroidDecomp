using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The Yapping Maw species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class YappingMawBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The 256ths of a pixel in a whole pixel, the fraction a link's product is counted in.</summary>
    private const int FractionsPerPixel = 256;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.YappingMaw];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new YappingMawEntity(this, slot, common);

    /// <summary>Creates the four auxiliary body-link projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Owns the root sprite object accompanying the body links.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Caps camera movement while Samus is held at the Maw's mouth.</summary>
    private readonly SamusScrollCap _scrollCap;

    /// <summary>Receives the species' end-of-update sound requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Maw AI to room, body-link, sprite-object, and Samus camera services.</summary>
    internal YappingMawBehavior(EnemyProjectileEngine projectiles, RoomSpriteObjects spriteObjects, SamusScrollCap scrollCap, EnemyAudioQueue audio)
    {
        _audio = audio;
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
        _scrollCap = scrollCap;
    }

    /// <summary>Carries out one cue of the Yapping Maw's animations: holding a grabbed Samus at a mouth offset, or the bite sound.</summary>
    private void ApplyYappingMawCue(YappingMawEnemyState state, YappingMawCue cue)
    {
        switch (cue.Kind)
        {
            case YappingMawCueKind.HoldSamusUpRight:
                SetYappingMawHeldOffset(state, directionIndex: 1);
                break;
            case YappingMawCueKind.HoldSamusUpLeft:
                SetYappingMawHeldOffset(state, directionIndex: 7);
                break;
            case YappingMawCueKind.HoldSamusDownRight:
                SetYappingMawHeldOffset(state, directionIndex: 3);
                break;
            case YappingMawCueKind.HoldSamusDownLeft:
                SetYappingMawHeldOffset(state, directionIndex: 5);
                break;
            case YappingMawCueKind.HoldSamusUp:
                SetYappingMawHeldOffset(state, directionIndex: 0);
                break;
            case YappingMawCueKind.HoldSamusDown:
                SetYappingMawHeldOffset(state, directionIndex: 4);
                break;
            case YappingMawCueKind.PlayBiteSound:
                PlayYappingMawAttackSound(state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(YappingMawCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Minimum separation used before moving Samus to the extended mouth.</summary>
    private const int YappingMawGrabSafetyDistance = 32;
    /// <summary>Maximum extension curl admitted by the four-link solver, in whole angle units.</summary>
    private const int YappingMawMaximumCurl = 128;
    /// <summary>Cooldown after Samus is released before the Maw can grab again.</summary>
    private const int YappingMawGrabCooldownFrames = 48;
    /// <summary>Delay after full retraction before Samus input is released.</summary>
    private const int YappingMawRetractedDelayFrames = 64;

    /// <summary>Initializes the Yapping Maw.</summary>
    internal void InitializeYappingMaw(RoomEnemySlot slot)
    {
        var state = new YappingMawEnemyState(slot)
        {
            Function = YappingMawAiFunction.WaitingForSamus,
            Origin = slot.Position.Whole,
            ActivationDistance = slot.SpawnSettings<YappingMawSpawnSettings>().ActivationDistance,
            RetractedDelayTimer = YappingMawRetractedDelayFrames,
        };
        slot.SetSpeciesState(state);

        state.Playback = new(YappingMawAnimations.StartingWith(slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down
            ? YappingMawProgram.AttackingDown
            : YappingMawProgram.AttackingUp), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        // Projectile slots are searched from the last downward, and the body segments are
        // spawned with parameters 3, 2, 1, 0, each recording its slot under its parameter.
        // Allocate in that order, then keep the links in root-to-mouth order.
        for (int parameter = 3; parameter >= 0; parameter--)
        {
            EnemyProjectile? body = _projectiles.AllocateEnemyProjectile();
            if (body is null)
            {
                throw new InvalidDataException(
                    $"Yapping Maw slot {slot.SlotIndex} could not allocate all four body projectiles.");
            }

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                body,
                RoomEnemyProjectileKind.YappingMawBody,
                SpriteGraphics.SpawnedBy(slot));
            body.Position = body.Position.WithWhole(slot.Position.Whole);
            body.InstallAnimation(new YappingMawBodyProjectileAnimation(slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down
                ? YappingMawBodyProjectileProgram.FacingDown
                : YappingMawBodyProjectileProgram.FacingUp));
            state.BodyProjectiles[parameter] = body;
        }

        // The root's kind follows the Maw's attack direction; the Maw holds the root it spawned.
        state.RootSpriteObject = _spriteObjects.SpawnRoomSpriteObject(
            new(slot.Position.X.Whole, CoordinateAxis.Wrap(slot.Position.Y.Whole + (slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down ? -8 : 8))),
            slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down
                ? RoomSpriteObjectKind.YappingMawRootVariantZero
                : RoomSpriteObjectKind.YappingMawRootVariantOne,
            SpriteGraphics.SpawnedBy(slot));
    }

    /// <summary>Runs the Maw's main behaviour and the four post-dispatch link-position writers.</summary>
    private void RunYappingMawMain(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        state.GrabCooldown = state.GrabCooldown.Decrement().Remaining;
        state.IsOffScreen = IsYappingMawPointOffScreen(slot, camera);

        if (samus is null)
        {
            throw new InvalidOperationException(
                "Yapping Maw main AI requires the active Samus actor for targeting.");
        }

        switch (state.Function)
        {
            case YappingMawAiFunction.WaitingForSamus:
                WaitForSamusWithYappingMaw(state, samus);
                break;
            case YappingMawAiFunction.BeginExtension:
                BeginYappingMawExtension(slot, state);
                break;
            case YappingMawAiFunction.ExtendingOrRetracting:
                AdvanceYappingMawExtension(slot, state, samus);
                break;
            case YappingMawAiFunction.RetractedDelay:
                WaitAfterYappingMawRetraction(state, samus);
                break;
            default:
                throw new InvalidDataException(
                    $"Yapping Maw function {state.Function} is not translated.");
        }

        // The mouth actor owns the final point. The four projectiles own root, quarter, half,
        // and three-quarter points of the tongue. The game writes coordinates even
        // though their frame behaviours are empty; the projectile pass advances only their one-frame
        // map selection and permanent sleep step.
        PositionYappingMawBodyProjectile(state, 0, state.Origin);
        PositionYappingMawBodyProjectile(state, 1, YappingMawLinkPosition(state, 0));
        PositionYappingMawBodyProjectile(state, 2, YappingMawLinkPosition(state, 1));
        PositionYappingMawBodyProjectile(state, 3, YappingMawLinkPosition(state, 2));
    }

    /// <summary>Waits for Samus, including the deliberately approximate distance helper.</summary>
    private static void WaitForSamusWithYappingMaw(
        YappingMawEnemyState state,
        SamusState samus)
    {
        (int distance, Bearing bearing) = CalculateYappingMawTarget(
            state.Origin,
            samus.Position.Whole);
        state.DistanceToSamus = distance;

        if (distance < YappingMawGrabSafetyDistance)
        {
            // A target already inside 32 pixels cannot safely trigger an extension and has
            // its grab gate refreshed to 48 every frame. This is why the cooldown must wrap.
            state.GrabCooldown = WrappingCountdown.Start(YappingMawGrabCooldownFrames);
            return;
        }

        if (!state.IsWithinActivationDistance(distance))
            return;

        // The game skips the assignment for a shorter target. Inflating a short
        // distance instead makes the mouth overshoot the position it aimed at.
        if (state.DistanceToSamus >= YappingMawAttackGeometry.MaximumTargetDistance)
            state.DistanceToSamus = YappingMawAttackGeometry.MaximumTargetDistance;

        state.AimAngle = bearing;
        state.Function = YappingMawAiFunction.BeginExtension;
    }

    /// <summary>Runs the one-frame setup function.</summary>
    internal static void BeginYappingMawExtension(RoomEnemySlot slot, YappingMawEnemyState state)
    {
        state.Extension = SignedSixteenSixteen.Zero;
        state.QuadraticSpeedRow = 0;
        state.QuadraticSpeedSign = EnemySpeedSign.Positive;
        state.SegmentRadius = state.DistanceToSamus / 2;
        state.CurlAngle = state.AimAngle.ToTableAngle().ToAngle();
        state.MirroredBend = state.CurlAngle >= RotationAngle.HalfTurn;

        state.Baseline = new Offset16<RoomPixels>(
            CalculateYappingMawX(TrigonometryAngle.HalfTurn, state.SegmentRadius),
            CalculateYappingMawY(TrigonometryAngle.HalfTurn, state.SegmentRadius / 2));

        int direction = AimSector(state.AimAngle);
        YappingMawAim aim = YappingMawAnimationDefinitions.Aims.At(direction);
        state.Aim = aim;
        state.Playback = new(YappingMawAnimations.StartingWith(YappingMawBites.AttackForDirection(aim)), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        state.HeldSamusOffset = YappingMawAttackGeometry.HeldSamusOffset(direction);
        state.Function = YappingMawAiFunction.ExtendingOrRetracting;
    }

    /// <summary>Runs the curved four-link solver and state transition.</summary>
    private void AdvanceYappingMawExtension(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        // The function only runs while the curl's whole part is in [0, YappingMawMaximumCurl]: setup starts it at zero,
        // the clamp below caps it, and the update that carries it negative leaves this function. In that range a
        // quarter of the signed whole part equals the game's two logical right shifts of the word; the check fails
        // loudly if that range is ever violated.
        int bendStep = Ensure.BetweenInclusive(0, YappingMawMaximumCurl, state.Extension.WholePixels) / 4;
        // The bend corrects the eight offsets in one chain, link by link and X before Y: an offset whose correction
        // wraps it past the last pixel of its word moves the next offset in the chain one pixel further.
        int wrappedIntoNext = 0;
        for (int segment = 0; segment < 4; segment++)
        {
            int multiple = bendStep * (segment + 1);
            TrigonometryAngle linkAngle = state.MirroredBend
                ? TrigonometryAngle.HalfTurn.TurnedBySteps(multiple)
                : TrigonometryAngle.HalfTurn.TurnedBySteps(-multiple);

            int rawX = CoordinateAxis.Wrap(CalculateYappingMawX(linkAngle, state.SegmentRadius) - state.Baseline.X);
            int rawY = CoordinateAxis.Wrap(CalculateYappingMawY(linkAngle, state.SegmentRadius / 2) - state.Baseline.Y);

            // The curl rotates a correction whose length is raw X.
            // Passing raw Y here is an attractive cleanup and is wrong; the game's
            // lopsided curve is a direct consequence of reusing raw X for both components.
            int correctionX = CoordinateAxis.Wrap(
                CalculateYappingMawX(state.CurlAngle.ToTrigonometryAngle(), rawX) -
                CalculateYappingMawX(RotationAngle.Zero.ToTrigonometryAngle(), rawX));
            int correctionY = CoordinateAxis.Wrap(
                CalculateYappingMawY(state.CurlAngle.ToTrigonometryAngle(), rawX) -
                CalculateYappingMawY(RotationAngle.Zero.ToTrigonometryAngle(), rawX));
            state.Corrections[segment] = new Offset16<RoomPixels>(
                CoordinateAxis.AsSigned(correctionX), CoordinateAxis.AsSigned(correctionY));
            int segmentX = Corrected(rawX, correctionX);
            int segmentY = Corrected(rawY, correctionY);
            state.SegmentOffsets[segment] = new Offset16<RoomPixels>(
                CoordinateAxis.AsSigned(segmentX), CoordinateAxis.AsSigned(segmentY));
        }

        slot.Position = slot.Position.WithWhole(YappingMawLinkPosition(state, 3));
        AddYappingMawQuadraticSpeed(state);

        if (state.Extension.WholePixels >= 0)
        {
            // A nonnegative whole part minus 128 cannot wrap, so the game's signed difference test is this
            // ordinary comparison.
            if (state.Extension.WholePixels >= YappingMawMaximumCurl)
            {
                state.Extension = SignedSixteenSixteen.FromParts(checked(YappingMawMaximumCurl), 0);
                // The turn-around steps half a row further than the update's own advance: from a positive speed to
                // the same row's negative speed, or from a negative speed on to the next row's positive one.
                if (state.QuadraticSpeedSign == EnemySpeedSign.Negative)
                {
                    state.QuadraticSpeedRow++;
                    state.QuadraticSpeedSign = EnemySpeedSign.Positive;
                }
                else
                {
                    state.QuadraticSpeedSign = EnemySpeedSign.Negative;
                }
            }

            if (state.HasGrabbedSamus)
                PositionSamusInYappingMaw(slot, state, samus);
            return;
        }

        state.Function = YappingMawAiFunction.RetractedDelay;
        state.GrabCooldown = WrappingCountdown.Start(YappingMawGrabCooldownFrames);

        // The six retracted lists are not one per aim. The right and left sectors have
        // dedicated art, while every other sector shares its attack direction's list. Preserve parameter-zero's early-return quirk: on its shared-list branches it
        // skips the final held-Samus placement on this one transition frame.
        bool skipHeldPlacement = false;
        if (slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Up)
        {
            state.Playback = new(YappingMawAnimations.StartingWith(state.Aim switch
            {
                YappingMawAim.Right => YappingMawProgram.CooldownUpRight,
                YappingMawAim.Left => YappingMawProgram.CooldownUpLeft,
                YappingMawAim.Up or YappingMawAim.UpRight or YappingMawAim.DownRight or YappingMawAim.Down or
                    YappingMawAim.DownLeft or YappingMawAim.UpLeft => YappingMawProgram.CooldownUp,
                _ => throw new InvalidOperationException($"Yapping Maw aim {state.Aim} is not an aim."),
            }), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Playback = new(YappingMawAnimations.StartingWith(state.Aim switch
            {
                YappingMawAim.Right => YappingMawProgram.CooldownDownRight,
                YappingMawAim.Left => YappingMawProgram.CooldownDownLeft,
                YappingMawAim.Up or YappingMawAim.UpRight or YappingMawAim.DownRight or YappingMawAim.Down or
                    YappingMawAim.DownLeft or YappingMawAim.UpLeft => YappingMawProgram.CooldownDown,
                _ => throw new InvalidOperationException($"Yapping Maw aim {state.Aim} is not an aim."),
            }), AnimationPlace.StartOfAnimation);
            skipHeldPlacement = state.Aim is not (YappingMawAim.Right or YappingMawAim.Left);
        }
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        if (!skipHeldPlacement && state.HasGrabbedSamus)
            PositionSamusInYappingMaw(slot, state, samus);

        // One offset of the chain, a word, moved by its correction, a word, and by one pixel when the previous offset
        // wrapped; records whether this one wraps in turn.
        int Corrected(int offset, int correction)
        {
            int sum = offset + correction + wrappedIntoNext;
            int wrapped = CoordinateAxis.Wrap(sum);
            wrappedIntoNext = wrapped != sum ? 1 : 0;
            return wrapped;
        }
    }

    /// <summary>Adds the current quadratic speed to the curl, carrying the fraction, then advances the record.</summary>
    internal static void AddYappingMawQuadraticSpeed(YappingMawEnemyState state)
    {
        state.Extension += EnemyQuadraticSpeeds.Row(state.QuadraticSpeedRow).Toward(state.QuadraticSpeedSign);
        state.QuadraticSpeedRow++;
    }

    /// <summary>Waits after retraction, including decrement-underflow expiration on frame 65.</summary>
    private void WaitAfterYappingMawRetraction(
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (state.HasGrabbedSamus)
            PositionSamusInYappingMaw(state.Owner, state, samus);

        state.RetractedDelayTimer--;
        if (state.RetractedDelayTimer >= 0 || samus.DeathSequence.IsActive)
            return;

        samus.Control.Unlock();
        state.HasGrabbedSamus = false;
        state.GrabCooldown = WrappingCountdown.Start(YappingMawGrabCooldownFrames);
        state.RetractedDelayTimer = YappingMawRetractedDelayFrames;
        state.Function = YappingMawAiFunction.WaitingForSamus;
    }

    /// <summary>Runs the observable portions of Samus command three plus holding Samus in the mouth.</summary>
    private void PositionSamusInYappingMaw(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        // Samus command three drops any active grapple;
        // its transitional pose must be committed before the Maw releases input. It normalizes
        // spin-jump/wall-jump bodies. The input handler itself was already replaced by the
        // touch callback; do not force unrelated falling/running poses to standing.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            samus.Grapple.Phase = GrapplePhase.Dropped;
        }
        else
        {
            SamusMovementType movement = SamusPoseMetadata.ReadMovementKind(samus);
            if (movement is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            {
                // Samus's release from the Yapping Maw selects her standing pose from the current direction,
                // then runs the ordinary pose metadata and animation initialization path.
                // Reusing those shared data-backed helpers prevents a Yapping-Maw-only pose
                // approximation from drifting away from every other Samus transition.
                samus.Pose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
                    ? SamusPoseId.FacingLeftNormalPose
                    : SamusPoseId.FacingRightNormalPose;
                SamusPoseMetadata.RefreshCollisionRadii(samus);
                SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
                samus.CommitPoseHistory();
            }
        }

        samus.Position = samus.Position.WithWhole(slot.Position.Whole + state.HeldSamusOffset);
        _scrollCap.CapSamusScrollingSpeed(samus);
    }

    /// <summary>Custom touch callback: grab, never ordinary contact damage.</summary>
    private static void ResolveYappingMawTouch(
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (!state.GrabCooldown.IsNegative || state.HasGrabbedSamus)
            return;

        state.GrabCooldown = WrappingCountdown.Start(0);
        state.HasGrabbedSamus = true;
        samus.Control.LockHandlers();
    }

    /// <summary>
    /// Runs after common shot AI on the Maw the hit struck. Ice releases a living Maw's captive; death additionally
    /// destroys all four body eprojs and the root sprite object, which the killed Maw's state still names.
    /// </summary>
    private static void ResolveYappingMawShotAfterCommon(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState? samus)
    {
        if (slot.Health != 0)
        {
            if (slot.FrozenTimer == 0)
                return;

            ReleaseSamusFromYappingMaw(state, samus);
            return;
        }

        foreach (EnemyProjectile? body in state.BodyProjectiles)
            body?.Delete();
        state.RootSpriteObject?.End();
        ReleaseSamusFromYappingMaw(state, samus);
    }

    /// <summary>Unlocks Samus input after retraction unless her death sequence owns it.</summary>
    private static void ReleaseSamusFromYappingMaw(
        YappingMawEnemyState state,
        SamusState? samus)
    {
        if (samus is not null && !samus.DeathSequence.IsActive)
            samus.Control.Unlock();
        state.HasGrabbedSamus = false;
    }

    /// <summary>Custom frozen tail for all five auxiliary actors.</summary>
    internal static void RunYappingMawFrozen(
        RoomEnemySlot slot,
        YappingMawEnemyState state)
    {
        PictureShading shading = SelectYappingMawFrozenShading(
            slot.Shading,
            slot.FrozenTimer);
        foreach (EnemyProjectile? body in state.BodyProjectiles)
        {
            if (body is not null)
                body.Graphics = body.Graphics.In(shading);
        }

        if (state.RootSpriteObject is not null)
            state.RootSpriteObject.Graphics = state.RootSpriteObject.Graphics.In(shading);
    }

    /// <summary>Flashes the frozen colours against the original colours during the last 89 ticks.</summary>
    private static PictureShading SelectYappingMawFrozenShading(
        PictureShading originalShading,
        int frozenTimer)
    {
        if (frozenTimer == 0)
            return originalShading;

        // The frozen colours flash back to the actor's own on alternating pairs only
        // during the final 89 ticks. The strict signed comparison excludes timer 90.
        if (WrappingCounter.IsBehind(frozenTimer, 90) && !EnemyDrawPhaseDefinitions.FrozenColoursShown.IsSetIn(frozenTimer))
            return originalShading;
        return EnemyShades.FrozenLook;
    }

    /// <summary>Animation callback used by all six held-Samus offset cues.</summary>
    internal static void SetYappingMawHeldOffset(
        YappingMawEnemyState state,
        int directionIndex)
    {
        state.HeldSamusOffset = YappingMawAttackGeometry.HeldSamusOffset(directionIndex);
    }

    /// <summary>Publishes the attack sound only when this Maw is visible.</summary>
    private void PlayYappingMawAttackSound(YappingMawEnemyState state)
    {
        if (!state.IsOffScreen)
            _audio.RequestSoundAtUpdateEnd(EnemySoundSource.YappingMaw, EnemySoundEffectDefinitions.YappingMawAttackSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Moves one of the four required body-link projectiles to its solved point.</summary>
    private static void PositionYappingMawBodyProjectile(
        YappingMawEnemyState state,
        int index,
        Position16<RoomPixels> position)
    {
        EnemyProjectile projectile = state.BodyProjectiles[index] ??
            throw new InvalidOperationException(
                $"Yapping Maw slot {state.Owner.SlotIndex} lost body projectile {index}.");
        projectile.Position = projectile.Position.WithWhole(position);
    }

    /// <summary>The root plus one solved link's signed word offsets, wrapping at sixteen bits.</summary>
    private static Position16<RoomPixels> YappingMawLinkPosition(YappingMawEnemyState state, int segment) =>
        state.Origin + state.SegmentOffsets[segment];

    /// <summary>Checks the Maw against the camera's 256-pixel horizontal and vertical spans.</summary>
    private static bool IsYappingMawPointOffScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        CoordinateAxis.Difference(slot.Position.X.Whole, camera.X) < 0 ||
        CoordinateAxis.AsSigned(camera.X + 256 - slot.Position.X.Whole) < 0 ||
        CoordinateAxis.Difference(slot.Position.Y.Whole, camera.Y) < 0 ||
        CoordinateAxis.AsSigned(camera.Y + 256 - slot.Position.Y.Whole) < 0;

    /// <summary>Calculates the Maw's target, a shared enemy helper only this family consumes.</summary>
    private static YappingMawTarget CalculateYappingMawTarget(
        Position16<RoomPixels> origin,
        Position16<RoomPixels> samus)
    {
        int deltaX = CoordinateAxis.Difference(samus.X, origin.X);
        int absoluteX = Math.Abs(deltaX);
        // The farthest difference's magnitude, 32768, is compared as the game's signed word reads it.
        if (CoordinateAxis.Difference(absoluteX, 255) >= 0)
            return new YappingMawTarget(absoluteX, default);

        int deltaY = CoordinateAxis.Difference(samus.Y, origin.Y);
        int absoluteY = Math.Abs(deltaY);
        if (CoordinateAxis.Difference(absoluteY, 255) >= 0)
            return new YappingMawTarget(absoluteY, default);

        // Both magnitudes are below 255 here, so each short product is below 255 pixels and their sum below 510:
        // neither the products' magnitudes nor their sum reach 32768, where the sum would read as negative.
        TrigonometryAngle positiveQuadrantAngle = EnemyMath.CalculateBearing(new(absoluteX, absoluteY)).ToMirroredTableAngle();
        int distance =
            Math.Abs(EnemyMath.ReadEightBitNegativeSineProduct(positiveQuadrantAngle, absoluteX)) +
            Math.Abs(EnemyMath.ReadEightBitCosineProduct(positiveQuadrantAngle, absoluteY));
        return new YappingMawTarget(distance, EnemyMath.CalculateBearing(new(deltaX, deltaY)));
    }

    /// <summary>
    /// Which of the eight directions, numbered clockwise from up, the aim <paramref name="aim"/> points in: eight equal
    /// sectors, each centred on its direction, so the aim is turned on half a sector before its sector is read. The
    /// bearing's units are read through its mirrored table angle, which counts the same units.
    /// </summary>
    internal static int AimSector(Bearing aim) =>
        aim.ToMirroredTableAngle().ToAngle().TurnedBySteps(EnemyTrigonometryAngleDefinitions.EighthTurn / 2).Sector(8);

    /// <summary>X is the Y helper a quarter turn behind.</summary>
    internal static int CalculateYappingMawX(TrigonometryAngle angle, int length) =>
        CalculateYappingMawY(angle.TurnedBySteps(-EnemyTrigonometryAngleDefinitions.QuarterTurn), length);

    /// <summary>
    /// The offset of a link <paramref name="length"/> along the turn <paramref name="angle"/>, -254 through 254 pixels:
    /// twice the length the short product uses (<see cref="ShortCircleProduct.RadiusInUse"/>) times the sine's whole
    /// part, counted in 256ths of a pixel and rounded down to whole pixels, so a point on the negative side with a fraction
    /// lies a whole pixel further out (#1620: the plain floor of the signed product, which the original's negation and
    /// byte fill compute for every sine and length); its consumers wrap it onto the room's axis.
    /// </summary>
    internal static int CalculateYappingMawY(TrigonometryAngle angle, int length)
    {
        // The game samples the angle reflected below zero, whose sine is this angle's sine reversed.
        int signedSample = -Trigonometry.SignedSixteenBitSine(angle);
        int sampleWhole = UnsignedEightEight.Whole(Math.Abs(signedSample));
        int product = sampleWhole * ShortCircleProduct.RadiusInUse(length);
        return 2 * WholePixelsRoundedDown(signedSample < 0 ? -product : product);
    }

    /// <summary>The whole pixels of <paramref name="fractions"/> 256ths of a pixel, rounded down: a negative fraction reaches the next whole pixel below.</summary>
    private static int WholePixelsRoundedDown(int fractions) =>
        fractions >= 0 ? fractions / FractionsPerPixel : -((-fractions + FractionsPerPixel - 1) / FractionsPerPixel);

    /// <summary>Routes bomb hits through the Maw's shared shot callback.</summary>
    internal static void AddYappingMawBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.YappingMaw &&
                                                       c.SelectedShotAi == EnemyReaction.YappingMawShot)
        {
            AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, c.StruckOccupant.Require<YappingMawEnemyState>(), c.Samus),
        });
    }

    /// <summary>Ports YappingMaw's main AI.</summary>
    internal void RunYappingMawMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunYappingMawMain(
            slot,
            slot.RequireSpeciesState<YappingMawEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances YappingMaw's typed animation.</summary>
    internal void AdvanceYappingMawTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, YappingMawAnimations.Segments, YappingMawFrameDefinitions.ShownFrames.Of,
            static (YappingMawEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyYappingMawCue(state, cue);
                return null;
            });

    /// <summary>Handles frozen release and death cleanup after common shot damage.</summary>
    internal static void AddYappingMawShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.YappingMaw &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.YappingMawShot)
        {
            AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, c.StruckOccupant.Require<YappingMawEnemyState>(), c.Samus),
        });
    }

    /// <summary>Registers contact as a grab that locks Samus without ordinary touch damage.</summary>
    internal static void AddYappingMawTouch(EnemySharedReactions table)
    {
        // The Maw's touch replaces common contact damage entirely. A qualifying overlap transfers Samus input/position
        // ownership to the mouth state machine.
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.YappingMaw &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.YappingMawTouch,
            context => ResolveYappingMawTouch(context.Slot.RequireSpeciesState<YappingMawEnemyState>(), context.Samus));
    }

    /// <summary>Routes the frozen look to all body links and the root sprite object.</summary>
    internal static void AddYappingMawFrozenAi(EnemySharedReactions table) =>
        table.AddFrozen(slot => slot.DefinitionId == EnemyDefinitionId.YappingMaw,
            slot => RunYappingMawFrozen(slot, slot.RequireSpeciesState<YappingMawEnemyState>()));
}
