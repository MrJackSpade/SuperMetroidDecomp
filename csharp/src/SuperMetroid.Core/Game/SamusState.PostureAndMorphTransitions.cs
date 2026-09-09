using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Posture, landing, crouch, Morph Ball, and Spring Ball pose transitions.
/// </summary>
public sealed partial class SamusState
{
    public bool TryApplyDirectCrouchToStandingTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == SamusPoseIds.FacingRightNormalPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == SamusPoseIds.FacingLeftNormalPose;
        if (!rightRoute && !leftRoute)
        {
            // This entry point is the exact `$27/$28 -> $01/$02` direct-exit route.
            // Animated stand-ups and cross-facing changes have separate initializers.
            throw new InvalidOperationException(
                $"Direct crouch exit ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
        }

        byte sourcePose = Pose;
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
        RefreshCollisionRadii(bus);
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
        byte targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == SamusPoseIds.NeutralJumpTransitionRightPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == SamusPoseIds.NeutralJumpTransitionLeftPose;
        if (!rightRoute && !leftRoute)
        {
            // Only the two neutral `$4B/$4C` transition records enter this routine; aimed
            // crouches deliberately reuse the same facing-selected target.
            throw new InvalidOperationException(
                $"Crouch jump ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
        }

        byte sourcePose = Pose;
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
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));

        // HandleJumpTransition_NormalJumping at $91:FC7D performs this after pose
        // initialization/collision but before Make_Samus_Jump. It writes only current Y;
        // the desktop state has no separately exposed PreviousYPosition word to mirror.
        if (sourcePose is SamusPoseIds.CrouchingRightPose or SamusPoseIds.CrouchingLeftPose)
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
        byte targetPose,
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
                SamusPoseIds.CrouchingTransitionRightPose or SamusPoseIds.CrouchingTransitionAimUpRightPose or
                SamusPoseIds.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseIds.CrouchingTransitionAimDiagonalDownRightPose;
        bool startsCrouchingLeft =
            (IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
             IsMoonwalkingFacingLeftPose(Pose) ||
             IsLeftFacingRanIntoWallPose(Pose) ||
             IsLeftFacingLandingPose(Pose)) &&
            targetPose is
                SamusPoseIds.CrouchingTransitionLeftPose or SamusPoseIds.CrouchingTransitionAimUpLeftPose or
                SamusPoseIds.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseIds.CrouchingTransitionAimDiagonalDownLeftPose;
        bool startsCrouching = startsCrouchingRight || startsCrouchingLeft;
        bool startsStandingRight = IsRightFacingCrouchingPose(Pose) &&
            targetPose is
                SamusPoseIds.StandingTransitionRightPose or SamusPoseIds.StandingTransitionAimUpRightPose or
                SamusPoseIds.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseIds.StandingTransitionAimDiagonalDownRightPose;
        bool startsStandingLeft = IsLeftFacingCrouchingPose(Pose) &&
            targetPose is
                SamusPoseIds.StandingTransitionLeftPose or SamusPoseIds.StandingTransitionAimUpLeftPose or
                SamusPoseIds.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseIds.StandingTransitionAimDiagonalDownLeftPose;
        bool startsStanding = startsStandingRight || startsStandingLeft;
        if (!startsCrouching && !startsStanding)
        {
            // `$91:F7B0/$F7D3` admit exactly the crouch/stand records enumerated above.
            throw new InvalidOperationException(
                $"Posture transition ${Pose:X2} -> ${targetPose:X2} is outside the crouch/stand ROM-table family.");
        }

        if (startsCrouching)
        {
            // `Samus_CrouchTrans` at `$91:F7B0` samples the high stage byte before later
            // posture movement can cancel running momentum. A stage-four crouch therefore
            // banks 180 palette-handler ticks even though the following stable crouch has
            // no horizontal Speed Booster state of its own.
            Shinespark.TryStoreFromSpeedBooster(HorizontalSpeed.SpeedBoostCounter);

            Pose = targetPose;
            RefreshCollisionRadii(bus);

            // Prospective command seven reads five from $91:ED36, installs the target's
            // radius 16, then moves center Y down five. Old radius 21 and new radius 16
            // therefore share exactly the same bottom collision boundary.
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + 5));
            InitializeAnimation(bus, initialFrame: 0);
            return true;
        }

        // Expanding from crouch radius 16 to standing-transition radius 21 invokes the
        // common pose-change collision routine before initialization. The shared helper
        // reads the target radius from this ROM rather than assuming the ordinary value 21.
        byte sourcePose = Pose;
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
        RefreshCollisionRadii(bus);
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
        byte targetPose,
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
            targetPose == SamusPoseIds.MorphingTransitionRightPose;
        bool startsMorphingLeft = sourceDirection == 4 &&
            targetPose == SamusPoseIds.MorphingTransitionLeftPose;
        bool startsMorphing = startsMorphingRight || startsMorphingLeft;
        bool startsUnmorphingRight =
            Pose is SamusPoseIds.MorphBallGroundRightPose or SamusPoseIds.MorphBallMovingRightPose or
                SamusPoseIds.MorphBallFallingRightPose or SamusPoseIds.SpringBallGroundRightPose or
                SamusPoseIds.SpringBallMovingRightPose or SamusPoseIds.SpringBallFallingRightPose or
                SamusPoseIds.SpringBallJumpRightPose &&
            targetPose == SamusPoseIds.UnmorphingTransitionRightPose;
        bool startsUnmorphingLeft =
            Pose is SamusPoseIds.MorphBallGroundLeftPose or SamusPoseIds.MorphBallMovingLeftPose or
                SamusPoseIds.MorphBallFallingLeftPose or SamusPoseIds.SpringBallGroundLeftPose or
                SamusPoseIds.SpringBallMovingLeftPose or SamusPoseIds.SpringBallFallingLeftPose or
                SamusPoseIds.SpringBallJumpLeftPose &&
            targetPose == SamusPoseIds.UnmorphingTransitionLeftPose;
        bool startsUnmorphing = startsUnmorphingRight || startsUnmorphingLeft;
        if (!startsMorphing && !startsUnmorphing)
        {
            // Morph and unmorph use a closed, facing-preserving table. Other ball pose
            // changes are handled by ApplyMorphBallPoseChange and must not enter here.
            throw new InvalidOperationException(
                $"Morph transition ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
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
                    $"Morph entry ${targetPose:X2} expanded radius {previousRadius} -> {Kinematics.YRadius}.");
            }
            BlockMoveResult alignment = ProbeChangedPoseVertical(
                bus, level, SamusPostureDefinitions.MorphEntryDownwardPixels << 16,
                (nmiFrameCounter & 1) == 0, plms, includeSolidEnemies: false);
            Kinematics.YPosition = unchecked((ushort)(
                Kinematics.YPosition + (alignment.AcceptedDisplacement >> 16)));

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
            if (MorphBallBounceState != 0)
            {
                MorphBallBounceState = 0;
                Kinematics.YSubspeed = 0;
                Kinematics.YSpeed = 0;
                Kinematics.YDirection = 0;
            }
            InitializeAnimation(bus, initialFrame: 0);
            return true;
        }

        byte sourcePose = Pose;
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
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
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
        byte targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        byte sourcePose = Pose;
        bool rightFacingRoute = sourcePose == SamusPoseIds.UnmorphingTransitionRightPose &&
            IsRightFacingFallingPose(targetPose);
        bool leftFacingRoute = sourcePose == SamusPoseIds.UnmorphingTransitionLeftPose &&
            IsLeftFacingFallingPose(targetPose);
        if (!rightFacingRoute && !leftFacingRoute)
        {
            // `$91:A3F6/$91:A476` expose only the falling family matching the active
            // unmorph body's direction. Rejecting every other pairing keeps this helper
            // from silently becoming a catch-all for unrelated movement initializers.
            throw new InvalidOperationException(
                $"Unmorph-to-falling transition ${sourcePose:X2} -> ${targetPose:X2} is not a same-facing retail route.");
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
    public void ApplyMorphBallPoseChange(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool sourceSupported = IsStableBallPose(Pose);
        bool targetSupported = IsStableBallPose(targetPose);
        if (!sourceSupported || !targetSupported)
        {
            throw new InvalidOperationException(
                $"Stable Morph-Ball transition ${Pose:X2} -> ${targetPose:X2} requires stable ball endpoints.");
        }

        byte previousDirection = ReadPoseXDirection(bus);
        int previousDelayList = AnimationDelayListAddress;
        Pose = targetPose;
        RefreshCollisionRadii(bus);

        // All ordinary ball poses point at `$91:B378`. Native writes $8000 to the new-frame
        // selector, causing `$91:FB5C` to return without changing frame OR timer. Assert the
        // table identity instead of depending on that retail-data fact silently.
        int targetDelayList = ResolveAnimationDelayList(bus);
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
            throw new InvalidOperationException($"Morph-Ball landing requires airborne pose $31/$32, not ${Pose:X2}.");

        if (MorphBallBounceState == 0 && Kinematics.YSpeed >= 3)
        {
            MorphBallBounceState = 1;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = ReadWord(
                bus,
                SamusMovementRomData.VerticalMotion.FallingTransitionSpeed);
            Kinematics.YSubspeed = ReadWord(
                bus,
                SamusMovementRomData.VerticalMotion.FallingTransitionSubspeed);
            return false;
        }

        if (MorphBallBounceState == 1)
        {
            MorphBallBounceState = 2;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(ReadWord(
                bus,
                SamusMovementRomData.VerticalMotion.FallingTransitionSpeed) - 1));
            Kinematics.YSubspeed = ReadWord(
                bus,
                SamusMovementRomData.VerticalMotion.FallingTransitionSubspeed);
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        byte groundedPose = IsFacingLeft(bus)
            ? SamusPoseIds.MorphBallGroundLeftPose
            : SamusPoseIds.MorphBallGroundRightPose;
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
            throw new InvalidOperationException($"Spring-Ball landing requires pose $7D-$80, not ${Pose:X2}.");

        if ((controllerInput & (ushort)SnesButton.A) != 0)
        {
            MorphBallBounceState = 0;
            SamusAerialMovement.InitializeJump(bus, this);
            byte jumpPose = IsFacingLeft(bus)
                ? SamusPoseIds.SpringBallJumpLeftPose
                : SamusPoseIds.SpringBallJumpRightPose;
            ApplyMorphBallPoseChange(bus, jumpPose);
            return false;
        }

        byte bounce = unchecked((byte)MorphBallBounceState);
        if (bounce == 0 && Kinematics.YSpeed >= 3)
        {
            MorphBallBounceState = 0x0601;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = ReadWord(
                bus,
                SamusMovementRomData.VerticalMotion.FallingTransitionSpeed);
            Kinematics.YSubspeed = ReadWord(
                bus,
                SamusMovementRomData.VerticalMotion.FallingTransitionSubspeed);
            return false;
        }

        if (bounce == 1)
        {
            MorphBallBounceState = 0x0602;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(ReadWord(
                bus,
                SamusMovementRomData.VerticalMotion.FallingTransitionSpeed) - 1));
            Kinematics.YSubspeed = ReadWord(
                bus,
                SamusMovementRomData.VerticalMotion.FallingTransitionSubspeed);
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        byte groundPose = IsFacingLeft(bus)
            ? SamusPoseIds.SpringBallGroundLeftPose
            : SamusPoseIds.SpringBallGroundRightPose;
        ApplyMorphBallPoseChange(bus, groundPose);
        return true;
    }

    /// <summary>
    /// Applies `$91:FC18` when grounded Spring Ball `$79/$7A` changes to `$7F/$80`.
    /// Moving-ground poses first pass through the same table-selected target, but the
    /// initializer only launches when the previous movement type was exactly `$11`.
    /// </summary>
    public void ApplySpringBallJump(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool validTarget = targetPose is SamusPoseIds.SpringBallJumpRightPose or SamusPoseIds.SpringBallJumpLeftPose;
        if (!IsGroundedSpringBallPose(Pose) || !validTarget)
            throw new InvalidOperationException(
                $"Spring-Ball jump ${Pose:X2} -> ${targetPose:X2} is outside the grounded-to-airborne Spring-Ball route.");

        ApplyMorphBallPoseChange(bus, targetPose);
        MorphBallBounceState = 0;
        SamusAerialMovement.InitializeJump(bus, this);
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
            throw new InvalidOperationException($"Morph-Ball walk-off requires grounded pose, not ${Pose:X2}.");

        byte fallingPose = IsFacingLeft(bus)
            ? springBall ? SamusPoseIds.SpringBallFallingLeftPose : SamusPoseIds.MorphBallFallingLeftPose
            : springBall ? SamusPoseIds.SpringBallFallingRightPose : SamusPoseIds.MorphBallFallingRightPose;
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
    public void ApplyWalkedOffFloorTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool supportedSource =
            IsRightFacingStandingPose(Pose) || IsLeftFacingStandingPose(Pose) ||
            IsRightFacingRunningPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsMoonwalkingPose(Pose) ||
            IsMoonwalkTurnJumpPose(Pose) ||
            Pose is SamusPoseIds.KnockbackRightPose or SamusPoseIds.KnockbackLeftPose ||
            IsRanIntoWallPose(Pose) ||
            IsRightFacingCrouchingPose(Pose) || IsLeftFacingCrouchingPose(Pose);
        byte expectedTarget = SelectFallingPoseForCurrentAim(bus);
        if (!supportedSource || targetPose != expectedTarget)
        {
            // The caller computes the target from this pose's ROM metadata immediately
            // before invoking us. A mismatch is an inconsistent request, not missing logic.
            throw new InvalidOperationException(
                $"Walk-off transition ${Pose:X2} -> ${targetPose:X2} does not match the pose's ROM-selected falling target ${expectedTarget:X2}.");
        }

        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 2;
        SamusAerialMovement.ConfigureEnvironmentGravity(bus, this);
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        // SamusFunc_F433 dispatches the new falling movement type through $91:F60D
        // before command five initializes the downward state. That initializer derives
        // the mode from extra dash speed; it must not retain the grounded release mode.
        // Base speed itself survives until the next aerial movement routine consumes it.
        InitializeOrdinaryAerialAcceleration();
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Ports the non-spinning direction lookup used by `$91:E8F2` for a grounded walk-off.
    /// </summary>
    public byte SelectFallingPoseForCurrentAim(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte shotDirection = ReadShotDirection(bus);
        return shotDirection switch
        {
            0 => SamusPoseIds.FallingAimUpRightPose,
            1 => SamusPoseIds.FallingAimDiagonalUpRightPose,
            2 => SamusPoseIds.FallingRightPose,
            3 => SamusPoseIds.FallingAimDiagonalDownRightPose,
            6 => SamusPoseIds.FallingAimDiagonalDownLeftPose,
            7 => SamusPoseIds.FallingLeftPose,
            8 => SamusPoseIds.FallingAimDiagonalUpLeftPose,
            9 => SamusPoseIds.FallingAimUpLeftPose,

            // Turn and crouch records store `$FB`/`$FF` rather than an arm direction.
            // Their facing byte still selects the ordinary unaimed falling pair.
            0xfb or 0xff => IsFacingLeft(bus)
                ? SamusPoseIds.FallingLeftPose
                : SamusPoseIds.FallingRightPose,
            // Stable grounded records publish only the eight admitted aim directions or
            // `$FB/$FF` sentinels. Compact directions four/five belong exclusively to
            // already-airborne `$17/$18/$2D/$2E` and cannot originate a walk-off.
            _ => throw new InvalidDataException(
                $"Walk-off source pose ${Pose:X2} has invalid grounded shot direction ${shotDirection:X2}."),
        };
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
        bool leavingScrewAttack = IsScrewAttackPose(Pose);
        byte targetPose = SelectAerialLandingPose(bus, wasSpinning, controllerInput);

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius > oldRadius)
        {
            ushort difference = unchecked((ushort)(Kinematics.YRadius - oldRadius));
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - difference));
        }

        ApplyAerialLandingCollisionCommand(leavingScrewAttack);
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies a normal or spinning landing through the cartridge's shared prospective-pose
    /// collision handler before committing the taller landing body.
    /// </summary>
    /// <returns>
    /// True when the selected landing pose fit. False means native collision retained the
    /// source pose or selected its stable-crouch fallback; grounded velocity is still cleared.
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

        byte sourcePose = Pose;
        bool leavingScrewAttack = IsScrewAttackPose(sourcePose);
        byte targetPose = SelectAerialLandingPose(bus, wasSpinning, controllerInput);
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
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
            InitializeAnimation(bus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            // `$91:FFA7` deliberately chooses stable crouch for a non-Morph source when
            // both the floor and ceiling reject the full landing body. This is what lets
            // a spin jump land in a 32-pixel passage without embedding Samus in its ceiling.
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
        }

        // Collision command five follows prospective-pose handling even when the chosen
        // pose was rejected. A rejected landing is still grounded and must lose air speed.
        ApplyAerialLandingCollisionCommand(leavingScrewAttack);
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>Chooses <c>$91:E95D</c>'s prospective landing pose.</summary>
    private byte SelectAerialLandingPose(
        ISnesAddressSpace bus,
        bool wasSpinning,
        ushort controllerInput)
    {
        byte direction = ReadPoseXDirection(bus);
        bool facingLeft = direction == 4;
        if (wasSpinning)
        {
            return facingLeft
                ? SamusPoseIds.SpinLandingLeftPose
                : SamusPoseIds.SpinLandingRightPose;
        }

        // `$91:E95D` checks `$FF` before indexing `$91:E9F3`; hurt/damage-boost art
        // deliberately stores that sentinel and lands through the ordinary facing pair.
        // Horizontal directions two/seven take `$91:E96E-$E98F`'s extra Shot-binding
        // test. Held Shot selects firing landing `$E6/$E7`; released Shot selects the
        // ordinary `$A4/$A5` pair. The six admitted aim directions select `$E0-$E5`.
        bool shotHeld = (controllerInput & (ushort)SnesButton.X) != 0;
        return ReadShotDirection(bus) switch
        {
            0 => SamusPoseIds.LandingAimUpRightPose,
            1 => SamusPoseIds.LandingAimDiagonalUpRightPose,
            2 => shotHeld ? SamusPoseIds.FiringLandingRightPose : SamusPoseIds.NormalLandingRightPose,
            3 => SamusPoseIds.LandingAimDiagonalDownRightPose,
            6 => SamusPoseIds.LandingAimDiagonalDownLeftPose,
            7 => shotHeld ? SamusPoseIds.FiringLandingLeftPose : SamusPoseIds.NormalLandingLeftPose,
            8 => SamusPoseIds.LandingAimDiagonalUpLeftPose,
            9 => SamusPoseIds.LandingAimUpLeftPose,
            0xff => facingLeft ? SamusPoseIds.NormalLandingLeftPose : SamusPoseIds.NormalLandingRightPose,
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
    /// block pose-change collision resolver before collision command five clears motion.
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
            throw new InvalidOperationException($"Compact landing requires pose $17/$18/$2D/$2E, not ${Pose:X2}.");

        byte sourcePose = Pose;
        byte targetPose = ReadShotDirection(bus) switch
        {
            4 => SamusPoseIds.NormalLandingRightPose,
            5 => SamusPoseIds.NormalLandingLeftPose,
            byte shotDirection => throw new InvalidOperationException(
                $"Compact pose ${Pose:X2} has unexpected shot direction ${shotDirection:X2}."),
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
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
            InitializeAnimation(bus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
        }

        // `$91:F010` collision command five runs after pose selection even when the larger
        // body falls back to crouch, so no launch/fall residue survives the landing seam.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 0;
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Consumes command $FD/$F8's command-three pose operand for every animation route in
    /// the current grounded/ordinary-air slice.
    /// </summary>
    public bool ApplyPendingVerifiedAnimationTransition(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (PendingTransitionalPose is not byte targetPose)
            return false;

        // Native pose initialization computes its own local delay without replacing
        // the buffer published by this frame's FX pass. Command three adds that live
        // buffer once more after installing the new animation.
        ushort commandAnimationBuffer = AnimationFrameBuffer;

        bool verified = (Pose, targetPose) is
            (SamusPoseIds.TurningRightToLeftPose, SamusPoseIds.FacingLeftNormalPose) or
            (SamusPoseIds.TurningLeftToRightPose, SamusPoseIds.FacingRightNormalPose) or
            (SamusPoseIds.TurningRightToLeftAimUpPose, SamusPoseIds.StandingAimUpLeftPose) or
            (SamusPoseIds.TurningLeftToRightAimUpPose, SamusPoseIds.StandingAimUpRightPose) or
            (SamusPoseIds.TurningRightToLeftAimDiagonalDownPose, SamusPoseIds.StandingAimDiagonalDownLeftPose) or
            (SamusPoseIds.TurningLeftToRightAimDiagonalDownPose, SamusPoseIds.StandingAimDiagonalDownRightPose) or
            (SamusPoseIds.TurningRightToLeftAimDiagonalUpPose, SamusPoseIds.StandingAimDiagonalUpLeftPose) or
            (SamusPoseIds.TurningLeftToRightAimDiagonalUpPose, SamusPoseIds.StandingAimDiagonalUpRightPose) or
            (SamusPoseIds.TurningRightToLeftCrouchingPose, SamusPoseIds.CrouchingLeftPose) or
            (SamusPoseIds.TurningLeftToRightCrouchingPose, SamusPoseIds.CrouchingRightPose) or
            (SamusPoseIds.TurningRightToLeftCrouchingAimUpPose, SamusPoseIds.CrouchingAimUpLeftPose) or
            (SamusPoseIds.TurningLeftToRightCrouchingAimUpPose, SamusPoseIds.CrouchingAimUpRightPose) or
            (SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalDownPose, SamusPoseIds.CrouchingAimDiagonalDownLeftPose) or
            (SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalDownPose, SamusPoseIds.CrouchingAimDiagonalDownRightPose) or
            (SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalUpPose, SamusPoseIds.CrouchingAimDiagonalUpLeftPose) or
            (SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalUpPose, SamusPoseIds.CrouchingAimDiagonalUpRightPose) or
            (SamusPoseIds.NeutralJumpTransitionRightPose, SamusPoseIds.NeutralJumpRightPose) or
            (SamusPoseIds.NeutralJumpTransitionLeftPose, SamusPoseIds.NeutralJumpLeftPose) or
            (SamusPoseIds.NormalJumpTransitionAimUpRightPose, SamusPoseIds.NormalJumpAimUpRightPose) or
            (SamusPoseIds.NormalJumpTransitionAimUpLeftPose, SamusPoseIds.NormalJumpAimUpLeftPose) or
            (SamusPoseIds.NormalJumpTransitionAimDiagonalUpRightPose, SamusPoseIds.NormalJumpAimDiagonalUpRightPose) or
            (SamusPoseIds.NormalJumpTransitionAimDiagonalUpLeftPose, SamusPoseIds.NormalJumpAimDiagonalUpLeftPose) or
            (SamusPoseIds.NormalJumpTransitionAimDiagonalDownRightPose, SamusPoseIds.NormalJumpAimDiagonalDownRightPose) or
            (SamusPoseIds.NormalJumpTransitionAimDiagonalDownLeftPose, SamusPoseIds.NormalJumpAimDiagonalDownLeftPose) or
            (SamusPoseIds.CrouchingTransitionRightPose, SamusPoseIds.CrouchingRightPose) or
            (SamusPoseIds.CrouchingTransitionLeftPose, SamusPoseIds.CrouchingLeftPose) or
            (SamusPoseIds.StandingTransitionRightPose, SamusPoseIds.FacingRightNormalPose) or
            (SamusPoseIds.StandingTransitionLeftPose, SamusPoseIds.FacingLeftNormalPose) or
            (SamusPoseIds.CrouchingTransitionAimUpRightPose, SamusPoseIds.CrouchingAimUpRightPose) or
            (SamusPoseIds.CrouchingTransitionAimUpLeftPose, SamusPoseIds.CrouchingAimUpLeftPose) or
            (SamusPoseIds.CrouchingTransitionAimDiagonalUpRightPose, SamusPoseIds.CrouchingAimDiagonalUpRightPose) or
            (SamusPoseIds.CrouchingTransitionAimDiagonalUpLeftPose, SamusPoseIds.CrouchingAimDiagonalUpLeftPose) or
            (SamusPoseIds.CrouchingTransitionAimDiagonalDownRightPose, SamusPoseIds.CrouchingAimDiagonalDownRightPose) or
            (SamusPoseIds.CrouchingTransitionAimDiagonalDownLeftPose, SamusPoseIds.CrouchingAimDiagonalDownLeftPose) or
            (SamusPoseIds.StandingTransitionAimUpRightPose, SamusPoseIds.StandingAimUpRightPose) or
            (SamusPoseIds.StandingTransitionAimUpLeftPose, SamusPoseIds.StandingAimUpLeftPose) or
            (SamusPoseIds.StandingTransitionAimDiagonalUpRightPose, SamusPoseIds.StandingAimDiagonalUpRightPose) or
            (SamusPoseIds.StandingTransitionAimDiagonalUpLeftPose, SamusPoseIds.StandingAimDiagonalUpLeftPose) or
            (SamusPoseIds.StandingTransitionAimDiagonalDownRightPose, SamusPoseIds.StandingAimDiagonalDownRightPose) or
            (SamusPoseIds.StandingTransitionAimDiagonalDownLeftPose, SamusPoseIds.StandingAimDiagonalDownLeftPose) or
            (SamusPoseIds.MorphingTransitionRightPose, SamusPoseIds.MorphBallGroundRightPose) or
            (SamusPoseIds.MorphingTransitionRightPose, SamusPoseIds.MorphBallFallingRightPose) or
            (SamusPoseIds.MorphingTransitionRightPose, SamusPoseIds.SpringBallGroundRightPose) or
            (SamusPoseIds.MorphingTransitionRightPose, SamusPoseIds.SpringBallFallingRightPose) or
            (SamusPoseIds.MorphingTransitionLeftPose, SamusPoseIds.MorphBallGroundLeftPose) or
            (SamusPoseIds.MorphingTransitionLeftPose, SamusPoseIds.MorphBallFallingLeftPose) or
            (SamusPoseIds.MorphingTransitionLeftPose, SamusPoseIds.SpringBallGroundLeftPose) or
            (SamusPoseIds.MorphingTransitionLeftPose, SamusPoseIds.SpringBallFallingLeftPose) or
            (SamusPoseIds.UnmorphingTransitionRightPose, SamusPoseIds.CrouchingRightPose) or
            (SamusPoseIds.UnmorphingTransitionLeftPose, SamusPoseIds.CrouchingLeftPose) or
            (SamusPoseIds.NormalLandingRightPose, SamusPoseIds.FacingRightNormalPose) or
            (SamusPoseIds.NormalLandingLeftPose, SamusPoseIds.FacingLeftNormalPose) or
            (SamusPoseIds.SpinLandingRightPose, SamusPoseIds.FacingRightNormalPose) or
            (SamusPoseIds.SpinLandingLeftPose, SamusPoseIds.FacingLeftNormalPose) or
            (SamusPoseIds.LandingAimUpRightPose, SamusPoseIds.StandingAimUpRightPose) or
            (SamusPoseIds.LandingAimUpLeftPose, SamusPoseIds.StandingAimUpLeftPose) or
            (SamusPoseIds.LandingAimDiagonalUpRightPose, SamusPoseIds.StandingAimDiagonalUpRightPose) or
            (SamusPoseIds.LandingAimDiagonalUpLeftPose, SamusPoseIds.StandingAimDiagonalUpLeftPose) or
            (SamusPoseIds.LandingAimDiagonalDownRightPose, SamusPoseIds.StandingAimDiagonalDownRightPose) or
            (SamusPoseIds.LandingAimDiagonalDownLeftPose, SamusPoseIds.StandingAimDiagonalDownLeftPose) or
            // `$91:B22D/$B231` is shared by ordinary and firing landings. `$E6/$E7`
            // therefore executes the literal same `$F8,$01/$02` terminal operands.
            (SamusPoseIds.FiringLandingRightPose, SamusPoseIds.FacingRightNormalPose) or
            (SamusPoseIds.FiringLandingLeftPose, SamusPoseIds.FacingLeftNormalPose) or
            // Every aerial-turn delay list ends in command `$F8 pp`. These pairs are the
            // literal operands from `$91:B3ED-$91:B490`, not inferred mirror poses.
            (SamusPoseIds.TurningRightToLeftJumpPose, SamusPoseIds.NormalJumpForwardLeftPose) or
            (SamusPoseIds.TurningLeftToRightJumpPose, SamusPoseIds.NormalJumpForwardRightPose) or
            (SamusPoseIds.TurningRightToLeftFallingPose, SamusPoseIds.FallingLeftPose) or
            (SamusPoseIds.TurningLeftToRightFallingPose, SamusPoseIds.FallingRightPose) or
            (SamusPoseIds.TurningRightToLeftJumpAimUpPose, SamusPoseIds.NormalJumpAimUpLeftPose) or
            (SamusPoseIds.TurningLeftToRightJumpAimUpPose, SamusPoseIds.NormalJumpAimUpRightPose) or
            (SamusPoseIds.TurningRightToLeftJumpAimDownPose, SamusPoseIds.NormalJumpAimDownLeftPose) or
            (SamusPoseIds.TurningLeftToRightJumpAimDownPose, SamusPoseIds.NormalJumpAimDownRightPose) or
            (SamusPoseIds.TurningRightToLeftFallingAimUpPose, SamusPoseIds.FallingAimUpLeftPose) or
            (SamusPoseIds.TurningLeftToRightFallingAimUpPose, SamusPoseIds.FallingAimUpRightPose) or
            (SamusPoseIds.TurningRightToLeftFallingAimDownPose, SamusPoseIds.FallingAimDownLeftPose) or
            (SamusPoseIds.TurningLeftToRightFallingAimDownPose, SamusPoseIds.FallingAimDownRightPose) or
            (SamusPoseIds.TurningRightToLeftJumpAimDiagonalUpPose, SamusPoseIds.NormalJumpAimDiagonalUpLeftPose) or
            (SamusPoseIds.TurningLeftToRightJumpAimDiagonalUpPose, SamusPoseIds.NormalJumpAimDiagonalUpRightPose) or
            (SamusPoseIds.TurningRightToLeftFallingAimDiagonalUpPose, SamusPoseIds.FallingAimDiagonalUpLeftPose) or
            (SamusPoseIds.TurningLeftToRightFallingAimDiagonalUpPose, SamusPoseIds.FallingAimDiagonalUpRightPose) or
            // `$91:B45B-$B478` ends every moonwalk turn/jump delay list with command
            // `$F8,$1A/$19`. Unlike an aerial turn, this is the first actual airborne pose,
            // so the special branch below also creates the dry-air jump velocity.
            (SamusPoseIds.MoonwalkTurnJumpLeftPose or SamusPoseIds.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseIds.MoonwalkTurnJumpAimDownLeftPose, SamusPoseIds.SpinJumpLeftPose) or
            (SamusPoseIds.MoonwalkTurnJumpRightPose or SamusPoseIds.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseIds.MoonwalkTurnJumpAimDownRightPose, SamusPoseIds.SpinJumpRightPose) or
            // `$91:B545/$B556` terminate Crystal Flash finish art in `$FD,$01/$02`.
            // Its installed movement handler observes type zero on the following frame.
            (SamusPoseIds.CrystalFlashRightPose, SamusPoseIds.FacingRightNormalPose) or
            (SamusPoseIds.CrystalFlashLeftPose, SamusPoseIds.FacingLeftNormalPose) or
            // All four drained release streams eventually publish ordinary standing art.
            // These are literal `$FD` operands from `$91:B257-$B298`.
            (SamusPoseIds.DrainedCrouchingRightPose or SamusPoseIds.DrainedStandingRightPose, SamusPoseIds.FacingRightNormalPose) or
            (SamusPoseIds.DrainedCrouchingLeftPose or SamusPoseIds.DrainedStandingLeftPose, SamusPoseIds.FacingLeftNormalPose);
        if (!verified)
        {
            // The allowlist above is the exhaustive set of `$F8/$FD` operands referenced
            // by active retail animation streams. Anything else means the caller supplied
            // a target that was not read from the current pose's animation bytecode.
            throw new InvalidOperationException(
                $"Animation transition ${Pose:X2} -> ${targetPose:X2} is not an active retail animation-command route.");
        }

        bool startsMoonwalkJump = IsMoonwalkTurnJumpPose(Pose) &&
            targetPose is SamusPoseIds.SpinJumpRightPose or SamusPoseIds.SpinJumpLeftPose;
        byte sourcePose = Pose;
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
            byte installedPose = startsMoonwalkJump
                ? SelectEquippedSpinPose(targetPose)
                : targetPose;
            // Animation-owned pose changes still run the target movement initializer.
            // In particular an aerial turn must stop decelerating in the old direction
            // when its final normal-jump/falling pose is installed, even if water made
            // the turn finish before base speed reached zero. Do not clear that speed.
            if (ReadMovementType(bus, installedPose) is SamusMovementType.NormalJumping or SamusMovementType.Falling)
                InitializeOrdinaryAerialAcceleration();
            ApplySimpleGroundedPoseChange(bus, sourcePose, installedPose, "Animation command");
        }
        if (sourcePose is SamusPoseIds.DrainedCrouchingRightPose or SamusPoseIds.DrainedCrouchingLeftPose or
            SamusPoseIds.DrainedStandingRightPose or SamusPoseIds.DrainedStandingLeftPose)
        {
            // The actor-owned release command has now reached its ROM-authored normal pose.
            // Clear only the host handler marker; pose initialization above already owns the
            // same radius and animation writes as native `$91:F404/$91:FB08`.
            Drained.CompleteRelease();
        }
        if (startsMoonwalkJump)
            SamusAerialMovement.InitializeJump(bus, this);
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
        byte expectedPose,
        byte targetPose,
        string transitionName)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != expectedPose)
        {
            throw new InvalidOperationException(
                $"{transitionName} requires pose ${expectedPose:X2}, not ${Pose:X2}.");
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        // $91:FB64-$FB67 retains the frame/timer when the ordinary pose is
        // unchanged. A prospective run rejected by the wall probe can resolve
        // back to the current wall-stop pose on every held-input frame.
        if (expectedPose != targetPose)
            InitializeAnimation(bus, initialFrame: 0);
    }

}
