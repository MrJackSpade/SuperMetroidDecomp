using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's grounded pose transitions: turns, aim, running and stopping, running into a wall, moonwalk and knockback pose collision.</summary>
internal static class SamusGroundedPoseTransitions
{
    /// <summary>
    /// Installs the fallback pose of an aimed crouching turn when every input is
    /// released. The definition's fallback is left-facing crouching for all six poses,
    /// so the turn is abandoned for left-facing crouch art regardless of its direction.
    /// Because the pose changes, the prospective pose's step skips the stop; only the pose setup runs.
    /// </summary>
    public static void TurnAimedCrouchingForNoTransition(SamusState samus, SamusPoseId targetPose)
    {
        if (!SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose))
            throw new InvalidOperationException($"Aimed crouching turn fallback requires a turning-while-jumping crouching turn, not {samus.Pose}.");
        if (!SamusPoseMetadata.IsRightFacingCrouchingPose(targetPose) && !SamusPoseMetadata.IsLeftFacingCrouchingPose(targetPose))
            throw new InvalidDataException($"Aimed crouching turn {samus.Pose} names non-crouching fallback {targetPose}.");
        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Aimed crouching turn release");
    }

    /// <summary>
    /// Stops Samus's momentum when her input chooses no new pose
    /// from a turn and the pose definition retains the current pose. The three turning movement types
    /// all stop her when her input chooses none. This stops reverse acceleration without
    /// restarting the unfinished turn animation or erasing velocity.
    /// </summary>
    public static void TurnForNoTransition(SamusState samus, SamusMovementType movementType)
    {
        if (movementType is not (SamusMovementType.TurningOnGround or
            SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling))
            throw new InvalidOperationException($"Turn fallback requires a turning movement type, not {movementType}.");
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
    }

    /// <summary>
    /// Folds the extra run speed into the base speed and starts the turn, for the grounded, aerial and spin-turn
    /// setups. Both speeds stay a few pixels an update, so their sum is a plain add, its fraction carrying into the
    /// whole pixels.
    /// </summary>
    internal static void FoldExtraRunSpeedIntoBaseAndStartTurn(SamusState samus)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.BaseSpeed += speed.ExtraRunSpeed;
        speed.ExtraRunSpeed = default;
        speed.AccelerationMode = SamusAccelerationMode.Turning;
    }

    /// <summary>
    /// Applies a same-facing arm-body transition within the standing
    /// family and the running family. Besides aimed bodies, this includes the
    /// real gun-extended running horizontal-fire poses.
    /// </summary>
    /// <remarks>
    /// All admitted poses have radius 21. If both endpoints are running, the transition keeps the running animation's
    /// current frame and timer; this matters when firing midway through the ten-frame run cycle. A transition between
    /// movement types initializes frame zero normally. Movement type changes take effect on
    /// the following frame because pose transitions occur after movement.
    /// </remarks>
    public static void ApplyGroundedAimTransition(SamusState samus, SamusPoseId targetPose)
    {
        bool sourceRight = SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsRightFacingRunningPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose);
        bool targetRight = SamusPoseMetadata.IsRightFacingStandingPose(targetPose) || SamusPoseMetadata.IsRightFacingRunningPose(targetPose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(targetPose);
        bool sourceLeft = SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose);
        bool targetLeft = SamusPoseMetadata.IsLeftFacingStandingPose(targetPose) || SamusPoseMetadata.IsLeftFacingRunningPose(targetPose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(targetPose);
        bool sameRightFamily = sourceRight && targetRight;
        bool sameLeftFamily = sourceLeft && targetLeft;
        bool sameRightCrouch = SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) &&
            SamusPoseMetadata.IsRightFacingCrouchingPose(targetPose);
        bool sameLeftCrouch = SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose) &&
            SamusPoseMetadata.IsLeftFacingCrouchingPose(targetPose);
        if (!sameRightFamily && !sameLeftFamily && !sameRightCrouch && !sameLeftCrouch)
        {
            // The input transitions never feed a facing reversal through this arm-only
            // transition seam; reversal has its own turn-pose initialization path.
            throw new InvalidOperationException(
                $"Grounded aim transition {samus.Pose} -> {targetPose} is outside this same-facing pose family.");
        }
        if (!SamusPoseMetadata.IsGroundedAimPose(samus.Pose) && !SamusPoseMetadata.IsGroundedAimPose(targetPose) &&
            !SamusPoseMetadata.IsGunExtendedPose(samus.Pose) && !SamusPoseMetadata.IsGunExtendedPose(targetPose))
        {
            throw new InvalidOperationException(
                "Grounded arm transition requires an aimed or gun-extended source/target pose.");
        }

        SamusPoseId sourcePose = samus.Pose;
        bool preservesRunningAnimation =
            (SamusPoseMetadata.IsRightFacingRunningPose(sourcePose) && SamusPoseMetadata.IsRightFacingRunningPose(targetPose)) ||
            (SamusPoseMetadata.IsLeftFacingRunningPose(sourcePose) && SamusPoseMetadata.IsLeftFacingRunningPose(targetPose));
        if (!preservesRunningAnimation)
        {
            // This is the pose setup's ordinary target installation. Replacing Pose
            // directly would omit the radius refresh and the new animation's start.
            ApplySimpleGroundedPoseChange(samus, sourcePose, targetPose, "Grounded arm");
            return;
        }

        // From a running pose to a running pose the animation keeps its frame, timer and delays: every live running arm
        // pose plays the same animation, so the current one is already exact and must not be rebound or reset here.
        int oldRadius = samus.Kinematics.Radius.Y;
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (samus.Kinematics.Radius.Y != oldRadius)
        {
            throw new InvalidDataException(
                $"Running arm pose {targetPose} unexpectedly changed radius {oldRadius} -> {samus.Kinematics.Radius.Y}.");
        }
    }

    /// <summary>
    /// Chooses the wall-stop pose by the direction Samus aims, of her ten. The input pose
    /// may be the current running pose after a killed-X-speed collision or the prospective
    /// running pose tested by the one-pixel arm-pump probe.
    /// </summary>
    private static SamusPoseId SelectRanIntoWallPose(SamusPoseId sourcePose)
    {
        return SamusPoseMetadata.ReadAim(sourcePose) is SamusPoseAim.Aiming { Direction: var direction } &&
            SamusAimedPoseChoiceDefinitions.WallStops.TryGetValue(direction, out SamusPoseId wallStop)
            ? wallStop
            : throw new InvalidDataException(
                $"Pose {sourcePose} aims in no shot direction ({SamusPoseMetadata.ReadAim(sourcePose)}).");
    }

    /// <summary>
    /// Checks whether the prospective pose runs into a wall.
    /// </summary>
    /// <remarks>
    /// A current running collision maps the current pose immediately. Otherwise, only a
    /// prospective running pose is interesting: the game first probes solid enemies one pixel
    /// ahead in the CURRENT pose direction and maps the prospective pose without moving when
    /// one is hit. Otherwise it moves Samus that pixel, retains the move when clear, and maps
    /// the prospective pose's shot direction when a block stops it. The retained move is the
    /// game's arm pump.
    /// </remarks>
    public static SamusPoseId? CheckProspectiveRunningPoseForWall(
        SamusState samus,
        RoomLevelData level,
        SamusPoseId? prospectivePose,
        bool currentXSpeedKilledByBlock,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);

        if (currentXSpeedKilledByBlock && SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Running)
            return SelectRanIntoWallPose(samus.Pose);

        if (prospectivePose is not { } target ||
            SamusPoseMetadata.ReadMovementType(target) != SamusMovementType.Running)
            return null;

        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        if (samus.Kinematics.InteractiveEnemies.Count != 0)
        {
            // Samus's solid-enemy collision probes one whole pixel ahead,
            // moving in her facing direction.
            SamusCollisionDirection direction = facingLeft
                ? SamusCollisionDirection.Left
                : SamusCollisionDirection.Right;
            SolidEnemyCollisionResult enemyProbe = SamusSolidEnemyCollision.Probe(
                samus.Kinematics, samus.Kinematics.InteractiveEnemies, direction, UnsignedSixteenSixteen.OnePixel);
            samus.Kinematics.RecordSolidEnemyCollision(direction, enemyProbe.Enemy);
            if (enemyProbe.Collided)
                return SelectRanIntoWallPose(target);
        }

        SignedSixteenSixteen onePixelForward = SignedSixteenSixteen.OnePixel.Toward(facingLeft ? HorizontalDirection.Left : HorizontalDirection.Right);
        BlockMoveResult onePixelProbe = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = onePixelForward, AlignToSlopeAfterMovement = false });
        return onePixelProbe.Collided
            ? SelectRanIntoWallPose(target)
            : null;
    }

    /// <summary>
    /// Installs the pose selected when the wall check finds a block wall. This deliberately
    /// accepts standing/running/landing sources as well as an existing wall-stop source:
    /// the check can test a proposed run before that run is installed, and changing aim
    /// while still pressing into the wall proposes another running pose for the same probe.
    /// A moonwalk that proposes a run is probed too: the probe moves one pixel along the
    /// moonwalk pose's facing, but the wall pose is picked from the proposed run's
    /// direction, so a moonwalk facing right can stop in the left-facing wall pose.
    /// </summary>
    public static void ApplyRanIntoWallPoseChange(SamusState samus, SamusPoseId targetPose)
    {
        bool moonwalking = SamusPoseMetadata.IsMoonwalkingPose(samus.Pose);
        bool rightSource = SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsRightFacingRunningPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(samus.Pose) || SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose) || moonwalking;
        bool leftSource = SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose) || moonwalking;
        bool rightRoute = rightSource &&
            (SamusPoseMetadata.IsRightFacingRanIntoWallPose(targetPose) || targetPose == SamusPoseId.StandingAimUpRightPose);
        bool leftRoute = leftSource &&
            (SamusPoseMetadata.IsLeftFacingRanIntoWallPose(targetPose) || targetPose == SamusPoseId.StandingAimUpLeftPose);
        if (!rightRoute && !leftRoute)
        {
            // Only block-backed ran-into-wall poses call this exact setup. Treat a
            // different source/target pair as misuse of the specialized operation.
            throw new InvalidOperationException(
                $"Ran-into-wall pose change {samus.Pose} -> {targetPose} is not a block-backed route.");
        }

        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Ran into wall");
    }

    /// <summary>
    /// Applies the moonwalk option gate and stable-pose changes surrounding
    /// the moonwalking pose setup.
    /// </summary>
    /// <remarks>
    /// Standing input transitions always publish a moonwalk candidate when Shoot and the
    /// backward direction are held. The settings value decides what that candidate
    /// means: enabled retains the moonwalk pose; disabled substitutes the ordinary standing
    /// turn art. Once active, aim changes and the direct return to forward running are
    /// ordinary radius-21 pose installations with no invented velocity adjustment.
    /// </remarks>
    public static void ApplyMoonwalkPoseChange(
        SamusState samus,
        SamusPoseId targetPose,
        bool moonwalkEnabled)
    {
        bool sourceStandingRight = SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose);
        bool sourceStandingLeft = SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose);
        bool targetVisualRight = SamusPoseMetadata.IsMoonwalkingFacingRightPose(targetPose);
        bool targetVisualLeft = SamusPoseMetadata.IsMoonwalkingFacingLeftPose(targetPose);
        bool entering =
            (sourceStandingRight && targetVisualRight) ||
            (sourceStandingLeft && targetVisualLeft);

        if (entering && !moonwalkEnabled)
        {
            // A landing takes the standing pose's input transitions, so a backward direction can choose the Moonwalk
            // pose before the option is tested; with Moonwalk off, the landing turns as standing does. The way the
            // candidate pose faces decides: the right-facing moonwalk poses travel left and become the right-to-left
            // turn; the left-facing ones travel right and become left-to-right.
            ApplyGroundedTurn(samus,
                targetVisualRight ? SamusPoseId.TurningRightToLeftPose : SamusPoseId.TurningLeftToRightPose);
            return;
        }

        bool sameVisualFamily =
            (SamusPoseMetadata.IsMoonwalkingFacingRightPose(samus.Pose) && targetVisualRight) ||
            (SamusPoseMetadata.IsMoonwalkingFacingLeftPose(samus.Pose) && targetVisualLeft);
        bool exitsToForwardRun =
            (SamusPoseMetadata.IsMoonwalkingFacingRightPose(samus.Pose) && targetPose == SamusPoseId.MovingRightNormalPose) ||
            (SamusPoseMetadata.IsMoonwalkingFacingLeftPose(samus.Pose) && targetPose == SamusPoseId.MovingLeftNormalPose);
        bool exitsToStandingFallback =
            (SamusPoseMetadata.IsMoonwalkingFacingRightPose(samus.Pose) && SamusPoseMetadata.IsRightFacingStandingPose(targetPose)) ||
            (SamusPoseMetadata.IsMoonwalkingFacingLeftPose(samus.Pose) && SamusPoseMetadata.IsLeftFacingStandingPose(targetPose));
        if ((!entering || !moonwalkEnabled) && !sameVisualFamily &&
            !exitsToForwardRun && !exitsToStandingFallback)
        {
            // The predicates above cover every stable moonwalk route;
            // any other route means the request chose the wrong pose change.
            throw new InvalidOperationException(
                $"Moonwalk pose change {samus.Pose} -> {targetPose} is not a stable route.");
        }

        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Moonwalk");
    }

    /// <summary>
    /// Applies the moonwalk turn when a stable moonwalk pose reverses while Jump is held.
    /// The six requested moonwalk turn-jump poses are not airborne yet: they retain the grounded
    /// turn movement while their three visible frames play.
    /// </summary>
    public static void ApplyMoonwalkTurnToSpinJump(SamusState samus, SamusPoseId targetPose)
    {
        // A moonwalk aims only level or diagonally; any other aim means the pose state is inconsistent.
        SamusPoseId expectedTarget = SamusPoseMetadata.ReadAimDirection(samus) is { } direction &&
            SamusAimedPoseChoiceDefinitions.MoonwalkTurnToSpinJumps.TryGetValue(direction, out SamusPoseId turnJump)
            ? turnJump
            : throw new InvalidDataException(
                $"Moonwalk aim {SamusPoseMetadata.ReadAim(samus)} is invalid for a stable moonwalk pose.");
        SamusPoseId inputTransitionTarget = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.MoonwalkTurnToSpinJumpLeftPose : SamusPoseId.MoonwalkTurnToSpinJumpRightPose;
        if (!SamusPoseMetadata.IsMoonwalkingPose(samus.Pose) || targetPose != expectedTarget && targetPose != inputTransitionTarget)
        {
            // Input requests the un-aimed turn; the pose setup then substitutes
            // the old muzzle's aimed turn, which is as valid a request.
            throw new InvalidOperationException(
                $"Moonwalk turn/jump {samus.Pose} -> {targetPose} is not one of the game's routes.");
        }

        // Keep the old muzzle direction before starting the turn's pose. The next input phase
        // uses the handed shot to admit the turning HUD producer and forcibly release charge.
        SamusTransitionShot sourceShot = SamusTransitionShot.FromPose(samus);
        FoldExtraRunSpeedIntoBaseAndStartTurn(samus);
        ApplySimpleGroundedPoseChange(samus, samus.Pose, expectedTarget, "Moonwalk turn/jump");
        samus.TransitionShot.Hand(sourceShot);
    }

    /// <summary>
    /// Applies the ordinary grounded start-moving transition from the normal standing pose,
    /// <see cref="SamusPoseId.FacingRightNormalPose"/> or <see cref="SamusPoseId.FacingLeftNormalPose"/>, to running the
    /// same way.
    /// </summary>
    public static void ApplyStandingToRunning(SamusState samus) =>
        ApplySimpleGroundedPoseChange(samus, FacingHerWay(samus, SamusPoseId.FacingRightNormalPose),
            FacingHerWay(samus, SamusPoseId.MovingRightNormalPose), "Standing to running");

    /// <summary>
    /// Applies the no-button fallback from the normal running pose, <see cref="SamusPoseId.MovingRightNormalPose"/> or
    /// <see cref="SamusPoseId.MovingLeftNormalPose"/>, to standing the same way, after ordinary running momentum has
    /// decelerated to zero.
    /// </summary>
    public static void ApplyRunningToStanding(SamusState samus)
    {
        ApplySimpleGroundedPoseChange(samus, FacingHerWay(samus, SamusPoseId.MovingRightNormalPose),
            FacingHerWay(samus, SamusPoseId.FacingRightNormalPose), "Running to standing");

        // The stop response (SamusMomentumResponse.Stop) cancels the running momentum, which ends the boost but leaves her
        // extra run speed intact: the following standing movement pass consumes that final no-base-speed displacement
        // before clearing every X-motion value.
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
    }

    /// <summary>The right-facing pose <paramref name="rightFacing"/>, mirrored when Samus faces left.</summary>
    private static SamusPoseId FacingHerWay(SamusState samus, SamusPoseId rightFacing) =>
        SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? SamusPoseMirror.Of(rightFacing) : rightFacing;

    /// <summary>
    /// Starts Samus running the way she faces from standing, a landing or a wall stop, whatever her aim or firing: the
    /// running pose's ordinary installation, which starts its animation. A landing's movement has already cleared her
    /// momentum this update; the run begins accelerating on the next.
    /// </summary>
    public static void StartRunning(SamusState samus, SamusPoseId targetPose)
    {
        bool right = (SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose) ||
                SamusPoseMetadata.IsRightFacingRanIntoWallPose(samus.Pose)) &&
            targetPose == SamusPoseId.MovingRightNormalPose;
        bool left = (SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose) ||
                SamusPoseMetadata.IsLeftFacingRanIntoWallPose(samus.Pose)) &&
            targetPose == SamusPoseId.MovingLeftNormalPose;
        if (!right && !left)
        {
            throw new InvalidOperationException(
                $"Running start {samus.Pose} -> {targetPose} is not one the game makes.");
        }

        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Running start");
    }

    /// <summary>
    /// Applies the grounded turn initialization. Input transitions
    /// first publish the generic standing turn; the setup reads the previous pose's aim
    /// (<see cref="SamusAimedPoseChoiceDefinitions.StandingTurns"/>) and may replace it with an aimed standing turn.
    /// </summary>
    public static void ApplyGroundedTurn(SamusState samus, SamusPoseId targetPose, GameplayAction controllerInput = GameplayAction.None)
    {
        // The two front-view poses take an unusual route.
        // They face neither way, so they cannot honestly be classified as either
        // a right-facing or left-facing source. The game instead skips the shot-direction
        // lookup entirely when the previous pose is forward-facing, retaining the generic
        // standing turn selected by that front-view pose's input transitions.
        bool wasForwardFacing = SamusPoseMetadata.IsForwardFacingPose(samus.Pose);

        // Crouching input transitions publish the crouching turn directly, whereas every admitted
        // standing, running and landing pose's transitions publish the standing turn. The setup still uses
        // the previous SamusMovementType.Crouching to choose the crouched turn that keeps her aim.
        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        bool wasCrouching = sourceMovementType == SamusMovementType.Crouching;
        bool wasMoonwalking = sourceMovementType == SamusMovementType.Moonwalking;

        // The shot reads the PREVIOUS moonwalk pose, not the selected turn pose.
        // The source aim is kept now so its projectile direction is still available after
        // ApplySimpleGroundedPoseChange replaces `Pose` below.
        SamusTransitionShot? moonwalkSourceShot = wasMoonwalking
            ? SamusTransitionShot.FromPose(samus)
            : null;

        bool rightSource = SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsRightFacingRunningPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkingFacingRightPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose);
        bool leftSource = SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkingFacingLeftPose(samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose);
        bool turnsLeft = targetPose == (wasCrouching
            ? SamusPoseId.TurningRightToLeftCrouchingPose
            : SamusPoseId.TurningRightToLeftPose) && rightSource;
        bool turnsRight = targetPose == (wasCrouching
            ? SamusPoseId.TurningLeftToRightCrouchingPose
            : SamusPoseId.TurningLeftToRightPose) && leftSource;
        bool leavesForwardView = wasForwardFacing &&
            targetPose is SamusPoseId.TurningRightToLeftPose or SamusPoseId.TurningLeftToRightPose;
        if (!turnsLeft && !turnsRight && !leavesForwardView)
        {
            throw new InvalidOperationException(
                $"Grounded turn {samus.Pose} -> {targetPose} is not one the game makes.");
        }

        // The turn's jump check reads held Jump, not a fresh press. Releasing Shoot from a buffered
        // Moonwalk selects a generic turn in the input transitions, then reaches this same
        // turn/jump setup without a new Jump edge.
        if (wasMoonwalking && (controllerInput & GameplayAction.Jump) != GameplayAction.None)
        {
            ApplyMoonwalkTurnToSpinJump(samus, turnsLeft
                ? SamusPoseId.MoonwalkTurnToSpinJumpLeftPose : SamusPoseId.MoonwalkTurnToSpinJumpRightPose);
            return;
        }

        // The turn reads the PREVIOUS pose before it starts the final turn's pose.
        // A previous crouching movement selects the crouched turns by aim; every other admitted source selects
        // the standing ones. Both cover Samus's ten aims, but her two straight-down aims belong to
        // the compact straight-down family, which this transition does not handle.
        SamusPoseId selectedTurnPose;
        if (wasForwardFacing)
        {
            // A forward-facing Samus keeps the generic turn the input transitions chose.
            selectedTurnPose = targetPose;
        }
        else
        {
            SamusPoseAim aim = SamusPoseMetadata.ReadAim(samus);
            IReadOnlyDictionary<SamusProjectileDirection, SamusPoseId> turns = wasCrouching
                ? SamusAimedPoseChoiceDefinitions.CrouchingTurns
                : SamusAimedPoseChoiceDefinitions.StandingTurns;
            // Grounded and crouched poses never aim straight down, nor outside their eight aims.
            selectedTurnPose = aim is SamusPoseAim.Aiming { Direction: var direction } && turns.TryGetValue(direction, out SamusPoseId turn)
                ? turn
                : throw new InvalidDataException($"{(wasCrouching ? "Crouched" : "Grounded")} turn aim {aim} is invalid.");
        }

        if (!wasForwardFacing &&
            ((turnsLeft && !SamusPoseMetadata.IsRightToLeftGroundTurnPose(selectedTurnPose)) ||
             (turnsRight && !SamusPoseMetadata.IsLeftToRightGroundTurnPose(selectedTurnPose))))
        {
            throw new InvalidOperationException(
                $"Grounded turn source {samus.Pose} has direction metadata inconsistent with target {targetPose}.");
        }

        // The turn consumes the extra run speed and selects SamusAccelerationMode.Turning. The direction selector reads
        // that mode as “move opposite the NEW facing direction” while the speed calculation decelerates, which is how
        // reversal preserves old momentum.
        FoldExtraRunSpeedIntoBaseAndStartTurn(samus);

        ApplySimpleGroundedPoseChange(samus, samus.Pose, selectedTurnPose, "Grounded turn");

        if (wasMoonwalking)
        {
            // The handed shot makes the turning HUD step treat the one-frame turning
            // state as shooting. The beam producer later fires it in this source pose's
            // direction once Samus's shot count leaves room for another shot.
            samus.TransitionShot.Hand(moonwalkSourceShot);
        }
    }

    /// <summary>
    /// Runs the pose update's shared expansion check before the interrupted hurt
    /// pose is installed. Crouch-to-hurt must keep the larger body above its floor.
    /// </summary>
    internal static bool TryResolveKnockbackPoseCollision(
        SamusState samus,
        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome outcome = samus.ResolveLargerPoseCollision(
            level, targetPose, frameCounter, roomObjects, out int adjustment);
        if (outcome != LargerPoseCollisionOutcome.Allowed)
        {
            if (outcome == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, adjustment);
        return true;
    }

    /// <summary>
    /// Installs one already-validated grounded pose and runs the common pose setup's
    /// metadata/radius work, initializing frame zero only for a changed pose.
    /// </summary>
    internal static void ApplySimpleGroundedPoseChange(
        SamusState samus,
        SamusPoseId expectedPose,
        SamusPoseId targetPose,
        string transitionName,
        bool refreshRadius = true)
    {
        if (samus.Pose != expectedPose)
        {
            throw new InvalidOperationException(
                $"{transitionName} requires pose {expectedPose}, not {samus.Pose}.");
        }

        samus.Pose = targetPose;
        // Aerial turn completion runs the pose change without the radius update; its source body
        // remains active until the next input phase, including compact down-aim targets.
        if (refreshRadius)
            SamusPoseMetadata.RefreshCollisionRadii(samus);
        // The animation keeps its frame and timer when the ordinary pose is
        // unchanged. A prospective run rejected by the wall probe can resolve
        // back to the current wall-stop pose on every held-input frame.
        if (expectedPose != targetPose)
            SamusPresentation.InitializeAnimation(samus, SamusPoseTransitions.StandingPoseInitialFrame(expectedPose, targetPose));
    }
}
