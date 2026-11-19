using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's humanoid and morphed knockback in air, water and lava, plus the
/// humanoid-only damage-boost escape route.
/// </summary>
/// <remarks>
/// This code follows the game's knockback check, knockback movement, jump launch and knockback setup.
/// Enemy damage calculation is deliberately not invented here: the hit supplies only the
/// collision result that enemy collision would have published, namely whether the source lies to
/// Samus's left or right. Every pose, speed, timer value, and subsequent input
/// transition is then taken from the game's definitions.
/// </remarks>
public static class SamusKnockbackMovement
{
    /// <summary>
    /// Consumes the damage request that terrain collision publishes before the
    /// knockback check runs in Samus's ordinary input phase.
    /// </summary>
    /// <remarks>
    /// Block collision deliberately does not install a hurt pose itself. It writes the shared
    /// knockback timer and horizontal direction; the later hit interruption admits
    /// or suppresses that request according to the current movement type. Keeping this as
    /// a shared handoff prevents spike blocks and spike-air from inventing bespoke motion.
    /// </remarks>
    internal static bool TryStartPendingHitInterruption(
        SamusState samus,
        GameplayAction controllerInput,
        bool timeIsFrozen,
        KnockbackPoseCollision? poseCollision)
    {
        Ensure.NotNull(samus);

        // A knockback starts only with a live producer timer, unfrozen time, and no
        // knockback direction installed. An installed direction means a previous request already
        // holds her movement; it must finish instead of being restarted.
        if (samus.KnockbackHold.IsOver ||
            timeIsFrozen ||
            samus.KnockbackDirection != SamusKnockbackDirection.None ||
            samus.KnockbackActive)
        {
            return false;
        }

        return Start(samus, controllerInput, KnockbackLaunch.FromPendingHit(samus), poseCollision);
    }

    /// <summary>
    /// Consumes the pending knockback start after the knockback check has admitted either the
    /// normal knockback-pose branch or the pose-preserving Morph/Spring Ball branch.
    /// </summary>
    /// <param name="samus">Mutable player owner receiving the hurt pose, timer, the knockback's hold on her movement, and cancellation of incompatible movement state.</param>
    /// <param name="controllerInput">Held actions used for the humanoid forward-held direction test; the Morph/Spring Ball branch ignores this input when selecting direction.</param>
    /// <param name="launch">How a knockback starts: its horizontal direction and timer (#633).</param>
    /// <param name="poseCollision">
    /// The room the hurt pose grows into, or null where the scene places Samus itself and no expansion collision runs:
    /// the opening cinematic's flashback and a grapple-beam hurt.
    /// </param>
    /// <returns>
    /// True when her movement lets the knockback take it. False for
    /// interrupt-suppression entries such as turning, grapple, and shinespark.
    /// </returns>
    internal static bool Start(SamusState samus, GameplayAction controllerInput, KnockbackLaunch launch, KnockbackPoseCollision? poseCollision)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(launch);
        if (!Enum.IsDefined(launch.KnockbackXDirection))
            throw new ArgumentOutOfRangeException(nameof(launch.KnockbackXDirection));
        if (launch.KnockbackTimer == 0)
            throw new ArgumentOutOfRangeException(nameof(launch.KnockbackTimer));
        if (samus.KnockbackActive || samus.KnockbackDirection != SamusKnockbackDirection.None)
            throw new InvalidOperationException("Normal knockback is already active or pending.");

        // Every movement type is morphed, upright, the glitch ball or one that refuses knockback
        // (SamusMovementTypeDefinitions). Any other cannot name another game behaviour; it means Samus carries
        // corrupt pose metadata rather than a movement family of its own.
        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        SamusKnockbackResponse response =
            SamusMovementTypeDefinitions.Movements.TryGetValue(sourceMovementType, out SamusMovementTypeDefinition? definition)
                ? definition.Knockback
                : throw new InvalidDataException($"Movement type {sourceMovementType} has no knockback response.");
        bool morphed = response == SamusKnockbackResponse.Ball;
        bool humanoid = response == SamusKnockbackResponse.Upright;

        // Enemy contact has already established the knockback timer before the knockback check selects a pose.
        // Turning, grapple, current knockback, damage boost, and special-movement bodies all
        // refuse knockback: they retain the timer/flicker but do not give the knockback her movement.
        samus.KnockbackHold = UpdateCountdown.For(launch.KnockbackTimer);
        if (response == SamusKnockbackResponse.Refused)
            return false;

        // A hit replaces an ordinary body with a knockback pose. A ball, by contrast, keeps its exact Morph or Spring Ball
        // pose; since Samus's pose update sees no pose change, the rolling animation frame and timer survive.
        bool facingLeft = SamusPoseMetadata.IsFacingLeft(samus.Pose);
        if (humanoid)
        {
            SamusPoseId targetPose = facingLeft
                ? SamusPoseId.KnockbackLeftPose
                : SamusPoseId.KnockbackRightPose;
            // Interrupted poses pass through the same expansion check as ordinary
            // input poses before the knockback start initializes knockback. Scripted cinematic
            // scenes without room terrain retain their explicitly positioned behavior.
            if (poseCollision is { } collision && !SamusGroundedPoseTransitions.TryResolveKnockbackPoseCollision(samus,
                    collision.Terrain.Level, targetPose, collision.FrameCounter, collision.Terrain.RoomObjects))
                return false;
            samus.Pose = targetPose;
            // The interrupted pose commits after movement. The input phase publishes its radius
            // next frame; keep the collision resolver's old live radius through this one.
        }
        else if (response == SamusKnockbackResponse.GlitchBall)
        {
            // A hit on the glitch ball, unlike the ball path, installs the glitch ball's knockback pose, then continues
            // through the same knockback-start hurt setup as a humanoid pose.
            samus.Pose = facingLeft
                ? SamusPoseId.GlitchBallKnockbackLeftPose
                : SamusPoseId.GlitchBallKnockbackRightPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            humanoid = true;
        }

        // A ball's knockback ignores both the damage's side and the controller: a ball facing right always goes
        // SamusKnockbackDirection.UpRight, and one facing left UpLeft. An upright Samus keeps the unusual choice of
        // down-left or down-right with forward held.
        samus.KnockbackXDirection = launch.KnockbackXDirection;
        if (morphed)
        {
            samus.KnockbackDirection = facingLeft ? SamusKnockbackDirection.UpLeft : SamusKnockbackDirection.UpRight;
        }
        else
        {
            bool forwardHeld = facingLeft
                ? (controllerInput & GameplayAction.Left) != GameplayAction.None
                : (controllerInput & GameplayAction.Right) != GameplayAction.None;
            samus.KnockbackDirection = launch.KnockbackXDirection == KnockbackHorizontalDirection.Left
                ? forwardHeld ? SamusKnockbackDirection.DownLeft : SamusKnockbackDirection.UpLeft
                : forwardHeld ? SamusKnockbackDirection.DownRight : SamusKnockbackDirection.UpRight;
        }

        // The hit's producer owns its hurt countdown, taken above: an enemy's touch supplies the ordinary
        // one and the intro Rinka's strike a longer one, which this shared start keeps.
        samus.KnockbackActive = true;
        // The knockback takes her movement, ending her flight after letting go of a
        // grapple swing if that was still running.
        samus.Grapple.ReleasedMovementActive = false;

        // The remainder of the knockback setup runs for both the humanoid and the ball. A pending bomb
        // jump cannot coexist with hurt movement, and shinespark/Screw contact damage is
        // cancelled before the knockback moves her.
        samus.BombJumpRequest = SamusBombJumpRequest.None;
        // Taking her movement ends the bomb jump's movement immediately, but
        // does not yet release its independent lock on her pose input.
        samus.BombJumpStarting = false;
        samus.BombJumpActive = false;
        samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.None;

        // The hurt flash starts after bomb-jump/contact damage is cancelled. This is an independent
        // shade/audio lifetime (SharedHurtFlashCount), not the hurt countdown the hit's producer supplied above.
        samus.HurtFlash.Start();

        // Hurt launch retains the bottom-edge and Gravity-Suit selection.
        samus.Kinematics.YSpeed =
            SamusVerticalMotion.Knockback(samus.LiquidPhysics.DetermineMovementMedium(samus));
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        SamusAerialMovement.ConfigureEnvironmentGravity(samus);
        if (humanoid)
            SamusPresentation.InitializeAnimation(samus);
        return true;
    }

    /// <summary>Runs one frame of the knockback's movement.</summary>
    internal static KnockbackMovementResult Step(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!samus.KnockbackActive)
            throw new InvalidOperationException("Knockback movement requires the knockback to hold Samus's movement.");
        if (samus.KnockbackDirection is not (SamusKnockbackDirection.UpLeft or SamusKnockbackDirection.UpRight or SamusKnockbackDirection.DownLeft or SamusKnockbackDirection.DownRight))
            throw new InvalidOperationException($"Invalid knockback direction {samus.KnockbackDirection}.");

        // The knockback keeps her movement even on the zero-timer frame.
        // Expiry belongs to the post-animation hit interruption, not this mover.

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));
        // The knockback keeps the speeds the pose's movement type selects.
        // Morphed bodies retain their own movement type throughout hurt movement.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(SamusPoseMetadata.ReadMovementType(samus));
        var requestedX = SamusHorizontalDisplacement.Toward(samus.KnockbackXDirection == KnockbackHorizontalDirection.Left, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedX.Displacement, CollisionMovementDirection = requestedX.CollisionDirection });
        // Knockback moves Samus as her ordinary sideways movement does: hitting a wall ends her horizontal momentum
        // before the vertical pass, so the next frame does not carry her on from the wall.
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = samus.KnockbackDirection is SamusKnockbackDirection.UpLeft or SamusKnockbackDirection.UpRight
            ? MoveWithSharedVerticalSpeedCalculation(level, samus, frameCounter, roomObjects)
            : MoveDownWithoutSpeedCalculation(level, samus, frameCounter, roomObjects);

        // A downward knockback's landing is read here, before the landing clears these values. The result keeps the
        // impact so the runtime can run the shared landing presentation at the same point of the update.
        bool landed = samus.KnockbackDirection is SamusKnockbackDirection.DownLeft or SamusKnockbackDirection.DownRight && vertical.Collided;

        if (vertical.Collided)
        {
            // Only a vertical collision stops the knockback, so horizontal wall
            // contact alone does not clear these vertical values. Bottom alignment is already
            // represented by the block collision's accepted displacement against the current radius.
            speed.AccelerationMode = SamusAccelerationMode.Accelerating;
            speed.BaseSpeed = default;
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.None;
        }

        return new KnockbackMovementResult(
            Landed: landed);
    }

    /// <summary>
    /// Applies the only cross-family transition in the knockback input transitions: Jump plus the
    /// direction opposite the knockback pose enters a damage boost and restores normal movement.
    /// </summary>
    public static void ApplyDamageBoostTransition(
        SamusState samus,
        SamusPoseId sourcePose,
        SamusPoseId targetPose)
    {
        Ensure.NotNull(samus);
        // The input phase selects a prospective pose; it does not mutate the live movement type.
        // The owning transition step must resolve higher-priority expiry first.
        bool valid = new SamusPoseChange(sourcePose, targetPose) is
            (SamusPoseId.KnockbackRightPose, SamusPoseId.DamageBoostRightPose) or
            (SamusPoseId.KnockbackLeftPose, SamusPoseId.DamageBoostLeftPose);
        if (!valid)
        {
            throw new InvalidOperationException(
                $"Damage boost {sourcePose} -> {targetPose} is not a transition.");
        }

        // The damage boost returns only her movement to her pose. The conditional jump
        // reset in the pose handling does not run for a merely prospective type change.
        // Preserve velocity and hurt values until the common expiry handoff consumes them.
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.KnockbackActive = false;
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>Installs the normal jump of the same facing that a damage boost's input selects.</summary>
    /// <param name="samus">The damage-boosting Samus.</param>
    /// <param name="targetPose">The neutral or forward normal jump the damage boost leaves by.</param>
    /// <param name="controllerNewInput">This frame's newly pressed actions, which the normal jump reads.</param>
    public static void ApplyDamageBoostPoseTransition(
        SamusState samus,
        SamusPoseId targetPose,
        GameplayAction controllerNewInput)
    {
        Ensure.NotNull(samus);
        bool valid = new SamusPoseChange(samus.Pose, targetPose) is
            (SamusPoseId.DamageBoostLeftPose,
                SamusPoseId.NeutralJumpLeftPose or SamusPoseId.NormalJumpForwardLeftPose) or
            (SamusPoseId.DamageBoostRightPose,
                SamusPoseId.NeutralJumpRightPose or SamusPoseId.NormalJumpForwardRightPose);
        if (!valid)
        {
            throw new InvalidOperationException(
                $"Damage-boost exit {samus.Pose} -> {targetPose} is not one of the damage boost's input transitions.");
        }

        // Both target families have the same radius. The pose change nevertheless runs
        // their ordinary setup: it preserves the live jump velocity,
        // reselects the acceleration mode from extra run speed, and can start a spark from
        // stored shine. The selected target's frame delays start at frame zero.
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (!SamusAerialPoseTransitions.InitializeInstalledNormalJumpPose(samus, SamusMovementType.DamageBoost, controllerNewInput))
            SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Finishes the hit once the knockback check finds its timer run out
    /// after movement/animation, including boosts which already restored normal movement.
    /// Returns whether this higher-priority transition superseded ordinary pose input.
    /// </summary>
    public static bool TryFinishExpiredHitInterruption(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!IsExpiredHitInterruptionPending(samus))
            return false;

        FinishExpiredHitInterruption(samus);
        return true;
    }

    /// <summary>
    /// Samples the zero-timer arm of the hit interruption.
    /// The result must be retained until Samus's pose handling applies transitions: an animation's
    /// pose-transition cue can clear the knockback direction after this decision without cancelling it.
    /// </summary>
    public static bool IsExpiredHitInterruptionPending(SamusState samus)
    {
        Ensure.NotNull(samus);
        return samus.KnockbackHold.IsOver && samus.KnockbackDirection != SamusKnockbackDirection.None;
    }

    /// <summary>
    /// Finishes a previously sampled hurt expiry, including when a higher-priority
    /// pose-transition cue has since taken her movement from the knockback.
    /// </summary>
    public static void FinishExpiredHitInterruption(SamusState samus)
    {
        Ensure.NotNull(samus);

        // Finishing the hit always aligns Samus's feet to the new radius, even when a pose-transition cue
        // first installed an animation-owned pose. That overlap is observable: a turn from radius 19 to
        // radius 10 moves Samus down nine pixels while preserving her feet. Reading the
        // previous pose instead of assuming a hurt pose also covers the no-animation
        // path, where both radii ordinarily match and the alignment moves her nowhere.
        int previousRadius = SamusPoseMetadata.ReadPoseYRadius(samus.PoseHistory.PreviousPose);
        int targetRadius = SamusPoseMetadata.ReadPoseYRadius(samus.Pose);
        samus.AlignBottomAfterPoseChange(previousRadius, targetRadius);

        // Samus's pose update finishes the hit straight away without installing its own
        // proposed falling pose. Any pose a higher-priority pose-transition cue installed is already live,
        // and the kept pose is neither reloaded nor restarted, so an interrupted roll stays continuous.
        FinishKnockback(samus);
    }

    /// <summary>Restores ordinary input and movement ownership and clears hurt motion values after expiry.</summary>
    private static void FinishKnockback(SamusState samus)
    {
        // Knockback's expiry restores the normal input mode as well as
        // movement, including when hurt interrupted a still-input-locked bomb rise.
        samus.BombJumpPoseInputLocked = false;
        // The expiry returns her movement to her pose. A Shinespark windup can
        // have been initialized after the knockback check latched this expiry, so end that
        // spark's hold while retaining its independently owned shade and timer.
        SamusMovementHandOver.BackToPose(samus, SamusMovementOwner.Knockback);
        // The exact knockback-end cleanup shared by humanoid and morphed completion. The falling
        // flag has no field of its own; moving down is its movement-visible
        // publication and is consumed by every normal/ball step.
        samus.KnockbackDirection = SamusKnockbackDirection.None;
        samus.KnockbackActive = false;
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
    }

    /// <summary>Uses ordinary aerial vertical speed calculations during rising knockback, including the direction reversal.</summary>
    private static BlockMoveResult MoveWithSharedVerticalSpeedCalculation(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        // Knockback runs the same vertical step as ordinary aerial movement, its reversal from rising to falling
        // included. The Rinka's hurt timer expires while Samus is still rising, and ordinary falling takes over after it.
        return SamusAerialMovement.StepVerticalWithSpeedCalculations(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, frameCounter, out _, out _);
    }

    /// <summary>Applies externally supplied downward displacement through the shared vertical collision path without accelerating it.</summary>
    private static BlockMoveResult MoveDownWithoutSpeedCalculation(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        // Any external Y displacement takes priority over the normal total-X-plus-one
        // slope-following probe, as in every no-speed vertical move; a downward knockback takes the exact same step.
        SignedSixteenSixteen requested = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);
        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = requested, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
    }
}
