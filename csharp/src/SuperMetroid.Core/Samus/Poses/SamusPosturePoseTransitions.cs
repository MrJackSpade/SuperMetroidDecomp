using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's posture transitions: crouching and standing, crouch jumps, morphing and unmorphing, and the morph ball's landings, bounces, spring-ball jumps and walk-offs.</summary>
internal static class SamusPosturePoseTransitions
{
    /// <summary>Applies the crouch input tables' direct same-facing exits ($27/$71/$73/$85 to $01, or $28/$72/$74/$86 to $02) after larger-pose collision admits the standing body.</summary>
    /// <param name="samus">The Samus this reads.</param>
    /// <param name="level">Live room block geometry used by the shared pose-expansion resolver.</param>
    /// <param name="targetPose">Final standing-right $01 or standing-left $02 pose matching the current crouch facing; animated stand-up and cross-facing routes are not accepted.</param>
    /// <param name="nmiFrameCounter">Native NMI counter supplied to changed-pose block reactions.</param>
    /// <param name="plms">Optional live PLM owner receiving those collision reactions.</param>
    /// <returns>True when the final standing pose and animation are installed; false when collision retains the source or substitutes stable crouch, which may change an aimed crouch even on failure.</returns>
    /// <remarks>These six-byte input records bypass animated $F7–$FC stand-up poses. Accepted expansion applies the whole-pixel Y adjustment but retains the crouching radius until the next alpha pass, matching $91:FDAE's previous-pose radius publication.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="level"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source and target are not a same-facing crouch-to-final-standing input-table route.</exception>
    public static bool TryApplyDirectCrouchToStandingTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {

        Ensure.NotNull(level);

        bool rightRoute = SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) &&
            targetPose == SamusPoseId.FacingRightNormalPose;
        bool leftRoute = SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose) &&
            targetPose == SamusPoseId.FacingLeftNormalPose;
        if (!rightRoute && !leftRoute)
        {
            // This entry point is the exact `$27/$28 -> $01/$02` direct-exit route.
            // Animated stand-ups and cross-facing changes have separate initializers.
            throw new InvalidOperationException(
                $"Direct crouch exit {samus.Pose} -> {targetPose} is not a ROM-table route.");
        }

        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // $91:FFA7 does not restore an aimed crouch when the attempted standing body
            // is boxed in. It selects the ordinary stable crouch from facing metadata.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        // `$91:FDAE` explicitly publishes the previous pose's radius while probing the
        // enlargement, and this direct transition does not call Samus_SetRadius afterward.
        // Ordinary alpha publishes the standing radius at the start of the next frame.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Applies the crouching table's `$4B/$4C` jump entry through the native ordering:
    /// enlarge radius 16 -> 19 with `$91:FDAE`, initialise movement type two, run the
    /// `$91:FC66` crouch-only Y adjustment, then call `Make_Samus_Jump`.
    /// </summary>
    /// <remarks>
    /// `$91:FC7D` tests the literal previous pose, not merely movement type five. Therefore
    /// only ordinary `$27/$28` subtract ten extra pixels; aimed `$71-$74/$85/$86` still
    /// jump, but retain only the collision resolver's bottom-alignment adjustment.
    /// </remarks>
    /// <returns>
    /// False when expansion is impossible. Simultaneous initial hits select ordinary stable
    /// crouch; rejection by a compensating opposite-side probe retains the source pose.
    /// </returns>
    public static bool TryApplyCrouchJumpTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {

        Ensure.NotNull(level);

        bool rightRoute = SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) &&
            targetPose == SamusPoseId.NeutralJumpTransitionRightPose;
        bool leftRoute = SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose) &&
            targetPose == SamusPoseId.NeutralJumpTransitionLeftPose;
        if (!rightRoute && !leftRoute)
        {
            // Only the two neutral `$4B/$4C` transition records enter this routine; aimed
            // crouches deliberately reuse the same facing-selected target.
            throw new InvalidOperationException(
                $"Crouch jump {samus.Pose} -> {targetPose} is not a ROM-table route.");
        }

        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));

        // Alpha owns the new radius next frame; collision correction and jump setup
        // below must not publish it during this frame's pose-commit stage.
        // HandleJumpTransition_NormalJumping at $91:FC7D performs this after pose
        // initialization/collision but before Make_Samus_Jump. It writes only current Y;
        // the desktop state has no separately exposed PreviousYPosition word to mirror.
        if (sourcePose is SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingLeftPose)
            samus.Kinematics.Position -= new Offset16<RoomPixels>(0, 10);

        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        SamusAerialMovement.InitializeJump(samus);
        return true;
    }


    /// <summary>
    /// Applies the ordinary or aimed crouch-start/stand-start transition records,
    /// including command seven's bottom alignment and the larger-radius collision branch
    /// from <c>HandlePoseChangeCollision</c>. The admitted target set is exactly
    /// `$35/$36/$3B/$3C/$F1-$FC`.
    /// </summary>
    /// <returns>
    /// False only when simultaneous floor and ceiling collision leave no verified room to
    /// expand from radius 16 to radius 21; native behavior then retains the crouching pose.
    /// </returns>
    public static bool TryApplyPostureTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
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
            // `$91:F7B0/$F7D3` admit exactly the crouch/stand records enumerated above.
            throw new InvalidOperationException(
                $"Posture transition {samus.Pose} -> {targetPose} is outside the crouch/stand ROM-table family.");
        }

        if (startsCrouching)
        {
            // `Samus_CrouchTrans` at `$91:F7B0` samples the high stage byte before later
            // posture movement can cancel running momentum. A stage-four crouch therefore
            // banks 180 palette-handler ticks even though the following stable crouch has
            // no horizontal Speed Booster state of its own.
            if (samus.Shinespark.TryStoreFromSpeedBooster(samus.HorizontalSpeed.SpeedBoostCounter))
            {
                // The successful native store replaces the shared palette handler
                // and timer even if an interrupted Flash was keeping them alive.
                samus.CrystalFlash.RelinquishPaletteHandler();
                // Reserve Mode keeps only X-Ray's HDMA object. Crouch storage replaces
                // shared palette handler eight with the stored-shine handler one.
                samus.Xray.RelinquishPaletteHandler();
            }

            samus.Pose = targetPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);

            // Prospective command seven reads five from $91:ED36, installs the target's
            // radius 16, then probes those five pixels down through $94:96AB. On level
            // ground the bottom boundary is unchanged; descending a slope, the probe meets
            // the surface first and the shorter clipped distance is what moves center Y.
            BlockMoveResult alignment = samus.ProbeChangedPoseVertical(
level, SnesSixteenSixteen.FromWhole(SamusPostureDefinitions.CrouchEntryDownwardPixels),
                (nmiFrameCounter & 1) == 0, targetPose, plms);
            samus.Kinematics.Position = samus.Kinematics.Position.WithWholeY(unchecked((ushort)(
                samus.Kinematics.Position.Y.Whole + (SnesSixteenSixteen.WholeOf(alignment.AcceptedDisplacement)))));
            // Command seven publishes the aligned whole Y before scrolling; the
            // posture change itself must not become camera movement.
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
            return true;
        }

        // Expanding from crouch radius 16 to standing-transition radius 21 invokes the
        // common pose-change collision routine before initialization. The shared helper
        // reads the target radius from this ROM rather than assuming the ordinary value 21.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        // `$91:FDAE` restores the source radius before returning. The prospective
        // standing-transition geometry was used only by the collision probes above;
        // ordinary alpha publishes the target radius at the start of the next frame.
        // Keeping that one-frame handoff is observable when the crouched body is resting
        // on a frozen enemy inside a ceiling, as in the Botwoon-pipe Mochtroid clip.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Applies the ordinary Morph-Ball entry and exit records selected by the crouch and
    /// ball transition tables. This includes item gating, command-seven bottom alignment,
    /// and the block-only expansion collision used when unmorphing.
    /// </summary>
    /// <remarks>
    /// `$37/$38` shrink the current humanoid radius to seven and command seven moves center
    /// Y down by the table's nine-pixel request, clipped against room blocks using the
    /// new radius. This is not the source/target radius difference. `$3D/$3E` expand 7 to 16 through
    /// `$91:FDAE`; floor collision normally moves center up nine. If both sides constrain a
    /// radius-seven body, `$91:FFA7` rejects the target and keeps Samus morphed.
    /// </remarks>
    public static bool TryApplyMorphTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {

        Ensure.NotNull(level);

        // `$91:F7CE` does not require movement type five. Falling straight-down `$2D/$2E`
        // and spin-jump input tables can select the same `$37/$38` initializer. The literal
        // previous-movement-type test below even has a dedicated type-three side effect.
        // Validate only the facing-preserving relationship guaranteed by the ROM table.
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
                $"Morph transition {samus.Pose} -> {targetPose} is not a ROM-table route.");
        }

        if (startsMorphing)
        {
            // InitializeSamusPose_MorphingTransition at $91:F7CE restores PreviousPose and
            // returns carry set when bit $0004 is absent. No radius, animation, or position
            // write from the rejected prospective pose survives that native frame.
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
level, SnesSixteenSixteen.FromWhole(SamusPostureDefinitions.MorphEntryDownwardPixels),
                (nmiFrameCounter & 1) == 0, targetPose, plms);
            samus.Kinematics.Position = samus.Kinematics.Position.WithWholeY(unchecked((ushort)(
                samus.Kinematics.Position.Y.Whole + (SnesSixteenSixteen.WholeOf(alignment.AcceptedDisplacement)))));
            // Command seven ($91:ED0E) replaces the previous whole-Y checkpoint
            // after alignment, so the camera does not count pose displacement as motion.
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);

            // `$91:F7D6-$F7E4` deliberately recognizes a spin-jump source and forces mode
            // two so the compact body retains decelerating aerial momentum after morphing.
            if (previousMovementType == SamusMovementType.SpinJumping)
                samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;

            // This is `$91:F7E7`, not the arm-cannon flare counter. Bomb-spread charging is
            // invalid once the body has entered either ordinary or Spring Ball form.
            samus.BombSpreadChargeTimeoutCounter = 0;

            // Prospective command seven also cancels an active bounce before starting the
            // transition. Ordinary crouch entry normally sees zero, but retaining the
            // literal writes makes externally stimulated debugger states deterministic.
            CancelBounceForPostureTransition(samus);
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
            return true;
        }

        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
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
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
        // Unmorph uses the same prospective command seven as morph. Its zero
        // alignment-table entry does not bypass the subsequent bounce cancellation.
        // The prospective collision probes used the target radius, but the live
        // word remains unchanged until alpha's Samus_SetRadius next frame. This
        // matters to the later enemy-projectile pickup overlap tests.
        CancelBounceForPostureTransition(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>Shared bounce cancellation in native prospective command seven.</summary>
    private static void CancelBounceForPostureTransition(SamusState samus)
    {
        if (samus.MorphBallBounceState == SamusBallBounceState.None)
            return;
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
    }


    /// <summary>
    /// Applies a same-facing falling pose selected while the `$3D/$3E` unmorph animation
    /// is still active, without replacing the live aerial velocity.
    /// </summary>
    /// <remarks>
    /// The cartridge's unmorph input table remains active before animation command `$FD`
    /// reaches its normal `$27/$28` crouch endpoint. Up, aim, and Fire input can therefore
    /// interrupt the transition with an ordinary movement-type-six falling record. This is
    /// not a fresh jump or an animation-completion shortcut: it enters the shared bank-$91
    /// changed-pose initializer, makes room for the target body's potentially larger radius,
    /// and preserves the existing X/Y speed words that describe Samus's current fall.
    /// </remarks>
    public static bool TryApplyUnmorphToFallingTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = samus.Pose;
        bool rightFacingRoute = sourcePose == SamusPoseId.UnmorphingTransitionRightPose &&
            SamusPoseMetadata.IsRightFacingFallingPose(targetPose);
        bool leftFacingRoute = sourcePose == SamusPoseId.UnmorphingTransitionLeftPose &&
            SamusPoseMetadata.IsLeftFacingFallingPose(targetPose);
        if (!rightFacingRoute && !leftFacingRoute)
        {
            // `$91:A3F6/$91:A476` expose only the falling family matching the active
            // unmorph body's direction. Rejecting every other pairing keeps this helper
            // from silently becoming a catch-all for unrelated movement initializers.
            throw new InvalidOperationException(
                $"Unmorph-to-falling transition {sourcePose} -> {targetPose} is not a same-facing retail route.");
        }

        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // Radius sixteen is already non-ball geometry. If both sides constrain an
            // attempted full-height falling body, `$91:FFA7` selects stable crouch just as
            // it does for aimed crouch and compact-aerial expansion. A failed compensating
            // probe instead retains the visible unmorph frame and all live speed words.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));

        // InitializeAnimation resets only the target art stream. In particular, do not call
        // Make_Samus_Jump or clear YDirection/YSpeed/YSubspeed: the input record can occur
        // halfway through an actual fall, as it does when leaving Parlor's Morph Ball tunnel.
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Installs a stable ordinary Morph-Ball pose while preserving the shared eight-frame
    /// rolling animation exactly as <c>InitializeSamusPose_MorphBall</c> requests.
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
        SamusAnimationDelayList previousDelayList = samus.AnimationDelayList;
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // All ordinary ball poses point at `$91:B378`. Native writes $8000 to the new-frame
        // selector, causing `$91:FB5C` to return without changing frame OR timer. Assert the
        // table identity instead of depending on that retail-data fact silently.
        SamusAnimationDelayList targetDelayList = SamusPresentation.ResolveAnimationDelayList(samus);
        if (previousDelayList != targetDelayList)
        {
            throw new InvalidDataException(
                $"Morph-Ball transition selected mismatched animation lists {previousDelayList}/{targetDelayList}.");
        }

        SamusFacingDirection currentDirection = SamusPoseMetadata.ReadFacingDirection(samus);
        bool reversed = (previousDirection == SamusFacingDirection.Right && currentDirection == SamusFacingDirection.Left) ||
            (previousDirection == SamusFacingDirection.Left && currentDirection == SamusFacingDirection.Right);
        if (reversed)
        {
            // `$91:FA32-$91:FA52` folds run momentum into base speed with one 16-bit carry,
            // clears the extra words, and selects mode one so displacement initially keeps
            // travelling in the old direction while the ball decelerates through its turn.
            uint combined = unchecked(samus.HorizontalSpeed.BaseFixed +
                SnesSixteenSixteen.Compose(samus.HorizontalSpeed.ExtraRunSpeed.Whole,
                    samus.HorizontalSpeed.ExtraRunSpeed.Fraction));
            samus.HorizontalSpeed.BaseSpeed = SnesUnsignedSixteenSixteen.FromRaw(combined);
            // `$91:FA4C` invokes `Samus_CancelSpeedBoost` between the 16.16 fold and the
            // explicit extra-word clear. Preserve that ordering even for ordinary Dash.
            samus.HorizontalSpeed.CancelRunningMomentum(currentDirection);
            samus.HorizontalSpeed.ExtraRunSpeed = default;
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
        }
    }


    /// <summary>
    /// Applies the winning solid-ceiling transition at $91:EFDF. Call after higher
    /// priority animation and hit interruptions, not from the collision scan.
    /// </summary>
    public static void ApplySolidCeilingCollision(SamusState samus)
    {
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
    }


    /// <summary>
    /// Resolves an ordinary Morph-Ball downward collision through `$91:EA07` and
    /// `$91:F1FC`, including both automatic rebounds and the final grounded pose.
    /// </summary>
    /// <returns>True only when the collision ends grounded; false means another rebound.</returns>
    public static bool ApplyMorphBallLanding(SamusState samus)
    {
        if (!SamusPoseMetadata.IsAirborneMorphBallPose(samus.Pose) && !SamusPoseMetadata.IsGroundedMorphBallPose(samus.Pose))
            throw new InvalidOperationException($"Morph-Ball landing requires airborne pose $31/$32, not {samus.Pose}.");

        // The cartridge tests the sign of a word subtraction, not an unsigned
        // magnitude. Moonfall's negative velocity therefore lands without a rebound.
        if (samus.MorphBallBounceState == SamusBallBounceState.None &&
            unchecked((short)(samus.Kinematics.YSpeed.Whole - SamusMovementDefinitions.FirstBallBounceMinimumSpeed)) >= 0)
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
            samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed.WithWhole(unchecked((ushort)(SamusVerticalMotionDefinitions.BallBounceSpeed.Whole - 1)));
            return false;
        }

        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.Kinematics.YSpeed = default;
        SamusPoseId groundedPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.MorphBallGroundLeftPose
            : SamusPoseId.MorphBallGroundRightPose;
        ApplyMorphBallPoseChange(samus, groundedPose);
        // $91:F02D-$F033 clears base momentum only when the bounce handler returns
        // carry clear. Both airborne rebounds above retain their horizontal speed.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
        return true;
    }


    /// <summary>
    /// Resolves `$91:F25E` for Spring Ball. Holding Jump immediately calls the same
    /// cartridge-backed jump initializer as a grounded Spring Ball press; otherwise the
    /// low bounce byte advances through two rebounds while high byte `$0600` records the
    /// native Spring Ball bounce family.
    /// </summary>
    public static bool ApplySpringBallLanding(SamusState samus, ControllerButton controllerInput)
    {
        if (!SamusPoseMetadata.IsAirborneSpringBallPose(samus.Pose) && !SamusPoseMetadata.IsGroundedSpringBallPose(samus.Pose))
            throw new InvalidOperationException($"Spring-Ball landing requires pose $7D-$80, not {samus.Pose}.");

        if ((controllerInput & ControllerButton.A) != ControllerButton.None)
        {
            samus.MorphBallBounceState = SamusBallBounceState.None;
            SamusAerialMovement.InitializeJump(samus);
            // Native landing relaunch only initializes velocity. It retains the
            // current airborne pose/animation, unlike a fresh grounded Jump input.
            // Changing pose here also selects a different next-frame movement path.
            return false;
        }

        // $91:F26E dispatches on the low (rebound) byte only, so an ordinary-ball
        // rebound left over from a Spring Ball toggle selects the same rebound step.
        if (samus.MorphBallBounceState == SamusBallBounceState.None &&
            unchecked((short)(samus.Kinematics.YSpeed.Whole - SamusMovementDefinitions.FirstBallBounceMinimumSpeed)) >= 0)
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
            samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed.WithWhole(unchecked((ushort)(SamusVerticalMotionDefinitions.BallBounceSpeed.Whole - 1)));
            return false;
        }

        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.Kinematics.YSpeed = default;
        SamusPoseId groundPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.SpringBallGroundLeftPose
            : SamusPoseId.SpringBallGroundRightPose;
        ApplyMorphBallPoseChange(samus, groundPose);
        // The common landing caller clears base speed when the bounce routine
        // returns carry clear. Spring Ball shares this with ordinary ball landing.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
        return true;
    }


    /// <summary>
    /// Applies `$91:FC18` when grounded Spring Ball `$79/$7A` changes to `$7F/$80`.
    /// Moving-ground poses first pass through the same table-selected target, but the
    /// initializer only launches when the previous movement type was exactly `$11`.
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
    /// Applies `$91:E8F2` after a bomb-jump frame moved down without a floor. Only the pose
    /// changes: the airborne ball's initializer leaves vertical speed for the next
    /// handler frame, whose underflow test turns the jump downward. Movement types whose
    /// `$90:E65A` entry is "no change" keep their pose.
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
                $"Bomb-jump falling result for humanoid pose {samus.Pose} ($91:E8F2 airborne) is not translated.");
    }


    /// <summary>
    /// Applies `$91:E8F2`'s type-four walk-off endpoint, retaining the rolling animation
    /// while changing to ordinary airborne pose `$31/$32` and starting dry-air gravity.
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
