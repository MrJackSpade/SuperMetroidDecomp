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
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == FacingRightNormalPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == FacingLeftNormalPose;
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
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == NeutralJumpTransitionRightPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == NeutralJumpTransitionLeftPose;
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
        if (sourcePose is CrouchingRightPose or CrouchingLeftPose)
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
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool startsCrouchingRight =
            (IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
             IsMoonwalkingFacingRightPose(Pose) ||
             IsRightFacingRanIntoWallPose(Pose) ||
             IsRightFacingLandingPose(Pose)) &&
            targetPose is
                CrouchingTransitionRightPose or CrouchingTransitionAimUpRightPose or
                CrouchingTransitionAimDiagonalUpRightPose or
                CrouchingTransitionAimDiagonalDownRightPose;
        bool startsCrouchingLeft =
            (IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
             IsMoonwalkingFacingLeftPose(Pose) ||
             IsLeftFacingRanIntoWallPose(Pose) ||
             IsLeftFacingLandingPose(Pose)) &&
            targetPose is
                CrouchingTransitionLeftPose or CrouchingTransitionAimUpLeftPose or
                CrouchingTransitionAimDiagonalUpLeftPose or
                CrouchingTransitionAimDiagonalDownLeftPose;
        bool startsCrouching = startsCrouchingRight || startsCrouchingLeft;
        bool startsStandingRight = IsRightFacingCrouchingPose(Pose) &&
            targetPose is
                StandingTransitionRightPose or StandingTransitionAimUpRightPose or
                StandingTransitionAimDiagonalUpRightPose or
                StandingTransitionAimDiagonalDownRightPose;
        bool startsStandingLeft = IsLeftFacingCrouchingPose(Pose) &&
            targetPose is
                StandingTransitionLeftPose or StandingTransitionAimUpLeftPose or
                StandingTransitionAimDiagonalUpLeftPose or
                StandingTransitionAimDiagonalDownLeftPose;
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
    /// `$37/$38` shrink radius 16 to 7 and command seven moves center Y down nine pixels;
    /// this keeps the old crouching bottom boundary exactly fixed. `$3D/$3E` expand 7 to
    /// 16 through `$91:FDAE`; floor collision normally moves center up nine. If both sides
    /// constrain a radius-seven body, `$91:FFA7` rejects the target and keeps Samus morphed.
    /// </remarks>
    public bool TryApplyMorphTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool startsMorphingRight = IsRightFacingCrouchingPose(Pose) &&
            targetPose == MorphingTransitionRightPose;
        bool startsMorphingLeft = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == MorphingTransitionLeftPose;
        bool startsMorphing = startsMorphingRight || startsMorphingLeft;
        bool startsUnmorphingRight =
            Pose is MorphBallGroundRightPose or MorphBallMovingRightPose or
                MorphBallFallingRightPose or SpringBallGroundRightPose or
                SpringBallMovingRightPose or SpringBallFallingRightPose or
                SpringBallJumpRightPose &&
            targetPose == UnmorphingTransitionRightPose;
        bool startsUnmorphingLeft =
            Pose is MorphBallGroundLeftPose or MorphBallMovingLeftPose or
                MorphBallFallingLeftPose or SpringBallGroundLeftPose or
                SpringBallMovingLeftPose or SpringBallFallingLeftPose or
                SpringBallJumpLeftPose &&
            targetPose == UnmorphingTransitionLeftPose;
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

            Pose = targetPose;
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + 9));

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
            Kinematics.YSpeed = ReadWord(bus, 0x909eb5);
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        if (MorphBallBounceState == 1)
        {
            MorphBallBounceState = 2;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(ReadWord(bus, 0x909eb5) - 1));
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        byte groundedPose = IsFacingLeft(bus)
            ? MorphBallGroundLeftPose
            : MorphBallGroundRightPose;
        ApplyMorphBallPoseChange(bus, groundedPose);
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
                ? SpringBallJumpLeftPose
                : SpringBallJumpRightPose;
            ApplyMorphBallPoseChange(bus, jumpPose);
            return false;
        }

        byte bounce = unchecked((byte)MorphBallBounceState);
        if (bounce == 0 && Kinematics.YSpeed >= 3)
        {
            MorphBallBounceState = 0x0601;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = ReadWord(bus, 0x909eb5);
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        if (bounce == 1)
        {
            MorphBallBounceState = 0x0602;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(ReadWord(bus, 0x909eb5) - 1));
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        byte groundPose = IsFacingLeft(bus)
            ? SpringBallGroundLeftPose
            : SpringBallGroundRightPose;
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
        bool validTarget = targetPose is SpringBallJumpRightPose or SpringBallJumpLeftPose;
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
            ? springBall ? SpringBallFallingLeftPose : MorphBallFallingLeftPose
            : springBall ? SpringBallFallingRightPose : MorphBallFallingRightPose;
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
            0 => FallingAimUpRightPose,
            1 => FallingAimDiagonalUpRightPose,
            2 => FallingRightPose,
            3 => FallingAimDiagonalDownRightPose,
            6 => FallingAimDiagonalDownLeftPose,
            7 => FallingLeftPose,
            8 => FallingAimDiagonalUpLeftPose,
            9 => FallingAimUpLeftPose,

            // Turn and crouch records store `$FB`/`$FF` rather than an arm direction.
            // Their facing byte still selects the ordinary unaimed falling pair.
            0xfb or 0xff => IsFacingLeft(bus)
                ? FallingLeftPose
                : FallingRightPose,
            // Stable grounded records publish only the eight admitted aim directions or
            // `$FB/$FF` sentinels. Compact directions four/five belong exclusively to
            // already-airborne `$17/$18/$2D/$2E` and cannot originate a walk-off.
            _ => throw new InvalidDataException(
                $"Walk-off source pose ${Pose:X2} has invalid grounded shot direction ${shotDirection:X2}."),
        };
    }

    /// <summary>
    /// Applies <c>$91:E95D</c>'s normal/spin/aimed landing choice and grounded collision cleanup
    /// at <c>$91:F010</c>. Expanding radius 19 to 21 moves Samus upward by two pixels so
    /// her feet stay on the same collision boundary, matching <c>$91:FF49</c>.
    /// </summary>
    public void ApplyAerialLanding(
        ISnesAddressSpace bus,
        bool wasSpinning,
        ushort controllerInput = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool leavingScrewAttack = IsScrewAttackPose(Pose);
        byte direction = ReadPoseXDirection(bus);
        bool facingLeft = direction == 4;
        byte targetPose;
        if (wasSpinning)
        {
            targetPose = facingLeft ? SpinLandingLeftPose : SpinLandingRightPose;
        }
        else
        {
            // `$91:E95D` checks `$FF` before indexing `$91:E9F3`; hurt/damage-boost art
            // deliberately stores that sentinel and lands through the ordinary facing pair.
            // Horizontal directions two/seven take `$91:E96E-$E98F`'s extra Shot-binding
            // test. Held Shot selects firing landing `$E6/$E7`; released Shot selects the
            // ordinary `$A4/$A5` pair. The six admitted aim directions select `$E0-$E5`.
            // Compact directions four/five intentionally remain in the collision-aware path.
            bool shotHeld = (controllerInput & (ushort)SnesButton.X) != 0;
            targetPose = ReadShotDirection(bus) switch
            {
                0 => LandingAimUpRightPose,
                1 => LandingAimDiagonalUpRightPose,
                2 => shotHeld ? FiringLandingRightPose : NormalLandingRightPose,
                3 => LandingAimDiagonalDownRightPose,
                6 => LandingAimDiagonalDownLeftPose,
                7 => shotHeld ? FiringLandingLeftPose : NormalLandingLeftPose,
                8 => LandingAimDiagonalUpLeftPose,
                9 => LandingAimUpLeftPose,
                0xff => facingLeft ? NormalLandingLeftPose : NormalLandingRightPose,
                // Directions four/five are consumed by TryApplyCompactAerialLanding,
                // because the 10 -> 21 expansion needs room collision data. Reaching this
                // roomless routine with either direction is therefore a caller error.
                byte shotDirection => throw new InvalidOperationException(
                    $"Landing from shot direction ${shotDirection:X2} requires the collision-aware compact landing operation."),
            };
        }

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius > oldRadius)
        {
            ushort difference = unchecked((ushort)(Kinematics.YRadius - oldRadius));
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - difference));
        }

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
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies `$91:E9F3` directions four/five when radius-ten straight-down Samus lands.
    /// Both entries select ordinary `$A4/$A5`; the 10 -> 21 expansion still runs the full
    /// block pose-change collision resolver before collision command five clears motion.
    /// </summary>
    public bool TryApplyCompactAerialLanding(
        ISnesAddressSpace bus,
        RoomLevelData level,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        if (!IsCompactAerialPose(Pose))
            throw new InvalidOperationException($"Compact landing requires pose $17/$18/$2D/$2E, not ${Pose:X2}.");

        byte sourcePose = Pose;
        byte targetPose = ReadShotDirection(bus) switch
        {
            4 => NormalLandingRightPose,
            5 => NormalLandingLeftPose,
            byte shotDirection => throw new InvalidOperationException(
                $"Compact pose ${Pose:X2} has unexpected shot direction ${shotDirection:X2}."),
        };
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
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

        bool verified = (Pose, targetPose) is
            (TurningRightToLeftPose, FacingLeftNormalPose) or
            (TurningLeftToRightPose, FacingRightNormalPose) or
            (TurningRightToLeftAimUpPose, StandingAimUpLeftPose) or
            (TurningLeftToRightAimUpPose, StandingAimUpRightPose) or
            (TurningRightToLeftAimDiagonalDownPose, StandingAimDiagonalDownLeftPose) or
            (TurningLeftToRightAimDiagonalDownPose, StandingAimDiagonalDownRightPose) or
            (TurningRightToLeftAimDiagonalUpPose, StandingAimDiagonalUpLeftPose) or
            (TurningLeftToRightAimDiagonalUpPose, StandingAimDiagonalUpRightPose) or
            (TurningRightToLeftCrouchingPose, CrouchingLeftPose) or
            (TurningLeftToRightCrouchingPose, CrouchingRightPose) or
            (TurningRightToLeftCrouchingAimUpPose, CrouchingAimUpLeftPose) or
            (TurningLeftToRightCrouchingAimUpPose, CrouchingAimUpRightPose) or
            (TurningRightToLeftCrouchingAimDiagonalDownPose, CrouchingAimDiagonalDownLeftPose) or
            (TurningLeftToRightCrouchingAimDiagonalDownPose, CrouchingAimDiagonalDownRightPose) or
            (TurningRightToLeftCrouchingAimDiagonalUpPose, CrouchingAimDiagonalUpLeftPose) or
            (TurningLeftToRightCrouchingAimDiagonalUpPose, CrouchingAimDiagonalUpRightPose) or
            (NeutralJumpTransitionRightPose, NeutralJumpRightPose) or
            (NeutralJumpTransitionLeftPose, NeutralJumpLeftPose) or
            (NormalJumpTransitionAimUpRightPose, NormalJumpAimUpRightPose) or
            (NormalJumpTransitionAimUpLeftPose, NormalJumpAimUpLeftPose) or
            (NormalJumpTransitionAimDiagonalUpRightPose, NormalJumpAimDiagonalUpRightPose) or
            (NormalJumpTransitionAimDiagonalUpLeftPose, NormalJumpAimDiagonalUpLeftPose) or
            (NormalJumpTransitionAimDiagonalDownRightPose, NormalJumpAimDiagonalDownRightPose) or
            (NormalJumpTransitionAimDiagonalDownLeftPose, NormalJumpAimDiagonalDownLeftPose) or
            (CrouchingTransitionRightPose, CrouchingRightPose) or
            (CrouchingTransitionLeftPose, CrouchingLeftPose) or
            (StandingTransitionRightPose, FacingRightNormalPose) or
            (StandingTransitionLeftPose, FacingLeftNormalPose) or
            (CrouchingTransitionAimUpRightPose, CrouchingAimUpRightPose) or
            (CrouchingTransitionAimUpLeftPose, CrouchingAimUpLeftPose) or
            (CrouchingTransitionAimDiagonalUpRightPose, CrouchingAimDiagonalUpRightPose) or
            (CrouchingTransitionAimDiagonalUpLeftPose, CrouchingAimDiagonalUpLeftPose) or
            (CrouchingTransitionAimDiagonalDownRightPose, CrouchingAimDiagonalDownRightPose) or
            (CrouchingTransitionAimDiagonalDownLeftPose, CrouchingAimDiagonalDownLeftPose) or
            (StandingTransitionAimUpRightPose, StandingAimUpRightPose) or
            (StandingTransitionAimUpLeftPose, StandingAimUpLeftPose) or
            (StandingTransitionAimDiagonalUpRightPose, StandingAimDiagonalUpRightPose) or
            (StandingTransitionAimDiagonalUpLeftPose, StandingAimDiagonalUpLeftPose) or
            (StandingTransitionAimDiagonalDownRightPose, StandingAimDiagonalDownRightPose) or
            (StandingTransitionAimDiagonalDownLeftPose, StandingAimDiagonalDownLeftPose) or
            (MorphingTransitionRightPose, MorphBallGroundRightPose) or
            (MorphingTransitionRightPose, MorphBallFallingRightPose) or
            (MorphingTransitionRightPose, SpringBallGroundRightPose) or
            (MorphingTransitionRightPose, SpringBallFallingRightPose) or
            (MorphingTransitionLeftPose, MorphBallGroundLeftPose) or
            (MorphingTransitionLeftPose, MorphBallFallingLeftPose) or
            (MorphingTransitionLeftPose, SpringBallGroundLeftPose) or
            (MorphingTransitionLeftPose, SpringBallFallingLeftPose) or
            (UnmorphingTransitionRightPose, CrouchingRightPose) or
            (UnmorphingTransitionLeftPose, CrouchingLeftPose) or
            (NormalLandingRightPose, FacingRightNormalPose) or
            (NormalLandingLeftPose, FacingLeftNormalPose) or
            (SpinLandingRightPose, FacingRightNormalPose) or
            (SpinLandingLeftPose, FacingLeftNormalPose) or
            (LandingAimUpRightPose, StandingAimUpRightPose) or
            (LandingAimUpLeftPose, StandingAimUpLeftPose) or
            (LandingAimDiagonalUpRightPose, StandingAimDiagonalUpRightPose) or
            (LandingAimDiagonalUpLeftPose, StandingAimDiagonalUpLeftPose) or
            (LandingAimDiagonalDownRightPose, StandingAimDiagonalDownRightPose) or
            (LandingAimDiagonalDownLeftPose, StandingAimDiagonalDownLeftPose) or
            // `$91:B22D/$B231` is shared by ordinary and firing landings. `$E6/$E7`
            // therefore executes the literal same `$F8,$01/$02` terminal operands.
            (FiringLandingRightPose, FacingRightNormalPose) or
            (FiringLandingLeftPose, FacingLeftNormalPose) or
            // Every aerial-turn delay list ends in command `$F8 pp`. These pairs are the
            // literal operands from `$91:B3ED-$91:B490`, not inferred mirror poses.
            (TurningRightToLeftJumpPose, NormalJumpForwardLeftPose) or
            (TurningLeftToRightJumpPose, NormalJumpForwardRightPose) or
            (TurningRightToLeftFallingPose, FallingLeftPose) or
            (TurningLeftToRightFallingPose, FallingRightPose) or
            (TurningRightToLeftJumpAimUpPose, NormalJumpAimUpLeftPose) or
            (TurningLeftToRightJumpAimUpPose, NormalJumpAimUpRightPose) or
            (TurningRightToLeftJumpAimDownPose, NormalJumpAimDownLeftPose) or
            (TurningLeftToRightJumpAimDownPose, NormalJumpAimDownRightPose) or
            (TurningRightToLeftFallingAimUpPose, FallingAimUpLeftPose) or
            (TurningLeftToRightFallingAimUpPose, FallingAimUpRightPose) or
            (TurningRightToLeftFallingAimDownPose, FallingAimDownLeftPose) or
            (TurningLeftToRightFallingAimDownPose, FallingAimDownRightPose) or
            (TurningRightToLeftJumpAimDiagonalUpPose, NormalJumpAimDiagonalUpLeftPose) or
            (TurningLeftToRightJumpAimDiagonalUpPose, NormalJumpAimDiagonalUpRightPose) or
            (TurningRightToLeftFallingAimDiagonalUpPose, FallingAimDiagonalUpLeftPose) or
            (TurningLeftToRightFallingAimDiagonalUpPose, FallingAimDiagonalUpRightPose) or
            // `$91:B45B-$B478` ends every moonwalk turn/jump delay list with command
            // `$F8,$1A/$19`. Unlike an aerial turn, this is the first actual airborne pose,
            // so the special branch below also creates the dry-air jump velocity.
            (MoonwalkTurnJumpLeftPose or MoonwalkTurnJumpAimUpLeftPose or
                MoonwalkTurnJumpAimDownLeftPose, SpinJumpLeftPose) or
            (MoonwalkTurnJumpRightPose or MoonwalkTurnJumpAimUpRightPose or
                MoonwalkTurnJumpAimDownRightPose, SpinJumpRightPose) or
            // `$91:B545/$B556` terminate Crystal Flash finish art in `$FD,$01/$02`.
            // Its installed movement handler observes type zero on the following frame.
            (CrystalFlashRightPose, FacingRightNormalPose) or
            (CrystalFlashLeftPose, FacingLeftNormalPose) or
            // All four drained release streams eventually publish ordinary standing art.
            // These are literal `$FD` operands from `$91:B257-$B298`.
            (DrainedCrouchingRightPose or DrainedStandingRightPose, FacingRightNormalPose) or
            (DrainedCrouchingLeftPose or DrainedStandingLeftPose, FacingLeftNormalPose);
        if (!verified)
        {
            // The allowlist above is the exhaustive set of `$F8/$FD` operands referenced
            // by active retail animation streams. Anything else means the caller supplied
            // a target that was not read from the current pose's animation bytecode.
            throw new InvalidOperationException(
                $"Animation transition ${Pose:X2} -> ${targetPose:X2} is not an active retail animation-command route.");
        }

        bool startsMoonwalkJump = IsMoonwalkTurnJumpPose(Pose) &&
            targetPose is SpinJumpRightPose or SpinJumpLeftPose;
        byte sourcePose = Pose;
        byte previousMovementType = ReadMovementType(bus, sourcePose);

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
            ApplySimpleGroundedPoseChange(bus, sourcePose, installedPose, "Animation command");
        }
        if (sourcePose is DrainedCrouchingRightPose or DrainedCrouchingLeftPose or
            DrainedStandingRightPose or DrainedStandingLeftPose)
        {
            // The actor-owned release command has now reached its ROM-authored normal pose.
            // Clear only the host handler marker; pose initialization above already owns the
            // same radius and animation writes as native `$91:F404/$91:FB08`.
            Drained.CompleteRelease();
        }
        if (startsMoonwalkJump)
            SamusAerialMovement.InitializeJump(bus, this);
        MorphBallBounceState = 0;
        return true;
    }

    /// <summary>
    /// Installs one already-validated grounded pose and runs the common $91:F404/$91:FB08
    /// metadata, radius, and frame-zero animation work modeled by this class.
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
        InitializeAnimation(bus, initialFrame: 0);
    }

}
