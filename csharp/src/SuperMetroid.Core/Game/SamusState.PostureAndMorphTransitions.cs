using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Posture, landing, crouch, Morph Ball, and Spring Ball pose transitions.
/// </summary>
public sealed partial class SamusState
{
    /// <summary>Applies the crouch input tables' direct same-facing exits ($27/$71/$73/$85 to $01, or $28/$72/$74/$86 to $02) after larger-pose collision admits the standing body.</summary>
    /// <param name="bus">Address space passed to native pose metadata, collision, and animation helpers.</param>
    /// <param name="level">Live room block geometry used by the shared pose-expansion resolver.</param>
    /// <param name="targetPose">Final standing-right $01 or standing-left $02 pose matching the current crouch facing; animated stand-up and cross-facing routes are not accepted.</param>
    /// <param name="nmiFrameCounter">Native NMI counter supplied to changed-pose block reactions.</param>
    /// <param name="plms">Optional live PLM owner receiving those collision reactions.</param>
    /// <returns>True when the final standing pose and animation are installed; false when collision retains the source or substitutes stable crouch, which may change an aimed crouch even on failure.</returns>
    /// <remarks>These six-byte input records bypass animated $F7–$FC stand-up poses. Accepted expansion applies the whole-pixel Y adjustment but retains the crouching radius until the next alpha pass, matching $91:FDAE's previous-pose radius publication.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="level"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source and target are not a same-facing crouch-to-final-standing input-table route.</exception>
    public bool TryApplyDirectCrouchToStandingTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == SamusPoseId.FacingRightNormalPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == SamusPoseId.FacingLeftNormalPose;
        if (!rightRoute && !leftRoute)
        {
            // This entry point is the exact `$27/$28 -> $01/$02` direct-exit route.
            // Animated stand-ups and cross-facing changes have separate initializers.
            throw new InvalidOperationException(
                $"Direct crouch exit ${(int)Pose:X2} -> ${(int)targetPose:X2} is not a ROM-table route.");
        }

        SamusPoseId sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
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
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        // `$91:FDAE` explicitly publishes the previous pose's radius while probing the
        // enlargement, and this direct transition does not call Samus_SetRadius afterward.
        // Ordinary alpha publishes the standing radius at the start of the next frame.
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
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
    public bool TryApplyCrouchJumpTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == SamusPoseId.NeutralJumpTransitionRightPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == SamusPoseId.NeutralJumpTransitionLeftPose;
        if (!rightRoute && !leftRoute)
        {
            // Only the two neutral `$4B/$4C` transition records enter this routine; aimed
            // crouches deliberately reuse the same facing-selected target.
            throw new InvalidOperationException(
                $"Crouch jump ${(int)Pose:X2} -> ${(int)targetPose:X2} is not a ROM-table route.");
        }

        SamusPoseId sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));

        // Alpha owns the new radius next frame; collision correction and jump setup
        // below must not publish it during this frame's pose-commit stage.
        // HandleJumpTransition_NormalJumping at $91:FC7D performs this after pose
        // initialization/collision but before Make_Samus_Jump. It writes only current Y;
        // the desktop state has no separately exposed PreviousYPosition word to mirror.
        if (sourcePose is SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingLeftPose)
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 10));

        InitializeAnimation(bus, initialFrame: 0);
        SamusAerialMovement.InitializeJump(bus, this);
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
    public bool TryApplyPostureTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool startsCrouchingRight =
            (IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
             IsMoonwalkingFacingRightPose(Pose) ||
             IsRightFacingRanIntoWallPose(Pose) ||
             IsRightFacingLandingPose(Pose)) &&
            targetPose is
                SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionAimUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose;
        bool startsCrouchingLeft =
            (IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
             IsMoonwalkingFacingLeftPose(Pose) ||
             IsLeftFacingRanIntoWallPose(Pose) ||
             IsLeftFacingLandingPose(Pose)) &&
            targetPose is
                SamusPoseId.CrouchingTransitionLeftPose or SamusPoseId.CrouchingTransitionAimUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose;
        bool startsCrouching = startsCrouchingRight || startsCrouchingLeft;
        bool startsStandingRight = IsRightFacingCrouchingPose(Pose) &&
            targetPose is
                SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionAimUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalDownRightPose;
        bool startsStandingLeft = IsLeftFacingCrouchingPose(Pose) &&
            targetPose is
                SamusPoseId.StandingTransitionLeftPose or SamusPoseId.StandingTransitionAimUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalDownLeftPose;
        bool startsStanding = startsStandingRight || startsStandingLeft;
        if (!startsCrouching && !startsStanding)
        {
            // `$91:F7B0/$F7D3` admit exactly the crouch/stand records enumerated above.
            throw new InvalidOperationException(
                $"Posture transition ${(int)Pose:X2} -> ${(int)targetPose:X2} is outside the crouch/stand ROM-table family.");
        }

        if (startsCrouching)
        {
            // `Samus_CrouchTrans` at `$91:F7B0` samples the high stage byte before later
            // posture movement can cancel running momentum. A stage-four crouch therefore
            // banks 180 palette-handler ticks even though the following stable crouch has
            // no horizontal Speed Booster state of its own.
            if (Shinespark.TryStoreFromSpeedBooster(HorizontalSpeed.SpeedBoostCounter))
            {
                // The successful native store replaces the shared palette handler
                // and timer even if an interrupted Flash was keeping them alive.
                CrystalFlash.RelinquishPaletteHandler();
                // Reserve Mode keeps only X-Ray's HDMA object. Crouch storage replaces
                // shared palette handler eight with the stored-shine handler one.
                Xray.RelinquishPaletteHandler();
            }

            Pose = targetPose;
            RefreshCollisionRadii(bus);

            // Prospective command seven reads five from $91:ED36, installs the target's
            // radius 16, then probes those five pixels down through $94:96AB. On level
            // ground the bottom boundary is unchanged; descending a slope, the probe meets
            // the surface first and the shorter clipped distance is what moves center Y.
            BlockMoveResult alignment = ProbeChangedPoseVertical(
                bus, level, SamusPostureDefinitions.CrouchEntryDownwardPixels << 16,
                (nmiFrameCounter & 1) == 0, targetPose, plms);
            Kinematics.YPosition = unchecked((ushort)(
                Kinematics.YPosition + (alignment.AcceptedDisplacement >> 16)));
            // Command seven publishes the aligned whole Y before scrolling; the
            // posture change itself must not become camera movement.
            WritePreviousYPosition(Kinematics.YPosition);
            InitializeAnimation(bus, initialFrame: 0);
            return true;
        }

        // Expanding from crouch radius 16 to standing-transition radius 21 invokes the
        // common pose-change collision routine before initialization. The shared helper
        // reads the target radius from this ROM rather than assuming the ordinary value 21.
        SamusPoseId sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        // `$91:FDAE` restores the source radius before returning. The prospective
        // standing-transition geometry was used only by the collision probes above;
        // ordinary alpha publishes the target radius at the start of the next frame.
        // Keeping that one-frame handoff is observable when the crouched body is resting
        // on a frozen enemy inside a ceiling, as in the Botwoon-pipe Mochtroid clip.
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
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
    public bool TryApplyMorphTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        // `$91:F7CE` does not require movement type five. Falling straight-down `$2D/$2E`
        // and spin-jump input tables can select the same `$37/$38` initializer. The literal
        // previous-movement-type test below even has a dedicated type-three side effect.
        // Validate only the facing-preserving relationship guaranteed by the ROM table.
        byte sourceDirection = ReadPoseXDirection(bus);
        bool startsMorphingRight = sourceDirection == 8 &&
            targetPose == SamusPoseId.MorphingTransitionRightPose;
        bool startsMorphingLeft = sourceDirection == 4 &&
            targetPose == SamusPoseId.MorphingTransitionLeftPose;
        bool startsMorphing = startsMorphingRight || startsMorphingLeft;
        bool startsUnmorphingRight =
            Pose is SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallMovingRightPose or
                SamusPoseId.MorphBallFallingRightPose or SamusPoseId.SpringBallGroundRightPose or
                SamusPoseId.SpringBallMovingRightPose or SamusPoseId.SpringBallFallingRightPose or
                SamusPoseId.SpringBallJumpRightPose &&
            targetPose == SamusPoseId.UnmorphingTransitionRightPose;
        bool startsUnmorphingLeft =
            Pose is SamusPoseId.MorphBallGroundLeftPose or SamusPoseId.MorphBallMovingLeftPose or
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
                $"Morph transition ${(int)Pose:X2} -> ${(int)targetPose:X2} is not a ROM-table route.");
        }

        if (startsMorphing)
        {
            // InitializeSamusPose_MorphingTransition at $91:F7CE restores PreviousPose and
            // returns carry set when bit $0004 is absent. No radius, animation, or position
            // write from the rejected prospective pose survives that native frame.
            if (!EquippedItems.HasAny(SamusEquipmentFlags.MorphBall))
                return false;

            SamusMovementType previousMovementType = ReadMovementType(bus);
            ushort previousRadius = Kinematics.YRadius;
            Pose = targetPose;
            RefreshCollisionRadii(bus);
            if (Kinematics.YRadius > previousRadius)
            {
                throw new InvalidDataException(
                    $"Morph entry ${(int)targetPose:X2} expanded radius {previousRadius} -> {Kinematics.YRadius}.");
            }
            BlockMoveResult alignment = ProbeChangedPoseVertical(
                bus, level, SamusPostureDefinitions.MorphEntryDownwardPixels << 16,
                (nmiFrameCounter & 1) == 0, targetPose, plms);
            Kinematics.YPosition = unchecked((ushort)(
                Kinematics.YPosition + (alignment.AcceptedDisplacement >> 16)));
            // Command seven ($91:ED0E) replaces the previous whole-Y checkpoint
            // after alignment, so the camera does not count pose displacement as motion.
            WritePreviousYPosition(Kinematics.YPosition);

            // `$91:F7D6-$F7E4` deliberately recognizes a spin-jump source and forces mode
            // two so the compact body retains decelerating aerial momentum after morphing.
            if (previousMovementType == SamusMovementType.SpinJumping)
                HorizontalSpeed.AccelerationMode = 2;

            // This is `$91:F7E7`, not the arm-cannon flare counter. Bomb-spread charging is
            // invalid once the body has entered either ordinary or Spring Ball form.
            BombSpreadChargeTimeoutCounter = 0;

            // Prospective command seven also cancels an active bounce before starting the
            // transition. Ordinary crouch entry normally sees zero, but retaining the
            // literal writes makes externally stimulated debugger states deterministic.
            CancelBounceForPostureTransition();
            InitializeAnimation(bus, initialFrame: 0);
            return true;
        }

        SamusPoseId sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
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
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        WritePreviousYPosition(Kinematics.YPosition);
        // Unmorph uses the same prospective command seven as morph. Its zero
        // alignment-table entry does not bypass the subsequent bounce cancellation.
        // The prospective collision probes used the target radius, but the live
        // word remains unchanged until alpha's Samus_SetRadius next frame. This
        // matters to the later enemy-projectile pickup overlap tests.
        CancelBounceForPostureTransition();
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>Shared bounce cancellation in native prospective command seven.</summary>
    private void CancelBounceForPostureTransition()
    {
        if (MorphBallBounceState == 0)
            return;
        MorphBallBounceState = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YDirection = 0;
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
    public bool TryApplyUnmorphToFallingTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        SamusPoseId sourcePose = Pose;
        bool rightFacingRoute = sourcePose == SamusPoseId.UnmorphingTransitionRightPose &&
            IsRightFacingFallingPose(targetPose);
        bool leftFacingRoute = sourcePose == SamusPoseId.UnmorphingTransitionLeftPose &&
            IsLeftFacingFallingPose(targetPose);
        if (!rightFacingRoute && !leftFacingRoute)
        {
            // `$91:A3F6/$91:A476` expose only the falling family matching the active
            // unmorph body's direction. Rejecting every other pairing keeps this helper
            // from silently becoming a catch-all for unrelated movement initializers.
            throw new InvalidOperationException(
                $"Unmorph-to-falling transition ${(int)sourcePose:X2} -> ${(int)targetPose:X2} is not a same-facing retail route.");
        }

        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
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
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));

        // InitializeAnimation resets only the target art stream. In particular, do not call
        // Make_Samus_Jump or clear YDirection/YSpeed/YSubspeed: the input record can occur
        // halfway through an actual fall, as it does when leaving Parlor's Morph Ball tunnel.
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Installs a stable ordinary Morph-Ball pose while preserving the shared eight-frame
    /// rolling animation exactly as <c>InitializeSamusPose_MorphBall</c> requests.
    /// </summary>
    public void ApplyMorphBallPoseChange(ISnesAddressSpace bus, SamusPoseId targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool sourceSupported = IsStableBallPose(Pose);
        bool targetSupported = IsStableBallPose(targetPose);
        if (!sourceSupported || !targetSupported)
        {
            throw new InvalidOperationException(
                $"Stable Morph-Ball transition ${(int)Pose:X2} -> ${(int)targetPose:X2} requires stable ball endpoints.");
        }

        byte previousDirection = ReadPoseXDirection(bus);
        int previousDelayList = AnimationDelayListAddress;
        Pose = targetPose;
        RefreshCollisionRadii(bus);

        // All ordinary ball poses point at `$91:B378`. Native writes $8000 to the new-frame
        // selector, causing `$91:FB5C` to return without changing frame OR timer. Assert the
        // table identity instead of depending on that retail-data fact silently.
        int targetDelayList = ResolveAnimationDelayList();
        if (previousDelayList != targetDelayList)
        {
            throw new InvalidDataException(
                $"Morph-Ball transition selected mismatched animation lists ${previousDelayList:X6}/${targetDelayList:X6}.");
        }

        byte currentDirection = ReadPoseXDirection(bus);
        bool reversed = (previousDirection == 8 && currentDirection == 4) ||
            (previousDirection == 4 && currentDirection == 8);
        if (reversed)
        {
            // `$91:FA32-$91:FA52` folds run momentum into base speed with one 16-bit carry,
            // clears the extra words, and selects mode one so displacement initially keeps
            // travelling in the old direction while the ball decelerates through its turn.
            uint combined = unchecked(HorizontalSpeed.BaseFixed +
                ((uint)HorizontalSpeed.ExtraRunSpeed << 16) +
                HorizontalSpeed.ExtraRunSubspeed);
            HorizontalSpeed.BaseSpeed = unchecked((ushort)(combined >> 16));
            HorizontalSpeed.BaseSubspeed = unchecked((ushort)combined);
            // `$91:FA4C` invokes `Samus_CancelSpeedBoost` between the 16.16 fold and the
            // explicit extra-word clear. Preserve that ordering even for ordinary Dash.
            HorizontalSpeed.CancelRunningMomentum(currentDirection);
            HorizontalSpeed.ExtraRunSpeed = 0;
            HorizontalSpeed.ExtraRunSubspeed = 0;
            HorizontalSpeed.AccelerationMode = 1;
        }
    }

    /// <summary>
    /// Applies the winning solid-ceiling transition at $91:EFDF. Call after higher
    /// priority animation and hit interruptions, not from the collision scan.
    /// </summary>
    public void ApplySolidCeilingCollision()
    {
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 2;
    }

    /// <summary>
    /// Resolves an ordinary Morph-Ball downward collision through `$91:EA07` and
    /// `$91:F1FC`, including both automatic rebounds and the final grounded pose.
    /// </summary>
    /// <returns>True only when the collision ends grounded; false means another rebound.</returns>
    public bool ApplyMorphBallLanding(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsAirborneMorphBallPose(Pose) && !IsGroundedMorphBallPose(Pose))
            throw new InvalidOperationException($"Morph-Ball landing requires airborne pose $31/$32, not ${(int)Pose:X2}.");

        // The cartridge tests the sign of a word subtraction, not an unsigned
        // magnitude. Moonfall's negative velocity therefore lands without a rebound.
        if (MorphBallBounceState == 0 &&
            unchecked((short)(Kinematics.YSpeed - SamusMovementRomData.FirstBallBounceMinimumSpeed)) >= 0)
        {
            MorphBallBounceState = 1;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed;
            Kinematics.YSubspeed = SamusVerticalMotionDefinitions.BallBounceSubspeed;
            return false;
        }

        if (MorphBallBounceState == 1)
        {
            MorphBallBounceState = 2;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(SamusVerticalMotionDefinitions.BallBounceSpeed - 1));
            Kinematics.YSubspeed = SamusVerticalMotionDefinitions.BallBounceSubspeed;
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        SamusPoseId groundedPose = IsFacingLeft(bus)
            ? SamusPoseId.MorphBallGroundLeftPose
            : SamusPoseId.MorphBallGroundRightPose;
        ApplyMorphBallPoseChange(bus, groundedPose);
        // $91:F02D-$F033 clears base momentum only when the bounce handler returns
        // carry clear. Both airborne rebounds above retain their horizontal speed.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        return true;
    }

    /// <summary>
    /// Resolves `$91:F25E` for Spring Ball. Holding Jump immediately calls the same
    /// cartridge-backed jump initializer as a grounded Spring Ball press; otherwise the
    /// low bounce byte advances through two rebounds while high byte `$0600` records the
    /// native Spring Ball bounce family.
    /// </summary>
    public bool ApplySpringBallLanding(ISnesAddressSpace bus, ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsAirborneSpringBallPose(Pose) && !IsGroundedSpringBallPose(Pose))
            throw new InvalidOperationException($"Spring-Ball landing requires pose $7D-$80, not ${(int)Pose:X2}.");

        if ((controllerInput & (ushort)SnesButton.A) != 0)
        {
            MorphBallBounceState = 0;
            SamusAerialMovement.InitializeJump(bus, this);
            // Native landing relaunch only initializes velocity. It retains the
            // current airborne pose/animation, unlike a fresh grounded Jump input.
            // Changing pose here also selects a different next-frame movement path.
            return false;
        }

        byte bounce = unchecked((byte)MorphBallBounceState);
        if (bounce == 0 &&
            unchecked((short)(Kinematics.YSpeed - SamusMovementRomData.FirstBallBounceMinimumSpeed)) >= 0)
        {
            MorphBallBounceState = 0x0601;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed;
            Kinematics.YSubspeed = SamusVerticalMotionDefinitions.BallBounceSubspeed;
            return false;
        }

        if (bounce == 1)
        {
            MorphBallBounceState = 0x0602;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(SamusVerticalMotionDefinitions.BallBounceSpeed - 1));
            Kinematics.YSubspeed = SamusVerticalMotionDefinitions.BallBounceSubspeed;
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        SamusPoseId groundPose = IsFacingLeft(bus)
            ? SamusPoseId.SpringBallGroundLeftPose
            : SamusPoseId.SpringBallGroundRightPose;
        ApplyMorphBallPoseChange(bus, groundPose);
        // The common landing caller clears base speed when the bounce routine
        // returns carry clear. Spring Ball shares this with ordinary ball landing.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        return true;
    }

    /// <summary>
    /// Applies `$91:FC18` when grounded Spring Ball `$79/$7A` changes to `$7F/$80`.
    /// Moving-ground poses first pass through the same table-selected target, but the
    /// initializer only launches when the previous movement type was exactly `$11`.
    /// </summary>
    public void ApplySpringBallJump(ISnesAddressSpace bus, SamusPoseId targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool validTarget = targetPose is SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose;
        if (!IsGroundedSpringBallPose(Pose) || !validTarget)
            throw new InvalidOperationException(
                $"Spring-Ball jump ${(int)Pose:X2} -> ${(int)targetPose:X2} is outside the grounded-to-airborne Spring-Ball route.");

        ApplyMorphBallPoseChange(bus, targetPose);
        MorphBallBounceState = 0;
        SamusAerialMovement.InitializeJump(bus, this);
    }

    /// <summary>
    /// Applies `$91:E8F2` after a bomb-jump frame moved down without a floor. Only the pose
    /// changes: the airborne ball's initializer leaves vertical speed for the next
    /// handler frame, whose underflow test turns the jump downward. Movement types whose
    /// `$90:E65A` entry is "no change" keep their pose.
    /// </summary>
    public void ApplyBombJumpFallingPose(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool left = IsFacingLeft(bus);
        if (IsGroundedSpringBallPose(Pose))
            ApplyMorphBallPoseChange(bus, left ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.SpringBallFallingRightPose);
        else if (IsGroundedMorphBallPose(Pose))
            ApplyMorphBallPoseChange(bus, left ? SamusPoseId.MorphBallFallingLeftPose : SamusPoseId.MorphBallFallingRightPose);
        else if (ReadMovementType(bus) is SamusMovementType.Standing or SamusMovementType.Running or
                 SamusMovementType.Crouching or SamusMovementType.Moonwalking or SamusMovementType.RanIntoWall)
            throw new InvalidOperationException(
                $"Bomb-jump falling result for humanoid pose ${(int)Pose:X2} ($91:E8F2 airborne) is not translated.");
    }

    /// <summary>
    /// Applies `$91:E8F2`'s type-four walk-off endpoint, retaining the rolling animation
    /// while changing to ordinary airborne pose `$31/$32` and starting dry-air gravity.
    /// </summary>
    public void ApplyMorphBallWalkOff(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool springBall = IsGroundedSpringBallPose(Pose);
        if (!IsGroundedMorphBallPose(Pose) && !springBall)
            throw new InvalidOperationException($"Morph-Ball walk-off requires grounded pose, not ${(int)Pose:X2}.");

        SamusPoseId fallingPose = IsFacingLeft(bus)
            ? springBall ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.MorphBallFallingLeftPose
            : springBall ? SamusPoseId.SpringBallFallingRightPose : SamusPoseId.MorphBallFallingRightPose;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 2;
        SamusAerialMovement.ConfigureEnvironmentGravity(bus, this);
        ApplyMorphBallPoseChange(bus, fallingPose);
    }

    /// <summary>
    /// Installs the ordinary or aimed falling pose selected by <c>$91:E8F2</c> when a
    /// grounded movement probe finds no floor. The collision command clears vertical
    /// speed and starts downward gravity before the pose is drawn.
    /// </summary>
    public void ApplyWalkedOffFloorTransition(
        ISnesAddressSpace bus, RoomLevelData level, SamusPoseId targetPose,
        ushort nmiFrameCounter = 0, RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        bool supportedSource =
            IsRightFacingStandingPose(Pose) || IsLeftFacingStandingPose(Pose) ||
            IsRightFacingLandingPose(Pose) || IsLeftFacingLandingPose(Pose) ||
            IsRightFacingRunningPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsMoonwalkingPose(Pose) ||
            IsMoonwalkTurnJumpPose(Pose) ||
            Pose is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose ||
            IsRanIntoWallPose(Pose) ||
            IsRightFacingCrouchingPose(Pose) || IsLeftFacingCrouchingPose(Pose);
        SamusPoseId expectedTarget = SelectFallingPoseForCurrentAim(bus);
        if (!supportedSource || targetPose != expectedTarget)
        {
            // The caller computes the target from this pose's ROM metadata immediately
            // before invoking us. A mismatch is an inconsistent request, not missing logic.
            throw new InvalidOperationException(
                $"Walk-off transition ${(int)Pose:X2} -> ${(int)targetPose:X2} does not match the pose's ROM-selected falling target ${(int)expectedTarget:X2}.");
        }

        // A crouched or compact source can grow when falling is selected. Native
        // F404 runs the same pose-expansion collision as an input-driven transition,
        // including its fractional clamp. Skipping it embeds the new body in the
        // floor after a near-ground unmorph. Rejection also skips command five.
        SamusPoseId sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus, level, targetPose, nmiFrameCounter, plms, out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return;
        }
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        // Command five's no-floor branch ($91:EFEF) preserves an ascending
        // velocity, even if a special handler left it on a standing pose. Crystal
        // Flash can do exactly that; unconditional reset shortens its return arc.
        if (Kinematics.YDirection != 1)
        {
            MorphBallBounceState = 0;
            Kinematics.YSpeed = 0;
            Kinematics.YSubspeed = 0;
            Kinematics.YDirection = 2;
        }
        SamusAerialMovement.ConfigureEnvironmentGravity(bus, this);
        Pose = targetPose;
        // Alpha publishes the falling radius next frame; pose commit retains the
        // radius used by this frame's grounded movement and collision probes.
        // SamusFunc_F433 dispatches the new falling movement type through $91:F60D
        // before command five initializes the downward state. That initializer derives
        // the mode from extra dash speed; it must not retain the grounded release mode.
        // Base speed itself survives until the next aerial movement routine consumes it.
        InitializeOrdinaryAerialAcceleration();
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Ports the facing lookup used by `$91:E8F2` for a grounded walk-off.
    /// </summary>
    public SamusPoseId SelectFallingPoseForCurrentAim(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        // PSP_Falling indexes its pair with PoseXDirection, not the muzzle direction.
        // Moonwalk's visual facing opposes this physics byte; aim input is reconsidered
        // by the next frame's input handler after this ordinary falling pose is installed.
        return IsFacingLeft(bus) ? SamusPoseId.FallingLeftPose : SamusPoseId.FallingRightPose;
    }

    /// <summary>
    /// Applies <c>$91:E95D</c>'s normal/spin/aimed landing choice in a scripted context that
    /// is known to have room for the destination pose, followed by grounded collision cleanup
    /// at <c>$91:F010</c>.
    /// </summary>
    /// <remarks>
    /// Playable room code must call <see cref="TryApplyAerialLanding"/> instead. The cartridge
    /// sends every prospective landing pose through <c>HandleCollDueToChangedPose</c>; omitting
    /// that probe lets the much taller landing body overlap a low ceiling. This roomless entry
    /// point remains for cinematic movement and focused collision-free fixtures only.
    /// </remarks>
    public void ApplyAerialLanding(
        ISnesAddressSpace bus,
        bool wasSpinning,
        ushort controllerInput = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        SamusPoseId sourcePose = Pose;
        bool leavingScrewAttack = IsScrewAttackPose(sourcePose);
        SamusPoseId targetPose = SelectAerialLandingPose(bus, wasSpinning, controllerInput);

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius > oldRadius)
        {
            ushort difference = unchecked((ushort)(Kinematics.YRadius - oldRadius));
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - difference));
        }

        ApplyAerialLandingCollisionCommand(leavingScrewAttack);
        InitializeAnimation(bus, initialFrame: StandingPoseInitialFrame(bus, sourcePose, targetPose));
    }

    /// <summary>
    /// Applies a normal or spinning landing through the cartridge's shared prospective-pose
    /// collision handler before committing the taller landing body.
    /// </summary>
    /// <returns>
    /// True when the selected landing pose fit. False means native collision retained the
    /// source pose or selected its stable-crouch fallback; native skips the landing command.
    /// </returns>
    public bool TryApplyAerialLanding(
        ISnesAddressSpace bus,
        RoomLevelData level,
        bool wasSpinning,
        ushort controllerInput,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        SamusPoseId sourcePose = Pose;
        bool leavingScrewAttack = IsScrewAttackPose(sourcePose);
        SamusPoseId targetPose = SelectAerialLandingPose(bus, wasSpinning, controllerInput);
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            Pose = targetPose;
            // Correct the center now, but retain the movement frame's live radius
            // until alpha, including when landing on a frozen enemy.
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
            InitializeAnimation(bus, initialFrame: StandingPoseInitialFrame(bus, sourcePose, targetPose));
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            // `$91:FFA7` deliberately chooses stable crouch for a non-Morph source when
            // both the floor and ceiling reject the full landing body. This is what lets
            // a spin jump land in a 32-pixel passage without embedding Samus in its ceiling.
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
        }

        // F404 compares the requested pose with the post-collision pose. A rejected
        // expansion or crouch substitution returns carry and skips command five; this
        // is not an accepted landing and must retain the vertical motion words.
        if (collision == LargerPoseCollisionOutcome.Allowed)
            ApplyAerialLandingCollisionCommand(leavingScrewAttack);
        else if (leavingScrewAttack)
            // Palette restoration belongs to F433, before the command carry gate.
            HorizontalSpeed.RequestNormalSuitPaletteRestore();
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    private static ushort StandingPoseInitialFrame(ISnesAddressSpace bus, SamusPoseId sourcePose, SamusPoseId targetPose)
    {
        // $91:F4DC keeps the gun raised when both the old and new standing
        // initializer's poses aim straight up, including landing and its completion.
        return ReadMovementType(bus, targetPose) == SamusMovementType.Standing &&
            ReadShotDirection(bus, sourcePose) is 0 or 9 &&
            ReadShotDirection(bus, targetPose) is 0 or 9 ? (ushort)1 : (ushort)0;
    }

    /// <summary>Chooses <c>$91:E95D</c>'s prospective landing pose.</summary>
    private SamusPoseId SelectAerialLandingPose(
        ISnesAddressSpace bus,
        bool wasSpinning,
        ushort controllerInput)
    {
        byte direction = ReadPoseXDirection(bus);
        bool facingLeft = direction == 4;
        if (wasSpinning)
        {
            return facingLeft
                ? SamusPoseId.SpinLandingLeftPose
                : SamusPoseId.SpinLandingRightPose;
        }

        // `$91:E95D` checks `$FF` before indexing `$91:E9F3`; hurt/damage-boost art
        // deliberately stores that sentinel and lands through the ordinary facing pair.
        // Horizontal directions two/seven take `$91:E96E-$E98F`'s extra Shot-binding
        // test. Held Shot selects firing landing `$E6/$E7`; released Shot selects the
        // ordinary `$A4/$A5` pair. The six admitted aim directions select `$E0-$E5`.
        bool shotHeld = (controllerInput & (ushort)SnesButton.X) != 0;
        return ReadShotDirection(bus) switch
        {
            0 => SamusPoseId.LandingAimUpRightPose,
            1 => SamusPoseId.LandingAimDiagonalUpRightPose,
            2 => shotHeld ? SamusPoseId.FiringLandingRightPose : SamusPoseId.NormalLandingRightPose,
            3 => SamusPoseId.LandingAimDiagonalDownRightPose,
            6 => SamusPoseId.LandingAimDiagonalDownLeftPose,
            7 => shotHeld ? SamusPoseId.FiringLandingLeftPose : SamusPoseId.NormalLandingLeftPose,
            8 => SamusPoseId.LandingAimDiagonalUpLeftPose,
            9 => SamusPoseId.LandingAimUpLeftPose,
            SamusMovementRomData.Poses.NoShotDirection => facingLeft ? SamusPoseId.NormalLandingLeftPose : SamusPoseId.NormalLandingRightPose,
            // Directions four/five are consumed by TryApplyCompactAerialLanding,
            // because the 10 -> 21 expansion needs room collision data.
            byte shotDirection => throw new InvalidOperationException(
                $"Landing from shot direction ${shotDirection:X2} requires the collision-aware compact landing operation."),
        };
    }

    /// <summary>Applies collision command five at <c>$91:F010</c>.</summary>
    private void ApplyAerialLandingCollisionCommand(bool leavingScrewAttack)
    {
        // $91:F1EC installs the same one-shot input handler used by F8.
        // Its short held-Jump window is evaluated next alpha, not during landing.
        if (!InputLocked) AutoJumpInputPending = true;
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;

        // `$91:F433` reloads the normal suit palette whenever a pose change leaves spin or
        // wall-jump movement while Screw Attack is equipped. The host palette copy occurs
        // at the later `$91:D6F7` phase, so publish that same deferred work here.
        if (leavingScrewAttack)
            HorizontalSpeed.RequestNormalSuitPaletteRestore();
        Kinematics.YDirection = 0;
    }

    /// <summary>
    /// Applies `$91:E9F3` directions four/five when radius-ten straight-down Samus lands.
    /// Both entries select ordinary `$A4/$A5`; the 10 -> 21 expansion still runs the full
    /// block pose-change collision resolver; command five clears motion only if accepted.
    /// </summary>
    public bool TryApplyCompactAerialLanding(
        ISnesAddressSpace bus,
        RoomLevelData level,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        if (!IsCompactAerialPose(Pose))
            throw new InvalidOperationException($"Compact landing requires pose $17/$18/$2D/$2E, not ${(int)Pose:X2}.");

        SamusPoseId sourcePose = Pose;
        SamusPoseId targetPose = ReadShotDirection(bus) switch
        {
            4 => SamusPoseId.NormalLandingRightPose,
            5 => SamusPoseId.NormalLandingLeftPose,
            byte shotDirection => throw new InvalidOperationException(
                $"Compact pose ${(int)Pose:X2} has unexpected shot direction ${shotDirection:X2}."),
        };
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            Pose = targetPose;
            // $91:FDAE leaves $0B00 at the compact body's radius: this frame's enemy and
            // projectile collision still use it, and alpha's SetSamusRadius takes the
            // landing body only next frame, as for an ordinary landing.
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
            InitializeAnimation(bus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
        }

        // Like ordinary landings, F404's changed-pose result gates command five.
        // A collision-selected crouch is not allowed to clear velocity or arm autojump.
        if (collision == LargerPoseCollisionOutcome.Allowed)
            ApplyAerialLandingCollisionCommand(leavingScrewAttack: false);
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Consumes command $FD/$F8's command-three pose operand for every animation route in
    /// the current grounded/ordinary-air slice.
    /// </summary>
    public bool ApplyPendingVerifiedAnimationTransition(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (PendingTransitionalPose is not SamusPoseId targetPose)
            return false;

        // Native pose initialization computes its own local delay without replacing
        // the buffer published by this frame's FX pass. Command three adds that live
        // buffer once more after installing the new animation.
        ushort commandAnimationBuffer = AnimationFrameBuffer;

        bool verified = (Pose, targetPose) is
            (SamusPoseId.TurningRightToLeftPose, SamusPoseId.FacingLeftNormalPose) or
            (SamusPoseId.TurningLeftToRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.TurningRightToLeftAimUpPose, SamusPoseId.StandingAimUpLeftPose) or
            (SamusPoseId.TurningLeftToRightAimUpPose, SamusPoseId.StandingAimUpRightPose) or
            (SamusPoseId.TurningRightToLeftAimDiagonalDownPose, SamusPoseId.StandingAimDiagonalDownLeftPose) or
            (SamusPoseId.TurningLeftToRightAimDiagonalDownPose, SamusPoseId.StandingAimDiagonalDownRightPose) or
            (SamusPoseId.TurningRightToLeftAimDiagonalUpPose, SamusPoseId.StandingAimDiagonalUpLeftPose) or
            (SamusPoseId.TurningLeftToRightAimDiagonalUpPose, SamusPoseId.StandingAimDiagonalUpRightPose) or
            (SamusPoseId.TurningRightToLeftCrouchingPose, SamusPoseId.CrouchingLeftPose) or
            (SamusPoseId.TurningLeftToRightCrouchingPose, SamusPoseId.CrouchingRightPose) or
            (SamusPoseId.TurningRightToLeftCrouchingAimUpPose, SamusPoseId.CrouchingAimUpLeftPose) or
            (SamusPoseId.TurningLeftToRightCrouchingAimUpPose, SamusPoseId.CrouchingAimUpRightPose) or
            (SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose, SamusPoseId.CrouchingAimDiagonalDownLeftPose) or
            (SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose, SamusPoseId.CrouchingAimDiagonalDownRightPose) or
            (SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose, SamusPoseId.CrouchingAimDiagonalUpLeftPose) or
            (SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose, SamusPoseId.CrouchingAimDiagonalUpRightPose) or
            (SamusPoseId.NeutralJumpTransitionRightPose, SamusPoseId.NeutralJumpRightPose) or
            (SamusPoseId.NeutralJumpTransitionLeftPose, SamusPoseId.NeutralJumpLeftPose) or
            (SamusPoseId.NormalJumpTransitionAimUpRightPose, SamusPoseId.NormalJumpAimUpRightPose) or
            (SamusPoseId.NormalJumpTransitionAimUpLeftPose, SamusPoseId.NormalJumpAimUpLeftPose) or
            (SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose, SamusPoseId.NormalJumpAimDiagonalUpRightPose) or
            (SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose, SamusPoseId.NormalJumpAimDiagonalUpLeftPose) or
            (SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose, SamusPoseId.NormalJumpAimDiagonalDownRightPose) or
            (SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose, SamusPoseId.NormalJumpAimDiagonalDownLeftPose) or
            (SamusPoseId.CrouchingTransitionRightPose, SamusPoseId.CrouchingRightPose) or
            (SamusPoseId.CrouchingTransitionLeftPose, SamusPoseId.CrouchingLeftPose) or
            (SamusPoseId.StandingTransitionRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.StandingTransitionLeftPose, SamusPoseId.FacingLeftNormalPose) or
            (SamusPoseId.CrouchingTransitionAimUpRightPose, SamusPoseId.CrouchingAimUpRightPose) or
            (SamusPoseId.CrouchingTransitionAimUpLeftPose, SamusPoseId.CrouchingAimUpLeftPose) or
            (SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose, SamusPoseId.CrouchingAimDiagonalUpRightPose) or
            (SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose, SamusPoseId.CrouchingAimDiagonalUpLeftPose) or
            (SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose, SamusPoseId.CrouchingAimDiagonalDownRightPose) or
            (SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose, SamusPoseId.CrouchingAimDiagonalDownLeftPose) or
            (SamusPoseId.StandingTransitionAimUpRightPose, SamusPoseId.StandingAimUpRightPose) or
            (SamusPoseId.StandingTransitionAimUpLeftPose, SamusPoseId.StandingAimUpLeftPose) or
            (SamusPoseId.StandingTransitionAimDiagonalUpRightPose, SamusPoseId.StandingAimDiagonalUpRightPose) or
            (SamusPoseId.StandingTransitionAimDiagonalUpLeftPose, SamusPoseId.StandingAimDiagonalUpLeftPose) or
            (SamusPoseId.StandingTransitionAimDiagonalDownRightPose, SamusPoseId.StandingAimDiagonalDownRightPose) or
            (SamusPoseId.StandingTransitionAimDiagonalDownLeftPose, SamusPoseId.StandingAimDiagonalDownLeftPose) or
            (SamusPoseId.MorphingTransitionRightPose, SamusPoseId.MorphBallGroundRightPose) or
            (SamusPoseId.MorphingTransitionRightPose, SamusPoseId.MorphBallFallingRightPose) or
            (SamusPoseId.MorphingTransitionRightPose, SamusPoseId.SpringBallGroundRightPose) or
            (SamusPoseId.MorphingTransitionRightPose, SamusPoseId.SpringBallFallingRightPose) or
            (SamusPoseId.MorphingTransitionLeftPose, SamusPoseId.MorphBallGroundLeftPose) or
            (SamusPoseId.MorphingTransitionLeftPose, SamusPoseId.MorphBallFallingLeftPose) or
            (SamusPoseId.MorphingTransitionLeftPose, SamusPoseId.SpringBallGroundLeftPose) or
            (SamusPoseId.MorphingTransitionLeftPose, SamusPoseId.SpringBallFallingLeftPose) or
            (SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.CrouchingRightPose) or
            (SamusPoseId.UnmorphingTransitionLeftPose, SamusPoseId.CrouchingLeftPose) or
            (SamusPoseId.NormalLandingRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.NormalLandingLeftPose, SamusPoseId.FacingLeftNormalPose) or
            (SamusPoseId.SpinLandingRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.SpinLandingLeftPose, SamusPoseId.FacingLeftNormalPose) or
            (SamusPoseId.LandingAimUpRightPose, SamusPoseId.StandingAimUpRightPose) or
            (SamusPoseId.LandingAimUpLeftPose, SamusPoseId.StandingAimUpLeftPose) or
            (SamusPoseId.LandingAimDiagonalUpRightPose, SamusPoseId.StandingAimDiagonalUpRightPose) or
            (SamusPoseId.LandingAimDiagonalUpLeftPose, SamusPoseId.StandingAimDiagonalUpLeftPose) or
            (SamusPoseId.LandingAimDiagonalDownRightPose, SamusPoseId.StandingAimDiagonalDownRightPose) or
            (SamusPoseId.LandingAimDiagonalDownLeftPose, SamusPoseId.StandingAimDiagonalDownLeftPose) or
            // `$91:B22D/$B231` is shared by ordinary and firing landings. `$E6/$E7`
            // therefore executes the literal same `$F8,$01/$02` terminal operands.
            (SamusPoseId.FiringLandingRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.FiringLandingLeftPose, SamusPoseId.FacingLeftNormalPose) or
            // Every aerial-turn delay list ends in command `$F8 pp`. These pairs are the
            // literal operands from `$91:B3ED-$91:B490`, not inferred mirror poses.
            (SamusPoseId.TurningRightToLeftJumpPose, SamusPoseId.NormalJumpForwardLeftPose) or
            (SamusPoseId.TurningLeftToRightJumpPose, SamusPoseId.NormalJumpForwardRightPose) or
            (SamusPoseId.TurningRightToLeftFallingPose, SamusPoseId.FallingLeftPose) or
            (SamusPoseId.TurningLeftToRightFallingPose, SamusPoseId.FallingRightPose) or
            (SamusPoseId.TurningRightToLeftJumpAimUpPose, SamusPoseId.NormalJumpAimUpLeftPose) or
            (SamusPoseId.TurningLeftToRightJumpAimUpPose, SamusPoseId.NormalJumpAimUpRightPose) or
            (SamusPoseId.TurningRightToLeftJumpAimDownPose, SamusPoseId.NormalJumpAimDownLeftPose) or
            (SamusPoseId.TurningLeftToRightJumpAimDownPose, SamusPoseId.NormalJumpAimDownRightPose) or
            (SamusPoseId.TurningRightToLeftFallingAimUpPose, SamusPoseId.FallingAimUpLeftPose) or
            (SamusPoseId.TurningLeftToRightFallingAimUpPose, SamusPoseId.FallingAimUpRightPose) or
            (SamusPoseId.TurningRightToLeftFallingAimDownPose, SamusPoseId.FallingAimDownLeftPose) or
            (SamusPoseId.TurningLeftToRightFallingAimDownPose, SamusPoseId.FallingAimDownRightPose) or
            (SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose, SamusPoseId.NormalJumpAimDiagonalUpLeftPose) or
            (SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose, SamusPoseId.NormalJumpAimDiagonalUpRightPose) or
            (SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose, SamusPoseId.FallingAimDiagonalUpLeftPose) or
            (SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose, SamusPoseId.FallingAimDiagonalUpRightPose) or
            // `$91:B45B-$B478` ends every moonwalk turn/jump delay list with command
            // `$F8,$1A/$19`. Super-special selection runs F433, but skips F404's
            // later jump initialization. Its unchanged vertical direction permits Moonfall.
            (SamusPoseId.MoonwalkTurnJumpLeftPose or SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimDownLeftPose, SamusPoseId.SpinJumpLeftPose) or
            (SamusPoseId.MoonwalkTurnJumpRightPose or SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimDownRightPose, SamusPoseId.SpinJumpRightPose) or
            // `$91:B545/$B556` terminate Crystal Flash finish art in `$FD,$01/$02`.
            // Its installed movement handler observes type zero on the following frame.
            (SamusPoseId.CrystalFlashRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.CrystalFlashLeftPose, SamusPoseId.FacingLeftNormalPose) or
            // All four drained release streams eventually publish ordinary standing art.
            // These are literal `$FD` operands from `$91:B257-$B298`.
            (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedStandingRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.DrainedCrouchingLeftPose or SamusPoseId.DrainedStandingLeftPose, SamusPoseId.FacingLeftNormalPose);
        if (!verified)
        {
            // The allowlist above is the exhaustive set of `$F8/$FD` operands referenced
            // by active retail animation streams. Anything else means the caller supplied
            // a target that was not read from the current pose's animation bytecode.
            throw new InvalidOperationException(
                $"Animation transition ${(int)Pose:X2} -> ${(int)targetPose:X2} is not an active retail animation-command route.");
        }

        bool startsMoonwalkJump = IsMoonwalkTurnJumpPose(Pose) &&
            targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose;
        SamusPoseId sourcePose = Pose;
        SamusMovementType previousMovementType = ReadMovementType(bus, sourcePose);

        // `$91:F404` runs movement-type initialization after the animation command has
        // selected the new normal-jump pose but before it initializes that pose's frame.
        // A live stored shine replaces the target with `$C7/$C8`, so never briefly seed
        // `$4D/$4E/$15/$16/$69/$6A` animation state in this branch.
        bool beganShinespark = TryBeginShinesparkWindup(
            bus,
            targetPose,
            previousMovementType);
        if (!beganShinespark)
        {
            // Moonwalk turn lists end in literal `$F8,$19/$1A`. That operand is still fed
            // through movement-type initialization, so equipment substitution occurs here
            // just as it does for an input-table jump from ordinary running.
            SamusPoseId installedPose = startsMoonwalkJump
                ? SelectEquippedSpinPose(targetPose)
                : targetPose;
            // Animation-owned pose changes still run the target movement initializer.
            // In particular an aerial turn must stop decelerating in the old direction
            // when its final normal-jump/falling pose is installed, even if water made
            // the turn finish before base speed reached zero. Do not clear that speed.
            if (ReadMovementType(bus, installedPose) is SamusMovementType.NormalJumping or SamusMovementType.Falling)
                InitializeOrdinaryAerialAcceleration();
            ApplySimpleGroundedPoseChange(bus, sourcePose, installedPose, "Animation command",
                refreshRadius: !IsAerialTurnPose(sourcePose));
        }
        if (sourcePose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or
            SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
        {
            // The actor-owned release command has now reached its ROM-authored normal pose.
            // Clear only the host handler marker; pose initialization above already owns the
            // same radius and animation writes as native `$91:F404/$91:FB08`.
            Drained.CompleteRelease();
        }
        // Super-special command three does not run FBBB/FC99. An input-table
        // transition out of the same turn art DOES initialize jump velocity; keeping
        // those two routes distinct preserves both ordinary jumping and Moonfall.
        MorphBallBounceState = 0;
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameTimer + commandAnimationBuffer));
        return true;
    }

    /// <summary>
    /// Installs one already-validated grounded pose and runs the common $91:F404/$91:FB08
    /// metadata/radius work, initializing frame zero only for a changed pose.
    /// </summary>
    private void ApplySimpleGroundedPoseChange(
        ISnesAddressSpace bus,
        SamusPoseId expectedPose,
        SamusPoseId targetPose,
        string transitionName,
        bool refreshRadius = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != expectedPose)
        {
            throw new InvalidOperationException(
                $"{transitionName} requires pose ${(int)expectedPose:X2}, not ${(int)Pose:X2}.");
        }

        Pose = targetPose;
        // Aerial turn completion runs F433 without SetRadius; its source body
        // remains active until next alpha, including compact down-aim targets.
        if (refreshRadius)
            RefreshCollisionRadii(bus);
        // $91:FB64-$FB67 retains the frame/timer when the ordinary pose is
        // unchanged. A prospective run rejected by the wall probe can resolve
        // back to the current wall-stop pose on every held-input frame.
        if (expectedPose != targetPose)
            InitializeAnimation(bus, initialFrame: StandingPoseInitialFrame(bus, expectedPose, targetPose));
    }

}
