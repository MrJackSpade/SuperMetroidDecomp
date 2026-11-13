using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's pose transitions: grounded, aerial, posture and morph-ball changes, turn and wall-jump handling, and the collision checks a larger new pose must pass, applied to the Samus state it owns.</summary>
internal static class SamusPoseTransitions
{

    /// <summary>
    /// Installs the ordinary or aimed falling pose selected by the walk-off transition when a
    /// grounded movement probe finds no floor. Starting the fall clears vertical
    /// speed and starts downward gravity before the pose is drawn.
    /// </summary>
    public static void ApplyWalkedOffFloorTransition(SamusState samus, 
RoomLevelData level, SamusPoseId targetPose,
        int frameCounter = 0, ISamusRoomObjects? roomObjects = null)
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
            // The caller computes the target from this pose's metadata immediately
            // before invoking us. A mismatch is an inconsistent request, not missing logic.
            throw new InvalidOperationException(
                $"Walk-off transition {samus.Pose} -> {targetPose} does not match the pose's falling target {expectedTarget}.");
        }

        // A crouched or compact source can grow when falling is selected. The game's
        // pose initializer runs the same pose-expansion collision as an input-driven transition,
        // including its fractional clamp. Skipping it embeds the new body in the
        // floor after a near-ground unmorph. Rejection also skips starting the fall.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
level, targetPose, frameCounter, roomObjects, out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        // Starting the fall with no floor below preserves an ascending
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
        // The input phase publishes the falling radius next frame; pose commit retains the
        // radius used by this frame's grounded movement and collision probes.
        // The pose refresh dispatches the new falling movement type through the falling initializer
        // before the fall's start initializes the downward state. That initializer derives
        // the mode from extra dash speed; it must not retain the grounded release mode.
        // Base speed itself survives until the next aerial movement behaviour consumes it.
        SamusAerialPoseTransitions.InitializeOrdinaryAerialAcceleration(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>
    /// Runs the facing lookup used by the walk-off transition for a grounded walk-off.
    /// </summary>
    public static SamusPoseId SelectFallingPoseForCurrentAim(SamusState samus)
    {
        // The falling pose selection indexes its pair with the pose's facing, not the muzzle direction.
        // Moonwalk's visual facing opposes this physics facing; aim input is reconsidered
        // by the next frame's input handler after this ordinary falling pose is installed.
        return SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? SamusPoseId.FallingLeftPose : SamusPoseId.FallingRightPose;
    }

    /// <summary>
    /// Applies the landing transition's normal/spin/aimed landing choice in a scripted context that
    /// is known to have room for the destination pose, followed by grounded collision cleanup.
    /// </summary>
    /// <remarks>
    /// Playable room code must call <see cref="SamusPoseTransitions.TryApplyAerialLanding"/> instead. The game
    /// sends every prospective landing pose through the changed-pose collision; omitting
    /// that probe lets the much taller landing body overlap a low ceiling. This roomless entry
    /// point remains for cinematic movement and focused collision-free fixtures only.
    /// </remarks>
    public static void ApplyAerialLanding(SamusState samus, 
        bool wasSpinning,
        ControllerButton controllerInput = ControllerButton.None)
    {
        SamusPoseId sourcePose = samus.Pose;
        bool leavingScrewAttack = SamusPoseMetadata.IsScrewAttackPose(sourcePose);
        SamusPoseId targetPose = SelectAerialLandingPose(samus, wasSpinning, controllerInput);

        int oldRadius = samus.Kinematics.Radius.Y;
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (samus.Kinematics.Radius.Y > oldRadius)
        {
            samus.Kinematics.Position -= new Offset16<RoomPixels>(0, samus.Kinematics.Radius.Y - oldRadius);
        }

        SettleAcceptedLanding(samus, leavingScrewAttack);
        SamusPresentation.InitializeAnimation(samus, initialFrame: StandingPoseInitialFrame(sourcePose, targetPose));
    }

    /// <summary>
    /// Applies a normal or spinning landing through the game's shared prospective-pose
    /// collision handler before committing the taller landing body.
    /// </summary>
    /// <returns>
    /// True when the selected landing pose fit. False means the game's collision retained the
    /// source pose or selected its stable-crouch fallback; the game then does not settle Samus on the landing.
    /// </returns>
    public static bool TryApplyAerialLanding(SamusState samus, 

        RoomLevelData level,
        bool wasSpinning,
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = samus.Pose;
        bool leavingScrewAttack = SamusPoseMetadata.IsScrewAttackPose(sourcePose);
        SamusPoseId targetPose = SelectAerialLandingPose(samus, wasSpinning, controllerInput);
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            frameCounter,
            roomObjects,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            samus.Pose = targetPose;
            // Correct the center now, but retain the movement frame's live radius
            // until the input phase, including when landing on a frozen enemy.
            samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
            SamusPresentation.InitializeAnimation(samus, initialFrame: StandingPoseInitialFrame(sourcePose, targetPose));
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            // Deliberately chooses stable crouch for a non-Morph source when
            // both the floor and ceiling reject the full landing body. This is what lets
            // a spin jump land in a 32-pixel passage without embedding Samus in its ceiling.
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

        // The changed-pose collision compares the requested pose with the post-collision pose. A rejected
        // expansion or crouch substitution is reported as blocked and does not settle the landing; this
        // is not an accepted landing and must retain the vertical motion.
        if (collision == LargerPoseCollisionOutcome.Allowed)
            SettleAcceptedLanding(samus, leavingScrewAttack);
        else if (leavingScrewAttack)
            // Palette restoration belongs to the pose change, before the blocked-collision gate on settling.
            samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Keeps the gun raised on frame one when both standing poses aim straight up;
    /// other standing transitions begin at frame zero.
    /// </summary>
    internal static int StandingPoseInitialFrame(SamusPoseId sourcePose, SamusPoseId targetPose)
    {
        // Keeps the gun raised when both the old and new standing
        // initializer's poses aim straight up, including landing and its completion.
        return SamusPoseMetadata.ReadMovementType(targetPose) == SamusMovementType.Standing &&
            SamusPoseMetadata.ReadAim(sourcePose).AimsStraightUp &&
            SamusPoseMetadata.ReadAim(targetPose).AimsStraightUp ? 1 : 0;
    }

    /// <summary>Chooses the landing transition's prospective landing pose.</summary>
    private static SamusPoseId SelectAerialLandingPose(SamusState samus, 
        bool wasSpinning,
        ControllerButton controllerInput)
    {
        bool facingLeft = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Left;
        if (wasSpinning)
        {
            return facingLeft
                ? SamusPoseId.SpinLandingLeftPose
                : SamusPoseId.SpinLandingRightPose;
        }

        // A pose that disables shooting lands through the ordinary facing pair before the
        // landing table is consulted; hurt and damage-boost art disable shooting.
        // Horizontal aims take the landing's extra Shot-binding test. Held Shot selects the
        // firing landing; released Shot selects the ordinary normal landing pair. The six
        // admitted aimed directions select the aimed landings.
        SamusPoseAim aim = SamusPoseMetadata.ReadAim(samus);
        if (aim.DisablesShooting)
            return facingLeft ? SamusPoseId.NormalLandingLeftPose : SamusPoseId.NormalLandingRightPose;
        bool shotHeld = (controllerInput & ControllerButton.X) != ControllerButton.None;
        return aim.Direction switch
        {
            SamusProjectileDirection.UpFacingRight => SamusPoseId.LandingAimUpRightPose,
            SamusProjectileDirection.UpRight => SamusPoseId.LandingAimDiagonalUpRightPose,
            SamusProjectileDirection.Right => shotHeld ? SamusPoseId.FiringLandingRightPose : SamusPoseId.NormalLandingRightPose,
            SamusProjectileDirection.DownRight => SamusPoseId.LandingAimDiagonalDownRightPose,
            SamusProjectileDirection.DownLeft => SamusPoseId.LandingAimDiagonalDownLeftPose,
            SamusProjectileDirection.Left => shotHeld ? SamusPoseId.FiringLandingLeftPose : SamusPoseId.NormalLandingLeftPose,
            SamusProjectileDirection.UpLeft => SamusPoseId.LandingAimDiagonalUpLeftPose,
            SamusProjectileDirection.UpFacingLeft => SamusPoseId.LandingAimUpLeftPose,
            // The straight-down aims are consumed by TryApplyCompactAerialLanding,
            // because the 10 -> 21 expansion needs room collision data.
            SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => throw new InvalidOperationException(
                $"Landing from aim {aim} requires the collision-aware compact landing operation."),
            _ => throw new InvalidOperationException(
                $"Landing from aim {aim} requires the collision-aware compact landing operation."),
        };
    }

    /// <summary>
    /// Settles Samus on a landing the changed-pose collision accepted: arms the autojump input, stops her horizontal and
    /// vertical motion, and requests the normal suit palette when she lands out of Screw Attack.
    /// </summary>
    private static void SettleAcceptedLanding(SamusState samus, bool leavingScrewAttack)
    {
        // Installs the same one-shot input handler a turn animation's transition installs.
        // Its short held-Jump window is evaluated in the next input phase, not during landing.
        if (!samus.Control.InputLocked) samus.InputSnapshot.AutoJumpInputPending = true;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;

        // Reloads the normal suit palette whenever a pose change leaves spin or
        // wall-jump movement while Screw Attack is equipped. The suit palette copy occurs
        // at the later palette phase, so publish that same deferred work here.
        if (leavingScrewAttack)
            samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
    }

    /// <summary>
    /// Applies the landing table's directions four/five when radius-ten straight-down Samus lands.
    /// Both entries select the ordinary normal landing; the 10 -> 21 expansion still runs the full
    /// block pose-change collision resolver; settling the landing clears motion only if accepted.
    /// </summary>
    public static bool TryApplyCompactAerialLanding(SamusState samus, 

        RoomLevelData level,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        if (!SamusPoseMetadata.IsCompactAerialPose(samus.Pose))
            throw new InvalidOperationException($"Compact landing requires a compact straight-down jumping or falling pose, not {samus.Pose}.");

        SamusPoseId sourcePose = samus.Pose;
        SamusPoseId targetPose = SamusPoseMetadata.ReadAimDirection(samus) switch
        {
            SamusProjectileDirection.DownFacingRight => SamusPoseId.NormalLandingRightPose,
            SamusProjectileDirection.DownFacingLeft => SamusPoseId.NormalLandingLeftPose,
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or
                SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft or SamusProjectileDirection.Left or
                SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft => throw new InvalidOperationException(
                $"Compact pose {samus.Pose} has unexpected aim {SamusPoseMetadata.ReadAim(samus)}."),
            _ => throw new InvalidOperationException(
                $"Compact pose {samus.Pose} has unexpected aim {SamusPoseMetadata.ReadAim(samus)}."),
        };
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            frameCounter,
            roomObjects,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            samus.Pose = targetPose;
            // Leaves the collision radius at the compact body's radius: this frame's enemy and
            // projectile collision still use it, and the input phase's radius update takes the
            // landing body only next frame, as for an ordinary landing.
            samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

        // Like ordinary landings, the changed-pose collision's result gates settling the landing.
        // A collision-selected crouch is not allowed to clear velocity or arm autojump.
        if (collision == LargerPoseCollisionOutcome.Allowed)
            SettleAcceptedLanding(samus, leavingScrewAttack: false);
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Installs the pose the current pose's animation names at its pose-transition cue: every pose a retail
    /// animation's cue names.
    /// </summary>
    public static bool ApplyPendingVerifiedAnimationTransition(SamusState samus)
    {
        if (samus.PendingTransitionalPose is not SamusPoseId targetPose)
            return false;

        // The game's pose initialization computes its own local delay without replacing
        // the buffer published by this frame's FX pass. The pose-transition cue adds that live
        // buffer once more after installing the new animation.
        int cueAnimationBuffer = samus.AnimationFrameBuffer;

        bool verified = new SamusPoseChange(samus.Pose, targetPose) is
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
            // Ordinary and firing landings share their animations, so the firing landings
            // end in the same transitions to normal standing.
            (SamusPoseId.FiringLandingRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.FiringLandingLeftPose, SamusPoseId.FacingLeftNormalPose) or
            // Every aerial-turn animation ends in a pose-transition cue. These pairs are the
            // poses the aerial-turn animations name, not inferred mirror poses.
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
            // Ends every moonwalk turn/jump animation with a transition to
            // the spin jump. Installing the animation's transition pose refreshes the pose, but skips the pose
            // initializer's later jump initialization. Its unchanged vertical direction permits Moonfall.
            (SamusPoseId.MoonwalkTurnJumpLeftPose or SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimDownLeftPose, SamusPoseId.SpinJumpLeftPose) or
            (SamusPoseId.MoonwalkTurnJumpRightPose or SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimDownRightPose, SamusPoseId.SpinJumpRightPose) or
            // Terminate Crystal Flash finish art in a transition to normal standing.
            // Its installed movement handler observes type zero on the following frame.
            (SamusPoseId.CrystalFlashRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.CrystalFlashLeftPose, SamusPoseId.FacingLeftNormalPose) or
            // All four drained release animations end in ordinary standing art.
            // These are the poses the drained release animations name.
            (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedStandingRightPose, SamusPoseId.FacingRightNormalPose) or
            (SamusPoseId.DrainedCrouchingLeftPose or SamusPoseId.DrainedStandingLeftPose, SamusPoseId.FacingLeftNormalPose);
        if (!verified)
        {
            // The list above is every pose a retail animation's pose-transition cue names.
            // Anything else means the caller supplied a target that was not read from the
            // current pose's animation.
            throw new InvalidOperationException(
                $"Animation transition {samus.Pose} -> {targetPose} is no retail animation's pose-transition cue.");
        }

        bool startsMoonwalkJump = SamusPoseMetadata.IsMoonwalkTurnJumpPose(samus.Pose) &&
            targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose;
        SamusPoseId sourcePose = samus.Pose;
        SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(sourcePose);

        // Runs movement-type initialization after the pose-transition cue has
        // selected the new normal-jump pose but before it initializes that pose's frame.
        // A live stored shine replaces the target with the shinespark windup, so never briefly seed
        // neutral, aimed-up or diagonally-up normal-jump animation state in this branch.
        bool beganShinespark = SamusAerialPoseTransitions.TryBeginShinesparkWindup(samus, 
            targetPose,
            previousMovementType);
        if (!beganShinespark)
        {
            // Moonwalk turn animations end in a transition to the spin jump. That pose still passes
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
            SamusGroundedPoseTransitions.ApplySimpleGroundedPoseChange(samus, sourcePose, installedPose, "Pose-transition cue",
                refreshRadius: !SamusPoseMetadata.IsAerialTurnPose(sourcePose));
        }
        if (sourcePose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or
            SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
        {
            // The drained release animation has now reached its authored normal pose.
            // Clear only the drained-release marker; pose initialization above already owns the
            // same radius and animation writes as the game's pose initializer.
            samus.Drained.CompleteRelease();
        }
        // The pose-transition cue does not run the jump initializers. An input-table
        // transition out of the same turn art DOES initialize jump velocity; keeping
        // those two routes distinct preserves both ordinary jumping and Moonfall.
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.AnimationFrameTimer = samus.AnimationFrameTimer.AddFrames(cueAnimationBuffer);
        return true;
    }
}
