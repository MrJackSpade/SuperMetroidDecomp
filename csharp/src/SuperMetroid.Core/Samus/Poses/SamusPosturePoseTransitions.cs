using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's posture transitions: crouching and standing, crouch jumps, morphing and unmorphing, and the morph ball's landings, bounces, spring-ball jumps and walk-offs.</summary>
internal static class SamusPosturePoseTransitions
{
    /// <summary>Applies the crouch input transitions' direct same-facing exits (right-facing crouches to right-facing standing, or left-facing to left-facing) after larger-pose collision admits the standing body.</summary>
    /// <param name="samus">The Samus this reads.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="targetPose">Final normal standing-right or standing-left pose matching the current crouch facing; animated stand-up and cross-facing routes are not accepted.</param>
    /// <param name="frameCounter">The frame counter supplied to changed-pose block reactions.</param>
    /// <remarks>These input transitions bypass the animated aimed stand-up poses. Accepted expansion applies the whole-pixel Y adjustment but retains the crouching radius until Samus's next input phase, matching the larger-radius check's previous-pose radius publication.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="terrain"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source and target are not a same-facing route from a crouch to a final standing pose.</exception>
    public static void ApplyDirectCrouchToStandingTransition(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        // This route is only the direct exit from crouching to normal standing.
        // Animated stand-ups and cross-facing changes have separate setups.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
                terrain,
                targetPose,
                frameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // An aimed crouch is not restored when the attempted standing body
            // is boxed in: Samus takes the ordinary stable crouch for her facing.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        samus.Pose = targetPose;
        // Explicitly publishes the previous pose's radius while probing the
        // enlargement, and this direct transition does not set the radius afterward.
        // The ordinary input phase publishes the standing radius at the start of the next frame.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Applies the crouch's neutral jump transition in this order:
    /// enlarge the crouching radius to the jump's with the larger-radius check, initialise normal-jumping movement, run the
    /// crouch-only Y adjustment, then start the jump.
    /// </summary>
    /// <remarks>
    /// Tests the literal previous pose, not merely the crouching movement type. Therefore
    /// only ordinary crouching subtracts its jump lift (<see cref="SamusPoseCollisionDefinitions.CrouchJumpLift"/>); aimed crouches still
    /// jump, but retain only the collision resolver's bottom-alignment adjustment.
    /// </remarks>
    public static void ApplyCrouchJumpTransition(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        // Only the two neutral jump-transition poses enter this transition; aimed
        // crouches deliberately reuse the same facing-selected target.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
                terrain,
                targetPose,
                frameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        samus.Pose = targetPose;
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);

        // The input phase owns the new radius next frame; collision correction and jump setup
        // below must not publish it during this frame's pose-commit stage.
        // The jump transition lowers Samus after pose initialization and collision but before the jump starts. It moves
        // only her current position and records no previous position.
        if (sourcePose is SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingLeftPose)
            samus.Kinematics.Position -= new Offset16<RoomPixels>(0, SamusPoseCollisionDefinitions.CrouchJumpLift);

        SamusPresentation.InitializeAnimation(samus);
        SamusAerialMovement.InitializeJump(samus);
    }

    /// <summary>
    /// Applies the ordinary or aimed crouch-start/stand-start transition poses,
    /// including the posture change's bottom alignment and the larger-radius collision branch
    /// of the pose-change collision. The admitted target set is exactly
    /// the unaimed and aimed crouching and standing transitions.
    /// </summary>
    public static void ApplyPostureTransition(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        // The crouching transitions start a crouch from standing, running, a moonwalk, a wall stop or a landing; the
        // standing transitions start a stand from a crouch. Each keeps the source's facing.
        bool startsCrouching = SamusPoseMetadata.IsCrouchingTransitionPose(targetPose);
        if (startsCrouching)
        {
            // The crouching transition samples the boost stage before later
            // posture movement can cancel running momentum. A crouch at boost stage four therefore
            // stores a shinespark charge (ShinesparkDefinitions.StoredShineFrames) even though the following stable crouch has
            // no horizontal Speed Booster state of its own.
            if (samus.Shinespark.TryStoreFromSpeedBooster(samus.HorizontalSpeed.SpeedBoostCounter))
            {
                // The successful store takes over Samus's special suit colouring
                // and timer even if an interrupted Flash was keeping them alive.
                samus.CrystalFlash.RelinquishSuitColouring();
                // Reserve Mode keeps only X-Ray's cone effect. Crouch storage replaces
                // X-ray's suit colouring with the stored shine's.
                samus.Xray.RelinquishSuitColouring();
            }

            samus.Pose = targetPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);

            // The posture change installs the target's crouching radius, then probes the crouch's downward entry
            // distance down through the downward block collision. On level
            // ground the bottom boundary is unchanged; descending a slope, the probe meets
            // the surface first and the shorter clipped distance is what moves center Y.
            BlockMoveResult alignment = samus.ProbeChangedPoseVertical(
                terrain, SignedSixteenSixteen.FromWholePixels(SamusPostureDefinitions.CrouchEntryDownwardPixels),
                SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), targetPose);
            // The crouching body lies inside the standing one, which was clear, so the probe moves her down by at most
            // its downward entry distance, clear of the terrain that bounds the room: she stays in it.
            samus.Kinematics.Position = samus.Kinematics.Position.WithWholeY(
                samus.Kinematics.Position.Y.Whole + alignment.AcceptedDisplacement.WholePixels);
            // The posture change publishes the aligned whole Y before scrolling; the
            // posture change itself must not become camera movement.
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
            SamusPresentation.InitializeAnimation(samus);
            return;
        }

        // Expanding from the crouch radius to the standing-transition radius invokes the
        // common pose-change collision check before initialization. The check reads the target radius from the pose
        // definition rather than assuming the ordinary standing radius.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
                terrain,
                targetPose,
                frameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        samus.Pose = targetPose;
        // The collision radius stays the source pose's. The prospective
        // standing-transition geometry was used only by the collision probes above;
        // the ordinary input phase publishes the target radius at the start of the next frame.
        // Keeping that one-frame handoff is observable when the crouched body is resting
        // on a frozen enemy inside a ceiling, as in the Botwoon-pipe Mochtroid clip.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Applies the ordinary Morph-Ball entry and exit poses selected by the crouch and
    /// ball input transitions. This includes item gating, the posture change's bottom alignment,
    /// and the block-only expansion collision used when unmorphing.
    /// </summary>
    /// <remarks>
    /// The morphing transitions shrink the current humanoid radius to the ball's and the posture change moves center
    /// Y down by the morph's downward entry distance (<see cref="SamusPostureDefinitions.MorphEntryDownwardPixels"/>), clipped against room blocks using the
    /// new radius. This is not the source/target radius difference. The unmorphing transitions expand the ball's radius
    /// to the crouching one through the larger-radius check; floor collision normally moves center up by the difference.
    /// If both sides constrain a ball-sized body, the crouch selector rejects the target and keeps Samus morphed.
    /// </remarks>
    public static void ApplyMorphTransition(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        // The morph does not require the crouching movement type. Falling straight-down
        // and spin-jump input transitions can select the same morphing-transition setup. The literal
        // previous-movement-type test below even has a dedicated spin-jumping side effect.
        // Morph and unmorph keep their facing; unmorphing starts from a stable ball pose, ordinary or Spring Ball.
        bool startsMorphing = targetPose is SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose;
        if (startsMorphing)
        {
            // The morphing-transition setup restores PreviousPose and
            // rejects the morph when Samus lacks the Morph Ball. No radius, animation, or position
            // write from the rejected prospective pose survives that frame.
            if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.MorphBall))
                return;

            SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(samus);
            samus.Pose = targetPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            BlockMoveResult alignment = samus.ProbeChangedPoseVertical(
                terrain, SignedSixteenSixteen.FromWholePixels(SamusPostureDefinitions.MorphEntryDownwardPixels),
                SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), targetPose);
            // The ball lies inside the body it shrank from, which was clear, so the probe moves her down by at most its
            // downward entry distance, clear of the terrain that bounds the room: she stays in it.
            samus.Kinematics.Position = samus.Kinematics.Position.WithWholeY(
                samus.Kinematics.Position.Y.Whole + alignment.AcceptedDisplacement.WholePixels);
            // The posture change replaces the previous whole-Y checkpoint
            // after alignment, so the camera does not count pose displacement as motion.
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);

            // Morphing from a spin jump sets
            // SamusAccelerationMode.Decelerating so the compact body retains decelerating aerial momentum after morphing.
            if (previousMovementType == SamusMovementType.SpinJumping)
                samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;

            // This is the bomb-spread charge timeout, not the arm-cannon flare counter. Bomb-spread charging is
            // invalid once the body has entered either ordinary or Spring Ball form.
            samus.BombSpreadCharge = UpdateCountdown.Lasting(Projectiles.MorphBallBombDefinitions.DownChargeTimeoutUpdates);

            // The posture change also cancels any bounce before starting the transition; an ordinary crouch has none
            // to cancel.
            CancelBounceForPostureTransition(samus);
            SamusPresentation.InitializeAnimation(samus);
            return;
        }

        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            terrain,
            targetPose,
            frameCounter,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // A Morph Ball source can only keep its pose here; a crouch applies only if the outcome asks for one.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        samus.Pose = targetPose;
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
        // Unmorph uses the same posture change as morph. Its zero
        // alignment does not bypass the subsequent bounce cancellation.
        // The prospective collision probes used the target radius, but the live
        // radius remains unchanged until the input phase sets the radius next frame. This
        // matters to the later enemy-projectile pickup overlap tests.
        CancelBounceForPostureTransition(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Applies the stand-up Up chooses from the Draygon-held ball the Baby's Hyper Beam freeze can install (#1535, #2516).
    /// The stand-up's own setup does nothing and its movement type has no jump-transition handling, so the pose change only
    /// makes room for the larger body through the larger-pose collision and starts the stand-up's animation.
    /// </summary>
    /// <param name="samus">The Samus this changes.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="targetPose">The Draygon-held stand-up, the only pose the ball's input transitions choose.</param>
    /// <param name="frameCounter">The frame counter supplied to changed-pose block reactions.</param>
    /// <remarks>
    /// As for the stand transitions, the collision radius stays the ball's until the input phase publishes the stand-up's
    /// at the start of the next frame.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="terrain"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source and target are not the Draygon-held ball and its stand-up.</exception>
    public static void ApplyDraygonHoldStandUp(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        SamusPoseId sourcePose = samus.Pose;

        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            terrain,
            targetPose,
            frameCounter,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        samus.Pose = targetPose;
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>Shared bounce cancellation of the posture change.</summary>
    private static void CancelBounceForPostureTransition(SamusState samus)
    {
        if (samus.MorphBallBounceState == SamusBallBounceState.None)
            return;
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Still;
    }

    /// <summary>
    /// Applies a same-facing falling pose selected while the unmorph animation
    /// is still active, without replacing the live aerial velocity.
    /// </summary>
    /// <remarks>
    /// The game's unmorph input transitions remain active before the animation's transition cue
    /// reaches its normal crouch endpoint. Up, aim, and Fire input can therefore
    /// interrupt the transition with an ordinary <see cref="SamusMovementType.Falling"/> pose. This is
    /// not a fresh jump or an animation-completion shortcut: it enters the shared
    /// changed-pose setup, makes room for the target body's potentially larger radius,
    /// and preserves the existing horizontal and vertical speeds that describe Samus's current fall.
    /// </remarks>
    public static void ApplyUnmorphToFallingTransition(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        // The unmorph's transition names the fall facing the unmorph body's way.
        SamusPoseId sourcePose = samus.Pose;

        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            terrain,
            targetPose,
            frameCounter,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // The crouching radius is already non-ball geometry. If both sides constrain an
            // attempted full-height falling body, the crouch selector selects stable crouch just as
            // it does for aimed crouch and compact-aerial expansion. A failed compensating
            // probe instead retains the visible unmorph frame and all of Samus's live speeds.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);

        // Only the target's animation restarts: the transition neither starts a jump nor clears Samus's vertical
        // direction and speed, because it can occur halfway through a fall, as it does when leaving Parlor's Morph Ball
        // tunnel.
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Installs a stable ordinary Morph-Ball pose while preserving the shared
    /// rolling animation exactly as the morph-ball pose setup requests.
    /// </summary>
    /// <remarks>
    /// Its callers change between stable ball poses, which all share one animation, so the change keeps the frame and timer
    /// running.
    /// </remarks>
    public static void ApplyMorphBallPoseChange(SamusState samus, SamusPoseId targetPose)
    {
        SamusFacingDirection previousDirection = SamusPoseMetadata.ReadFacingDirection(samus);
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        SamusFacingDirection currentDirection = SamusPoseMetadata.ReadFacingDirection(samus);
        bool reversed = (previousDirection == SamusFacingDirection.Right && currentDirection == SamusFacingDirection.Left) ||
            (previousDirection == SamusFacingDirection.Left && currentDirection == SamusFacingDirection.Right);
        if (reversed)
        {
            // Reversing folds the run momentum into the base speed, the fraction carrying into the whole pixels,
            // clears the extra speeds, and selects SamusAccelerationMode.Turning so displacement initially keeps
            // travelling in the old direction while the ball decelerates through its turn.
            samus.HorizontalSpeed.BaseSpeed += samus.HorizontalSpeed.ExtraRunSpeed;
            // The speed boost is cancelled between folding the extra run speed into the base speed and the
            // explicit extra-speed clear, in that order even for ordinary Dash.
            samus.HorizontalSpeed.CancelRunningMomentum(currentDirection);
            samus.HorizontalSpeed.ExtraRunSpeed = default;
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
        }
    }

    /// <summary>
    /// Applies the winning solid-ceiling transition. Call after higher
    /// priority animation and hit interruptions, not from the collision scan.
    /// </summary>
    public static void ApplySolidCeilingCollision(SamusState samus)
    {
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
    }

    /// <summary>
    /// Resolves an ordinary Morph-Ball downward collision through the solid-vertical transition and
    /// the ball bounce, including both automatic rebounds and the final grounded pose.
    /// </summary>
    public static void ApplyMorphBallLanding(SamusState samus)
    {
        // A rising ball, as in Moonfall, lands without a rebound.
        if (samus.MorphBallBounceState == SamusBallBounceState.None &&
            SamusSpeedMagnitude.IsAtLeast(samus.Kinematics.VerticalVelocity.Speed.Whole, SamusMovementDefinitions.FirstBallBounceMinimumSpeed))
        {
            samus.MorphBallBounceState = SamusBallBounceState.FirstRebound;
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Rising(SamusVerticalMotionDefinitions.BallBounceSpeed);
            return;
        }

        if (samus.MorphBallBounceState == SamusBallBounceState.FirstRebound)
        {
            samus.MorphBallBounceState = SamusBallBounceState.SecondRebound;
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Rising(SamusVerticalMotionDefinitions.SecondBallBounceSpeed);
            return;
        }

        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Still;
        SamusPoseId groundedPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.MorphBallGroundLeftPose
            : SamusPoseId.MorphBallGroundRightPose;
        ApplyMorphBallPoseChange(samus, groundedPose);
        // The base speed is cleared only when the bounce ends with the ball on the
        // ground. Both airborne rebounds above retain their horizontal speed.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
    }

    /// <summary>
    /// Resolves the Spring Ball bounce. Holding Jump immediately calls the same
    /// jump setup as a grounded Spring Ball press; otherwise the
    /// bounce advances through the Spring Ball family's two rebounds.
    /// </summary>
    public static void ApplySpringBallLanding(SamusState samus, GameplayAction controllerInput)
    {
        if ((controllerInput & GameplayAction.Jump) != GameplayAction.None)
        {
            samus.MorphBallBounceState = SamusBallBounceState.None;
            SamusAerialMovement.InitializeJump(samus);
            // The landing relaunch only initializes velocity. It retains the
            // current airborne pose/animation, unlike a fresh grounded Jump input.
            // Changing pose here also selects a different next-frame movement path.
            return;
        }

        // The bounce is chosen by the rebound count only, so an ordinary-ball
        // rebound left over from a Spring Ball toggle selects the same rebound step.
        if (samus.MorphBallBounceState == SamusBallBounceState.None &&
            SamusSpeedMagnitude.IsAtLeast(samus.Kinematics.VerticalVelocity.Speed.Whole, SamusMovementDefinitions.FirstBallBounceMinimumSpeed))
        {
            samus.MorphBallBounceState = SamusBallBounceState.SpringBallFirstRebound;
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Rising(SamusVerticalMotionDefinitions.BallBounceSpeed);
            return;
        }

        if (samus.MorphBallBounceState is SamusBallBounceState.FirstRebound or SamusBallBounceState.SpringBallFirstRebound)
        {
            samus.MorphBallBounceState = SamusBallBounceState.SpringBallSecondRebound;
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Rising(SamusVerticalMotionDefinitions.SecondBallBounceSpeed);
            return;
        }

        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Still;
        SamusPoseId groundPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.SpringBallGroundLeftPose
            : SamusPoseId.SpringBallGroundRightPose;
        ApplyMorphBallPoseChange(samus, groundPose);
        // The common landing clears base speed when the bounce ends with
        // the ball on the ground. Spring Ball shares this with ordinary ball landing.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
    }

    /// <summary>
    /// Applies the Spring Ball jump when grounded Spring Ball changes to the Spring Ball jump.
    /// Moving-ground poses first pass through the same input-selected target, but the
    /// setup only launches when the previous movement type was exactly grounded Spring Ball.
    /// </summary>
    public static void ApplySpringBallJump(SamusState samus, SamusPoseId targetPose)
    {
        ApplyMorphBallPoseChange(samus, targetPose);
        samus.MorphBallBounceState = SamusBallBounceState.None;
        SamusAerialMovement.InitializeJump(samus);
    }

    /// <summary>
    /// Applies the walk-off transition after a bomb-jump frame moved down without a floor. Only the pose
    /// changes: the airborne ball's setup leaves vertical speed for the next
    /// bomb-jump frame, whose passing-zero test turns the jump downward. An upright body (standing, running, crouching,
    /// moonwalking or stopped at a wall) takes the plain fall of its facing. Movement types whose
    /// falling entry is "no change" keep their pose.
    /// </summary>
    public static void ApplyBombJumpFallingPose(SamusState samus, SamusTerrain terrain, int frameCounter)
    {
        bool left = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        if (SamusPoseMetadata.IsGroundedSpringBallPose(samus.Pose))
            ApplyMorphBallPoseChange(samus, left ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.SpringBallFallingRightPose);
        else if (SamusPoseMetadata.IsGroundedMorphBallPose(samus.Pose))
            ApplyMorphBallPoseChange(samus, left ? SamusPoseId.MorphBallFallingLeftPose : SamusPoseId.MorphBallFallingRightPose);
        else if (SamusPoseMetadata.ReadMovementType(samus) is SamusMovementType.Standing or SamusMovementType.Running or
                 SamusMovementType.Crouching or SamusMovementType.Moonwalking or SamusMovementType.RanIntoWall)
            ApplyUprightBombJumpFall(samus, left ? SamusPoseId.FallingLeftPose : SamusPoseId.FallingRightPose, terrain, frameCounter);
    }

    /// <summary>
    /// Installs the plain fall <paramref name="fall"/> an upright bomb-jumping Samus takes when the jump passes its apex
    /// with no floor below, as any pose change is installed: through the larger-pose collision, which a crouching body
    /// grows through and a low ceiling can refuse, and then the fall's own setup, which picks the acceleration mode from her
    /// extra run speed. The bomb jump keeps its vertical speed, as it does for a ball.
    /// </summary>
    private static void ApplyUprightBombJumpFall(SamusState samus, SamusPoseId fall, SamusTerrain terrain, int frameCounter)
    {
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            terrain,
            fall,
            frameCounter,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        samus.Kinematics.Position = samus.Kinematics.Position.WithWholeY(
            samus.Kinematics.Position.Y.Whole + centerAdjustment);
        samus.Pose = fall;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusAerialPoseTransitions.InitializeOrdinaryAerialAcceleration(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Applies the walk-off transition's morph-ball endpoint, retaining the rolling animation
    /// while changing to ordinary airborne <see cref="SamusPoseId.MorphBallFallingRightPose"/> and <see cref="SamusPoseId.MorphBallFallingLeftPose"/> and starting the gravity of the medium she moves in.
    /// </summary>
    public static void ApplyMorphBallWalkOff(SamusState samus)
    {
        bool springBall = SamusPoseMetadata.IsGroundedSpringBallPose(samus.Pose);

        SamusPoseId fallingPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? springBall ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.MorphBallFallingLeftPose
            : springBall ? SamusPoseId.SpringBallFallingRightPose : SamusPoseId.MorphBallFallingRightPose;
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
        SamusAerialMovement.ConfigureEnvironmentGravity(samus);
        ApplyMorphBallPoseChange(samus, fallingPose);
    }
}
