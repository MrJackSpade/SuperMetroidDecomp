using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's humanoid and morphed knockback in air, water and lava, plus the
/// humanoid-only damage-boost escape route.
/// </summary>
/// <remarks>
/// This code follows the game's knockback check, knockback movement, jump launch and knockback initializer.
/// Enemy damage calculation is deliberately not invented here: a caller supplies only the
/// collision result that enemy collision would have published, namely whether the source lies to
/// Samus's left or right. Every pose, speed-table entry, timer value, and subsequent input
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
        ControllerButton controllerInput,
        bool timeIsFrozen,
        RoomLevelData? level = null,
        int frameCounter = 0,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(samus);

        // Requires a live producer timer, unfrozen time, and zero in the
        // installed direction. A nonzero direction means a previous request already
        // holds her movement; it must finish instead of being restarted.
        if (samus.KnockbackTimer == 0 ||
            timeIsFrozen ||
            samus.KnockbackDirection != SamusKnockbackDirection.None ||
            samus.KnockbackActive)
        {
            return false;
        }

        return Start(samus, controllerInput, KnockbackLaunch.FromPendingHit(samus), level, frameCounter, roomObjects);
    }

    /// <summary>
    /// Consumes the pending knockback start after the knockback check has admitted either the
    /// normal knockback-pose branch or the pose-preserving Morph/Spring Ball branch.
    /// </summary>
    /// <param name="samus">Mutable player owner receiving the hurt pose, timer, the knockback's hold on her movement, and cancellation of incompatible movement state.</param>
    /// <param name="controllerInput">Held buttons used for the humanoid forward-held direction test; the Morph/Spring Ball branch ignores this input when selecting direction.</param>
    /// <param name="launch">How a knockback starts: its horizontal direction and timer (#633).</param>
    /// <param name="level">Optional live room geometry for hurt-pose expansion collision; null leaves that collision pass to explicitly positioned cinematic or diagnostic callers.</param>
    /// <param name="frameCounter">The frame counter passed to pose-expansion block reactions when room geometry is supplied.</param>
    /// <param name="roomObjects">Optional live room-object owner receiving those collision reactions.</param>
    /// <returns>
    /// True when the movement-type table gives the knockback her movement. False for
    /// interrupt-suppression entries such as turning, grapple, and shinespark.
    /// </returns>
    internal static bool Start(SamusState samus, ControllerButton controllerInput, KnockbackLaunch launch, RoomLevelData? level = null, int frameCounter = 0, ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(samus);
        Ensure.NotNull(launch);
        if (!Enum.IsDefined(launch.KnockbackXDirection))
            throw new ArgumentOutOfRangeException(nameof(launch.KnockbackXDirection));
        if (launch.KnockbackTimer == 0)
            throw new ArgumentOutOfRangeException(nameof(launch.KnockbackTimer));
        if (samus.KnockbackActive || samus.KnockbackDirection != SamusKnockbackDirection.None)
            throw new InvalidOperationException("Normal knockback is already active or pending.");

        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        bool morphed = sourceMovementType is
            SamusMovementType.MorphBallGround or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.GlitchBallKnockback or
            SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling;
        bool humanoid = sourceMovementType is
            SamusMovementType.Standing or
            SamusMovementType.Running or
            SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or
            SamusMovementType.Crouching or
            SamusMovementType.Falling or
            SamusMovementType.CompactAirborne or
            SamusMovementType.Moonwalking or
            SamusMovementType.WallJumping or
            SamusMovementType.RanIntoWall;
        bool glitchBall = sourceMovementType == SamusMovementType.GlitchBall;
        bool suppressesKnockback = sourceMovementType is
            SamusMovementType.Knockback or
            SamusMovementType.TurningOnGround or
            SamusMovementType.PostureTransition or
            SamusMovementType.Grappling or
            SamusMovementType.TurningWhileJumping or
            SamusMovementType.TurningWhileFalling or
            SamusMovementType.DamageBoost or
            SamusMovementType.DraygonHeld or
            SamusMovementType.Special;
        if (!morphed && !humanoid && !glitchBall && !suppressesKnockback)
        {
            // The knockback check contains exactly 28 entries, indexed by the movement type.
            // A larger value cannot name another game behavior; it means the caller supplied
            // corrupt pose metadata rather than an unmodelled movement family.
            throw new InvalidDataException(
                $"Movement type {sourceMovementType} lies outside the 28-entry knockback table.");
        }

        // Enemy contact has already established the knockback timer before the knockback check selects a pose.
        // Turning, grapple, current knockback, damage boost, and special-movement bodies all
        // refuse knockback: they retain the timer/flicker but do not give the knockback her movement.
        samus.KnockbackTimer = launch.KnockbackTimer;
        if (suppressesKnockback)
            return false;

        // Replaces an ordinary body with a knockback pose. The ball branch, by contrast,
        // republishes the exact Morph/Spring Ball pose. Because Samus's pose update sees no
        // pose change in that branch, the rolling animation frame and timer survive.
        bool facingLeft = SamusPoseMetadata.IsFacingLeft(samus.Pose);
        if (humanoid)
        {
            SamusPoseId targetPose = facingLeft
                ? SamusPoseId.KnockbackLeftPose
                : SamusPoseId.KnockbackRightPose;
            // Interrupted poses pass through the same expansion check as ordinary
            // input poses before the knockback start initializes knockback. Scripted cinematic
            // callers without room terrain retain their explicitly positioned behavior.
            if (level is not null && !SamusGroundedPoseTransitions.TryResolveKnockbackPoseCollision(samus, 
level, targetPose, frameCounter, roomObjects))
                return false;
            samus.Pose = targetPose;
            // The interrupted pose commits after movement. The input phase publishes its radius
            // next frame; keep the collision resolver's old live radius through this one.
        }
        else if (glitchBall)
        {
            // A hit on the glitch ball, unlike the ball path, installs the glitch ball's knockback pose, then continues
            // through the same knockback-start hurt initializer as a humanoid pose.
            samus.Pose = facingLeft
                ? SamusPoseId.GlitchBallKnockbackLeftPose
                : SamusPoseId.GlitchBallKnockbackRightPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            humanoid = true;
        }

        // Deliberately ignores both the damage-source side and controller input
        // when choosing a ball's vertical branch: right-facing ball art always gets up-right
        // direction two, and left-facing art gets up-left direction one. The humanoid branch
        // retains the unusual forward-held selection of down-left/down-right directions.
        samus.KnockbackXDirection = launch.KnockbackXDirection;
        if (morphed)
        {
            samus.KnockbackDirection = facingLeft ? SamusKnockbackDirection.UpLeft : SamusKnockbackDirection.UpRight;
        }
        else
        {
            bool forwardHeld = facingLeft
                ? (controllerInput & ControllerButton.Left) != ControllerButton.None
                : (controllerInput & ControllerButton.Right) != ControllerButton.None;
            samus.KnockbackDirection = launch.KnockbackXDirection == KnockbackHorizontalDirection.Left
                ? forwardHeld ? SamusKnockbackDirection.DownLeft : SamusKnockbackDirection.UpLeft
                : forwardHeld ? SamusKnockbackDirection.DownRight : SamusKnockbackDirection.UpRight;
        }

        // Enemy and enemy-projectile collision normally publish five, which is the default
        // supplied above. The intro Rinka deliberately publishes eleven;
        // accepting the producer-owned value here prevents this shared initializer from
        // shortening that visibly longer scripted reaction to ordinary gameplay timing.
        samus.KnockbackActive = true;
        // The knockback takes her movement, ending her flight after letting go of a
        // grapple swing if that was still running.
        samus.Grapple.ReleasedMovementActive = false;

        // The remainder of the knockback initializer runs for both the humanoid and the ball. A pending bomb
        // jump cannot coexist with hurt movement, and shinespark/Screw contact damage is
        // cancelled before the knockback moves her.
        samus.BombJumpRequest = SamusBombJumpRequest.None;
        // Taking her movement ends the bomb jump's movement immediately, but
        // does not yet release its independent lock on her pose input.
        samus.BombJumpStarting = false;
        samus.BombJumpActive = false;
        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.None;

        // Starts the hurt flash after cancelling bomb-jump/contact damage. This is an independent
        // 60-call palette/audio lifetime, not the five-frame knockback timer established above.
        samus.HurtFlash.Start();

        // Hurt launch retains the bottom-edge and Gravity-Suit selection.
        samus.Kinematics.YSpeed =
            SamusVerticalMotion.Knockback(samus.LiquidPhysics.DetermineMovementMedium(samus));
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        SamusAerialMovement.ConfigureEnvironmentGravity(samus);
        if (humanoid)
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }

    /// <summary>Runs one frame of the knockback's movement.</summary>
    internal static KnockbackMovementResult Step(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
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
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedX.Displacement, CollisionMovementDirection = requestedX.CollisionDirection });
        // Knockback uses the ordinary left/right wrapper, whose collision branch
        // cancels X momentum before the vertical pass. Retaining the clipped move's
        // base speed incorrectly accelerates the next frame away from the obstacle.
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = samus.KnockbackDirection is SamusKnockbackDirection.UpLeft or SamusKnockbackDirection.UpRight
            ? MoveWithSharedVerticalSpeedCalculation(level, samus, frameCounter, roomObjects)
            : MoveDownWithoutSpeedCalculation(level, samus, frameCounter, roomObjects);

        // Observes these values inside downward collision, before the landing
        // clears them. Preserve the impact snapshot in the typed result so the runtime can
        // execute the shared landing-presentation step at the same logical seam.
        bool landed = samus.KnockbackDirection is SamusKnockbackDirection.DownLeft or SamusKnockbackDirection.DownRight && vertical.Collided;

        if (vertical.Collided)
        {
            // Is reached only after the vertical helper, so horizontal wall
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
    /// Applies the only cross-family transition in the knockback input tables: Jump plus the
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
                $"Damage boost {sourcePose} -> {targetPose} is not a retail transition.");
        }

        // Returns only her movement to her pose. The conditional jump
        // reset in the pose handling does not run for a merely prospective type change.
        // Preserve velocity and hurt values until the common expiry handoff consumes them.
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.KnockbackActive = false;
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>Installs a same-family target selected by the two damage-boost tables.</summary>
    /// <param name="samus">The damage-boosting Samus.</param>
    /// <param name="targetPose">The table's normal-jump exit.</param>
    /// <param name="controllerNewInput">This frame's newly pressed buttons, which the normal-jump bookkeeping reads.</param>
    public static void ApplyDamageBoostPoseTransition(
        SamusState samus,
        SamusPoseId targetPose,
        ControllerButton controllerNewInput)
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
                $"Damage-boost exit {samus.Pose} -> {targetPose} is not in the transition table.");
        }

        // Both target families have the same radius. The pose change nevertheless runs
        // their ordinary initializer: it preserves the live 1/65536-pixel jump velocity,
        // reselects the acceleration mode from extra run speed, and can start a spark from
        // stored shine. The selected target's delay stream starts at frame zero.
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (!SamusAerialPoseTransitions.InitializeInstalledNormalJumpPose(samus, SamusMovementType.DamageBoost, controllerNewInput))
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>
    /// Consumes the zero-timer branch of the knockback check and finishes the expired hit
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
        return samus.KnockbackTimer == 0 && samus.KnockbackDirection != SamusKnockbackDirection.None;
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
        // path, where both radii ordinarily match and the alignment is a no-op.
        int previousRadius = SamusPoseMetadata.ReadPoseYRadius(
            unchecked(samus.PoseHistory.PreviousPose));
        int targetRadius = SamusPoseMetadata.ReadPoseYRadius(samus.Pose);
        samus.AlignBottomAfterPoseChange(previousRadius, targetRadius);

        // Samus's pose update finishes the hit straight away without installing its own
        // proposed falling pose. Any pose a higher-priority pose-transition cue installed is already live.
        EndWithoutPoseChange(samus);
    }

    /// <summary>Finishes a Morph or Spring Ball hit while retaining its current pose, collision radii and rolling animation.</summary>
    private static KnockbackMovementResult EndWithoutPoseChange(SamusState samus)
    {
        // Republishes the current pose, so Samus's pose update neither reloads its
        // radii nor resets its animation. This is the detail that keeps an interrupted
        // rolling Morph/Spring Ball visually continuous across the five hurt frames.
        return FinishKnockback(samus);
    }

    /// <summary>Restores ordinary input and movement ownership and clears hurt motion values after expiry.</summary>
    private static KnockbackMovementResult FinishKnockback(SamusState samus)
    {
        // Knockback's expiry restores the normal input mode as well as
        // movement, including when hurt interrupted a still-input-locked bomb rise.
        samus.BombJumpPoseInputLocked = false;
        // Returns her movement to her pose. A Shinespark windup can
        // have been initialized after the knockback check latched this expiry, so end that
        // spark's hold while retaining its independently owned palette and timer.
        SamusMovementHandOver.BackToPose(samus, SamusMovementOwner.Knockback);
        // The exact knockback-end cleanup shared by humanoid and morphed completion. The falling
        // flag has no field of its own; Y-direction two is its movement-visible
        // publication and is consumed by every normal/ball step.
        samus.KnockbackDirection = SamusKnockbackDirection.None;
        samus.KnockbackActive = false;
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        return new KnockbackMovementResult();
    }

    /// <summary>Uses ordinary aerial vertical speed calculations during rising knockback, including the direction reversal.</summary>
    private static BlockMoveResult MoveWithSharedVerticalSpeedCalculation(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects)
    {
        // Calls the exact same vertical step as ordinary aerial
        // movement, so reuse that owner instead of maintaining a subtly narrower
        // copy. The retail Rinka timer expires while Samus is still rising; her reported
        // midair hang came from failing to dispatch ordinary falling after that expiry, not
        // from this calculation. Sharing the step still preserves the real direction-
        // reversal behavior for any longer knockback stimulus without inventing a new path.
        return SamusAerialMovement.StepVerticalWithSpeedCalculations(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, frameCounter, out _, out _);
    }

    /// <summary>Applies externally supplied downward displacement through the shared vertical collision path without accelerating it.</summary>
    private static BlockMoveResult MoveDownWithoutSpeedCalculation(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects)
    {
        // Gives nonzero external Y priority over its normal total-X-plus-one
        // slope-following probe. This down-knockback path consumes the exact same helper.
        SignedSixteenSixteen requested = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);
        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = requested, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
    }

}
