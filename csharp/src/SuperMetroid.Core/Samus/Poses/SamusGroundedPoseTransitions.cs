using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's grounded pose transitions: turns, aim, running and stopping, running into a wall, moonwalk and knockback pose collision.</summary>
internal static class SamusGroundedPoseTransitions
{

    /// <summary>
    /// Installs the lookup-failure pose of an aimed crouching turn when every input is
    /// released. Definition byte two is `$28` for all six records,
    /// so the turn is abandoned for left-facing crouch art regardless of its direction.
    /// Because the pose changes, the prospective pose handler skips command two; only the pose initializer runs.
    /// </summary>
    public static void ApplyAimedCrouchingTurnInputFallback(SamusState samus, SamusPoseId targetPose)
    {
        if (!SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose))
            throw new InvalidOperationException($"Aimed crouching turn fallback requires a type-$17 crouching turn, not {samus.Pose}.");
        if (!SamusPoseMetadata.IsRightFacingCrouchingPose(targetPose) && !SamusPoseMetadata.IsLeftFacingCrouchingPose(targetPose))
            throw new InvalidDataException($"Aimed crouching turn {samus.Pose} names non-crouching fallback {targetPose}.");
        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Aimed crouching turn release");
    }


    /// <summary>
    /// Applies prospective pose change command two when a turn's input lookup
    /// fails and the pose definition retains the current pose. Movement types $0E, $17 and
    /// $18 all select command two in the lookup-failure table. This stops reverse acceleration without
    /// restarting the unfinished turn animation or erasing velocity words.
    /// </summary>
    public static void ApplyTurnInputFallback(SamusState samus, SamusMovementType movementType)
    {
        if (movementType is not (SamusMovementType.TurningOnGround or
            SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling))
            throw new InvalidOperationException($"Turn fallback requires a turning movement type, not {movementType}.");
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
    }


    /// <summary>
    /// Exact 16.16 add-with-carry equivalent shared by the grounded, aerial, and spin-turn
    /// initializers. It deliberately permits word overflow, matching the game's arithmetic.
    /// </summary>
    internal static void FoldExtraRunSpeedIntoBaseAndStartTurn(SamusState samus)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        uint combinedSpeed = unchecked(speed.BaseFixed +
            speed.ExtraRunSpeed.RawValue);
        speed.BaseSpeed = UnsignedSixteenSixteen.FromRaw(combinedSpeed);
        speed.ExtraRunSpeed = default;
        speed.AccelerationMode = SamusAccelerationMode.Turning;
    }


    /// <summary>
    /// Applies a same-facing arm-body transition within the movement-type-zero standing
    /// family and movement-type-one running family. Besides aimed bodies, this includes the
    /// real `$0B/$0C` horizontal-fire records.
    /// </summary>
    /// <remarks>
    /// All admitted records have radius 21. If both endpoints are running, the pose initializer writes
    /// 32768 to the new-frame word and the animation setup preserves the current frame and timer;
    /// this matters when firing midway through the ten-frame run cycle. A transition between
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
            // The input tables never feed a facing reversal through this arm-only
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
            // This is the pose initializer's ordinary target installation. Replacing Pose
            // directly would omit radius refresh, delay-list selection, and command cleanup.
            ApplySimpleGroundedPoseChange(samus, sourcePose, targetPose, "Grounded arm");
            return;
        }

        // Sees previous movement type one and publishes 32768. The sign test
        // in the animation setup exits before touching frame, timer, or delay-buffer state. Every live
        // running arm pose points to the same delay list, so the cached list address is
        // already exact and must not be rebound or reset here.
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
    /// Implements the ten-entry shot-direction selector of the wall-stop pose. The input pose
    /// may be the current running pose after a killed-X-speed collision or the prospective
    /// running pose tested by the one-pixel arm-pump probe.
    /// </summary>
    public static SamusPoseId SelectRanIntoWallPose(SamusPoseId sourcePose)
    {
        int shotDirection = SamusPoseMetadata.ReadShotDirection(sourcePose);
        return shotDirection switch
        {
            0 => SamusPoseId.StandingAimUpRightPose,
            1 => SamusPoseId.RanIntoWallAimUpRightPose,
            2 or 4 => SamusPoseId.RanIntoWallRightPose,
            3 => SamusPoseId.RanIntoWallAimDownRightPose,
            5 or 7 => SamusPoseId.RanIntoWallLeftPose,
            6 => SamusPoseId.RanIntoWallAimDownLeftPose,
            8 => SamusPoseId.RanIntoWallAimUpLeftPose,
            9 => SamusPoseId.StandingAimUpLeftPose,
            _ => throw new InvalidDataException(
                $"Pose {sourcePose} has invalid shot-direction byte ${shotDirection:X2}."),
        };
    }


    /// <summary>
    /// Ports <c>CheckIfProspectivePoseRunsIntoAWall</c>.
    /// </summary>
    /// <remarks>
    /// A current type-one collision maps the current pose immediately. Otherwise, only a
    /// prospective type-one pose is interesting: the game first probes solid enemies one pixel
    /// ahead in the CURRENT pose direction and maps the prospective pose without moving when
    /// one is hit. Otherwise it moves Samus that pixel, retains the move when clear, and maps
    /// the prospective pose's shot direction when a block stops it. The retained move is the
    /// retail arm-pump bug.
    /// </remarks>
    public static SamusPoseId? CheckProspectiveRunningPoseForWall(SamusState samus, 

        RoomLevelData level,
        SamusPoseId? prospectivePose,
        bool currentXSpeedKilledByBlock,
        out BlockMoveResult? onePixelProbe,
        ISamusRoomPlms? plms = null)
    {

        Ensure.NotNull(level);
        onePixelProbe = null;

        if (currentXSpeedKilledByBlock && SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Running)
            return SelectRanIntoWallPose(samus.Pose);

        if (prospectivePose is not { } target ||
            SamusPoseMetadata.ReadMovementKind(target) != SamusMovementType.Running)
            return null;

        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        if (samus.Kinematics.InteractiveEnemies.Count != 0)
        {
            // Samus_vs_SolidEnemy_CollisionDetection with a requested distance of one whole pixel
            // and the facing direction stored as the collision movement direction.
            SamusCollisionDirection direction = facingLeft
                ? SamusCollisionDirection.Left
                : SamusCollisionDirection.Right;
            SolidEnemyCollisionResult enemyProbe = SamusSolidEnemyCollision.Probe(
                samus.Kinematics, samus.Kinematics.InteractiveEnemies, direction, distance: 1, distanceSubposition: 0);
            samus.Kinematics.RecordSolidEnemyCollision(direction, enemyProbe.EnemySlot);
            if (enemyProbe.Collided)
                return SelectRanIntoWallPose(target);
        }

        int onePixelForward = facingLeft
            ? -(1 << 16)
            : 1 << 16;
        onePixelProbe = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = onePixelForward, AlignToSlopeAfterMovement = false });
        return onePixelProbe.Value.Collided
            ? SelectRanIntoWallPose(target)
            : null;
    }


    /// <summary>
    /// Installs the pose selected when the wall check finds a block wall. This deliberately
    /// accepts standing/running/landing sources as well as an existing wall-stop source:
    /// Can test a prospective run before that run is installed, and changing aim
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
            // Only block-backed ran-into-wall records call this exact initializer. Treat a
            // different source/target pair as misuse of the specialized operation.
            throw new InvalidOperationException(
                $"Ran-into-wall pose change {samus.Pose} -> {targetPose} is not a block-backed retail route.");
        }

        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Ran into wall");
    }


    /// <summary>Applies `$89/$8A/$CF-$D2`'s same-facing exit to a type-one running pose.</summary>
    public static void ApplyRanIntoWallToRunning(SamusState samus, SamusPoseId targetPose)
    {
        bool rightRoute = SamusPoseMetadata.IsRightFacingRanIntoWallPose(samus.Pose) &&
            SamusPoseMetadata.IsRightFacingRunningPose(targetPose);
        bool leftRoute = SamusPoseMetadata.IsLeftFacingRanIntoWallPose(samus.Pose) &&
            SamusPoseMetadata.IsLeftFacingRunningPose(targetPose);
        if (!rightRoute && !leftRoute)
        {
            // Running exits are a strict same-facing subset of the ran-into-wall table.
            throw new InvalidOperationException(
                $"Ran-into-wall running exit {samus.Pose} -> {targetPose} is not a retail route.");
        }

        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Ran into wall to running");
    }


    /// <summary>
    /// Applies the movement-type-$10 option gate and stable-pose changes surrounding
    /// <c>InitializeSamusPose_Moonwalking</c>.
    /// </summary>
    /// <remarks>
    /// Standing transition tables always publish a moonwalk candidate when Shoot and the
    /// backward direction are held. The settings word decides what that candidate
    /// means: enabled retains `$49/$4A/$75-$78`; disabled substitutes ordinary `$25/$26`
    /// turn art. Once active, aim changes and the direct return to forward running are
    /// ordinary radius-21 pose installations with no invented velocity adjustment.
    /// </remarks>
    public static void ApplyMoonwalkPoseChange(SamusState samus, 
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
            // Normal landing `$A4/$A5` points at the same grounded input records as
            // ordinary standing. Consequently a backward direction can nominate the
            // corresponding Moonwalk pose before the option gate runs. The initializer
            // does not special-case the old landing movement type: with Moonwalk off it
            // substitutes the same ordinary `$25/$26` turn used from `$01/$02`.
            // Tests the candidate pose-X byte. `$4A/$76/$78` store four
            // and become right-to-left `$25`; `$49/$75/$77` store eight and become `$26`.
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
            // The complete stable moonwalk table is represented by the predicates above;
            // reaching this branch means the caller selected the wrong pose-change API.
            throw new InvalidOperationException(
                $"Moonwalk pose change {samus.Pose} -> {targetPose} is not a stable retail route.");
        }

        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Moonwalk");
    }


    /// <summary>
    /// Applies the moonwalk turn when a stable moonwalk pose reverses while Jump is held.
    /// The six requested `$BF-$C4` records are not airborne yet: they retain the grounded
    /// turn movement handler while their three visible frames play.
    /// </summary>
    public static void ApplyMoonwalkTurnJump(SamusState samus, SamusPoseId targetPose)
    {

        SamusPoseId expectedTarget = SamusPoseMetadata.ReadShotDirection(samus) switch
        {
            1 => SamusPoseId.MoonwalkTurnJumpAimUpLeftPose,
            2 => SamusPoseId.MoonwalkTurnJumpLeftPose,
            3 => SamusPoseId.MoonwalkTurnJumpAimDownLeftPose,
            6 => SamusPoseId.MoonwalkTurnJumpAimDownRightPose,
            7 => SamusPoseId.MoonwalkTurnJumpRightPose,
            8 => SamusPoseId.MoonwalkTurnJumpAimUpRightPose,
            // Stable moonwalk poses publish only the six directions enumerated above.
            // Another value means the pose state or metadata is inconsistent, not missing logic.
            _ => throw new InvalidDataException(
                $"Moonwalk shot direction ${SamusPoseMetadata.ReadShotDirection(samus):X2} is invalid for a stable moonwalk pose."),
        };
        SamusPoseId inputTableTarget = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.MoonwalkTurnJumpLeftPose : SamusPoseId.MoonwalkTurnJumpRightPose;
        if (!SamusPoseMetadata.IsMoonwalkingPose(samus.Pose) || targetPose != expectedTarget && targetPose != inputTableTarget)
        {
            // Input requests the un-aimed turn; the pose initializer then substitutes
            // the old muzzle's aimed turn. Already-resolved diagnostic callers are valid too.
            throw new InvalidOperationException(
                $"Moonwalk turn/jump {samus.Pose} -> {targetPose} is not a retail route.");
        }

        // Preserve the old muzzle direction before installing turn art. The next alpha
        // uses the handed shot to admit the turning HUD producer and forcibly release charge.
        int sourceShotDirection = SamusPoseMetadata.ReadShotDirection(samus);
        FoldExtraRunSpeedIntoBaseAndStartTurn(samus);
        ApplySimpleGroundedPoseChange(samus, samus.Pose, expectedTarget, "Moonwalk turn/jump");
        samus.PendingTransitionShot = new SamusTransitionShot(sourceShotDirection);
    }


    /// <summary>
    /// Applies the verified ordinary transition from standing-right <see cref="SamusPoseId.FacingRightNormalPose"/> to running-
    /// right <see cref="SamusPoseId.MovingRightNormalPose"/> at the end-of-frame transition seam.
    /// </summary>
    /// <remarks>
    /// This is intentionally not a general pose setter. For this transition, momentum
    /// routine index zero is a no-op, both poses have radius 21, and <see cref="SamusPoseId.MovingRightNormalPose"/> uses the
    /// already-supported default render/tile paths. Other transitions can change momentum,
    /// align radii, invoke collision, or run specialized handlers and remain rejected.
    /// </remarks>
    public static void ApplyStandingRightToRunningRight(SamusState samus)
    {
        if (samus.Pose != SamusPoseId.FacingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Standing-right to running-right transition requires pose $01, not {samus.Pose}.");
        }

        samus.Pose = SamusPoseId.MovingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // The pose initializer resets animation frame zero and loads MovingRightNormalPose's first
        // delay byte after SamusFunc_F433 refreshes pose direction/movement metadata.
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }


    /// <summary>
    /// Applies the verified no-button fallback from running-right <see cref="SamusPoseId.MovingRightNormalPose"/> to standing-right
    /// <see cref="SamusPoseId.FacingRightNormalPose"/> after ordinary running momentum has decelerated to zero.
    /// </summary>
    public static void ApplyRunningRightToStandingRight(SamusState samus)
    {
        if (samus.Pose != SamusPoseId.MovingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Running-right to standing-right transition requires pose $09, not {samus.Pose}.");
        }

        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // MovingRightNormalPose's new-pose-unless-buttons byte is $01. Once Samus_Pose_Func2 selects
        // momentum index two, the momentum cancellation cancels the boost words but deliberately leaves the
        // numeric extra pair intact. The following standing movement pass consumes that
        // final no-base-speed displacement before clearing every X-motion word.
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }


    /// <summary>Applies the ordinary grounded $02 -> $0A start-moving-left transition.</summary>
    public static void ApplyStandingLeftToRunningLeft(SamusState samus) =>
        ApplySimpleGroundedPoseChange(samus, 
            SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingLeftNormalPose,
            "Standing-left to running-left");


    /// <summary>Applies the no-button grounded $0A -> $02 fallback.</summary>
    public static void ApplyRunningLeftToStandingLeft(SamusState samus)
    {
        ApplySimpleGroundedPoseChange(samus, 
            SamusPoseId.MovingLeftNormalPose,
            SamusPoseId.FacingLeftNormalPose,
            "Running-left to standing-left");

        // This is the mirrored momentum-index-two cancellation route used by `$09 -> $01`.
        // ExtraRunSpeed/Subspeed remain available to standing's ordered movement/clear pass.
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
    }


    /// <summary>
    /// Applies the shared standing/landing transition-table route from any right-facing
    /// landing (including firing `$E6`) to running right `$09`, or from the mirrored left
    /// family (including `$E7`) to `$0A`. Landing movement has already cleared momentum in
    /// this frame; the new running pose begins accelerating on the next frame.
    /// </summary>
    public static void ApplyLandingToRunning(SamusState samus, SamusPoseId targetPose)
    {
        bool right = SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose) &&
            targetPose == SamusPoseId.MovingRightNormalPose;
        bool left = SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose) &&
            targetPose == SamusPoseId.MovingLeftNormalPose;
        if (!right && !left)
        {
            throw new InvalidOperationException(
                $"Landing-to-run transition {samus.Pose} -> {targetPose} is not verified.");
        }

        ApplySimpleGroundedPoseChange(samus, samus.Pose, targetPose, "Landing-to-run");
    }


    /// <summary>
    /// Applies the grounded turn initialization. Input tables
    /// first publish generic `$25/$26`; the initializer indexes the previous pose's shot
    /// direction through the grounded turn table and may replace it with `$8B-$8E/$9C/$9D`.
    /// </summary>
    public static void ApplyGroundedTurn(SamusState samus, SamusPoseId targetPose, ControllerButton controllerInput = ControllerButton.None)
    {

        // Gives the two front-view poses a deliberately unusual route.
        // Their pose-X direction is zero, so they cannot honestly be classified as either
        // a right-facing or left-facing source. The game instead skips the shot-direction
        // lookup entirely when the previous pose is `$00/$9B`, retaining the generic
        // `$25/$26` selected by that front-view pose's input transition table.
        bool wasForwardFacing = SamusPoseMetadata.IsForwardFacingPose(samus.Pose);

        // Crouching transition tables publish `$43/$44` directly, whereas every admitted
        // standing/running/landing table publishes `$25/$26`. The initializer still uses
        // previous movement type five to choose the full crouched aim-preserving table.
        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        bool wasCrouching = sourceMovementType == SamusMovementType.Crouching;
        bool wasMoonwalking = sourceMovementType == SamusMovementType.Moonwalking;

        // Reads the PREVIOUS moonwalk pose, not the selected turn pose.
        // Preserve the complete source byte now so the low-nibble projectile direction is
        // still available after ApplySimpleGroundedPoseChange replaces `Pose` below.
        int moonwalkSourceShotDirection = wasMoonwalking
            ? SamusPoseMetadata.ReadShotDirection(samus)
            : 0;

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
                $"Grounded turn {samus.Pose} -> {targetPose} is not a verified transition.");
        }

        // F8F9 checks held Jump, not a fresh press. Releasing Shoot from a buffered
        // Moonwalk selects a generic turn in the input table, then reaches this same
        // turn/jump initializer without a new Jump edge.
        if (wasMoonwalking && (controllerInput & ControllerButton.A) != ControllerButton.None)
        {
            ApplyMoonwalkTurnJump(samus, turnsLeft
                ? SamusPoseId.MoonwalkTurnJumpLeftPose : SamusPoseId.MoonwalkTurnJumpRightPose);
            return;
        }

        // Reads the PREVIOUS pose record before it installs the final turn art.
        // Previous movement type five selects the second shot-direction table; every other admitted source uses
        // the first. Both tables have ten entries, but shot directions four/five belong to
        // the compact straight-down family that remains untranslated.
        SamusPoseId selectedTurnPose;
        if (wasForwardFacing)
        {
            // This is the literal early branch: keep the transition-table
            // result. Reading byte three from `$00/$9B` here would manufacture an aim/facing
            // direction that the game explicitly declines to inspect.
            selectedTurnPose = targetPose;
        }
        else
        {
            int shotDirection = SamusPoseMetadata.ReadShotDirection(samus);
            selectedTurnPose = wasCrouching
                ? shotDirection switch
                {
                    0 => SamusPoseId.TurningRightToLeftCrouchingAimUpPose,
                    1 => SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose,
                    2 => SamusPoseId.TurningRightToLeftCrouchingPose,
                    3 => SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose,
                    6 => SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose,
                    7 => SamusPoseId.TurningLeftToRightCrouchingPose,
                    8 => SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose,
                    9 => SamusPoseId.TurningLeftToRightCrouchingAimUpPose,
                    // Grounded/crouched pose metadata cannot publish the compact-aerial
                    // directions 4/5 (or any value outside the eight-entry set).
                    _ => throw new InvalidDataException(
                        $"Crouched turn shot direction ${shotDirection:X2} is invalid."),
                }
                : shotDirection switch
                {
                    0 => SamusPoseId.TurningRightToLeftAimUpPose,
                    1 => SamusPoseId.TurningRightToLeftAimDiagonalUpPose,
                    2 => SamusPoseId.TurningRightToLeftPose,
                    3 => SamusPoseId.TurningRightToLeftAimDiagonalDownPose,
                    6 => SamusPoseId.TurningLeftToRightAimDiagonalDownPose,
                    7 => SamusPoseId.TurningLeftToRightPose,
                    8 => SamusPoseId.TurningLeftToRightAimDiagonalUpPose,
                    9 => SamusPoseId.TurningLeftToRightAimUpPose,
                    _ => throw new InvalidDataException(
                        $"Grounded turn shot direction ${shotDirection:X2} is invalid."),
                };
        }

        if (!wasForwardFacing &&
            ((turnsLeft && !SamusPoseMetadata.IsRightToLeftGroundTurnPose(selectedTurnPose)) ||
             (turnsRight && !SamusPoseMetadata.IsLeftToRightGroundTurnPose(selectedTurnPose))))
        {
            throw new InvalidOperationException(
                $"Grounded turn source {samus.Pose} has direction metadata inconsistent with target {targetPose}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Folds the run-button/speed-booster component into base speed
        // with two independent 16-bit ADCs and their carry. Use one wrapping 16.16 add to
        // preserve exactly that pair of operations, including overflow at the high word.
        uint combinedSpeed = unchecked(speed.BaseFixed +
            speed.ExtraRunSpeed.RawValue);
        speed.BaseSpeed = UnsignedSixteenSixteen.FromRaw(combinedSpeed);

        // The handler consumes the extra component and selects acceleration mode
        // one. The direction selector interprets that mode as “move opposite the NEW facing direction”
        // while the speed calculation decelerates, which is how reversal preserves old momentum.
        speed.ExtraRunSpeed = default;
        speed.AccelerationMode = SamusAccelerationMode.Turning;

        ApplySimpleGroundedPoseChange(samus, samus.Pose, selectedTurnPose, "Grounded turn");

        if (wasMoonwalking)
        {
            // The handed shot makes the turning HUD handler treat the one-frame turning
            // state as shooting. The beam producer later fires it in this source pose's
            // direction when a beam slot can actually be initialized.
            samus.PendingTransitionShot = new SamusTransitionShot(moonwalkSourceShotDirection);
        }
    }

    /// <summary>
    /// Runs UpdateSamusPose's shared expansion check before the interrupted hurt
    /// pose is installed. Crouch-to-hurt must keep the larger body above its floor.
    /// </summary>
    internal static bool TryResolveKnockbackPoseCollision(SamusState samus, 
RoomLevelData level, SamusPoseId targetPose,
        int frameCounter, ISamusRoomPlms? plms)
    {
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome outcome = samus.ResolveLargerPoseCollision(
level, targetPose, frameCounter, plms, out int adjustment);
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
    /// Installs one already-validated grounded pose and runs the common pose initializer's
    /// metadata/radius work, initializing frame zero only for a changed pose.
    /// </summary>
    internal static void ApplySimpleGroundedPoseChange(SamusState samus, 
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
        // Aerial turn completion runs F433 without SetRadius; its source body
        // remains active until next alpha, including compact down-aim targets.
        if (refreshRadius)
            SamusPoseMetadata.RefreshCollisionRadii(samus);
        // Retains the frame/timer when the ordinary pose is
        // unchanged. A prospective run rejected by the wall probe can resolve
        // back to the current wall-stop pose on every held-input frame.
        if (expectedPose != targetPose)
            SamusPresentation.InitializeAnimation(samus, initialFrame: SamusPoseTransitions.StandingPoseInitialFrame(expectedPose, targetPose));
    }
}
