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
    public static void TurnAimedCrouchingForNoTransition(SamusState samus, SamusPoseId targetPose) =>
        ApplySimpleGroundedPoseChange(samus, targetPose);

    /// <summary>
    /// Stops Samus's momentum when her input chooses no new pose
    /// from a turn and the pose definition retains the current pose. The three turning movement types
    /// all stop her when her input chooses none. This stops reverse acceleration without
    /// restarting the unfinished turn animation or erasing velocity.
    /// </summary>
    public static void TurnForNoTransition(SamusState samus)
    {
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
    /// All admitted poses have the standing radius. If both endpoints are running, the transition keeps the running animation's
    /// current frame and timer; this matters when firing midway through the run cycle. A transition between
    /// movement types initializes frame zero normally. Movement type changes take effect on
    /// the following frame because pose transitions occur after movement.
    /// </remarks>
    public static void ApplyGroundedAimTransition(SamusState samus, SamusPoseId targetPose)
    {
        // The input transitions never feed a facing reversal through this arm-only
        // transition seam; reversal has its own turn-pose initialization path.
        SamusPoseId sourcePose = samus.Pose;
        bool preservesRunningAnimation =
            (SamusPoseMetadata.IsRightFacingRunningPose(sourcePose) && SamusPoseMetadata.IsRightFacingRunningPose(targetPose)) ||
            (SamusPoseMetadata.IsLeftFacingRunningPose(sourcePose) && SamusPoseMetadata.IsLeftFacingRunningPose(targetPose));
        if (!preservesRunningAnimation)
        {
            // This is the pose setup's ordinary target installation. Replacing Pose
            // directly would omit the radius refresh and the new animation's start.
            ApplySimpleGroundedPoseChange(samus, targetPose);
            return;
        }

        // From a running pose to a running pose the animation keeps its frame, timer and delays: every live running arm
        // pose plays the same animation, so the current one is already exact and must not be rebound or reset here.
        samus.ChangePoseKeepingAnimation(targetPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
    }

    /// <summary>
    /// Chooses the wall-stop pose by the direction Samus aims, of her ten. The input pose
    /// may be the current running pose after a killed-X-speed collision or the prospective
    /// running pose tested by the one-pixel arm-pump probe.
    /// </summary>
    private static SamusPoseId SelectRanIntoWallPose(SamusPoseId sourcePose)
    {
        return SamusPoseMetadata.ReadAim(sourcePose) is SamusPoseAim.Aiming { Direction: var direction }
            ? SamusAimedPoseChoiceDefinitions.WallStops[direction]
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
        SamusTerrain terrain,
        SamusPoseId? prospectivePose,
        bool currentXSpeedKilledByBlock)
    {
        Ensure.NotNull(terrain);

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
        BlockMoveResult onePixelProbe = SamusBlockCollision.MoveHorizontal(terrain, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = onePixelForward, AlignToSlopeAfterMovement = false });
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
    public static void ApplyRanIntoWallPoseChange(SamusState samus, SamusPoseId targetPose) =>
        ApplySimpleGroundedPoseChange(samus, targetPose);

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

        ApplySimpleGroundedPoseChange(samus, targetPose);
    }

    /// <summary>
    /// Applies the moonwalk turn when a stable moonwalk pose reverses while Jump is held.
    /// The six requested moonwalk turn-jump poses are not airborne yet: they retain the grounded
    /// turn movement while their three visible frames play.
    /// </summary>
    /// <remarks>
    /// Input requests the un-aimed turn jump; the pose setup substitutes the old muzzle's aimed turn jump.
    /// </remarks>
    public static void ApplyMoonwalkTurnToSpinJump(SamusState samus)
    {
        // A moonwalk aims only level or diagonally; any other aim means the pose state is inconsistent.
        SamusPoseId expectedTarget = SamusPoseMetadata.ReadAimDirection(samus) is { } direction &&
            SamusAimedPoseChoiceDefinitions.MoonwalkTurnToSpinJumps.TryGetValue(direction, out SamusPoseId turnJump)
            ? turnJump
            : throw new InvalidDataException(
                $"Moonwalk aim {SamusPoseMetadata.ReadAim(samus)} is invalid for a stable moonwalk pose.");
        // Keep the old muzzle direction before starting the turn's pose. The next input phase
        // uses the handed shot to admit the turning HUD producer and forcibly release charge.
        SamusTransitionShot sourceShot = SamusTransitionShot.FromPose(samus);
        FoldExtraRunSpeedIntoBaseAndStartTurn(samus);
        ApplySimpleGroundedPoseChange(samus, expectedTarget);
        samus.TransitionShot.Hand(sourceShot);
    }

    /// <summary>
    /// Applies the ordinary grounded start-moving transition from the normal standing pose,
    /// <see cref="SamusPoseId.FacingRightNormalPose"/> or <see cref="SamusPoseId.FacingLeftNormalPose"/>, to running the
    /// same way.
    /// </summary>
    public static void ApplyStandingToRunning(SamusState samus) =>
        ApplySimpleGroundedPoseChange(samus, FacingHerWay(samus, SamusPoseId.MovingRightNormalPose));

    /// <summary>
    /// Applies the no-button fallback from the normal running pose, <see cref="SamusPoseId.MovingRightNormalPose"/> or
    /// <see cref="SamusPoseId.MovingLeftNormalPose"/>, to standing the same way, after ordinary running momentum has
    /// decelerated to zero.
    /// </summary>
    public static void ApplyRunningToStanding(SamusState samus)
    {
        ApplySimpleGroundedPoseChange(samus, FacingHerWay(samus, SamusPoseId.FacingRightNormalPose));

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
    public static void StartRunning(SamusState samus, SamusPoseId targetPose) =>
        ApplySimpleGroundedPoseChange(samus, targetPose);

    /// <summary>
    /// Applies the grounded turn initialization. Input transitions
    /// first publish the generic standing turn; the setup reads the previous pose's aim
    /// (<see cref="SamusAimedPoseChoiceDefinitions.StandingTurns"/>) and may replace it with an aimed standing turn.
    /// </summary>
    public static void ApplyGroundedTurn(SamusState samus, SamusPoseId targetPose, GameplayAction controllerInput = GameplayAction.None)
    {
        // The two front-view poses take an unusual route. They face neither way, so they cannot honestly be classified as
        // either a right-facing or left-facing source. The game instead skips the shot-direction lookup entirely when the
        // previous pose is forward-facing, retaining the generic standing turn selected by that front-view pose's input
        // transitions (SamusGroundTurnSource.FacingForward).
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

        // The turn's jump check reads held Jump, not a fresh press. Releasing Shoot from a buffered
        // Moonwalk selects a generic turn in the input transitions, then reaches this same
        // turn/jump setup without a new Jump edge.
        if (wasMoonwalking && (controllerInput & GameplayAction.Jump) != GameplayAction.None)
        {
            ApplyMoonwalkTurnToSpinJump(samus);
            return;
        }

        // The turn reads the PREVIOUS pose before it starts the final turn's pose. A forward-facing Samus keeps the generic
        // turn the input transitions chose; every other pose that turns on the ground aims (bound as the poses load), and a
        // previous crouching movement selects the crouched turn keeping that aim, every other the standing one. A
        // straight-down aim turns as its diagonal-down neighbour.
        SamusPoseId selectedTurnPose = SamusGroundTurnSources.Of(samus.Pose).Turn(targetPose, wasCrouching
            ? SamusAimedPoseChoiceDefinitions.CrouchingTurns
            : SamusAimedPoseChoiceDefinitions.StandingTurns);

        // The turn consumes the extra run speed and selects SamusAccelerationMode.Turning. The direction selector reads
        // that mode as “move opposite the NEW facing direction” while the speed calculation decelerates, which is how
        // reversal preserves old momentum.
        FoldExtraRunSpeedIntoBaseAndStartTurn(samus);

        ApplySimpleGroundedPoseChange(samus, selectedTurnPose);

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
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome outcome = samus.ResolveLargerPoseCollision(
            terrain, targetPose, frameCounter, out int adjustment);
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
    /// Installs the grounded pose a setup chose in place of her current one and runs the common pose setup's
    /// metadata/radius work, initializing frame zero only for a changed pose.
    /// </summary>
    internal static void ApplySimpleGroundedPoseChange(
        SamusState samus,
        SamusPoseId targetPose,
        bool refreshRadius = true)
    {
        SamusPoseId sourcePose = samus.Pose;
        // The animation keeps its frame and timer when the ordinary pose is
        // unchanged. A prospective run rejected by the wall probe can resolve
        // back to the current wall-stop pose on every held-input frame.
        if (sourcePose == targetPose)
            samus.ChangePoseKeepingAnimation(targetPose);
        else
            samus.ChangePose(targetPose, SamusPoseTransitions.StandingPoseInitialFrame(sourcePose, targetPose));
        // Aerial turn completion runs the pose change without the radius update; its source body
        // remains active until the next input phase, including compact down-aim targets.
        if (refreshRadius)
            SamusPoseMetadata.RefreshCollisionRadii(samus);
    }
}
