using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's ordinary jump, spin-jump, wall-jump, aerial-turn, and falling movement, including
/// their shared choice of water or lava physics.
/// </summary>
/// <remarks>
/// This keeps the game's speed magnitudes, each with its fraction of a pixel, and separate vertical
/// direction. It does not use floating point, host elapsed time, or a guessed gravity
/// curve. Solid enemies stop Samus through the shared block collision, and the displacement enemies and blocks
/// add to her is the shared extra displacement. Ordinary
/// Dash and equipped Speed Booster momentum are retained through environment-selected
/// jumps, including the equipped vertical bonus. Terrain and solid/frozen-enemy wall jumps
/// plus all three liquid launches are distinct results.
/// </remarks>
public static class SamusAerialMovement
{
    /// <summary>
    /// Starts a jump through every environment/equipment path.
    /// Air, water and lava each select their jump speed from the normal or
    /// Hi-Jump speeds; Gravity Suit selects the air speed before that equipment choice.
    /// </summary>
    public static void InitializeJump(SamusState samus)
    {
        Ensure.NotNull(samus);

        SamusLiquidMedium medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        bool hiJumpEquipped = samus.EquippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots);
        // Speed Booster's bonus then raises the launch speed part by part (SpeedBoosterJumpBonus).
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Rising(
            SamusVerticalMotion.Launch(medium, hiJumpEquipped, kind: SamusLaunchKind.Jump));
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureEnvironmentGravity(samus);
    }

    /// <summary>
    /// Starts a wall jump in air, water or lava. The Speed Booster bonus raises its launch speed exactly as it raises a
    /// normal jump's.
    /// </summary>
    public static void InitializeWallJump(SamusState samus)
    {
        Ensure.NotNull(samus);

        SamusLiquidMedium medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        bool hiJumpEquipped = samus.EquippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots);
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Rising(
            SamusVerticalMotion.Launch(medium, hiJumpEquipped, kind: SamusLaunchKind.WallJump));
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureEnvironmentGravity(samus);
    }

    /// <summary>
    /// Applies the Speed Booster jump bonus when Speed Booster is equipped: the dash's extra run speed raises the launch
    /// speed part by part, as <see cref="SpeedBoosterJumpBonus"/> describes.
    /// </summary>
    private static void ApplyEquippedSpeedBoosterJumpBonus(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
            return;

        samus.Kinematics.VerticalVelocity = samus.Kinematics.VerticalVelocity.WithSpeed(SpeedBoosterJumpBonus.Boosted(samus.Kinematics.VerticalVelocity.Speed, samus.HorizontalSpeed.ExtraRunSpeed));
    }

/// <summary>
    /// Chooses Samus's vertical acceleration for the medium she moves in (<see cref="SamusLiquidMedium"/>).
    /// </summary>
    public static void ConfigureEnvironmentGravity(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Kinematics.YAcceleration =
            SamusVerticalMotion.Gravity(samus.LiquidPhysics.DetermineMovementMedium(samus));
    }

    /// <summary>
    /// Executes one frame of normal-jumping movement.
    /// </summary>
    internal static AerialMovementResult StepNormalJump(
        SamusMovement.NormalJumpingMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter)
    {
        ValidateCommon(terrain.Level, samus);
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: movement.Type,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);

        // The neutral and aimed jump-transition poses are genuine normal-jumping poses, but the game treats them as
        // a transition: base X speed is forced to zero, only external X/Y displacement is
        // applied, and normal vertical speed does not move Samus on this frame.
        if (SamusPoseMetadata.IsJumpTransitionPose(samus.Pose))
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
            BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(terrain, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

            // The transition's vertical move applies only the extra displacement: none moves nothing, a downward one
            // moves Samus one more pixel, and an upward one moves her exactly as given.
            SignedSixteenSixteen? externalY = SamusExtraDisplacement.CalculateExtraOnlyVerticalDisplacement(
                samus.Kinematics);
            BlockMoveResult? vertical = null;
            if (externalY is { } displacement)
            {
                vertical = SamusBlockCollision.MoveVertical(terrain, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
            }
            return new AerialMovementResult(
                Landed: externalY is { IsNegative: false } && vertical is { Collided: true },
                HitCeiling: externalY is { IsNegative: true } && vertical is { Collided: true });
        }

        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        MoveNormalAerialX(
            terrain,
            samus,
            controllerInput,
            movementType: movement.Type);
        return FinishVerticalMovement(terrain, samus, frameCounter);
    }

    /// <summary>
    /// Executes one frame of spin-jumping movement,
    /// including the wall-jump block contact and launch test.
    /// </summary>
    internal static AerialMovementResult StepSpinJump(
        SamusMovement.SpinJumpingMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        GameplayAction controllerNewInput)
    {
        ValidateCommon(terrain.Level, samus);

        // Space Jump is tried before ordinary spin movement. Space Jump is not a
        // generic double-jump: it only accepts a fresh Jump edge while descending and
        // while the falling speed lies inside the game's dry-air window.
        bool fullySubmergedWithoutGravity =
            !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
            samus.LiquidPhysics.IsTopBoundarySubmerged(samus);
        if (!fullySubmergedWithoutGravity)
            RestartSpaceJump(samus, controllerNewInput);

        // The underwater Space Jump sound plays only at the last tick of the spin's
        // start and middle frames. Movement owns it, so it is published before the
        // later animation step can advance the timer/frame pair.
        if (fullySubmergedWithoutGravity &&
            samus.AnimationFrameTimer.IsAt(1) &&
            (samus.AnimationIsOn(SamusAnimationFrames.SpaceJumpSpinStart) || samus.AnimationIsOn(SamusAnimationFrames.SpaceJumpSpinMiddle)))
        {
            samus.AudioRequests.QueueSound(SamusSound.UnderwaterSpaceJump, priority: SoundRequestPriority.Normal);
        }

        // Screw Attack's damaging body is republished every eligible dry-air spin frame.
        // A fully charged ordinary/Space-Jump spin instead publishes the pseudo Screw Attack, but only
        // outside full liquid physics. The runtime clears the damage mode before every movement step,
        // so neither stale damage mode can leak into another pose.
        if (!fullySubmergedWithoutGravity && SamusPoseMetadata.IsScrewAttackPose(samus.Pose))
            samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.ScrewAttack;
        else if (!fullySubmergedWithoutGravity && samus.BeamCharge.HasCharged(BeamChargeDefinitions.FullyChargedCounter))
            samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.PseudoScrew;

        // Bomb-block breaking is independent of contact-damage publication above: liquid
        // suppresses Samus damage, but the bomb-block setup still reads pose/boost through the shared
        // collision gate (SamusState.BreaksCollisionBombBlocks) and can break the terrain.
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: movement.Type,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateAerialBaseSpeed(movementType: movement.Type);

        // If acceleration did not overshoot the cap, spin jump retains motion only while
        // turning (SamusAccelerationMode.Turning) or while the input matching the pose's facing direction is held.
        // A speed the maximum capped bypasses this test.
        bool forwardHeld = IsForwardHeld(samus, controllerInput);
        bool allowHorizontal = calculation.ReachedMaximum ||
            speed.AccelerationMode == SamusAccelerationMode.Turning ||
            forwardHeld;
        if (!allowHorizontal)
        {
            // With no forward input left, only base motion stops.
            // Dash momentum is intentionally retained for a later direction press/landing.
            ClearBaseHorizontalMotion(speed);
            calculation = new AerialBaseSpeedResult(default, ReachedMaximum: false);
        }
        else if (speed.AccelerationMode == SamusAccelerationMode.Accelerating)
        {
            // SamusAccelerationMode.Decelerating is not ordinary deceleration here. The aerial speed calculation tests only whether Samus is turning,
            // so it continues accelerating on subsequent airborne frames.
            speed.AccelerationMode = SamusAccelerationMode.Decelerating;
        }

        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(terrain, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        bool wallJumpTriggered = CheckBlockWallJump(
            terrain,
            samus,
            controllerInput,
            controllerNewInput);
        if (wallJumpTriggered)
        {
            // A triggered wall-jump check skips gravity, so gravity and vertical displacement
            // do not execute on the trigger frame. Runtime pose handling consumes the
            // wall jump after movement and installs the wall-jump pose with the authored launch speed.
            return new AerialMovementResult(
                Landed: false,
                HitCeiling: false,
                WallJumpTriggered: true);
        }

        return FinishVerticalMovement(
            terrain,
            samus,
            frameCounter);
    }

    /// <summary>
    /// Executes wall-jumping movement, including its animation-frame/charge-
    /// selected contact damage before the ordinary jumping movement.
    /// </summary>
    internal static AerialMovementResult StepWallJump(
        SamusMovement.WallJumpingMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter)
    {
        ValidateCommon(terrain.Level, samus);

        // From the Screw Attack spin's start the somersault always attacks as the Screw Attack.
        // From the ordinary spin's start it attacks as the pseudo Screw Attack only once the beam is fully charged
        // (BeamChargeDefinitions.FullyChargedCounter). The jump's frames before the spin remain harmless even when fully charged.
        if (samus.AnimationHasReached(SamusAnimationFrames.WallJumpScrewAttackSpinStart))
            samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.ScrewAttack;
        else if (samus.AnimationHasReached(SamusAnimationFrames.WallJumpOrdinarySpinStart) && samus.BeamCharge.HasCharged(BeamChargeDefinitions.FullyChargedCounter))
            samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.PseudoScrew;

        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: movement.Type,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        MoveNormalAerialX(
            terrain,
            samus,
            controllerInput,
            movementType: movement.Type);
        return FinishVerticalMovement(terrain, samus, frameCounter);
    }

    /// <summary>
    /// Executes damage-boost movement, which moves Samus as normal jumping does: damage boost receives the ordinary
    /// variable-height jump cutoff, its movement type's X speeds, block collision, gravity, and landing.
    /// </summary>
    internal static AerialMovementResult StepDamageBoost(
        SamusMovement.DamageBoostMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter)
    {
        ValidateCommon(terrain.Level, samus);

        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: movement.Type,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        MoveNormalAerialX(
            terrain,
            samus,
            controllerInput,
            movementType: movement.Type);
        return FinishVerticalMovement(terrain, samus, frameCounter);
    }

    /// <summary>
    /// Executes the jumping and falling turns. Unlike ordinary aerial
    /// motion this calls the deceleration-ALLOWED X movement and does not apply variable-jump
    /// release; the three-frame turn animation therefore preserves reversal momentum.
    /// </summary>
    internal static AerialMovementResult StepTurningInAir(
        SamusMovement.AerialTurnMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        ValidateCommon(terrain.Level, samus);

        // Crouched aimed turns use the turning-while-jumping movement type even though they normally remain on
        // the floor. If a producer gives one a vertical direction, the turn movement immediately
        // moves Samus as the ordinary jumping turns do (unaimed, aimed up, down and diagonally up).
        SamusMovementType movementType = movement.Type;
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        // The turn moves Samus horizontally as every X move does, extra run speed included, before cancelling the boost
        // later in this frame.
        // Turning cannot actively dash, but retained boost still publishes contact
        // damage for this final frame. Clearing the counter is not retroactive.
        speed.HandleExtraRunSpeed(movementType, controllerInput: GameplayAction.None,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) != SamusLiquidMedium.Air);
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType);
        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(terrain, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // Before the shared gravity step, an upward speed passing zero at the apex becomes a stationary downward state
        // on this same frame.
        if (samus.Kinematics.VerticalVelocity.HasPassedApex)
        {
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
        }

        // The turn holds Samus still vertically, as the grounded aimed crouching turns do, so Moonfall pauses gravity
        // during its turn.
        AerialMovementResult result;
        if (samus.Kinematics.VerticalVelocity.HasNoDirection)
        {
            SamusGroundedMovement.RunNoSpeedCalculationGroundingProbe(terrain, samus, frameCounter);
            result = new AerialMovementResult(false, false);
        }
        else
            result = FinishVerticalMovement(terrain, samus, frameCounter, ceilingResponse: SamusCeilingResponse.Deferred);
        // Turning clears the collision-to-pose request after movement. A floor
        // collision clamps position but does not reset accumulated falling speed
        // or trigger landing presentation. Likewise, an upward move into the ceiling
        // only publishes a ceiling result: this movement clears it before pose
        // processing can stop the jump. Keep gravity and physical clipping, but
        // preserve upward velocity even while the head is touching the ceiling.

        // Cancel speed boost and explicitly clear both extra speeds.
        speed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        speed.ExtraRunSpeed = default;
        return result with { Landed = false, HitCeiling = false };
    }

    /// <summary>Executes one frame of <see cref="SamusMovementType.Falling"/> movement.</summary>
    internal static AerialMovementResult StepFalling(
        SamusMovement.FallingMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter)
    {
        ValidateCommon(terrain.Level, samus);
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: movement.Type,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);

        MoveNormalAerialX(
            terrain,
            samus,
            controllerInput,
            movementType: movement.Type);

        // An upward speed that passed zero becomes a stationary downward state
        // before the vertical movement can turn the speed past zero into moonfall-like
        // motion. An ordinary walk-off already enters moving down.
        if (samus.Kinematics.VerticalVelocity.HasPassedApex)
        {
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
        }

        AerialMovementResult result = FinishVerticalMovement(terrain, samus, frameCounter);
        // The movement selects fast-fall art after physics, before Samus is animated.
        // Aimed falling poses have their own animations and keep them.
        if (samus.Pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose or
            SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingGunExtendedLeftPose &&
            SamusSpeedMagnitude.IsAtLeast(samus.Kinematics.VerticalVelocity.Speed.Whole, SamusMovementDefinitions.FastFallAnimationSpeed) &&
            !samus.AnimationHasReached(SamusAnimationFrames.FastFallStart))
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusAnimationFrames.FastFall,
                SamusMovementDefinitions.FastFallAnimationTimer);
        return result;
    }

    /// <summary>
    /// Executes one frame of Samus's flight after release from a grapple swing.
    /// </summary>
    /// <remarks>
    /// This is a movement owner, not <see cref="SamusMovementType.NormalJumping"/> movement. It begins on the same
    /// frame that the grapple release derives launch velocity, survives the beam's following-frame
    /// cleanup, accelerates, tops out and slows at the release's own speeds,
    /// and hands her movement back to her pose after the upward speed passes zero, any downward pass, or
    /// a ceiling hit. Keeping that lifetime explicit prevents a grapple launch from silently
    /// acquiring ordinary jump input/caps one frame too early.
    /// </remarks>
    internal static AerialMovementResult StepReleasedFromGrapple(
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter)
    {
        ValidateCommon(terrain.Level, samus);
        if (!samus.Grapple.ReleasedMovementActive)
            throw new InvalidOperationException("Released-grapple flight is not moving Samus.");

        SamusKinematicsState state = samus.Kinematics;
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool releasedFlightEnds = false;

        // An upward speed that has passed zero ends the released flight at its apex: the flight clears the speed, turns
        // downward and hands movement back to the pose, but deliberately finishes this final frame of the flight.
        if (state.VerticalVelocity.HasPassedApex)
        {
            state.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
            releasedFlightEnds = true;
        }

        // The released flight always slows until its speed passes zero; only then can an update without input stop her
        // base speed.
        speed.AccelerationMode = SamusAccelerationMode.Decelerating;
        // The release's speeds are the same in every medium, so the medium Samus is in does not choose them.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(SamusHorizontalSpeedDefinitions.GrappleRelease);

        bool horizontalInput = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        BlockMoveResult horizontal;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !horizontalInput)
        {
            // With no direction held the flight clears its base speed and moves Samus by nothing.
            speed.BaseSpeed = default;
            // With no direction held the speed is not calculated, and the previous total speeds stay.
            SamusBlockCollision.MoveHorizontal(terrain, state, samus, samus.Pose, new HorizontalBlockMove { Displacement = SignedSixteenSixteen.Zero });
        }
        else
        {
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(terrain, state, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        AerialMovementResult result = FinishVerticalMovement(
            terrain,
            samus,
            frameCounter);
        // Any downward pass, or a ceiling hit, ends the
        // released flight. Only a free upward pass keeps it for another frame.
        if (samus.SolidVerticalCollisionResult != SamusVerticalCollisionResult.None)
            releasedFlightEnds = true;

        samus.Grapple.ReleasedMovementActive = !releasedFlightEnds;
        return result;
    }

    /// <summary>Applies the selected medium's base speed and wall collision, preserving the no-input bypass and wall-jump exception.</summary>
    private static void MoveNormalAerialX(
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        SamusMovementType movementType)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateAerialBaseSpeed(movementType);

        // A wall jump promotes accelerating to decelerating
        // after calculating speed, then bypasses the held-direction admission test.
        if (movementType == SamusMovementType.WallJumping &&
            speed.AccelerationMode == SamusAccelerationMode.Accelerating)
            speed.AccelerationMode = SamusAccelerationMode.Decelerating;

        bool directionHeld = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !directionHeld)
        {
            // Clear both the frame's horizontal displacement and persistent base speed.
            speed.BaseSpeed = default;
            // Samus then does not move horizontally, and the previous total speeds are kept: no slope alignment runs
            // either.
            return;
        }

        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(terrain, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
    }

    /// <summary>Runs the shared Y mover and reduces its collision scan to the landing and ceiling signals her movement reads.</summary>
    private static AerialMovementResult FinishVerticalMovement(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter,
        SamusCeilingResponse ceilingResponse = SamusCeilingResponse.Immediate)
    {
        BlockMoveResult vertical = StepVerticalWithSpeedCalculations(terrain, samus, frameCounter, out bool hitCeiling, out bool downwardDisplacement, ceilingResponse);
        bool landed = downwardDisplacement && vertical.Collided;

        return new AerialMovementResult(landed, hitCeiling);
    }

    /// <summary>
    /// Runs the shared vertical movement with its speed calculations
    /// without adding any horizontal movement.
    /// </summary>
    /// <remarks>
    /// Ordinary jumps and falls call this after their X pass, but the drained Samus's
    /// movement calls this directly. Keeping one implementation
    /// is important: the old-speed displacement, equality-only terminal-speed check, and
    /// ceiling response are observable game behavior, not general-purpose physics.
    /// </remarks>
    internal static BlockMoveResult StepVerticalWithSpeedCalculations(SamusTerrain terrain, SamusState samus, int frameCounter, out bool hitCeiling, out bool downwardDisplacement, SamusCeilingResponse ceilingResponse = SamusCeilingResponse.Immediate)
    {
        ValidateCommon(terrain.Level, samus);
        SamusKinematicsState state = samus.Kinematics;

        // The displacement takes the OLD velocity before gravity changes it. A newly initialized fall from rest therefore
        // moves zero pixels on its first update even though gravity has already accumulated for next frame. Gravity stops
        // only at exactly the terminal whole speed: a fall above it is not clamped, which matters for externally scripted
        // velocities.
        SignedSixteenSixteen displacement = state.VerticalVelocity.Displacement;
        state.VerticalVelocity = state.VerticalVelocity.AfterGravity(state.YAcceleration, SamusVerticalMotionDefinitions.TerminalFallWholeSpeed);
        displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            displacement);
        downwardDisplacement = !displacement.IsNegative;
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(terrain, state, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });

        // Both vertical movers, downward and upward, publish this result after every move. A downward pass always
        // publishes one: Landed on a landing, otherwise Falling unless a wall jump's WallJump is retained.
        if (downwardDisplacement)
            samus.LeaveDownwardMoveResult(landed: vertical.Collided);
        else
            samus.LeaveUpwardMoveResult(hitCeiling: vertical.Collided);

        hitCeiling = displacement.IsNegative && vertical.Collided;
        if (hitCeiling && ceilingResponse != SamusCeilingResponse.Deferred)
        {
            // Ordinary movements consume the ceiling collision as a jump stop. A turn
            // instead clears that pending result, so the turn defers these writes.
            state.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
        }

        return vertical;
    }

    /// <summary>Ends upward movement on Jump release or the speed passing zero.</summary>
    private static void ApplyVariableJumpCutoff(
        SamusKinematicsState state,
        GameplayAction controllerInput)
    {
        if (!state.VerticalVelocity.IsRising)
            return;

        bool jumpHeld = (controllerInput & GameplayAction.Jump) != GameplayAction.None;
        if (!jumpHeld || state.VerticalVelocity.HasPassedApex)
        {
            state.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
        }
    }

    /// <summary>Tests the directional button matching the current facing pose.</summary>
    private static bool IsForwardHeld(
        SamusState samus,
        GameplayAction controllerInput) =>
        SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? (controllerInput & GameplayAction.Left) != GameplayAction.None
            : (controllerInput & GameplayAction.Right) != GameplayAction.None;

    /// <summary>
    /// Runs the wall-jump check's block and solid-enemy wall probes; returns whether the jump chord launched a wall jump.
    /// </summary>
    private static bool CheckBlockWallJump(
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        GameplayAction controllerNewInput)
    {
        // The check stops before she probes enemies or terrain: touching a block can move her by a fraction of a pixel.
        if (!samus.PoseHistory.AllowsWallJumpProbe)
            return false;

        SamusAnimationPlace firstEligibleFrame = SamusPoseMetadata.IsScrewAttackPose(samus.Pose)
            ? SamusAnimationFrames.ScrewAttackWallJumpReady
            : SamusAnimationFrames.SpinJumpWallJumpReady;
        // Once the spin reaches the frames a launch can start on, the check withdraws an earlier launch's enemy shake
        // before it reads the held direction; earlier frames leave it waiting.
        bool launchFrameReached = samus.AnimationHasReached(firstEligibleFrame);
        if (launchFrameReached)
            samus.Kinematics.WallJumpLaunch.Withdraw();

        bool heldLeft = (controllerInput & GameplayAction.Left) != GameplayAction.None;
        bool heldRight = (controllerInput & GameplayAction.Right) != GameplayAction.None;
        if (!heldLeft && !heldRight)
            return false;

        // Test Left first and probe RIGHT; the mirror Right chord probes
        // LEFT. That counter-intuitive direction is deliberate: the button points away from
        // the wall behind the newly facing spin pose.
        SignedSixteenSixteen requested = SignedSixteenSixteen.FromWholePixels(SamusMovementDefinitions.WallJumpReachPixels.Toward(
            heldLeft ? HorizontalDirection.Right : HorizontalDirection.Left));
        BlockMoveResult probe = SamusBlockCollision.ProbeWallHorizontal(
            terrain,
            samus.Kinematics, samus, samus.Pose,
            requested);
        if (!probe.Collided)
            return false;

        // The accepted travel is a displacement either way; the distance is its whole pixels without their sign.
        int distance = Math.Abs(probe.AcceptedDisplacement.WholePixels);
        if (!launchFrameReached)
        {
            SamusAerialPoseTransitions.ApplyWallContactAnimationRewind(samus);
            return false;
        }

        bool jumpNew = (controllerNewInput & GameplayAction.Jump) != GameplayAction.None;
        bool triggered = jumpNew && distance < SamusMovementDefinitions.WallJumpReachPixels;
        if (triggered)
        {
            // A wall jump off a solid enemy and one off terrain both record the wall jump as her vertical collision
            // result, which others read, as well as returning it; the result below also tells
            // the runtime to install the wall-jump pose explicitly.
            samus.LeaveWallJumpResult();
            // A launch off a solid or frozen enemy asks the next enemy pass to shake that enemy; a launch off
            // terrain asks nothing.
            if (probe.EnemyCollision is { Enemy: { } launchedFrom })
                samus.Kinematics.WallJumpLaunch.Launch(launchedFrom);
        }
        return triggered;
    }

    /// <summary>
    /// The air/partially-submerged Space Jump gate of spin-jumping movement.
    /// Screw Attack can use this physics when both items are
    /// equipped even though its visible pose is the Screw Attack pose.
    /// </summary>
    private static void RestartSpaceJump(
        SamusState samus,
        GameplayAction controllerNewInput)
    {
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump) ||
            !samus.Kinematics.VerticalVelocity.IsFalling)
        {
            return;
        }

        // A fresh Jump restarts the spin while the fall's speed lies within the Space Jump window.
        UnsignedSixteenSixteen fallingSpeed = samus.Kinematics.VerticalVelocity.Speed;
        // The remembered medium is written by the preceding animation pass, not recomputed here. This
        // matters while crossing a surface: the top-boundary gate above and remembered
        // velocity window can intentionally describe different samples for one frame.
        UnsignedSixteenSixteen minimumSpeed = samus.LiquidPhysics.MediumLastEntered != SamusLiquidMedium.Air
            ? SamusVerticalMotionDefinitions.SpaceJumpLiquidMinimumFallingSpeed
            : SamusVerticalMotionDefinitions.SpaceJumpAirMinimumFallingSpeed;
        if (fallingSpeed.IsSlowerThan(minimumSpeed) ||
            !fallingSpeed.IsSlowerThan(SamusVerticalMotionDefinitions.SpaceJumpFallingSpeedLimit) ||
            (controllerNewInput & GameplayAction.Jump) == GameplayAction.None)
        {
            return;
        }

        // This is the same jump start used by a grounded launch: authored initial
        // magnitude, Speed Booster's split bonus, dry-air gravity, and upward direction.
        SamusAerialMovement.InitializeJump(samus);
    }

    /// <summary>Clears ordinary base X velocity and acceleration while leaving separate boost momentum intact.</summary>
    private static void ClearBaseHorizontalMotion(SamusHorizontalSpeedState speed)
    {
        speed.BaseSpeed = default;
        speed.AccelerationMode = SamusAccelerationMode.Accelerating;
    }

    /// <summary>Checks that terrain and Samus state are available before an aerial step.</summary>
    private static void ValidateCommon(
        RoomLevelData level,
        SamusState samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
    }
}
