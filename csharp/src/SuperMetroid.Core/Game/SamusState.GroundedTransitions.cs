using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Grounded aiming, running, moonwalk, wall-contact, and turn transitions.
/// </summary>
public sealed partial class SamusState
{
    /// <summary>
    /// Applies a same-facing input/fallback transition within normal-jump type two or
    /// falling type six without replacing the live 16.16 velocity words. The horizontal
    /// gun-extended records `$13/$14/$67/$68` use this same native initialization seam;
    /// “aim” survives in the historical method name only because that was the first slice.
    /// </summary>
    public void ApplyAerialAimTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (IsCompactAerialPose(Pose) || IsCompactAerialPose(targetPose))
        {
            // Compact poses change Samus's collision height. This overload intentionally
            // has no room data, so accepting the request would bypass the native expansion
            // collision probe; callers must select the collision-aware API explicitly.
            throw new InvalidOperationException(
                "Radius-ten aerial transitions require TryApplyCompactAerialTransition and active room collision data.");
        }
        if (!IsSameFacingAerialAimFireOrForwardTransition(Pose, targetPose))
        {
            // This helper models one same-facing ROM family, not the general pose dispatcher.
            // A cross-family request therefore violates the method contract rather than
            // identifying an untranslated member of the admitted family.
            throw new InvalidOperationException(
                $"Aerial aim/fire/forward transition ${Pose:X2} -> ${targetPose:X2} is outside the same-facing family handled by this operation.");
        }

        byte sourcePose = Pose;
        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius != oldRadius)
        {
            // The admitted family deliberately excludes compact straight-down `$17/$18`
            // and `$2D/$2E`; reaching a different radius proves a caller crossed that seam.
            // Every retail member admitted above has the same collision radius. A mismatch
            // can only come from inconsistent/corrupt pose metadata, not an omitted route.
            throw new InvalidDataException(
                $"Aerial pose ${targetPose:X2} unexpectedly changes radius {oldRadius} -> {Kinematics.YRadius} inside an equal-radius transition family.");
        }
        if (sourcePose != Pose && ReadMovementType(bus) == SamusMovementType.Falling)
            InitializeFallingPoseMomentum();
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies same-facing transitions where either endpoint is compact straight-down
    /// `$17/$18/$2D/$2E`, preserving live velocity while reproducing pose-change collision.
    /// </summary>
    /// <remarks>
    /// Entering radius ten from radius nineteen requires no collision work at `$91:FDAE`.
    /// Leaving it probes all nine newly occupied pixels above and below. If both initial
    /// directions are blocked, `$91:FFA7` selects ordinary crouch; if a compensating shift's
    /// second probe is blocked, `$91:FE82` retains the source. Neither case installs the target.
    /// </remarks>
    public bool TryApplyCompactAerialTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightJump = IsRightFacingNormalJumpPose(Pose) &&
            IsRightFacingNormalJumpPose(targetPose);
        bool leftJump = IsLeftFacingNormalJumpPose(Pose) &&
            IsLeftFacingNormalJumpPose(targetPose);
        bool rightFall = IsRightFacingFallingPose(Pose) &&
            IsRightFacingFallingPose(targetPose);
        bool leftFall = IsLeftFacingFallingPose(Pose) &&
            IsLeftFacingFallingPose(targetPose);
        if ((!rightJump && !leftJump && !rightFall && !leftFall) ||
            (!IsCompactAerialPose(Pose) && !IsCompactAerialPose(targetPose)))
        {
            // This is the exact precondition of the collision-aware compact-pose helper.
            // Other aerial changes use ApplyAerialAimTransition or a movement-specific API.
            throw new InvalidOperationException(
                $"Compact aerial transition ${Pose:X2} -> ${targetPose:X2} is not a same-family ROM route.");
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
        if (sourcePose != Pose && ReadMovementType(bus) == SamusMovementType.Falling)
            InitializeFallingPoseMomentum();
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies the falling pose initializer at $91:F60D. Extra run speed, not base
    /// speed or held direction, selects whether the next movement frame decelerates.
    /// </summary>
    private void InitializeFallingPoseMomentum() => HorizontalSpeed.AccelerationMode =
        HorizontalSpeed.ExtraRunSpeed != 0 || HorizontalSpeed.ExtraRunSubspeed != 0
            ? SamusHorizontalAccelerationModes.Decelerating
            : SamusHorizontalAccelerationModes.Accelerating;

    /// <summary>
    /// Applies falling input-lookup failure's command eight ($91:EC8E). Cancel the
    /// dash component after movement, preserving the base speed and acceleration mode.
    /// The caller admits only a still-falling body, so no new-fall Y reset is needed.
    /// </summary>
    public void ApplyFallingInputFallback(ISnesAddressSpace bus)
    {
        if (ReadMovementType(bus) != SamusMovementType.Falling)
            throw new InvalidOperationException("Falling input fallback requires a falling pose.");
        HorizontalSpeed.CancelRunningMomentum((byte)ReadFacingDirection(bus));
        HorizontalSpeed.ExtraRunSpeed = 0;
        HorizontalSpeed.ExtraRunSubspeed = 0;
    }

    /// <summary>
    /// Applies a same-facing arm-body transition within the movement-type-zero standing
    /// family and movement-type-one running family. Besides aimed bodies, this includes the
    /// real `$0B/$0C` horizontal-fire records.
    /// </summary>
    /// <remarks>
    /// All admitted records have radius 21. If both endpoints are running, `$91:F50C` writes
    /// `$8000` to the new-frame word and `$91:FB5C` preserves the current frame and timer;
    /// this matters when firing midway through the ten-frame run cycle. A transition between
    /// movement types initializes frame zero normally. Movement type changes take effect on
    /// the following frame because pose transitions occur after movement.
    /// </remarks>
    public void ApplyGroundedAimTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool sourceRight = IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
            IsRightFacingRanIntoWallPose(Pose) ||
            IsRightFacingLandingPose(Pose);
        bool targetRight = IsRightFacingStandingPose(targetPose) || IsRightFacingRunningPose(targetPose) ||
            IsRightFacingRanIntoWallPose(targetPose);
        bool sourceLeft = IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsLeftFacingRanIntoWallPose(Pose) ||
            IsLeftFacingLandingPose(Pose);
        bool targetLeft = IsLeftFacingStandingPose(targetPose) || IsLeftFacingRunningPose(targetPose) ||
            IsLeftFacingRanIntoWallPose(targetPose);
        bool sameRightFamily = sourceRight && targetRight;
        bool sameLeftFamily = sourceLeft && targetLeft;
        bool sameRightCrouch = IsRightFacingCrouchingPose(Pose) &&
            IsRightFacingCrouchingPose(targetPose);
        bool sameLeftCrouch = IsLeftFacingCrouchingPose(Pose) &&
            IsLeftFacingCrouchingPose(targetPose);
        if (!sameRightFamily && !sameLeftFamily && !sameRightCrouch && !sameLeftCrouch)
        {
            // The native input tables never feed a facing reversal through this arm-only
            // transition seam; reversal has its own turn-pose initialization path.
            throw new InvalidOperationException(
                $"Grounded aim transition ${Pose:X2} -> ${targetPose:X2} is outside this same-facing pose family.");
        }
        if (!IsGroundedAimPose(Pose) && !IsGroundedAimPose(targetPose) &&
            !IsGunExtendedPose(Pose) && !IsGunExtendedPose(targetPose))
        {
            throw new InvalidOperationException(
                "Grounded arm transition requires an aimed or gun-extended source/target pose.");
        }

        byte sourcePose = Pose;
        bool preservesRunningAnimation =
            (IsRightFacingRunningPose(sourcePose) && IsRightFacingRunningPose(targetPose)) ||
            (IsLeftFacingRunningPose(sourcePose) && IsLeftFacingRunningPose(targetPose));
        if (!preservesRunningAnimation)
        {
            // This is `$91:F404/$91:FB08`'s ordinary target installation. Replacing Pose
            // directly would omit radius refresh, delay-list selection, and command cleanup.
            ApplySimpleGroundedPoseChange(bus, sourcePose, targetPose, "Grounded arm");
            return;
        }

        // `$91:F50C-$F51A` sees previous movement type one and publishes `$8000`. The BMI
        // at `$91:FB5C` exits before touching frame, timer, or delay-buffer state. Every live
        // running arm pose points to the same `$91:B20A` list, so the cached list address is
        // already exact and must not be rebound or reset here.
        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius != oldRadius)
        {
            throw new InvalidDataException(
                $"Running arm pose ${targetPose:X2} unexpectedly changed radius {oldRadius} -> {Kinematics.YRadius}.");
        }
    }

    /// <summary>
    /// Implements `$91:EB56-$91:EB87`'s ten-entry shot-direction selector. The input pose
    /// may be the current running pose after a killed-X-speed collision or the prospective
    /// running pose tested by the one-pixel arm-pump probe.
    /// </summary>
    public static byte SelectRanIntoWallPose(ISnesAddressSpace bus, byte sourcePose)
    {
        byte shotDirection = ReadShotDirection(bus, sourcePose);
        return shotDirection switch
        {
            0 => SamusPoseIds.StandingAimUpRightPose,
            1 => SamusPoseIds.RanIntoWallAimUpRightPose,
            2 or 4 => SamusPoseIds.RanIntoWallRightPose,
            3 => SamusPoseIds.RanIntoWallAimDownRightPose,
            5 or 7 => SamusPoseIds.RanIntoWallLeftPose,
            6 => SamusPoseIds.RanIntoWallAimDownLeftPose,
            8 => SamusPoseIds.RanIntoWallAimUpLeftPose,
            9 => SamusPoseIds.StandingAimUpLeftPose,
            _ => throw new InvalidDataException(
                $"Pose ${sourcePose:X2} has invalid shot-direction byte ${shotDirection:X2}."),
        };
    }

    /// <summary>
    /// Ports the block-backed portion of <c>CheckIfProspectivePoseRunsIntoAWall</c> at
    /// <c>$91:EADE</c>. Solid-enemy collision is a separate earlier probe and remains an
    /// explicit boundary until the actor system exists.
    /// </summary>
    /// <remarks>
    /// A current type-one collision maps the current pose immediately. Otherwise, only a
    /// prospective type-one pose is interesting: native moves Samus one whole pixel in the
    /// CURRENT pose direction, retains that move when clear, and maps the prospective pose's
    /// shot direction when blocked. The retained move is the retail arm-pump bug.
    /// </remarks>
    public byte? CheckProspectiveRunningPoseForWall(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte? prospectivePose,
        bool currentXSpeedKilledByBlock,
        out BlockMoveResult? onePixelProbe,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        onePixelProbe = null;

        if (currentXSpeedKilledByBlock && ReadMovementKind(bus) == SamusMovementType.Running)
            return SelectRanIntoWallPose(bus, Pose);

        if (prospectivePose is not { } target ||
            ReadMovementKind(bus, target) != SamusMovementType.Running)
            return null;

        int onePixelForward = IsFacingLeft(bus)
            ? -0x00010000
            : 0x00010000;
        onePixelProbe = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            Kinematics,
            onePixelForward,
            plms: plms);
        return onePixelProbe.Value.Collided
            ? SelectRanIntoWallPose(bus, target)
            : null;
    }

    /// <summary>
    /// Installs the pose selected when `$91:EADE` finds a block wall. This deliberately
    /// accepts standing/running/landing sources as well as an existing wall-stop source:
    /// `$91:EADE` can test a prospective run before that run is installed, and changing aim
    /// while still pressing into the wall proposes another running pose for the same probe.
    /// </summary>
    public void ApplyRanIntoWallPoseChange(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool rightSource = IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
            IsRightFacingRanIntoWallPose(Pose) || IsRightFacingLandingPose(Pose);
        bool leftSource = IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsLeftFacingRanIntoWallPose(Pose) || IsLeftFacingLandingPose(Pose);
        bool rightRoute = rightSource &&
            (IsRightFacingRanIntoWallPose(targetPose) || targetPose == SamusPoseIds.StandingAimUpRightPose);
        bool leftRoute = leftSource &&
            (IsLeftFacingRanIntoWallPose(targetPose) || targetPose == SamusPoseIds.StandingAimUpLeftPose);
        if (!rightRoute && !leftRoute)
        {
            // Only block-backed ran-into-wall records call this exact initializer. Treat a
            // different source/target pair as misuse of the specialized operation.
            throw new InvalidOperationException(
                $"Ran-into-wall pose change ${Pose:X2} -> ${targetPose:X2} is not a block-backed retail route.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Ran into wall");
    }

    /// <summary>Applies `$89/$8A/$CF-$D2`'s same-facing exit to a type-one running pose.</summary>
    public void ApplyRanIntoWallToRunning(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool rightRoute = IsRightFacingRanIntoWallPose(Pose) &&
            IsRightFacingRunningPose(targetPose);
        bool leftRoute = IsLeftFacingRanIntoWallPose(Pose) &&
            IsLeftFacingRunningPose(targetPose);
        if (!rightRoute && !leftRoute)
        {
            // Running exits are a strict same-facing subset of the ran-into-wall table.
            throw new InvalidOperationException(
                $"Ran-into-wall running exit ${Pose:X2} -> ${targetPose:X2} is not a retail route.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Ran into wall to running");
    }

    /// <summary>
    /// Applies the movement-type-$10 option gate and stable-pose changes surrounding
    /// <c>InitializeSamusPose_Moonwalking</c> at <c>$91:F88C</c>.
    /// </summary>
    /// <remarks>
    /// Standing transition tables always publish a moonwalk candidate when Shoot and the
    /// backward direction are held. The native settings word decides what that candidate
    /// means: enabled retains `$49/$4A/$75-$78`; disabled substitutes ordinary `$25/$26`
    /// turn art. Once active, aim changes and the direct return to forward running are
    /// ordinary radius-21 pose installations with no invented velocity adjustment.
    /// </remarks>
    public void ApplyMoonwalkPoseChange(
        ISnesAddressSpace bus,
        byte targetPose,
        bool moonwalkEnabled)
    {
        ArgumentNullException.ThrowIfNull(bus);

        bool sourceStandingRight = IsRightFacingStandingPose(Pose) ||
            IsRightFacingRanIntoWallPose(Pose) ||
            IsRightFacingLandingPose(Pose);
        bool sourceStandingLeft = IsLeftFacingStandingPose(Pose) ||
            IsLeftFacingRanIntoWallPose(Pose) ||
            IsLeftFacingLandingPose(Pose);
        bool targetVisualRight = IsMoonwalkingFacingRightPose(targetPose);
        bool targetVisualLeft = IsMoonwalkingFacingLeftPose(targetPose);
        bool entering =
            (sourceStandingRight && targetVisualRight) ||
            (sourceStandingLeft && targetVisualLeft);

        if (entering && !moonwalkEnabled)
        {
            // Normal landing `$A4/$A5` points at the same grounded input records as
            // ordinary standing. Consequently a backward direction can nominate the
            // corresponding Moonwalk pose before the option gate runs. The initializer
            // does not special-case the old landing movement type: with Moonwalk off it
            // substitutes the same ordinary `$25/$26` turn used from `$01/$02`.
            // `$91:F893-$F8A9` tests the candidate pose-X byte. `$4A/$76/$78` store four
            // and become right-to-left `$25`; `$49/$75/$77` store eight and become `$26`.
            ApplyGroundedTurn(
                bus,
                targetVisualRight ? SamusPoseIds.TurningRightToLeftPose : SamusPoseIds.TurningLeftToRightPose);
            return;
        }

        bool sameVisualFamily =
            (IsMoonwalkingFacingRightPose(Pose) && targetVisualRight) ||
            (IsMoonwalkingFacingLeftPose(Pose) && targetVisualLeft);
        bool exitsToForwardRun =
            (IsMoonwalkingFacingRightPose(Pose) && targetPose == SamusPoseIds.MovingRightNormalPose) ||
            (IsMoonwalkingFacingLeftPose(Pose) && targetPose == SamusPoseIds.MovingLeftNormalPose);
        bool exitsToStandingFallback =
            (IsMoonwalkingFacingRightPose(Pose) && IsRightFacingStandingPose(targetPose)) ||
            (IsMoonwalkingFacingLeftPose(Pose) && IsLeftFacingStandingPose(targetPose));
        if ((!entering || !moonwalkEnabled) && !sameVisualFamily &&
            !exitsToForwardRun && !exitsToStandingFallback)
        {
            // The complete stable moonwalk table is represented by the predicates above;
            // reaching this branch means the caller selected the wrong pose-change API.
            throw new InvalidOperationException(
                $"Moonwalk pose change ${Pose:X2} -> ${targetPose:X2} is not a stable retail route.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Moonwalk");
    }

    /// <summary>
    /// Applies `$91:F8D3-$91:F950` when a stable moonwalk pose reverses while Jump is held.
    /// The six requested `$BF-$C4` records are not airborne yet: they retain the grounded
    /// turn movement handler while their three visible frames play.
    /// </summary>
    public void ApplyMoonwalkTurnJump(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);

        byte expectedTarget = ReadShotDirection(bus) switch
        {
            1 => SamusPoseIds.MoonwalkTurnJumpAimUpLeftPose,
            2 => SamusPoseIds.MoonwalkTurnJumpLeftPose,
            3 => SamusPoseIds.MoonwalkTurnJumpAimDownLeftPose,
            6 => SamusPoseIds.MoonwalkTurnJumpAimDownRightPose,
            7 => SamusPoseIds.MoonwalkTurnJumpRightPose,
            8 => SamusPoseIds.MoonwalkTurnJumpAimUpRightPose,
            // Stable moonwalk poses publish only the six directions enumerated above.
            // Another value means WRAM/pose metadata is inconsistent, not missing logic.
            _ => throw new InvalidDataException(
                $"Moonwalk shot direction ${ReadShotDirection(bus):X2} is invalid for a stable moonwalk pose."),
        };
        byte inputTableTarget = IsFacingLeft(bus)
            ? SamusPoseIds.MoonwalkTurnJumpLeftPose : SamusPoseIds.MoonwalkTurnJumpRightPose;
        if (!IsMoonwalkingPose(Pose) || targetPose != expectedTarget && targetPose != inputTableTarget)
        {
            // Input requests the un-aimed turn; the pose initializer then substitutes
            // the old muzzle's aimed turn. Already-resolved diagnostic callers are valid too.
            throw new InvalidOperationException(
                $"Moonwalk turn/jump ${Pose:X2} -> ${targetPose:X2} is not a retail route.");
        }

        // Preserve the old muzzle direction before installing turn art. The next alpha
        // uses the tag to admit the turning HUD producer and forcibly release charge.
        byte sourceShotDirection = ReadShotDirection(bus);
        FoldExtraRunSpeedIntoBaseAndStartTurn();
        ApplySimpleGroundedPoseChange(bus, Pose, expectedTarget, "Moonwalk turn/jump");
        PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.MoonwalkPoseHandoffTag | sourceShotDirection));
    }

    /// <summary>
    /// Applies the verified ordinary transition from standing-right pose $01 to running-
    /// right pose $09 at the end-of-frame bank-$91 transition seam.
    /// </summary>
    /// <remarks>
    /// This is intentionally not a general pose setter. For this transition, momentum
    /// routine index zero is a no-op, both poses have radius 21, and pose $09 uses the
    /// already-supported default render/tile paths. Other transitions can change momentum,
    /// align radii, invoke collision, or run specialized handlers and remain rejected.
    /// </remarks>
    public void ApplyStandingRightToRunningRight(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != SamusPoseIds.FacingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Standing-right to running-right transition requires pose $01, not ${Pose:X2}.");
        }

        Pose = SamusPoseIds.MovingRightNormalPose;
        RefreshCollisionRadii(bus);

        // $91:F404 -> $91:FB08 resets animation frame zero and loads pose $09's first
        // delay byte after SamusFunc_F433 refreshes pose direction/movement metadata.
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies the verified no-button fallback from running-right pose $09 to standing-right
    /// pose $01 after ordinary running momentum has decelerated to zero.
    /// </summary>
    public void ApplyRunningRightToStandingRight(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != SamusPoseIds.MovingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Running-right to standing-right transition requires pose $09, not ${Pose:X2}.");
        }

        Pose = SamusPoseIds.FacingRightNormalPose;
        RefreshCollisionRadii(bus);

        // Pose $09's new-pose-unless-buttons byte is $01. Once Samus_Pose_Func2 selects
        // momentum index two, `$91:ECD0` cancels `$0B3C/$0B3E` but deliberately leaves the
        // numeric extra pair intact. The following standing movement pass consumes that
        // final no-base-speed displacement before clearing every X-motion word.
        HorizontalSpeed.CancelRunningMomentum(ReadPoseXDirection(bus));
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>Applies the ordinary grounded $02 -> $0A start-moving-left transition.</summary>
    public void ApplyStandingLeftToRunningLeft(ISnesAddressSpace bus) =>
        ApplySimpleGroundedPoseChange(
            bus,
            SamusPoseIds.FacingLeftNormalPose,
            SamusPoseIds.MovingLeftNormalPose,
            "Standing-left to running-left");

    /// <summary>Applies the no-button grounded $0A -> $02 fallback.</summary>
    public void ApplyRunningLeftToStandingLeft(ISnesAddressSpace bus)
    {
        ApplySimpleGroundedPoseChange(
            bus,
            SamusPoseIds.MovingLeftNormalPose,
            SamusPoseIds.FacingLeftNormalPose,
            "Running-left to standing-left");

        // This is the mirrored `$91:ECD0` momentum-index-two route used by `$09 -> $01`.
        // ExtraRunSpeed/Subspeed remain available to standing's ordered movement/clear pass.
        HorizontalSpeed.CancelRunningMomentum(ReadPoseXDirection(bus));
    }

    /// <summary>
    /// Applies the shared standing/landing transition-table route from any right-facing
    /// landing (including firing `$E6`) to running right `$09`, or from the mirrored left
    /// family (including `$E7`) to `$0A`. Landing movement has already cleared momentum in
    /// this frame; the new running pose begins accelerating on the next frame.
    /// </summary>
    public void ApplyLandingToRunning(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool right = IsRightFacingLandingPose(Pose) &&
            targetPose == SamusPoseIds.MovingRightNormalPose;
        bool left = IsLeftFacingLandingPose(Pose) &&
            targetPose == SamusPoseIds.MovingLeftNormalPose;
        if (!right && !left)
        {
            throw new InvalidOperationException(
                $"Landing-to-run transition ${Pose:X2} -> ${targetPose:X2} is not verified.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Landing-to-run");
    }

    /// <summary>
    /// Applies bank-$91's grounded turn initialization at <c>$91:F8D3</c>. Input tables
    /// first publish generic `$25/$26`; the initializer indexes the previous pose's shot
    /// direction through `$91:F9C2` and may replace it with `$8B-$8E/$9C/$9D`.
    /// </summary>
    public void ApplyGroundedTurn(ISnesAddressSpace bus, byte targetPose, ushort controllerInput = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // `$91:F8D3-$F8DB` gives the two front-view poses a deliberately unusual route.
        // Their pose-X direction is zero, so they cannot honestly be classified as either
        // a right-facing or left-facing source. Native code instead skips the shot-direction
        // lookup entirely when the previous pose is `$00/$9B`, retaining the generic
        // `$25/$26` selected by that front-view pose's input transition table.
        bool wasForwardFacing = IsForwardFacingPose(Pose);

        // Crouching transition tables publish `$43/$44` directly, whereas every admitted
        // standing/running/landing table publishes `$25/$26`. The initializer still uses
        // previous movement type five to choose the full crouched aim-preserving table.
        SamusMovementType sourceMovementType = ReadMovementType(bus);
        bool wasCrouching = sourceMovementType == SamusMovementType.Crouching;
        bool wasMoonwalking = sourceMovementType == SamusMovementType.Moonwalking;

        // `$91:F8F3-$F903` reads the PREVIOUS moonwalk pose, not the selected turn pose.
        // Preserve the complete source byte now so the low-nibble projectile direction is
        // still available after ApplySimpleGroundedPoseChange replaces `Pose` below.
        byte moonwalkSourceShotDirection = wasMoonwalking
            ? ReadShotDirection(bus)
            : (byte)0;

        bool rightSource = IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
            IsMoonwalkingFacingRightPose(Pose) ||
            IsRightFacingRanIntoWallPose(Pose) ||
            IsRightFacingCrouchingPose(Pose) ||
            IsRightFacingLandingPose(Pose);
        bool leftSource = IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsMoonwalkingFacingLeftPose(Pose) ||
            IsLeftFacingRanIntoWallPose(Pose) ||
            IsLeftFacingCrouchingPose(Pose) ||
            IsLeftFacingLandingPose(Pose);
        bool turnsLeft = targetPose == (wasCrouching
            ? SamusPoseIds.TurningRightToLeftCrouchingPose
            : SamusPoseIds.TurningRightToLeftPose) && rightSource;
        bool turnsRight = targetPose == (wasCrouching
            ? SamusPoseIds.TurningLeftToRightCrouchingPose
            : SamusPoseIds.TurningLeftToRightPose) && leftSource;
        bool leavesForwardView = wasForwardFacing &&
            targetPose is SamusPoseIds.TurningRightToLeftPose or SamusPoseIds.TurningLeftToRightPose;
        if (!turnsLeft && !turnsRight && !leavesForwardView)
        {
            throw new InvalidOperationException(
                $"Grounded turn ${Pose:X2} -> ${targetPose:X2} is not a verified transition.");
        }

        // F8F9 checks held Jump, not a fresh press. Releasing Shoot from a buffered
        // Moonwalk selects a generic turn in the input table, then reaches this same
        // turn/jump initializer without a new Jump edge.
        if (wasMoonwalking && (controllerInput & (ushort)SnesButton.A) != 0)
        {
            ApplyMoonwalkTurnJump(bus, turnsLeft
                ? SamusPoseIds.MoonwalkTurnJumpLeftPose : SamusPoseIds.MoonwalkTurnJumpRightPose);
            return;
        }

        // `$91:F8D3` reads the PREVIOUS pose record before it installs the final turn art.
        // Previous movement type five selects `$91:F9CC`; every other admitted source uses
        // `$91:F9C2`. Both tables have ten entries, but shot directions four/five belong to
        // the compact straight-down family that remains untranslated.
        byte selectedTurnPose;
        if (wasForwardFacing)
        {
            // This is the literal early branch to `$91:F931`: keep the transition-table
            // result. Reading byte three from `$00/$9B` here would manufacture an aim/facing
            // direction that the cartridge explicitly declines to inspect.
            selectedTurnPose = targetPose;
        }
        else
        {
            byte shotDirection = ReadShotDirection(bus);
            selectedTurnPose = wasCrouching
                ? shotDirection switch
                {
                    0 => SamusPoseIds.TurningRightToLeftCrouchingAimUpPose,
                    1 => SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalUpPose,
                    2 => SamusPoseIds.TurningRightToLeftCrouchingPose,
                    3 => SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalDownPose,
                    6 => SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalDownPose,
                    7 => SamusPoseIds.TurningLeftToRightCrouchingPose,
                    8 => SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalUpPose,
                    9 => SamusPoseIds.TurningLeftToRightCrouchingAimUpPose,
                    // Grounded/crouched pose metadata cannot publish the compact-aerial
                    // directions 4/5 (or any value outside the native eight-entry set).
                    _ => throw new InvalidDataException(
                        $"Crouched turn shot direction ${shotDirection:X2} is invalid."),
                }
                : shotDirection switch
                {
                    0 => SamusPoseIds.TurningRightToLeftAimUpPose,
                    1 => SamusPoseIds.TurningRightToLeftAimDiagonalUpPose,
                    2 => SamusPoseIds.TurningRightToLeftPose,
                    3 => SamusPoseIds.TurningRightToLeftAimDiagonalDownPose,
                    6 => SamusPoseIds.TurningLeftToRightAimDiagonalDownPose,
                    7 => SamusPoseIds.TurningLeftToRightPose,
                    8 => SamusPoseIds.TurningLeftToRightAimDiagonalUpPose,
                    9 => SamusPoseIds.TurningLeftToRightAimUpPose,
                    _ => throw new InvalidDataException(
                        $"Grounded turn shot direction ${shotDirection:X2} is invalid."),
                };
        }

        if (!wasForwardFacing &&
            ((turnsLeft && !IsRightToLeftGroundTurnPose(selectedTurnPose)) ||
             (turnsRight && !IsLeftToRightGroundTurnPose(selectedTurnPose))))
        {
            throw new InvalidOperationException(
                $"Grounded turn source ${Pose:X2} has direction metadata inconsistent with target ${targetPose:X2}.");
        }

        SamusHorizontalSpeedState speed = HorizontalSpeed;

        // $91:F931-$91:F941 folds the run-button/speed-booster component into base speed
        // with two independent 16-bit ADCs and their carry. Use one wrapping 16.16 add to
        // preserve exactly that pair of operations, including overflow at the high word.
        uint combinedSpeed = unchecked(speed.BaseFixed +
            ((uint)speed.ExtraRunSpeed << 16) + speed.ExtraRunSubspeed);
        speed.BaseSpeed = unchecked((ushort)(combinedSpeed >> 16));
        speed.BaseSubspeed = unchecked((ushort)combinedSpeed);

        // The native handler consumes the extra component and selects acceleration mode
        // one. $90:8EA9 interprets that mode as “move opposite the NEW facing direction”
        // while $90:9A7E decelerates, which is how reversal preserves old momentum.
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.AccelerationMode = 1;

        ApplySimpleGroundedPoseChange(bus, Pose, selectedTurnPose, "Grounded turn");

        if (wasMoonwalking)
        {
            // The `$0100` tag makes HUD handler `$90:DD74` treat the one-frame turning
            // state as shooting. `$90:BA5F` later masks it away and consumes only this
            // source pose's direction byte when a beam slot can actually be initialized.
            PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.MoonwalkPoseHandoffTag | moonwalkSourceShotDirection));
        }
    }

    /// <summary>
    /// Ports the jumping/falling turn initializers at <c>$91:F952/$91:F98A</c>.
    /// The input tables publish only generic `$2F/$30/$87/$88`; the initializer reads the
    /// PREVIOUS pose's shot-direction byte and substitutes one of sixteen exact turn poses.
    /// </summary>
    /// <returns>
    /// True when the selected pose fits and is installed. False means the native larger-
    /// pose collision path retained the compact source or forced stable crouch.
    /// </returns>
}
