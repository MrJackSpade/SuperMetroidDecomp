using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's pose transitions: grounded, aerial, posture and morph-ball changes, turn and wall-jump handling, and the collision checks a larger new pose must pass, applied to the Samus state it owns.</summary>
internal static class SamusPoseTransitions
{

    /// <summary>
    /// Installs the ordinary or aimed falling pose selected by <c>$91:E8F2</c> when a
    /// grounded movement probe finds no floor. The collision command clears vertical
    /// speed and starts downward gravity before the pose is drawn.
    /// </summary>
    public static void ApplyWalkedOffFloorTransition(SamusState samus, 
RoomLevelData level, SamusPoseId targetPose,
        int nmiFrameCounter = 0, RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        bool supportedSource =
            SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRunningPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkingPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkTurnJumpPose(samus.Pose) ||
            samus.Pose is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose ||
            SamusPoseMetadata.IsRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose);
        SamusPoseId expectedTarget = SelectFallingPoseForCurrentAim(samus);
        if (!supportedSource || targetPose != expectedTarget)
        {
            // The caller computes the target from this pose's ROM metadata immediately
            // before invoking us. A mismatch is an inconsistent request, not missing logic.
            throw new InvalidOperationException(
                $"Walk-off transition ${(int)samus.Pose:X2} -> ${(int)targetPose:X2} does not match the pose's ROM-selected falling target ${(int)expectedTarget:X2}.");
        }

        // A crouched or compact source can grow when falling is selected. Native
        // F404 runs the same pose-expansion collision as an input-driven transition,
        // including its fractional clamp. Skipping it embeds the new body in the
        // floor after a near-ground unmorph. Rejection also skips command five.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
level, targetPose, nmiFrameCounter, plms, out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        // Command five's no-floor branch ($91:EFEF) preserves an ascending
        // velocity, even if a special handler left it on a standing pose. Crystal
        // Flash can do exactly that; unconditional reset shortens its return arc.
        if (samus.Kinematics.YDirection != SamusVerticalDirection.Up)
        {
            samus.MorphBallBounceState = SamusBallBounceState.None;
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }
        SamusAerialMovement.ConfigureEnvironmentGravity(samus);
        samus.Pose = targetPose;
        // Alpha publishes the falling radius next frame; pose commit retains the
        // radius used by this frame's grounded movement and collision probes.
        // SamusFunc_F433 dispatches the new falling movement type through $91:F60D
        // before command five initializes the downward state. That initializer derives
        // the mode from extra dash speed; it must not retain the grounded release mode.
        // Base speed itself survives until the next aerial movement routine consumes it.
        SamusAerialPoseTransitions.InitializeOrdinaryAerialAcceleration(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>
    /// Ports the facing lookup used by `$91:E8F2` for a grounded walk-off.
    /// </summary>
    public static SamusPoseId SelectFallingPoseForCurrentAim(SamusState samus)
    {
        // PSP_Falling indexes its pair with PoseXDirection, not the muzzle direction.
        // Moonwalk's visual facing opposes this physics byte; aim input is reconsidered
        // by the next frame's input handler after this ordinary falling pose is installed.
        return SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? SamusPoseId.FallingLeftPose : SamusPoseId.FallingRightPose;
    }

    /// <summary>
    /// Applies <c>$91:E95D</c>'s normal/spin/aimed landing choice in a scripted context that
    /// is known to have room for the destination pose, followed by grounded collision cleanup
    /// at <c>$91:F010</c>.
    /// </summary>
    /// <remarks>
    /// Playable room code must call <see cref="SamusPoseTransitions.TryApplyAerialLanding"/> instead. The cartridge
    /// sends every prospective landing pose through <c>HandleCollDueToChangedPose</c>; omitting
    /// that probe lets the much taller landing body overlap a low ceiling. This roomless entry
    /// point remains for cinematic movement and focused collision-free fixtures only.
    /// </remarks>
    public static void ApplyAerialLanding(SamusState samus, 
        bool wasSpinning,
        SnesButton controllerInput = SnesButton.None)
    {
        SamusPoseId sourcePose = samus.Pose;
        bool leavingScrewAttack = SamusPoseMetadata.IsScrewAttackPose(sourcePose);
        SamusPoseId targetPose = SelectAerialLandingPose(samus, wasSpinning, controllerInput);

        ushort oldRadius = samus.Kinematics.Radius.Y;
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (samus.Kinematics.Radius.Y > oldRadius)
        {
            ushort difference = unchecked((ushort)(samus.Kinematics.Radius.Y - oldRadius));
            samus.Kinematics.Position -= new Offset16<RoomPixels>(0, unchecked((short)difference));
        }

        ApplyAerialLandingCollisionCommand(samus, leavingScrewAttack);
        SamusPresentation.InitializeAnimation(samus, initialFrame: StandingPoseInitialFrame(sourcePose, targetPose));
    }

    /// <summary>
    /// Applies a normal or spinning landing through the cartridge's shared prospective-pose
    /// collision handler before committing the taller landing body.
    /// </summary>
    /// <returns>
    /// True when the selected landing pose fit. False means native collision retained the
    /// source pose or selected its stable-crouch fallback; native skips the landing command.
    /// </returns>
    public static bool TryApplyAerialLanding(SamusState samus, 

        RoomLevelData level,
        bool wasSpinning,
        SnesButton controllerInput,
        int nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = samus.Pose;
        bool leavingScrewAttack = SamusPoseMetadata.IsScrewAttackPose(sourcePose);
        SamusPoseId targetPose = SelectAerialLandingPose(samus, wasSpinning, controllerInput);
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            samus.Pose = targetPose;
            // Correct the center now, but retain the movement frame's live radius
            // until alpha, including when landing on a frozen enemy.
            samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
            SamusPresentation.InitializeAnimation(samus, initialFrame: StandingPoseInitialFrame(sourcePose, targetPose));
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            // `$91:FFA7` deliberately chooses stable crouch for a non-Morph source when
            // both the floor and ceiling reject the full landing body. This is what lets
            // a spin jump land in a 32-pixel passage without embedding Samus in its ceiling.
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

        // F404 compares the requested pose with the post-collision pose. A rejected
        // expansion or crouch substitution returns carry and skips command five; this
        // is not an accepted landing and must retain the vertical motion words.
        if (collision == LargerPoseCollisionOutcome.Allowed)
            ApplyAerialLandingCollisionCommand(samus, leavingScrewAttack);
        else if (leavingScrewAttack)
            // Palette restoration belongs to F433, before the command carry gate.
            samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Keeps the gun raised on frame one when both standing poses aim straight up;
    /// other standing transitions begin at frame zero.
    /// </summary>
    internal static ushort StandingPoseInitialFrame(SamusPoseId sourcePose, SamusPoseId targetPose)
    {
        // $91:F4DC keeps the gun raised when both the old and new standing
        // initializer's poses aim straight up, including landing and its completion.
        return SamusPoseMetadata.ReadMovementType(targetPose) == SamusMovementType.Standing &&
            SamusPoseMetadata.ReadShotDirection(sourcePose) is 0 or 9 &&
            SamusPoseMetadata.ReadShotDirection(targetPose) is 0 or 9 ? (ushort)1 : (ushort)0;
    }

    /// <summary>Chooses <c>$91:E95D</c>'s prospective landing pose.</summary>
    private static SamusPoseId SelectAerialLandingPose(SamusState samus, 
        bool wasSpinning,
        SnesButton controllerInput)
    {
        byte direction = SamusPoseMetadata.ReadPoseXDirection(samus);
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
        bool shotHeld = (controllerInput & SnesButton.X) != 0;
        return SamusPoseMetadata.ReadShotDirection(samus) switch
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
    private static void ApplyAerialLandingCollisionCommand(SamusState samus, bool leavingScrewAttack)
    {
        // $91:F1EC installs the same one-shot input handler used by F8.
        // Its short held-Jump window is evaluated next alpha, not during landing.
        if (!samus.Control.AlphaLocked) samus.InputSnapshot.AutoJumpInputPending = true;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;

        // `$91:F433` reloads the normal suit palette whenever a pose change leaves spin or
        // wall-jump movement while Screw Attack is equipped. The host palette copy occurs
        // at the later `$91:D6F7` phase, so publish that same deferred work here.
        if (leavingScrewAttack)
            samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
    }

    /// <summary>
    /// Applies `$91:E9F3` directions four/five when radius-ten straight-down Samus lands.
    /// Both entries select ordinary `$A4/$A5`; the 10 -> 21 expansion still runs the full
    /// block pose-change collision resolver; command five clears motion only if accepted.
    /// </summary>
    public static bool TryApplyCompactAerialLanding(SamusState samus, 

        RoomLevelData level,
        int nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        if (!SamusPoseMetadata.IsCompactAerialPose(samus.Pose))
            throw new InvalidOperationException($"Compact landing requires pose $17/$18/$2D/$2E, not ${(int)samus.Pose:X2}.");

        SamusPoseId sourcePose = samus.Pose;
        SamusPoseId targetPose = SamusPoseMetadata.ReadShotDirection(samus) switch
        {
            4 => SamusPoseId.NormalLandingRightPose,
            5 => SamusPoseId.NormalLandingLeftPose,
            byte shotDirection => throw new InvalidOperationException(
                $"Compact pose ${(int)samus.Pose:X2} has unexpected shot direction ${shotDirection:X2}."),
        };
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            samus.Pose = targetPose;
            // $91:FDAE leaves $0B00 at the compact body's radius: this frame's enemy and
            // projectile collision still use it, and alpha's SetSamusRadius takes the
            // landing body only next frame, as for an ordinary landing.
            samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

        // Like ordinary landings, F404's changed-pose result gates command five.
        // A collision-selected crouch is not allowed to clear velocity or arm autojump.
        if (collision == LargerPoseCollisionOutcome.Allowed)
            ApplyAerialLandingCollisionCommand(samus, leavingScrewAttack: false);
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Consumes command $FD/$F8's command-three pose operand for every animation route in
    /// the current grounded/ordinary-air slice.
    /// </summary>
    public static bool ApplyPendingVerifiedAnimationTransition(SamusState samus)
    {
        if (samus.PendingTransitionalPose is not SamusPoseId targetPose)
            return false;

        // Native pose initialization computes its own local delay without replacing
        // the buffer published by this frame's FX pass. Command three adds that live
        // buffer once more after installing the new animation.
        int commandAnimationBuffer = samus.AnimationFrameBuffer;

        bool verified = (samus.Pose, targetPose) is
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
                $"Animation transition ${(int)samus.Pose:X2} -> ${(int)targetPose:X2} is not an active retail animation-command route.");
        }

        bool startsMoonwalkJump = SamusPoseMetadata.IsMoonwalkTurnJumpPose(samus.Pose) &&
            targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose;
        SamusPoseId sourcePose = samus.Pose;
        SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(sourcePose);

        // `$91:F404` runs movement-type initialization after the animation command has
        // selected the new normal-jump pose but before it initializes that pose's frame.
        // A live stored shine replaces the target with `$C7/$C8`, so never briefly seed
        // `$4D/$4E/$15/$16/$69/$6A` animation state in this branch.
        bool beganShinespark = SamusAerialPoseTransitions.TryBeginShinesparkWindup(samus, 
            targetPose,
            previousMovementType);
        if (!beganShinespark)
        {
            // Moonwalk turn lists end in literal `$F8,$19/$1A`. That operand is still fed
            // through movement-type initialization, so equipment substitution occurs here
            // just as it does for an input-table jump from ordinary running.
            SamusPoseId installedPose = startsMoonwalkJump
                ? SamusAerialPoseTransitions.SelectEquippedSpinPose(samus, targetPose)
                : targetPose;
            // Animation-owned pose changes still run the target movement initializer.
            // In particular an aerial turn must stop decelerating in the old direction
            // when its final normal-jump/falling pose is installed, even if water made
            // the turn finish before base speed reached zero. Do not clear that speed.
            if (SamusPoseMetadata.ReadMovementType(installedPose) is SamusMovementType.NormalJumping or SamusMovementType.Falling)
                SamusAerialPoseTransitions.InitializeOrdinaryAerialAcceleration(samus);
            SamusGroundedPoseTransitions.ApplySimpleGroundedPoseChange(samus, sourcePose, installedPose, "Animation command",
                refreshRadius: !SamusPoseMetadata.IsAerialTurnPose(sourcePose));
        }
        if (sourcePose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or
            SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
        {
            // The actor-owned release command has now reached its ROM-authored normal pose.
            // Clear only the host handler marker; pose initialization above already owns the
            // same radius and animation writes as native `$91:F404/$91:FB08`.
            samus.Drained.CompleteRelease();
        }
        // Super-special command three does not run FBBB/FC99. An input-table
        // transition out of the same turn art DOES initialize jump velocity; keeping
        // those two routes distinct preserves both ordinary jumping and Moonfall.
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.AnimationFrameTimer = NativeCountdown.FromRaw(unchecked((ushort)(samus.AnimationFrameTimer.RawValue + commandAnimationBuffer)));
        return true;
    }
}
