using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The Yapping Maw species owner: its per-room state, behaviour and the interactions it registers.</summary>
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

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
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
                SetYappingMawHeldOffset(state, YappingMawAim.UpRight);
                break;
            case YappingMawCueKind.HoldSamusUpLeft:
                SetYappingMawHeldOffset(state, YappingMawAim.UpLeft);
                break;
            case YappingMawCueKind.HoldSamusDownRight:
                SetYappingMawHeldOffset(state, YappingMawAim.DownRight);
                break;
            case YappingMawCueKind.HoldSamusDownLeft:
                SetYappingMawHeldOffset(state, YappingMawAim.DownLeft);
                break;
            case YappingMawCueKind.HoldSamusUp:
                SetYappingMawHeldOffset(state, YappingMawAim.Up);
                break;
            case YappingMawCueKind.HoldSamusDown:
                SetYappingMawHeldOffset(state, YappingMawAim.Down);
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
    /// <summary>The curl, in whole angle units, per angle step each link of the chain bends by: the bend step is a quarter of the curl.</summary>
    private const int CurlPerBendStep = 4;
    /// <summary>Cooldown after Samus is released before the Maw can grab again.</summary>
    private const int YappingMawGrabCooldownFrames = 48;
    /// <summary>Delay after full retraction before Samus input is released.</summary>
    private const int YappingMawRetractedDelayFrames = 64;

    /// <summary>Initializes the Yapping Maw.</summary>
    internal void InitializeYappingMaw(RoomEnemySlot slot)
    {
        var state = new YappingMawEnemyState(slot)
        {
            Action = YappingMawAction.WaitingForSamus,
            Origin = slot.Position.Whole,
            ActivationDistance = slot.SpawnSettings<YappingMawSpawnSettings>().ActivationDistance,
            RetractedDelayTimer = YappingMawRetractedDelayFrames,
        };
        slot.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(slot, YappingMawAnimations.StartingWith(slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down
            ? YappingMawSegment.AttackingDown
            : YappingMawSegment.AttackingUp));

        // Projectile slots are searched from the last downward, and the body segments are
        // spawned as links 3, 2, 1, 0, each recording its slot under its link.
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
                EnemyProjectileBehavior.Still,
                SpriteGraphics.SpawnedBy(slot));
            body.Position = body.Position.WithWhole(slot.Position.Whole);
            body.InstallAnimation(new YappingMawBodyProjectileAnimation(slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down
                ? YappingMawBodyProjectileSegment.FacingDown
                : YappingMawBodyProjectileSegment.FacingUp));
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

        switch (state.Action)
        {
            case YappingMawAction.WaitingForSamus:
                WaitForSamusWithYappingMaw(state, samus);
                break;
            case YappingMawAction.BeginExtension:
                BeginYappingMawExtension(slot, state);
                break;
            case YappingMawAction.ExtendingOrRetracting:
                AdvanceYappingMawExtension(slot, state, samus);
                break;
            case YappingMawAction.RetractedDelay:
                WaitAfterYappingMawRetraction(state, samus);
                break;
            default:
                throw new InvalidDataException(
                    $"Yapping Maw has no behaviour for action {state.Action}.");
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
        state.Action = YappingMawAction.BeginExtension;
    }

    /// <summary>Runs the one-frame setup action.</summary>
    internal static void BeginYappingMawExtension(RoomEnemySlot slot, YappingMawEnemyState state)
    {
        state.Extension = SignedSixteenSixteen.Zero;
        state.SpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        state.QuadraticSpeedSign = EnemySpeedSign.Positive;
        state.SegmentRadius = state.DistanceToSamus / 2;
        state.CurlAngle = state.AimAngle.ToTableAngle().ToAngle();
        state.MirroredBend = state.CurlAngle >= RotationAngle.HalfTurn;

        state.Baseline = new Offset16<RoomPixels>(
            CalculateYappingMawX(TrigonometryAngle.HalfTurn, state.SegmentRadius),
            CalculateYappingMawY(TrigonometryAngle.HalfTurn, state.SegmentRadius / 2));

        YappingMawAim aim = AimOf(state.AimAngle);
        state.Aim = aim;
        state.Playback = EnemyAnimationPlayback.Start(slot, YappingMawAnimations.StartingWith(YappingMawBites.AttackForDirection(aim)));

        state.HeldSamusOffset = YappingMawAttackGeometry.HeldSamusOffset(aim);
        state.Action = YappingMawAction.ExtendingOrRetracting;
    }

    /// <summary>
    /// The angle steps each link of the chain bends by for a curl of <paramref name="curl"/> whole angle units. The
    /// extension only runs while the curl is 0 through <see cref="YappingMawMaximumCurl"/>: setup starts it at zero, the
    /// clamp caps it, and the update that carries it negative leaves the extension; a curl outside that range is refused.
    /// </summary>
    internal static int BendStepOf(int curl) => Ensure.BetweenInclusive(0, YappingMawMaximumCurl, curl) / CurlPerBendStep;

    /// <summary>Runs the curved four-link solver and state transition.</summary>
    private void AdvanceYappingMawExtension(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        int bendStep = BendStepOf(state.Extension.WholePixels);
        // The bend corrects the eight offsets in one chain, link by link and X before Y (YappingMawBendChain).
        var chain = new YappingMawBendChain();
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
            int segmentX = chain.Corrected(rawX, correctionX);
            int segmentY = chain.Corrected(rawY, correctionY);
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
                // The turn-around steps half a mark further than the update's own advance: from a positive speed to
                // the same mark's negative speed, or from a negative speed on to the next mark's positive one.
                if (state.QuadraticSpeedSign == EnemySpeedSign.Negative)
                {
                    state.SpeedCurvePlace += QuadraticSpeedCurvePlace.OneMark;
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

        state.Action = YappingMawAction.RetractedDelay;
        state.GrabCooldown = WrappingCountdown.Start(YappingMawGrabCooldownFrames);

        // The six retracted animations are not one per aim. The right and left sectors have
        // dedicated art, while every other sector shares its attack direction's animation.
        VerticalDirection reach = slot.SpawnSettings<YappingMawSpawnSettings>().Attack;
        if (reach == VerticalDirection.Up)
        {
            state.Playback = new(YappingMawAnimations.StartingWith(state.Aim switch
            {
                YappingMawAim.Right => YappingMawSegment.CooldownUpRight,
                YappingMawAim.Left => YappingMawSegment.CooldownUpLeft,
                YappingMawAim.Up or YappingMawAim.UpRight or YappingMawAim.DownRight or YappingMawAim.Down or
                    YappingMawAim.DownLeft or YappingMawAim.UpLeft => YappingMawSegment.CooldownUp,
                _ => throw new InvalidOperationException($"Yapping Maw aim {state.Aim} is not an aim."),
            }), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Playback = new(YappingMawAnimations.StartingWith(state.Aim switch
            {
                YappingMawAim.Right => YappingMawSegment.CooldownDownRight,
                YappingMawAim.Left => YappingMawSegment.CooldownDownLeft,
                YappingMawAim.Up or YappingMawAim.UpRight or YappingMawAim.DownRight or YappingMawAim.Down or
                    YappingMawAim.DownLeft or YappingMawAim.UpLeft => YappingMawSegment.CooldownDown,
                _ => throw new InvalidOperationException($"Yapping Maw aim {state.Aim} is not an aim."),
            }), AnimationPlace.StartOfAnimation);
        }
        slot.AnimationClock.Restart();

        if (state.HasGrabbedSamus &&
            YappingMawRetractionHold.CarriesHeldSamus(reach, bitStraightAcross: state.Aim is YappingMawAim.Right or YappingMawAim.Left))
            PositionSamusInYappingMaw(slot, state, samus);
    }

    /// <summary>Adds the current quadratic speed to the curl, carrying the fraction, then advances its place along the curve.</summary>
    internal static void AddYappingMawQuadraticSpeed(YappingMawEnemyState state)
    {
        state.Extension += state.SpeedCurvePlace.Speed.Toward(state.QuadraticSpeedSign);
        state.SpeedCurvePlace += QuadraticSpeedCurvePlace.OneMark;
    }

    /// <summary>Waits after retraction, including the expiry past zero on frame 65.</summary>
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
        state.Action = YappingMawAction.WaitingForSamus;
    }

    /// <summary>Runs the observable portions of unspinning Samus plus holding Samus in the mouth.</summary>
    private void PositionSamusInYappingMaw(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        // Unspinning Samus drops any active grapple;
        // its transitional pose must be committed before the Maw releases input. It normalizes
        // spin-jump/wall-jump bodies. The input handler itself was already replaced by the
        // touch reaction; do not force unrelated falling/running poses to standing.
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

    /// <summary>Custom touch reaction: grab, never ordinary contact damage.</summary>
    private static void ResolveYappingMawTouch(
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (!state.GrabCooldown.IsNegative || state.HasGrabbedSamus)
            return;

        state.GrabCooldown = WrappingCountdown.Start(0);
        state.HasGrabbedSamus = true;
        samus.Control.LockOutPlayer();
    }

    /// <summary>
    /// Runs after common shot AI on the Maw the hit struck. Ice releases a living Maw's captive; death additionally
    /// destroys all four body-link projectiles and the root sprite object, which the killed Maw's state still names.
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

    /// <summary>Flashes the frozen colours against the Maw's own colours during the last 89 ticks.</summary>
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

    /// <summary>Animation behaviour used by all six held-Samus offset cues.</summary>
    internal static void SetYappingMawHeldOffset(
        YappingMawEnemyState state,
        YappingMawAim aim)
    {
        state.HeldSamusOffset = YappingMawAttackGeometry.HeldSamusOffset(aim);
    }

    /// <summary>Publishes the attack sound only when this Maw is visible.</summary>
    private void PlayYappingMawAttackSound(YappingMawEnemyState state)
    {
        if (!state.IsOffScreen)
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.YappingMawAttackSound, priority: SoundRequestPriority.Normal);
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

    /// <summary>The root plus one solved link's signed offsets, wrapping around the coordinate range.</summary>
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

    /// <summary>
    /// The axis distance, in pixels, from which the Maw's target distance is that axis's distance alone, kept exact
    /// (#1627): nearer on both axes, the distance is the sum of two short products, which the game forms only below it,
    /// and the Maw's reach follows whichever it measures.
    /// </summary>
    private const int ShortProductReach = 255;

    /// <summary>Calculates the Maw's target, a shared enemy helper only this family consumes.</summary>
    private static YappingMawTarget CalculateYappingMawTarget(
        Position16<RoomPixels> origin,
        Position16<RoomPixels> samus)
    {
        int deltaX = CoordinateAxis.Difference(samus.X, origin.X);
        int absoluteX = Math.Abs(deltaX);
        // The farthest difference's magnitude, 32768, is compared as the game's signed comparison reads it.
        if (CoordinateAxis.Difference(absoluteX, ShortProductReach) >= 0)
            return new YappingMawTarget(absoluteX, default);

        int deltaY = CoordinateAxis.Difference(samus.Y, origin.Y);
        int absoluteY = Math.Abs(deltaY);
        if (CoordinateAxis.Difference(absoluteY, ShortProductReach) >= 0)
            return new YappingMawTarget(absoluteY, default);

        // Both magnitudes are below the reach here, so each short product is below 255 pixels and their sum below 510:
        // neither the products' magnitudes nor their sum reach 32768, where the sum would read as negative.
        TrigonometryAngle positiveQuadrantAngle = EnemyMath.CalculateBearing(new(absoluteX, absoluteY)).ToMirroredTableAngle();
        int distance =
            Math.Abs(EnemyMath.ReadEightBitNegativeSineProduct(positiveQuadrantAngle, absoluteX)) +
            Math.Abs(EnemyMath.ReadEightBitCosineProduct(positiveQuadrantAngle, absoluteY));
        return new YappingMawTarget(distance, EnemyMath.CalculateBearing(new(deltaX, deltaY)));
    }

    /// <summary>
    /// Which of the eight aims the bearing <paramref name="aim"/> points in: eight equal sectors in the aims' clockwise
    /// order from up, each centred on its direction, so the aim is turned on half a sector before its sector is read. The
    /// bearing's units are read through its mirrored table angle, which counts the same units.
    /// </summary>
    internal static YappingMawAim AimOf(Bearing aim) => YappingMawAnimationDefinitions.Aims.At(
        aim.ToMirroredTableAngle().ToAngle().TurnedBySteps(EnemyTrigonometryAngleDefinitions.EighthTurn / 2).Sector(8));

    /// <summary>X is the Y helper a quarter turn behind.</summary>
    internal static int CalculateYappingMawX(TrigonometryAngle angle, int length) =>
        CalculateYappingMawY(angle.TurnedBySteps(-EnemyTrigonometryAngleDefinitions.QuarterTurn), length);

    /// <summary>
    /// The offset of a link <paramref name="length"/> along the turn <paramref name="angle"/>, -254 through 254 pixels:
    /// twice the length the short product uses (<see cref="ShortCircleProduct.RadiusInUse"/>) times the sine's whole
    /// part, counted in 256ths of a pixel and rounded down to whole pixels, so a point on the negative side with a fraction
    /// lies a whole pixel further out (#1620: the plain floor of the signed product, which the game's negation and
    /// sign fill compute for every sine and length); its consumers wrap it onto the room's axis.
    /// </summary>
    internal static int CalculateYappingMawY(TrigonometryAngle angle, int length)
    {
        // The game samples the angle reflected below zero, whose sine is this angle's sine reversed.
        int signedSample = -Trigonometry.FineSignedSine(angle);
        int sampleWhole = UnsignedEightEight.Whole(Math.Abs(signedSample));
        int product = sampleWhole * ShortCircleProduct.RadiusInUse(length);
        return 2 * WholePixelsRoundedDown(signedSample < 0 ? -product : product);
    }

    /// <summary>The whole pixels of <paramref name="fractions"/> 256ths of a pixel, rounded down: a negative fraction reaches the next whole pixel below.</summary>
    private static int WholePixelsRoundedDown(int fractions) =>
        fractions >= 0 ? fractions / FractionsPerPixel : -((-fractions + FractionsPerPixel - 1) / FractionsPerPixel);

    /// <summary>Routes bomb hits through the Maw's shared shot reaction.</summary>
    internal static EnemyBombReaction YappingMawBombReaction { get; } = new()
    {
        AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, c.StruckOccupant.Require<YappingMawEnemyState>(), c.Samus),
    };

    /// <summary>Runs YappingMaw's main AI.</summary>
    internal void RunYappingMawMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunYappingMawMain(
            slot,
            slot.RequireSpeciesState<YappingMawEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances YappingMaw's typed animation.</summary>
    internal void AdvanceYappingMawTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, YappingMawAnimations.Segments,
            static (YappingMawEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyYappingMawCue(state, cue);
                return null;
            });

    /// <summary>Handles frozen release and death cleanup after common shot damage.</summary>
    internal static EnemyShotReaction YappingMawShotReaction { get; } = new()
    {
        AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, c.StruckOccupant.Require<YappingMawEnemyState>(), c.Samus),
    };

    /// <summary>Runs contact as a grab that locks Samus without ordinary touch damage.</summary>
    internal static void ReactToYappingMawTouch(EnemyTouchContext context) =>
        // The Maw's touch replaces common contact damage entirely. A qualifying overlap transfers Samus input/position
        // ownership to the mouth state machine.
        ResolveYappingMawTouch(context.Slot.RequireSpeciesState<YappingMawEnemyState>(), context.Samus);
}
