using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's posture transitions: crouching and standing, crouch jumps, morphing and unmorphing, and the morph ball's landings, bounces, spring-ball jumps and walk-offs.</summary>
internal static class SamusPosturePoseTransitions
{
    /// <summary>Applies the crouch input tables' direct same-facing exits (right-facing crouches to right-facing standing, or left-facing to left-facing) after larger-pose collision admits the standing body.</summary>
    /// <param name="samus">The Samus this reads.</param>
    /// <param name="level">Live room block geometry used by the shared pose-expansion resolver.</param>
    /// <param name="targetPose">Final normal standing-right or standing-left pose matching the current crouch facing; animated stand-up and cross-facing routes are not accepted.</param>
    /// <param name="frameCounter">The frame counter supplied to changed-pose block reactions.</param>
    /// <param name="roomObjects">Optional live room-object owner receiving those collision reactions.</param>
    /// <returns>True when the final standing pose and animation are installed; false when collision retains the source or substitutes stable crouch, which may change an aimed crouch even on failure.</returns>
    /// <remarks>These input records bypass the animated aimed stand-up poses. Accepted expansion applies the whole-pixel Y adjustment but retains the crouching radius until the next alpha pass, matching the larger-radius check's previous-pose radius publication.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="level"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source and target are not a same-facing crouch-to-final-standing input-table route.</exception>
    public static bool TryApplyDirectCrouchToStandingTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);

        bool rightRoute = SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) &&
            targetPose == SamusPoseId.FacingRightNormalPose;
        bool leftRoute = SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose) &&
            targetPose == SamusPoseId.FacingLeftNormalPose;
        if (!rightRoute && !leftRoute)
        {
            // This entry point is the exact crouching-to-normal-standing direct-exit route.
            // Animated stand-ups and cross-facing changes have separate initializers.
            throw new InvalidOperationException(
                $"Direct crouch exit {samus.Pose} -> {targetPose} is not a transition-table route.");
        }

        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                frameCounter,
                roomObjects,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // Does not restore an aimed crouch when the attempted standing body
            // is boxed in. It selects the ordinary stable crouch from facing metadata.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        // Explicitly publishes the previous pose's radius while probing the
        // enlargement, and this direct transition does not set the radius afterward.
        // The ordinary input phase publishes the standing radius at the start of the next frame.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Applies the crouching table's neutral jump-transition entry through the ordering:
    /// enlarge radius 16 -> 19 with the larger-radius check, initialise normal-jumping movement, run the
    /// crouch-only Y adjustment, then start the jump.
    /// </summary>
    /// <remarks>
    /// Tests the literal previous pose, not merely the crouching movement type. Therefore
    /// only ordinary crouching subtracts ten extra pixels; aimed crouches still
    /// jump, but retain only the collision resolver's bottom-alignment adjustment.
    /// </remarks>
    /// <returns>
    /// False when expansion is impossible. Simultaneous initial hits select ordinary stable
    /// crouch; rejection by a compensating opposite-side probe retains the source pose.
    /// </returns>
    public static bool TryApplyCrouchJumpTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);

        bool rightRoute = SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) &&
            targetPose == SamusPoseId.NeutralJumpTransitionRightPose;
        bool leftRoute = SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose) &&
            targetPose == SamusPoseId.NeutralJumpTransitionLeftPose;
        if (!rightRoute && !leftRoute)
        {
            // Only the two neutral jump-transition records enter this transition; aimed
            // crouches deliberately reuse the same facing-selected target.
            throw new InvalidOperationException(
                $"Crouch jump {samus.Pose} -> {targetPose} is not a transition-table route.");
        }

        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                frameCounter,
                roomObjects,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);

        // The input phase owns the new radius next frame; collision correction and jump setup
        // below must not publish it during this frame's pose-commit stage.
        // The normal-jumping jump transition performs this after pose
        // initialization/collision but before the jump starts. It writes only current Y;
        // the desktop state has no separately exposed previous Y position to mirror.
        if (sourcePose is SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingLeftPose)
            samus.Kinematics.Position -= new Offset16<RoomPixels>(0, 10);

        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        SamusAerialMovement.InitializeJump(samus);
        return true;
    }


    /// <summary>
    /// Applies the ordinary or aimed crouch-start/stand-start transition records,
    /// including the posture change's bottom alignment and the larger-radius collision branch
    /// of the pose-change collision. The admitted target set is exactly
    /// the unaimed and aimed crouching and standing transitions.
    /// </summary>
    /// <returns>
    /// False only when simultaneous floor and ceiling collision leave no verified room to
    /// expand from radius 16 to radius 21; the game then retains the crouching pose.
    /// </returns>
    public static bool TryApplyPostureTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);

        bool startsCrouchingRight =
            (SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsRightFacingRunningPose(samus.Pose) ||
             SamusPoseMetadata.IsMoonwalkingFacingRightPose(samus.Pose) ||
             SamusPoseMetadata.IsRightFacingRanIntoWallPose(samus.Pose) ||
             SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose)) &&
            targetPose is
                SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionAimUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose;
        bool startsCrouchingLeft =
            (SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose) ||
             SamusPoseMetadata.IsMoonwalkingFacingLeftPose(samus.Pose) ||
             SamusPoseMetadata.IsLeftFacingRanIntoWallPose(samus.Pose) ||
             SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose)) &&
            targetPose is
                SamusPoseId.CrouchingTransitionLeftPose or SamusPoseId.CrouchingTransitionAimUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose;
        bool startsCrouching = startsCrouchingRight || startsCrouchingLeft;
        bool startsStandingRight = SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) &&
            targetPose is
                SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionAimUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalDownRightPose;
        bool startsStandingLeft = SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose) &&
            targetPose is
                SamusPoseId.StandingTransitionLeftPose or SamusPoseId.StandingTransitionAimUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalDownLeftPose;
        bool startsStanding = startsStandingRight || startsStandingLeft;
        if (!startsCrouching && !startsStanding)
        {
            // Admit exactly the crouch/stand records enumerated above.
            throw new InvalidOperationException(
                $"Posture transition {samus.Pose} -> {targetPose} is outside the crouch/stand transition-table family.");
        }

        if (startsCrouching)
        {
            // The crouching transition samples the boost stage before later
            // posture movement can cancel running momentum. A stage-four crouch therefore
            // stores a 180-update shinespark charge even though the following stable crouch has
            // no horizontal Speed Booster state of its own.
            if (samus.Shinespark.TryStoreFromSpeedBooster(samus.HorizontalSpeed.SpeedBoostCounter))
            {
                // The successful store takes over Samus's special suit colouring
                // and timer even if an interrupted Flash was keeping them alive.
                samus.CrystalFlash.RelinquishPaletteHandler();
                // Reserve Mode keeps only X-Ray's cone effect. Crouch storage replaces
                // X-ray's suit colouring with the stored shine's.
                samus.Xray.RelinquishPaletteHandler();
            }

            samus.Pose = targetPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);

            // The posture change reads five from its distance table, installs the target's
            // radius 16, then probes those five pixels down through the downward block collision. On level
            // ground the bottom boundary is unchanged; descending a slope, the probe meets
            // the surface first and the shorter clipped distance is what moves center Y.
            BlockMoveResult alignment = samus.ProbeChangedPoseVertical(
level, SignedSixteenSixteen.FromWholePixels(SamusPostureDefinitions.CrouchEntryDownwardPixels),
                SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), targetPose, roomObjects);
            samus.Kinematics.Position = samus.Kinematics.Position.WithWrappedWholeY(
                samus.Kinematics.Position.Y.Whole + alignment.AcceptedDisplacement.WholePixels);
            // The posture change publishes the aligned whole Y before scrolling; the
            // posture change itself must not become camera movement.
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
            return true;
        }

        // Expanding from crouch radius 16 to standing-transition radius 21 invokes the
        // common pose-change collision check before initialization. The shared helper
        // reads the target radius from the pose definition rather than assuming the ordinary value 21.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                frameCounter,
                roomObjects,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        // Restores the source radius before returning. The prospective
        // standing-transition geometry was used only by the collision probes above;
        // the ordinary input phase publishes the target radius at the start of the next frame.
        // Keeping that one-frame handoff is observable when the crouched body is resting
        // on a frozen enemy inside a ceiling, as in the Botwoon-pipe Mochtroid clip.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Applies the ordinary Morph-Ball entry and exit records selected by the crouch and
    /// ball transition tables. This includes item gating, the posture change's bottom alignment,
    /// and the block-only expansion collision used when unmorphing.
    /// </summary>
    /// <remarks>
    /// The morphing transitions shrink the current humanoid radius to seven and the posture change moves center
    /// Y down by the table's nine-pixel request, clipped against room blocks using the
    /// new radius. This is not the source/target radius difference. The unmorphing transitions expand 7 to 16 through
    /// the larger-radius check; floor collision normally moves center up nine. If both sides constrain a
    /// radius-seven body, the crouch selector rejects the target and keeps Samus morphed.
    /// </remarks>
    public static bool TryApplyMorphTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);

        // Does not require the crouching movement type. Falling straight-down
        // and spin-jump input tables can select the same morphing-transition initializer. The literal
        // previous-movement-type test below even has a dedicated type-three side effect.
        // Validate only the facing-preserving relationship guaranteed by the transition table.
        SamusFacingDirection sourceFacing = SamusPoseMetadata.ReadFacingDirection(samus);
        bool startsMorphingRight = sourceFacing == SamusFacingDirection.Right &&
            targetPose == SamusPoseId.MorphingTransitionRightPose;
        bool startsMorphingLeft = sourceFacing == SamusFacingDirection.Left &&
            targetPose == SamusPoseId.MorphingTransitionLeftPose;
        bool startsMorphing = startsMorphingRight || startsMorphingLeft;
        bool startsUnmorphingRight =
            samus.Pose is SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallMovingRightPose or
                SamusPoseId.MorphBallFallingRightPose or SamusPoseId.SpringBallGroundRightPose or
                SamusPoseId.SpringBallMovingRightPose or SamusPoseId.SpringBallFallingRightPose or
                SamusPoseId.SpringBallJumpRightPose &&
            targetPose == SamusPoseId.UnmorphingTransitionRightPose;
        bool startsUnmorphingLeft =
            samus.Pose is SamusPoseId.MorphBallGroundLeftPose or SamusPoseId.MorphBallMovingLeftPose or
                SamusPoseId.MorphBallFallingLeftPose or SamusPoseId.SpringBallGroundLeftPose or
                SamusPoseId.SpringBallMovingLeftPose or SamusPoseId.SpringBallFallingLeftPose or
                SamusPoseId.SpringBallJumpLeftPose &&
            targetPose == SamusPoseId.UnmorphingTransitionLeftPose;
        bool startsUnmorphing = startsUnmorphingRight || startsUnmorphingLeft;
        if (!startsMorphing && !startsUnmorphing)
        {
            // Morph and unmorph use a closed, facing-preserving table. Other ball pose
            // changes are handled by ApplyMorphBallPoseChange and must not enter here.
            throw new InvalidOperationException(
                $"Morph transition {samus.Pose} -> {targetPose} is not a transition-table route.");
        }

        if (startsMorphing)
        {
            // The morphing-transition initializer restores PreviousPose and
            // rejects the morph when Samus lacks the Morph Ball. No radius, animation, or position
            // write from the rejected prospective pose survives that frame.
            if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.MorphBall))
                return false;

            SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(samus);
            int previousRadius = samus.Kinematics.Radius.Y;
            samus.Pose = targetPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            if (samus.Kinematics.Radius.Y > previousRadius)
            {
                throw new InvalidDataException(
                    $"Morph entry {targetPose} expanded radius {previousRadius} -> {samus.Kinematics.Radius.Y}.");
            }
            BlockMoveResult alignment = samus.ProbeChangedPoseVertical(
level, SignedSixteenSixteen.FromWholePixels(SamusPostureDefinitions.MorphEntryDownwardPixels),
                SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), targetPose, roomObjects);
            samus.Kinematics.Position = samus.Kinematics.Position.WithWrappedWholeY(
                samus.Kinematics.Position.Y.Whole + alignment.AcceptedDisplacement.WholePixels);
            // The posture change replaces the previous whole-Y checkpoint
            // after alignment, so the camera does not count pose displacement as motion.
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);

            // Deliberately recognizes a spin-jump source and forces mode
            // two so the compact body retains decelerating aerial momentum after morphing.
            if (previousMovementType == SamusMovementType.SpinJumping)
                samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;

            // This is the bomb-spread charge timeout, not the arm-cannon flare counter. Bomb-spread charging is
            // invalid once the body has entered either ordinary or Spring Ball form.
            samus.BombSpreadChargeTimeoutCounter = 0;

            // The posture change also cancels an active bounce before starting the
            // transition. Ordinary crouch entry normally sees zero, but retaining the
            // literal writes keeps a bounce that is somehow active deterministic.
            CancelBounceForPostureTransition(samus);
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
            return true;
        }

        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            frameCounter,
            roomObjects,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // A morph source can only produce RetainSource here. Keep the defensive branch
            // explicit so a future caller cannot accidentally apply non-morph crouch fallback.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
        // Unmorph uses the same posture change as morph. Its zero
        // alignment-table entry does not bypass the subsequent bounce cancellation.
        // The prospective collision probes used the target radius, but the live
        // radius remains unchanged until the input phase sets the radius next frame. This
        // matters to the later enemy-projectile pickup overlap tests.
        CancelBounceForPostureTransition(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>Shared bounce cancellation of the posture change.</summary>
    private static void CancelBounceForPostureTransition(SamusState samus)
    {
        if (samus.MorphBallBounceState == SamusBallBounceState.None)
            return;
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
    }


    /// <summary>
    /// Applies a same-facing falling pose selected while the unmorph animation
    /// is still active, without replacing the live aerial velocity.
    /// </summary>
    /// <remarks>
    /// The game's unmorph input table remains active before the animation's transition cue
    /// reaches its normal crouch endpoint. Up, aim, and Fire input can therefore
    /// interrupt the transition with an ordinary movement-type-six falling record. This is
    /// not a fresh jump or an animation-completion shortcut: it enters the shared
    /// changed-pose initializer, makes room for the target body's potentially larger radius,
    /// and preserves the existing horizontal and vertical speeds that describe Samus's current fall.
    /// </remarks>
    public static bool TryApplyUnmorphToFallingTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = samus.Pose;
        bool rightFacingRoute = sourcePose == SamusPoseId.UnmorphingTransitionRightPose &&
            SamusPoseMetadata.IsRightFacingFallingPose(targetPose);
        bool leftFacingRoute = sourcePose == SamusPoseId.UnmorphingTransitionLeftPose &&
            SamusPoseMetadata.IsLeftFacingFallingPose(targetPose);
        if (!rightFacingRoute && !leftFacingRoute)
        {
            // Expose only the falling family matching the active
            // unmorph body's direction. Rejecting every other pairing keeps this helper
            // from silently becoming a catch-all for unrelated movement initializers.
            throw new InvalidOperationException(
                $"Unmorph-to-falling transition {sourcePose} -> {targetPose} is not a same-facing retail route.");
        }

        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            frameCounter,
            roomObjects,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // Radius sixteen is already non-ball geometry. If both sides constrain an
            // attempted full-height falling body, the crouch selector selects stable crouch just as
            // it does for aimed crouch and compact-aerial expansion. A failed compensating
            // probe instead retains the visible unmorph frame and all of Samus's live speeds.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);

        // InitializeAnimation resets only the target art stream. In particular, do not call
        // start a jump or clear YDirection/YSpeed/YSubspeed: the input record can occur
        // halfway through an actual fall, as it does when leaving Parlor's Morph Ball tunnel.
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Installs a stable ordinary Morph-Ball pose while preserving the shared eight-frame
    /// rolling animation exactly as the morph-ball pose initializer requests.
    /// </summary>
    public static void ApplyMorphBallPoseChange(SamusState samus, SamusPoseId targetPose)
    {
        bool sourceSupported = SamusPoseMetadata.IsStableBallPose(samus.Pose);
        bool targetSupported = SamusPoseMetadata.IsStableBallPose(targetPose);
        if (!sourceSupported || !targetSupported)
        {
            throw new InvalidOperationException(
                $"Stable Morph-Ball transition {samus.Pose} -> {targetPose} requires stable ball endpoints.");
        }

        SamusFacingDirection previousDirection = SamusPoseMetadata.ReadFacingDirection(samus);
        SamusPoseId sourcePose = samus.Pose;
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // All ordinary ball poses share one animation, and the change keeps the frame and timer running.
        // Assert the shared animation instead of depending on that fact silently.
        if (!ReferenceEquals(SamusAnimationDelays.AnimationForPose(sourcePose), SamusAnimationDelays.AnimationForPose(targetPose)))
        {
            throw new InvalidDataException(
                $"Morph-Ball transition {sourcePose} -> {targetPose} selected different animation programs.");
        }

        SamusFacingDirection currentDirection = SamusPoseMetadata.ReadFacingDirection(samus);
        bool reversed = (previousDirection == SamusFacingDirection.Right && currentDirection == SamusFacingDirection.Left) ||
            (previousDirection == SamusFacingDirection.Left && currentDirection == SamusFacingDirection.Right);
        if (reversed)
        {
            // Folds run momentum into base speed, the fraction's overflow passing into the whole pixels,
            // clears the extra speeds, and selects mode one so displacement initially keeps
            // travelling in the old direction while the ball decelerates through its turn.
            samus.HorizontalSpeed.BaseSpeed += samus.HorizontalSpeed.ExtraRunSpeed;
            // Cancels the speed boost between the 1/65536-pixel fold and the
            // explicit extra-speed clear. Preserve that ordering even for ordinary Dash.
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
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
    }


    /// <summary>
    /// Resolves an ordinary Morph-Ball downward collision through the solid-vertical transition and
    /// the ball bounce, including both automatic rebounds and the final grounded pose.
    /// </summary>
    /// <returns>True only when the collision ends grounded; false means another rebound.</returns>
    public static bool ApplyMorphBallLanding(SamusState samus)
    {
        if (!SamusPoseMetadata.IsAirborneMorphBallPose(samus.Pose) && !SamusPoseMetadata.IsGroundedMorphBallPose(samus.Pose))
            throw new InvalidOperationException($"Morph-Ball landing requires an airborne morph-ball pose, not {samus.Pose}.");

        // The speed is compared by whether its difference from the minimum is negative, so
        // Moonfall's negative velocity lands without a rebound.
        if (samus.MorphBallBounceState == SamusBallBounceState.None &&
            SamusSpeedMagnitude.IsAtLeast(samus.Kinematics.YSpeed.Whole, SamusMovementDefinitions.FirstBallBounceMinimumSpeed))
        {
            samus.MorphBallBounceState = SamusBallBounceState.FirstRebound;
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed;
            return false;
        }

        if (samus.MorphBallBounceState == SamusBallBounceState.FirstRebound)
        {
            samus.MorphBallBounceState = SamusBallBounceState.SecondRebound;
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed.WithWhole(SamusVerticalMotionDefinitions.BallBounceSpeed.Whole - 1);
            return false;
        }

        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.Kinematics.YSpeed = default;
        SamusPoseId groundedPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.MorphBallGroundLeftPose
            : SamusPoseId.MorphBallGroundRightPose;
        ApplyMorphBallPoseChange(samus, groundedPose);
        // Clears base momentum only when the bounce ends with the ball on the
        // ground. Both airborne rebounds above retain their horizontal speed.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
        return true;
    }


    /// <summary>
    /// Resolves the Spring Ball bounce. Holding Jump immediately calls the same
    /// jump initializer as a grounded Spring Ball press; otherwise the
    /// bounce advances through the Spring Ball family's two rebounds.
    /// </summary>
    public static bool ApplySpringBallLanding(SamusState samus, ControllerButton controllerInput)
    {
        if (!SamusPoseMetadata.IsAirborneSpringBallPose(samus.Pose) && !SamusPoseMetadata.IsGroundedSpringBallPose(samus.Pose))
            throw new InvalidOperationException($"Spring-Ball landing requires an airborne Spring Ball pose, not {samus.Pose}.");

        if ((controllerInput & ControllerButton.A) != ControllerButton.None)
        {
            samus.MorphBallBounceState = SamusBallBounceState.None;
            SamusAerialMovement.InitializeJump(samus);
            // The landing relaunch only initializes velocity. It retains the
            // current airborne pose/animation, unlike a fresh grounded Jump input.
            // Changing pose here also selects a different next-frame movement path.
            return false;
        }

        // Dispatches on the rebound count only, so an ordinary-ball
        // rebound left over from a Spring Ball toggle selects the same rebound step.
        if (samus.MorphBallBounceState == SamusBallBounceState.None &&
            SamusSpeedMagnitude.IsAtLeast(samus.Kinematics.YSpeed.Whole, SamusMovementDefinitions.FirstBallBounceMinimumSpeed))
        {
            samus.MorphBallBounceState = SamusBallBounceState.SpringBallFirstRebound;
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed;
            return false;
        }

        if (samus.MorphBallBounceState is SamusBallBounceState.FirstRebound or SamusBallBounceState.SpringBallFirstRebound)
        {
            samus.MorphBallBounceState = SamusBallBounceState.SpringBallSecondRebound;
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed.WithWhole(SamusVerticalMotionDefinitions.BallBounceSpeed.Whole - 1);
            return false;
        }

        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.Kinematics.YSpeed = default;
        SamusPoseId groundPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.SpringBallGroundLeftPose
            : SamusPoseId.SpringBallGroundRightPose;
        ApplyMorphBallPoseChange(samus, groundPose);
        // The common landing caller clears base speed when the bounce ends with
        // the ball on the ground. Spring Ball shares this with ordinary ball landing.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
        return true;
    }


    /// <summary>
    /// Applies the Spring Ball jump when grounded Spring Ball changes to the Spring Ball jump.
    /// Moving-ground poses first pass through the same table-selected target, but the
    /// initializer only launches when the previous movement type was exactly grounded Spring Ball.
    /// </summary>
    public static void ApplySpringBallJump(SamusState samus, SamusPoseId targetPose)
    {
        bool validTarget = targetPose is SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose;
        if (!SamusPoseMetadata.IsGroundedSpringBallPose(samus.Pose) || !validTarget)
            throw new InvalidOperationException(
                $"Spring-Ball jump {samus.Pose} -> {targetPose} is outside the grounded-to-airborne Spring-Ball route.");

        ApplyMorphBallPoseChange(samus, targetPose);
        samus.MorphBallBounceState = SamusBallBounceState.None;
        SamusAerialMovement.InitializeJump(samus);
    }


    /// <summary>
    /// Applies the walk-off transition after a bomb-jump frame moved down without a floor. Only the pose
    /// changes: the airborne ball's initializer leaves vertical speed for the next
    /// handler frame, whose passing-zero test turns the jump downward. Movement types whose
    /// falling entry is "no change" keep their pose.
    /// </summary>
    public static void ApplyBombJumpFallingPose(SamusState samus)
    {
        bool left = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        if (SamusPoseMetadata.IsGroundedSpringBallPose(samus.Pose))
            ApplyMorphBallPoseChange(samus, left ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.SpringBallFallingRightPose);
        else if (SamusPoseMetadata.IsGroundedMorphBallPose(samus.Pose))
            ApplyMorphBallPoseChange(samus, left ? SamusPoseId.MorphBallFallingLeftPose : SamusPoseId.MorphBallFallingRightPose);
        else if (SamusPoseMetadata.ReadMovementType(samus) is SamusMovementType.Standing or SamusMovementType.Running or
                 SamusMovementType.Crouching or SamusMovementType.Moonwalking or SamusMovementType.RanIntoWall)
            throw new InvalidOperationException(
                $"Bomb-jump falling result for humanoid pose {samus.Pose} (airborne walk-off) is not translated.");
    }


    /// <summary>
    /// Applies the walk-off transition's type-four endpoint, retaining the rolling animation
    /// while changing to ordinary airborne <see cref="SamusPoseId.MorphBallFallingRightPose"/> and <see cref="SamusPoseId.MorphBallFallingLeftPose"/> and starting dry-air gravity.
    /// </summary>
    public static void ApplyMorphBallWalkOff(SamusState samus)
    {
        bool springBall = SamusPoseMetadata.IsGroundedSpringBallPose(samus.Pose);
        if (!SamusPoseMetadata.IsGroundedMorphBallPose(samus.Pose) && !springBall)
            throw new InvalidOperationException($"Morph-Ball walk-off requires grounded pose, not {samus.Pose}.");

        SamusPoseId fallingPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? springBall ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.MorphBallFallingLeftPose
            : springBall ? SamusPoseId.SpringBallFallingRightPose : SamusPoseId.MorphBallFallingRightPose;
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        SamusAerialMovement.ConfigureEnvironmentGravity(samus);
        ApplyMorphBallPoseChange(samus, fallingPose);
    }
}
