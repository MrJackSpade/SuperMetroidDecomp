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
    public static void ApplyWalkedOffFloorTransition(
        SamusState samus,
        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);
        bool supportedSource =
            SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRunningPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkingPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkTurnJumpPose(samus.Pose) ||
            SamusPoseMetadata.IsKnockbackPose(samus.Pose) ||
            SamusPoseMetadata.IsRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose);
        SamusPoseId expectedTarget = SelectFallingPoseForCurrentAim(samus);
        if (!supportedSource || targetPose != expectedTarget)
        {
            // The target comes from this pose's metadata immediately
            // before this runs. A mismatch is an inconsistent request, not missing logic.
            throw new InvalidOperationException(
                $"Walk-off transition {samus.Pose} -> {targetPose} does not match the pose's falling target {expectedTarget}.");
        }

        // A crouched or compact source can grow when falling is selected. The game's
        // pose setup runs the same pose-expansion collision as an input-driven transition,
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
        // velocity, even if an ability that moved her left it on a standing pose. Crystal
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
        // The pose refresh runs the new falling movement type through the falling setup
        // before the fall's start initializes the downward state. That setup derives
        // the mode from extra dash speed; it must not retain the grounded release mode.
        // Base speed itself survives until the next aerial movement behaviour consumes it.
        SamusAerialPoseTransitions.InitializeOrdinaryAerialAcceleration(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Runs the facing lookup used by the walk-off transition for a grounded walk-off.
    /// </summary>
    public static SamusPoseId SelectFallingPoseForCurrentAim(SamusState samus)
    {
        // The falling pose selection indexes its pair with the pose's facing, not the muzzle direction.
        // Moonwalk's visual facing opposes this physics facing; aim input is reconsidered
        // by the next frame's input phase after this ordinary falling pose is installed.
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
    /// point is for cinematic movement only.
    /// </remarks>
    public static void ApplyAerialLanding(
        SamusState samus,
        bool wasSpinning,
        GameplayAction controllerInput)
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
        SamusPresentation.InitializeAnimation(samus, StandingPoseInitialFrame(sourcePose, targetPose));
    }

    /// <summary>
    /// Applies a normal or spinning landing through the game's shared prospective-pose
    /// collision response before committing the taller landing body.
    /// </summary>
    /// <returns>
    /// True when the selected landing pose fit. False means the game's collision retained the
    /// source pose or selected its stable-crouch fallback; the game then does not settle Samus on the landing.
    /// </returns>
    public static bool TryApplyAerialLanding(
        SamusState samus,
        RoomLevelData level,
        bool wasSpinning,
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects)
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
            SamusPresentation.InitializeAnimation(samus, StandingPoseInitialFrame(sourcePose, targetPose));
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            // A non-Morph source takes the stable crouch when
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
            // Shade restoration belongs to the pose change, before the blocked-collision gate on settling.
            samus.HorizontalSpeed.RequestSuitShadeRestore();
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Keeps the gun raised on frame one when both standing poses aim straight up;
    /// other standing transitions begin at frame zero.
    /// </summary>
    internal static SamusAnimationFrame StandingPoseInitialFrame(SamusPoseId sourcePose, SamusPoseId targetPose)
    {
        // The gun stays raised when both the old and new standing
        // setup's poses aim straight up, including landing and its completion.
        return SamusPoseMetadata.ReadMovementType(targetPose) == SamusMovementType.Standing &&
            SamusPoseMetadata.ReadAim(sourcePose).AimsStraightUp &&
            SamusPoseMetadata.ReadAim(targetPose).AimsStraightUp
            ? SamusAnimationDelays.AnimationForPose(targetPose).AfterStart
            : SamusAnimationDelays.AnimationForPose(targetPose).Start;
    }

    /// <summary>Chooses the landing transition's prospective landing pose.</summary>
    private static SamusPoseId SelectAerialLandingPose(
        SamusState samus,
        bool wasSpinning,
        GameplayAction controllerInput)
    {
        bool facingLeft = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Left;
        if (wasSpinning)
        {
            return facingLeft
                ? SamusPoseId.SpinLandingLeftPose
                : SamusPoseId.SpinLandingRightPose;
        }

        // A pose that disables shooting lands through the ordinary facing pair before her
        // aim chooses a landing; hurt and damage-boost art disable shooting.
        // Horizontal aims take the landing's extra Shot-binding test. Held Shot selects the
        // firing landing; released Shot selects the ordinary normal landing pair. The six
        // admitted aimed directions select the aimed landings.
        SamusPoseAim aim = SamusPoseMetadata.ReadAim(samus);
        if (aim.DisablesShooting)
            return facingLeft ? SamusPoseId.NormalLandingLeftPose : SamusPoseId.NormalLandingRightPose;
        bool shotHeld = (controllerInput & GameplayAction.Shoot) != GameplayAction.None;
        // The straight-down aims are consumed by TryApplyCompactAerialLanding, because the 10 -> 21 expansion needs room
        // collision data.
        SamusLandingChoice landing = aim.Direction is { } direction &&
            SamusAimedPoseChoiceDefinitions.Landings.TryGetValue(direction, out SamusLandingChoice? choice)
            ? choice
            : throw new InvalidOperationException(
                $"Landing from aim {aim} requires the collision-aware compact landing operation.");
        return shotHeld ? landing.ShootHeld : landing.ShootReleased;
    }

    /// <summary>
    /// Settles Samus on a landing the changed-pose collision accepted: arms the autojump input, stops her horizontal and
    /// vertical motion, and requests the normal suit shade when she lands out of Screw Attack.
    /// </summary>
    private static void SettleAcceptedLanding(SamusState samus, bool leavingScrewAttack)
    {
        // The landing sets the same one-shot pose input a turn animation's transition installs.
        // Its short held-Jump window is evaluated in the next input phase, not during landing.
        if (!samus.Control.InputLocked) samus.InputSnapshot.AutoJumpInputPending = true;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;

        // The normal suit shade is reloaded whenever a pose change leaves spin or
        // wall-jump movement while Screw Attack is equipped. The suit shade shows
        // at the later shade phase, so the landing requests it here.
        if (leavingScrewAttack)
            samus.HorizontalSpeed.RequestSuitShadeRestore();
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
    }

    /// <summary>
    /// Applies the landing for her two straight-down aims when radius-ten straight-down Samus lands.
    /// Both select the ordinary normal landing; the expansion from radius ten to 21 still runs the full
    /// block pose-change collision resolver; settling the landing clears motion only if accepted.
    /// </summary>
    public static bool TryApplyCompactAerialLanding(
        SamusState samus,
        RoomLevelData level,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);
        if (!SamusPoseMetadata.IsCompactAerialPose(samus.Pose))
            throw new InvalidOperationException($"Compact landing requires a compact straight-down jumping or falling pose, not {samus.Pose}.");

        SamusPoseId sourcePose = samus.Pose;
        SamusProjectileDirection? aim = SamusPoseMetadata.ReadAimDirection(samus);
        SamusPoseId targetPose = aim == SamusProjectileDirection.DownFacingRight ? SamusPoseId.NormalLandingRightPose
            : aim == SamusProjectileDirection.DownFacingLeft ? SamusPoseId.NormalLandingLeftPose
            : throw new InvalidOperationException($"Compact pose {samus.Pose} has unexpected aim {SamusPoseMetadata.ReadAim(samus)}.");
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            level,
            targetPose,
            frameCounter,
            roomObjects,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            samus.Pose = targetPose;
            // The collision radius stays the compact body's: this frame's enemy and
            // projectile collision still use it, and the input phase's radius update takes the
            // landing body only next frame, as for an ordinary landing.
            samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
            SamusPresentation.InitializeAnimation(samus);
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
    /// Installs the pose the current pose's animation names at its pose-transition cue: every pose an
    /// animation's cue names.
    /// </summary>
    public static bool ApplyPendingAnimationTransition(SamusState samus)
    {
        if (samus.PendingTransitionalPose is not SamusPoseId targetPose)
            return false;

        // The game's pose initialization computes its own local delay without replacing
        // the extra animation delay this frame's liquid step published. The pose-transition cue adds that
        // delay once more after installing the new animation.
        int cueExtraAnimationDelay = samus.ExtraAnimationDelay;

        // Only a pose-transition cue of the current pose's own animation leaves a pose pending; anything else means the
        // request named a target that was not read from that animation.
        if (!SamusAnimationDelays.AnimationForPose(samus.Pose).Segments.Any(segment => segment.End.PublishedPoses.Contains(targetPose)))
        {
            throw new InvalidOperationException(
                $"Animation transition {samus.Pose} -> {targetPose} is no pose-transition cue of the pose's animation.");
        }

        bool startsMoonwalkJump = SamusPoseMetadata.IsMoonwalkTurnJumpPose(samus.Pose) &&
            targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose;
        SamusPoseId sourcePose = samus.Pose;
        SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(sourcePose);

        // The movement type is initialised after the pose-transition cue has
        // selected the new normal-jump pose but before it initializes that pose's frame.
        // A live stored shine replaces the target with the shinespark windup, so never briefly seed
        // neutral, aimed-up or diagonally-up normal-jump animation state for it.
        bool beganShinespark = SamusAerialPoseTransitions.TryBeginShinesparkWindup(samus,
            targetPose,
            previousMovementType);
        if (!beganShinespark)
        {
            // Moonwalk turn animations end in a transition to the spin jump. That pose still passes
            // through movement-type initialization, so equipment substitution occurs here
            // just as it does for an input-transition jump from ordinary running.
            SamusPoseId installedPose = startsMoonwalkJump
                ? SamusAerialPoseTransitions.SelectEquippedSpinPose(samus, targetPose)
                : targetPose;
            // Animation-owned pose changes still run the target movement setup.
            // In particular an aerial turn must stop decelerating in the old direction
            // when its final normal-jump/falling pose is installed, even if water made
            // the turn finish before base speed reached zero. Do not clear that speed.
            if (SamusPoseMetadata.ReadMovementType(installedPose) is SamusMovementType.NormalJumping or SamusMovementType.Falling)
                SamusAerialPoseTransitions.InitializeOrdinaryAerialAcceleration(samus);
            SamusGroundedPoseTransitions.ApplySimpleGroundedPoseChange(samus, sourcePose, installedPose, "Pose-transition cue",
                refreshRadius: !SamusPoseMetadata.IsAerialTurnPose(sourcePose));
        }
        if (SamusPoseMetadata.IsDrainedPose(sourcePose))
        {
            // The drained release animation has now reached its authored normal pose.
            // Clear only the drained-release marker; pose initialization above already owns the
            // same radius and animation writes as the game's pose setup.
            samus.Drained.CompleteRelease();
        }
        // The pose-transition cue does not run the jump setups. An input
        // transition out of the same turn art DOES initialize jump velocity; keeping
        // those two routes distinct preserves both ordinary jumping and Moonfall.
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.AnimationFrameTimer = samus.AnimationFrameTimer.AddFrames(cueExtraAnimationDelay);
        return true;
    }
}
