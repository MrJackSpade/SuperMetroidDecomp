using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The steps Samus's pose setups (<see cref="SamusPoseTransitionSetup"/>) and her responses to input that chooses no new
/// pose (<see cref="SamusNoTransitionSetup"/>) run (#2571): each reads her state, the pose it sets up and what the update
/// gives it, the controller's actions, the frame counter and the moonwalk setting, so the setups run through the pose
/// phase without naming it.
/// </summary>
internal sealed class SamusPoseSetupSteps
{
    /// <summary>Supplies the held and newly pressed controller buttons.</summary>
    private readonly GameplayActionInput _playerActions;
    /// <summary>Provides the frame counter the posture probes read.</summary>
    private readonly FrameClock _clock;
    /// <summary>Whether the player's options let her moonwalk.</summary>
    private readonly Func<bool> _moonwalkEnabled;

    /// <summary>Steps reading <paramref name="playerActions"/>, <paramref name="clock"/> and <paramref name="moonwalkEnabled"/>.</summary>
    /// <param name="playerActions">Supplies the held and newly pressed controller buttons.</param>
    /// <param name="clock">Provides the frame counter the posture probes read.</param>
    /// <param name="moonwalkEnabled">Whether the player's options let her moonwalk.</param>
    internal SamusPoseSetupSteps(GameplayActionInput playerActions, FrameClock clock, Func<bool> moonwalkEnabled)
    {
        _playerActions = Ensure.NotNull(playerActions);
        _clock = Ensure.NotNull(clock);
        _moonwalkEnabled = Ensure.NotNull(moonwalkEnabled);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.StartJump"/> setup.</summary>
    internal void SetUpStartJump(SamusState samus, SamusPoseId targetPose)
    {
        // Every standing turn on the ground, the aimed turns and the moonwalk turns among them, continues taking
        // the standing pose's input transitions, which can interrupt the art with either same-facing spin or
        // neutral jump; the landings expose the same standing input transitions while their brief transitions
        // are active. Preserve newly pressed Shoot as well: the normal-jump setup owns the simultaneous
        // Jump+Fire bridge when the selected body is SamusMovementType.NormalJumping.
        SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(samus, targetPose, _playerActions.NewlyPressed);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.JumpFromCrouch"/> setup.</summary>
    internal void SetUpJumpFromCrouch(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        // The crouched aim poses use the ordinary neutral-jump transition
        // art for every crouched aim direction, keeping the crouching-turn poses' ten-pixel adjustment.
        SamusPosturePoseTransitions.ApplyCrouchJumpTransition(samus, terrain.Level, targetPose, _clock.Counter,
            terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.StartDamageBoost"/> setup.</summary>
    internal static void SetUpStartDamageBoost(SamusControlFrame frame, SamusState samus, SamusPoseId targetPose)
    {
        // The accepted pose restores normal movement while
        // preserving the hurt arc and remaining timer.
        SamusKnockbackMovement.ApplyDamageBoostTransition(samus, frame.PoseAtFrameStart, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.JumpFromDamageBoost"/> setup.</summary>
    internal void SetUpJumpFromDamageBoost(SamusState samus, SamusPoseId targetPose)
    {
        SamusKnockbackMovement.ApplyDamageBoostPoseTransition(samus, targetPose, _playerActions.NewlyPressed);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.StartMoonwalkTurnJump"/> setup.</summary>
    internal static void SetUpStartMoonwalkTurnJump(SamusState samus, SamusPoseId targetPose)
    {
        // The stable moonwalk poses' input transitions select a generic
        // reversal target. The reversal check validates it against the old
        // shot direction, folds momentum, and leaves the moonwalk turn-jump poses
        // grounded until either its input transitions or the turn transition starts the jump.
        SamusGroundedPoseTransitions.ApplyMoonwalkTurnJump(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.ChangeMoonwalkPose"/> setup.</summary>
    internal void SetUpChangeMoonwalkPose(SamusState samus, SamusPoseId targetPose)
    {
        SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(samus, targetPose, _moonwalkEnabled());
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.StartRunning"/> setup.</summary>
    internal static void SetUpStartRunning(SamusState samus, SamusPoseId targetPose)
    {
        SamusGroundedPoseTransitions.StartRunning(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.ChangeGroundedAim"/> setup.</summary>
    internal static void SetUpChangeGroundedAim(SamusState samus, SamusPoseId targetPose)
    {
        SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.TurnOnGround"/> setup.</summary>
    internal void SetUpTurnOnGround(SamusState samus, SamusPoseId targetPose)
    {
        // Ceres releases controls while the forward-facing pose is still active. Its own input transitions map
        // Left/Right to generic turning poses; the reversal check recognizes the old front-facing pose, which
        // faces neither way, and keeps that target.
        SamusGroundedPoseTransitions.ApplyGroundedTurn(samus, targetPose, _playerActions.Current);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.CrouchOrStand"/> setup.</summary>
    internal void SetUpCrouchOrStand(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        SamusPosturePoseTransitions.ApplyPostureTransition(samus, terrain.Level, targetPose, _clock.Counter,
            terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.StandFromCrouch"/> setup.</summary>
    internal void SetUpStandFromCrouch(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        // These direct facing-normal poses bypass the animated
        // aimed standing-transition family, but still run pose-expansion
        // collision before installing the final standing body.
        SamusPosturePoseTransitions.ApplyDirectCrouchToStandingTransition(samus, terrain.Level, targetPose,
            _clock.Counter, terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.MorphOrUnmorph"/> setup.</summary>
    internal void SetUpMorphOrUnmorph(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        SamusPosturePoseTransitions.ApplyMorphTransition(samus, terrain.Level, targetPose, _clock.Counter,
            terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.ChangeBallPose"/> setup.</summary>
    internal static void SetUpChangeBallPose(SamusState samus, SamusPoseId targetPose)
    {
        // These six poses all share one animation.
        // The setup preserves frame/timer and applies turning
        // reversal momentum only when direction actually changes.
        SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.JumpSpringBall"/> setup.</summary>
    internal static void SetUpJumpSpringBall(SamusState samus, SamusPoseId targetPose)
    {
        SamusPosturePoseTransitions.ApplySpringBallJump(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.FallFromUnmorph"/> setup.</summary>
    internal void SetUpFallFromUnmorph(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        // Route an animation/collision-selected falling body through
        // ordinary expansion collision while preserving the fall.
        // Posture transitions read no input transitions:
        // the posture-transition movement type's input lookup returns at once.
        SamusPosturePoseTransitions.ApplyUnmorphToFallingTransition(samus, terrain.Level, targetPose,
            _clock.Counter, terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.TurnInAir"/> setup.</summary>
    internal void SetUpTurnInAir(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        // The input transitions select the generic jumping and falling turns. The turn
        // reads the source pose's shot direction, chooses the exact
        // aimed aerial turn art when needed, folds momentum, and runs compact
        // pose-expansion collision before committing the turn.
        SamusAerialPoseTransitions.ApplyAerialTurn(samus, terrain.Level, targetPose, _clock.Counter,
            terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.ChangeSpinDirection"/> setup.</summary>
    internal static void SetUpChangeSpinDirection(SamusState samus, SamusPoseId targetPose)
    {
        SamusAerialPoseTransitions.ApplySpinJumpDirectionTransition(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.AimFromSpin"/> setup.</summary>
    internal void SetUpAimFromSpin(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        // Aim and Fire cancel both compact spin and wall-
        // jump bodies through the same normal-jump setup without
        // restarting the jump arc. The wall-jump Shot transition, for example, maps Shot
        // directly from the right wall jump to the right gun-extended normal jump. The jump's start performs
        // changed-pose collision for every radius expansion; a rejected
        // body leaves the compact source pose installed.
        SamusAerialPoseTransitions.ApplySpinOrWallJumpToNormalJumpTransition(samus, terrain.Level, targetPose,
            _clock.Counter, _playerActions.NewlyPressed, terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.ChangeAerialAim"/> setup.</summary>
    internal static void SetUpChangeAerialAim(SamusState samus, SamusPoseId targetPose)
    {
        SamusAerialPoseTransitions.ApplySameFacingAerialTransition(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.ChangeCompactAerialPose"/> setup.</summary>
    internal void SetUpChangeCompactAerialPose(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        SamusAerialPoseTransitions.ApplyCompactAerialTransition(samus, terrain.Level, targetPose, _clock.Counter,
            terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.AimShinespark"/> setup.</summary>
    internal static void SetUpAimShinespark(SamusState samus, SamusPoseId targetPose)
    {
        // These are ordinary held/new input transitions, but
        // their transition hands her movement to a special owner instead of
        // invoking a normal movement-type setup.
        SamusAerialPoseTransitions.ApplyShinesparkDirectionTransition(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.ChangeDraygonGrabbedPose"/> setup.</summary>
    internal static void SetUpChangeDraygonGrabbedPose(SamusState samus, SamusPoseId targetPose)
    {
        // Both sets of input transitions stay within one facing; the change keeps it and starts the target's
        // own animation.
        SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, targetPose);
    }

    /// <summary>Runs the <see cref="SamusPoseTransitionSetup.StandUpFromDraygonHold"/> setup.</summary>
    internal void SetUpStandUpFromDraygonHold(SamusState samus, SamusPoseId targetPose, SamusTerrain terrain)
    {
        SamusPosturePoseTransitions.ApplyDraygonHoldStandUp(samus, terrain.Level, targetPose, _clock.Counter,
            terrain.RoomObjects);
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.StopKnockback"/> setup.</summary>
    internal static void RespondStopKnockback(SamusState samus, SamusPoseId source, SamusPoseId? fallback)
    {
        // The retained hurt pose still stops Samus's momentum.
        // Reset deceleration and cancel the momentum flag after movement.
        // Extra speed survives the stop, but the next normal mover
        // consumes the cancellation through HandleExtraRunSpeed.
        if (fallback == source)
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.ReleaseDamageBoost"/> setup.</summary>
    internal void RespondReleaseDamageBoost(SamusState samus, SamusPoseId source, SamusPoseId? fallback)
    {
        // Releasing every boost chord reaches the neutral jump poses:
        // the same setup as its held-Jump input transition.
        if (fallback is { } boostFallback && boostFallback != source)
            SamusKnockbackMovement.ApplyDamageBoostPoseTransition(samus, boostFallback, _playerActions.NewlyPressed);
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.ReturnToNeutralDraygonHold"/> setup.</summary>
    internal static void RespondReturnToNeutralDraygonHold(SamusState samus, SamusPoseId? fallback)
    {
        // Zero input uses the pose's fallback pose, returning aimed/firing/
        // struggling art to the neutral Draygon-grabbed poses without touching owner placement.
        if (fallback is { } draygonFallback)
            SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, draygonFallback);
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.SpinFromWallJump"/> setup.</summary>
    internal static void RespondSpinFromWallJump(SamusState samus, SamusPoseId? fallback)
    {
        // The pose definition leaves the launch animation for ordinary spin
        // art. The wall-jumping movement type kills Samus's X speed. The pose change
        // reports a replacement only when the pose's initialization itself replaces the
        // installed pose; then the pose update skips the kill. So
        // a spin jump keeps its launch speed only when initialization promotes it
        // to Space Jump or Screw Attack; otherwise the kill clears base/extra speed.
        if (fallback is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose)
        {
            SamusPoseId spinFallback = fallback.Value;
            SamusAerialPoseTransitions.ApplySpinJumpDirectionTransition(samus, spinFallback);
            if (samus.Pose == spinFallback)
                samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.DecelerateFallingBall"/> setup.</summary>
    internal static void RespondDecelerateFallingBall(SamusControlFrame frame, SamusState samus, SamusPoseId source, SamusPoseId? fallback)
    {
        if (fallback == source)
        {
            samus.HorizontalSpeed.DecelerateForNoTransition(
                frame.DeceleratingFallbackHasMomentum, SamusPoseMetadata.ReadFacingDirection(samus));
        }
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.StopBall"/> setup.</summary>
    internal static void RespondStopBall(SamusState samus, SamusPoseId source, SamusPoseId? fallback)
    {
        // A still ball and Spring Ball in the air or falling kill Samus's X speed: after this frame's movement,
        // the kill clears mode and base speed and goes on to cancel boost and extra running speed.
        if (fallback == source)
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.StopRollingBall"/> setup.</summary>
    internal static void RespondStopRollingBall(SamusState samus, SamusPoseId? fallback)
    {
        // Killing X speed clears base and extra momentum after the
        // current movement frame for both grounded ball movement types.
        if (fallback is { } stillBall)
        {
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
            SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, stillBall);
        }
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.StopRunning"/> setup.</summary>
    internal static void RespondStopRunning(SamusState samus, SamusPoseId source, SamusPoseId? fallback)
    {
        if (fallback == source)
            DecelerateRetainedRun(samus);
        else if (fallback is not null)
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            SamusGroundedPoseTransitions.ApplyRunningToStanding(samus);
        }
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.StopAimedRunning"/> setup.</summary>
    internal static void RespondStopAimedRunning(SamusState samus, SamusPoseId source, SamusPoseId? fallback)
    {
        if (fallback == source)
            DecelerateRetainedRun(samus);
        else if (fallback is { } standing)
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, standing);
        }
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.StopMoonwalk"/> setup.</summary>
    internal void RespondStopMoonwalk(SamusState samus, SamusPoseId? fallback)
    {
        // The stop selected the literal standing/aim fallback during the input phase.
        // Applying it after movement preserves the one-last-step timing.
        if (fallback is { } moonwalkFallback)
            SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(samus, moonwalkFallback, _moonwalkEnabled());
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.LowerGroundedAim"/> setup.</summary>
    internal static void RespondLowerGroundedAim(SamusState samus, SamusPoseId? fallback)
    {
        // Only the input fallback path above populates this value; the aimed wall stops name the plain wall stops,
        // which share their radius, animation and cleanup. Validate and apply it through the same
        // radius/animation seam as held-input aim changes; never assign the authored value directly to Pose.
        if (fallback is { } aimFallback)
            SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, aimFallback);
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.AbandonAimedCrouchingTurn"/> setup.</summary>
    internal static void RespondAbandonAimedCrouchingTurn(SamusControlFrame frame, SamusState samus, SamusPoseId source, SamusPoseId? fallback)
    {
        if (frame.UsePoseDefinitionFallback && samus.Pose == source && fallback is { } crouchingTurnFallback)
            SamusGroundedPoseTransitions.TurnAimedCrouchingForNoTransition(samus, crouchingTurnFallback);
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.StopTurn"/> setup.</summary>
    internal static void RespondStopTurn(SamusControlFrame frame, SamusState samus, SamusPoseId source)
    {
        // A definition without a fallback pose retains the animation, not the momentum
        // response. The stop still cancels reverse acceleration,
        // for ground turns exactly as for aerial ones.
        if (frame.UsePoseDefinitionFallback && samus.Pose == source)
            SamusGroundedPoseTransitions.TurnForNoTransition(samus, frame.MovementTypeAtFrameStart);
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.ContinueNormalJump"/> setup.</summary>
    internal static void RespondContinueNormalJump(SamusControlFrame frame, SamusState samus, SamusPoseId? fallback)
    {
        // An unchanged prospective pose must not reset animation or invoke
        // compact-radius expansion. Decelerating still folds momentum after
        // movement and rechecks the remaining base speed there.
        if (fallback is { } jumpFallback)
        {
            if (jumpFallback != samus.Pose)
                SamusAerialPoseTransitions.ApplySameFacingAerialTransition(samus, jumpFallback);
            samus.HorizontalSpeed.DecelerateForNoTransition(
                frame.DeceleratingFallbackHasMomentum, SamusPoseMetadata.ReadFacingDirection(samus));
        }
    }

    /// <summary>Runs the <see cref="SamusNoTransitionSetup.LowerAerialAim"/> setup.</summary>
    internal static void RespondLowerAerialAim(SamusState samus, SamusPoseId? fallback)
    {
        // Install the fallback's pose metadata and setup first.
        // Falling's run-speed kill, after this response, then clears only the extra component;
        // it must not be folded into base speed like running deceleration.
        // A retained compact pose selects history/momentum work only;
        // pose initialization and animation restart require a change.
        if (fallback is { } aerialFallback && aerialFallback != samus.Pose)
            SamusAerialPoseTransitions.ApplySameFacingAerialTransition(samus, aerialFallback);
    }

    /// <summary>
    /// Retaining the running fallback selected deceleration in the input phase. After movement, fold extra dash speed into
    /// base and cancel boost before decelerating; changing only the mode leaves stale dash speed alive and changes the
    /// subsequent stopping trajectory.
    /// </summary>
    private static void DecelerateRetainedRun(SamusState samus) =>
        samus.HorizontalSpeed.DecelerateForNoTransition(selectedDeceleration: true, SamusPoseMetadata.ReadFacingDirection(samus));
}
