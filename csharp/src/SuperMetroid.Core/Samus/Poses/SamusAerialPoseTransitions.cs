using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's airborne pose transitions: jumps, spin and wall jumps, grapple wall jumps and drops, Shinespark wind-up, aerial turns and aim.</summary>
internal static class SamusAerialPoseTransitions
{
    /// <summary>
    /// Shared momentum initialization of the normal-jump and falling pose
    /// setups. Base velocity survives; extra dash momentum
    /// alone selects <see cref="SamusAccelerationMode.Decelerating"/> instead of ordinary acceleration.
    /// </summary>
    internal static void InitializeOrdinaryAerialAcceleration(SamusState samus)
    {
        samus.HorizontalSpeed.AccelerationMode =
            !samus.HorizontalSpeed.ExtraRunSpeed.IsZero
                ? SamusAccelerationMode.Decelerating
                : SamusAccelerationMode.Accelerating;
    }

    /// <summary>
    /// Applies a normal-jump body selected when aim or Fire cancels a spin, Space Jump,
    /// Screw Attack, or wall-jump pose. This is the shared
    /// spin, Space Jump, Screw Attack and wall-jump to normal-jumping route through
    /// the spin-jump, wall-jump and crouch-adjustment setups.
    /// </summary>
    /// <remarks>
    /// The transition does not start a new jump: the gun-extended normal-jump poses are deliberately
    /// excluded by the crouch-adjustment setup, so the live vertical velocity survives. Spin bodies are
    /// shorter than the resulting normal-jump body, however, so the shared changed-pose
    /// collision pass must still be allowed to reject the expansion under a low ceiling.
    /// The wall jumps' input transitions use this very same route for their diagonal-aim
    /// poses and gun-extended Shot poses; they do not have a separate wall-fire transition.
    /// </remarks>
    public static void ApplySpinOrWallJumpToNormalJumpTransition(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter,
        GameplayAction controllerNewInput)
    {
        Ensure.NotNull(terrain);

        // The spin, Space Jump, Screw Attack and wall-jump input transitions emit only same-facing
        // normal-jump poses when aim or Shoot cancels rotation.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            terrain,
            targetPose,
            frameCounter,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.RetainSource)
            return;
        if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            // Samus takes the ordinary crouch when the expanded body is blocked above and
            // below. The pose change still runs and sees the spin/wall-jump previous movement type.
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            if (samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
                samus.HorizontalSpeed.ShowSuitShadeAtColourStep();
            return;
        }

        samus.Pose = targetPose;
        // The changed-pose collision keeps the source collision radius for the rest of this update.
        // The input phase's SetRadius publishes the new pose radius on the next update.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);

        // Aim may cancel the spin into a pose that consumes stored shine. That starts the shinespark's windup, as a
        // normal jump's setup does, before ordinary acceleration.
        bool beganShinespark = TryBeginShinesparkWindup(samus,
            targetPose, SamusPoseMetadata.IsWallJumpPose(sourcePose)
                ? SamusMovementType.WallJumping : SamusMovementType.SpinJumping);
        if (!beganShinespark)
            InitializeOrdinaryAerialAcceleration(samus);

        // The pose-metadata refresh reloads the ordinary suit shade whenever the previous movement
        // type was spin/wall-jump and Screw Attack is equipped, even if the visible source
        // happened to be generic spin or Space Jump art.
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            samus.HorizontalSpeed.ShowSuitShadeAtColourStep();

        if (beganShinespark)
            return;

        // The newly installed pose's shot direction is published on the exact Fire
        // edge that selected this pose. The projectile producer consumes it in the input phase.
        if ((controllerNewInput & GameplayAction.Shoot) != GameplayAction.None)
            samus.TransitionShot.Hand(SamusTransitionShot.FromPose(samus));

        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>Applies a normal-jump or falling turn, selecting the turn that keeps her aim (<see cref="SamusAimedPoseChoiceDefinitions.JumpingTurns"/>, <see cref="SamusAimedPoseChoiceDefinitions.FallingTurns"/>) after the shared larger-pose collision pass.</summary>
    /// <param name="samus">The Samus this reads.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="frameCounter">The frame counter supplied to changed-pose block reactions.</param>
    /// <remarks>An accepted turn folds extra run speed into base turn momentum, preserves vertical velocity, adjusts the whole-pixel Y center if required, and initializes animation while retaining the source collision radius until Samus's next input phase.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="terrain"/> is null.</exception>
    /// <exception cref="InvalidDataException">The source pose aims in no shot direction.</exception>
    /// <remarks>
    /// The jumping and falling input transitions name the generic turn of their family; the turn installed is the one the
    /// source's aim selects, a normal jump's from the jumping turns and a fall's from the falling turns.
    /// </remarks>
    public static void ApplyAerialTurn(
        SamusState samus,
        SamusTerrain terrain,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        SamusPoseId sourcePose = samus.Pose;
        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        bool jumping = sourceMovementType == SamusMovementType.NormalJumping;

        // The turn keeps her aim. A straight-down aim takes the down-aimed turn away from the way she faces.
        SamusProjectileDirection? shotDirection = SamusPoseMetadata.ReadAimDirection(samus);
        IReadOnlyDictionary<SamusProjectileDirection, SamusPoseId> turns = jumping
            ? SamusAimedPoseChoiceDefinitions.JumpingTurns
            : SamusAimedPoseChoiceDefinitions.FallingTurns;
        SamusPoseId selectedTurnPose = shotDirection is { } direction && turns.TryGetValue(direction, out SamusPoseId turn)
            ? turn
            : throw new InvalidDataException(
                $"{(jumping ? "Jump" : "Fall")} pose {sourcePose} aims in no shot direction ({SamusPoseMetadata.ReadAim(samus)}).");

        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            terrain,
            selectedTurnPose,
            frameCounter,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.RetainSource)
            return;
        if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        // The changed-pose collision resolves expansion before the pose change runs the final movement type.
        // A crouch substitution never reaches the aerial-turn momentum setup.
        SamusGroundedPoseTransitions.FoldExtraRunSpeedIntoBaseAndStartTurn(samus);
        samus.Pose = selectedTurnPose;
        // The changed-pose step preserves the source radius until the next input phase.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Applies the spin-jump setup when spin/wall-jump input selects the opposite spin pose. A true
    /// direction reversal folds extra speed into base speed and selects <see cref="SamusAccelerationMode.Turning"/>, preserving
    /// old-world direction while the newly facing pose decelerates.
    /// </summary>
    public static void ApplySpinJumpDirectionTransition(SamusState samus, SamusPoseId targetPose)
    {
        SamusFacingDirection oldDirection = SamusPoseMetadata.ReadFacingDirection(samus);
        SamusFacingDirection newDirection = SamusPoseMetadata.ReadFacingDirection(targetPose);
        if (oldDirection != newDirection)
        {
            SamusGroundedPoseTransitions.FoldExtraRunSpeedIntoBaseAndStartTurn(samus);

            // Unlike the grounded and aerial turn setups, the spin-jump setup cancels the speed boost right after
            // folding the extra speed in, so her running momentum ends on this update.
            samus.HorizontalSpeed.CancelRunningMomentum(oldDirection);
        }

        // Ordinary and wall-jump definition fallbacks publish the generic spin jump, whereas
        // the dedicated Screw Attack and Space Jump input transitions can publish their already-
        // specialized opposite-facing pose directly. Reduce either form back to the
        // generic direction before applying the spin-jump setup's live equipment priority. This is
        // observable when both items are equipped: an authored right-to-left Screw Attack pose must remain
        // Screw art, while the same directional intent with Screw unequipped becomes Space
        // Jump art instead of keeping the item the selected pose was drawn for.
        samus.Pose = SelectEquippedSpinPose(samus, newDirection, start: SpinPoseStart.SkipFirstFrame);
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // The reversed spin starts on its second frame.
        SamusPresentation.InitializeAnimation(samus, SamusAnimationDelays.AnimationForPose(samus.Pose).AfterStart);
    }

    /// <summary>
    /// Applies the wall-jump check, the wall-jump trigger, and the jump launch after the block
    /// wall test succeeds. The launch takes the dry-air speeds.
    /// </summary>
    /// <param name="samus">The Samus this reads.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="frameCounter">The frame counter passed to the changed-pose collision resolver.</param>
    public static void ApplyWallJumpTrigger(
        SamusState samus,
        SamusTerrain terrain,
        int frameCounter)
    {
        // Only the spin-jumping mover reports the wall-jump check's trigger. The cached movement type, not the
        // live pose, runs that mover: a drained pose stored without the pose setup keeps
        // spinning and still wall-jumps. The trigger reads only the way she faces.
        SamusPoseId targetPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose;
        // The accepted wall contact still passes through the ordinary changed-pose
        // collision check. Expanding a compact spin body beneath a ceiling can
        // move the center away from it, reject the expansion, or select crouch.
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            terrain, targetPose, frameCounter, out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.RetainSource)
            return;
        if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            samus.ApplyPoseChangeCollisionCrouchFallback(samus.Pose);
            return;
        }
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);

        // A wall jump from a Screw Attack spin asks for the normal suit shade back before the jump starts; the shade
        // itself is applied later in the update.
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            samus.HorizontalSpeed.ShowSuitShadeAtColourStep();

        samus.Pose = targetPose;
        // The collision radius stays the spin body's; the input phase's radius update takes
        // the wall-jump body only next frame, so this frame's enemy and projectile
        // collision still use the compact spin radius.

        // The wall jump sets her accelerating and clears the base speed before
        // the jump installs the launch. It does *not* touch the extra run
        // speed or the running momentum: a wall jump made
        // out of a Dash therefore carries that speed into wall-jumping movement.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;

        // A triggered wall jump queues the landing sound at normal priority
        // after clearing the base speed and before pose setup.
        samus.AudioRequests.QueueSound(RoomSound.Landing, priority: SoundRequestPriority.Normal);

        SamusAerialMovement.InitializeWallJump(samus);
        samus.LiquidPhysics.SpawnWallJumpDust(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Applies the pose/launch half of the grapple wall jump after the grapple wall-grace probe
    /// has accepted a fresh Jump edge. This route intentionally reverses the contact pose:
    /// the left wall-contact pose launches through the left-facing wall jump, while the right
    /// launches through the right-facing one.
    /// </summary>
    /// <remarks>
    /// The eventual seven-pixel horizontal push remains the ordinary wall-jump animation-cue
    /// path already implemented by <see cref="SamusPresentation.Animate"/>. This method performs only the
    /// immediate cleanup, the X-speed kill, and environment-selected
    /// vertical launch that occur when the wall-jump phase runs.
    /// </remarks>
    public static void ApplyGrappleWallJump(SamusState samus)
    {
        // The wall jump reads the contact pose's facing, not the wall side its name gives. Facing right selects
        // the left wall jump; any other facing selects the right.
        samus.Pose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Right) ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // The grapple's wall jump clears the base speed, then the normal pose transition
        // reaches the jump launch. Like the wall jump without the grapple, it leaves both the
        // extra run speed and the running momentum intact, so Dash momentum survives this wall launch.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;

        // The grapple's end sound is queued at essential priority.
        // It also clears the beam's charge and its flare, so neither the wall jump's charged contact nor the next
        // projectile pass inherits a charge built before the grapple (#1577).
        samus.AudioRequests.QueueSound(GrappleBeamDefinitions.Sounds.Stop.SoundEffect, GrappleBeamDefinitions.Sounds.Stop.Priority);
        samus.BeamCharge.ClearChargeAndFlare();

        SamusAerialMovement.InitializeWallJump(samus);
        samus.LiquidPhysics.SpawnWallJumpDust(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Commits the pose selected when the grapple beam drops Samus.
    /// The drop names its exact target pose and this method runs the shared
    /// block-only pose-expansion collision; the drop itself already cleared Samus's fall momentum.
    /// </summary>
    public static void ApplyGrappleDropTransition(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        // The standing and crouching poses, unaimed and aimed, are every pose a grapple drop
        // takes. Other releases take the grapple's wall jump or the swing release instead.
        // The enemy death animation requests this cleanup for a grapple-killed enemy even
        // when the beam never attached. Samus can therefore still be standing, aiming,
        // or jumping. The game's drop selects by radius/shot direction in those poses too;
        // only its output domain is restricted, not the source to connected-grapple art.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
            terrain,
            targetPose,
            frameCounter,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            samus.Pose = targetPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
            if (samus.Pose != samus.PoseHistory.PreviousPose)
                SamusPresentation.InitializeAnimation(samus);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

    }

    /// <summary>
    /// Applies the early wall-jump rewind when an early spin frame finds the wall-jump chord.
    /// This does not launch Samus: it rewinds the spin to its end with a one-update wait, so the spin's next frame is
    /// its first wall-jump-ready one.
    /// </summary>
    /// <remarks>
    /// Tests only for the Screw Attack poses. It runs for whatever pose holds the
    /// cached spin-jump movement type, including a drained pose stored without
    /// the pose setup.
    /// </remarks>
    public static void ApplyWallContactAnimationRewind(SamusState samus)
    {
        samus.AnimationFrameTimer = WrappingCountdown.OneFrame;

        // Contact rewinds the spin to its end, whose next frame, after the one-update wait, is its first
        // wall-jump-ready frame. Screw Attack's spin is longer than the ordinary spin and Space Jump's.
        samus.SetAnimationFrame(SamusPoseMetadata.IsScrewAttackPose(samus.Pose)
            ? SamusWallContactAnimationDefinitions.ScrewAttackRewindFrame
            : SamusWallContactAnimationDefinitions.SpinJumpRewindFrame);
    }

    /// <summary>
    /// Applies the spin-jump setup's equipment, liquid and start-sound selection to the spin jump facing
    /// <paramref name="facing"/>: the left-facing spins facing left, otherwise the right-facing ones.
    /// </summary>
    internal static SamusPoseId SelectEquippedSpinPose(SamusState samus, SamusFacingDirection facing, SpinPoseStart start = SpinPoseStart.FirstFrame)
    {
        SamusEquippedSpinPoses equipped = facing == SamusFacingDirection.Left
            ? SamusPoseFamilyDefinitions.SpinPosesFacingLeft
            : SamusPoseFamilyDefinitions.SpinPosesFacingRight;
        SamusPoseId genericPose = equipped.Generic;

        // Full submersion returns before either equipment substitution or the sound.
        // Gravity bypasses this test; water's disabled-physics option is handled by the
        // shared top-boundary query, including the water-before-lava precedence.
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) && samus.LiquidPhysics.IsTopBoundarySubmerged(samus))
            return genericPose;

        // The game tests Screw Attack first. A save with both items equipped therefore uses
        // Screw Attack art, not Space Jump art, while retaining Space Jump's repeat-jump physics.
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
        {
            if (start != SpinPoseStart.SkipFirstFrame)
                samus.AudioRequests.QueueSound(SamusSound.ScrewAttack, priority: SoundRequestPriority.Normal);
            return equipped.ScrewAttack;
        }
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump))
        {
            // Unlike ordinary/Screw Attack spins, the game also queues this on reversal.
            samus.AudioRequests.QueueSound(SamusSound.SpaceJump, priority: SoundRequestPriority.Normal);
            return equipped.SpaceJump;
        }
        if (start != SpinPoseStart.SkipFirstFrame && samus.Presentation.Mode != SamusPresentationMode.Cinematic)
            samus.AudioRequests.QueueSound(SamusSound.SpinJump, priority: SoundRequestPriority.Normal);
        return genericPose;
    }

    /// <summary>
    /// Applies the normal-jump pose setup after a normal-jump
    /// transition has selected one of the six launch bodies.
    /// </summary>
    /// <remarks>
    /// This deliberately receives the already-selected target instead of testing the
    /// neutral and aimed jump-transition art. The game does not consume stored shine when Jump is
    /// first pressed; it waits until the animation's pose-transition cue installs the neutral,
    /// aimed-up or diagonally-up normal jump.
    /// That delay is visible both to animation and to the stored shine's shade countdown
    /// (<see cref="Abilities.ShinesparkDefinitions.StoredShineFrames"/>).
    /// </remarks>
    internal static bool TryBeginShinesparkWindup(
        SamusState samus,
        SamusPoseId targetPose,
        SamusMovementType previousMovementType)
    {
        if (!SamusPoseMetadata.BeginsShinesparkFromStoredShine(targetPose) ||
            !samus.Shinespark.TryBeginWindup(samus, SamusPoseMetadata.ReadFacingDirection(targetPose)))
        {
            return false;
        }

        // From a normal jump (the PREVIOUS movement type), Samus is lifted one pixel and
        // the result becomes both current and previous Y, so this frame's vertical scroll sees no motion.
        if (previousMovementType == SamusMovementType.NormalJumping)
        {
            samus.Kinematics.Position -= SamusPoseCollisionDefinitions.NormalJumpLift;
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
        }

        SamusPresentation.InitializeAnimation(samus);
        return true;
    }

    /// <summary>
    /// Applies a shinespark windup's directional input-transition match, whose launch only the windup's input lists
    /// choose, and installs its exact special movement owner. No ordinary jump velocity or grounded momentum step runs.
    /// </summary>
    public static void ApplyShinesparkDirectionTransition(SamusState samus, ShinesparkLaunch launch)
    {
        samus.Shinespark.BeginDirectionalLaunch(samus, launch);
    }

    /// <summary>
    /// Applies the ordinary jump transitions her input selects, including the jump transition's jump start:
    /// the routes with no equipment or aim, among them interrupting any same-facing landing with a fresh Jump press.
    /// </summary>
    public static void ApplyOrdinaryJumpTransition(
        SamusState samus,
        SamusPoseId targetPose,
        GameplayAction controllerNewInput = GameplayAction.None)
    {
        // The setup installs a jump transition or the spin jump that the current pose's input transitions select.
        // Movement-specific launches (damage boost, grapple, bomb jump, wall jump, and Shinespark) each start their own
        // way.
        samus.Pose = targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose
            ? SelectEquippedSpinPose(samus, SamusPoseMetadata.ReadFacingDirection(targetPose))
            : targetPose;
        // The pose update happens after movement. The next input phase, including
        // the intro-demo input mode, owns publication of the new live radius.
        SamusPresentation.InitializeAnimation(samus);
        SamusAerialMovement.InitializeJump(samus);

        // Only the normal-jumping setup hands the newly installed pose's direction to the weapon firing; spin-jump's
        // separate setup never publishes this bridge even when Shoot and Jump share a frame.
        ArmNormalJumpTransitionShot(samus, controllerNewInput);
    }

    /// <summary>
    /// Drops the pending transition shot, as every update does after the HUD and projectile step: a shot refused by the
    /// cooldown or by Samus's shot count loses it too, and no later shot takes it up.
    /// </summary>
    public static void DropTransitionShot(SamusState samus) => samus.TransitionShot.Drop();

    /// <summary>
    /// The normal-jumping pose setup for a normal-jump pose already
    /// installed by a transition that reaches the ordinary setup. Stored shine
    /// diverts the six shinespark-capable poses into a windup; otherwise retained extra run
    /// speed selects deceleration and a fresh Shoot press arms the transition shot.
    /// </summary>
    /// <returns>True when the setup began a shinespark windup instead.</returns>
    internal static bool InitializeInstalledNormalJumpPose(
        SamusState samus,
        SamusMovementType previousMovementType,
        GameplayAction controllerNewInput)
    {
        if (TryBeginShinesparkWindup(samus, samus.Pose, previousMovementType))
            return true;
        InitializeOrdinaryAerialAcceleration(samus);
        ArmNormalJumpTransitionShot(samus, controllerNewInput);
        return false;
    }

    /// <summary>
    /// Hands the next update's weapon firing a shot in the pose's direction when a fresh Shoot press accompanies an accepted
    /// normal-jump pose.
    /// </summary>
    internal static void ArmNormalJumpTransitionShot(SamusState samus, GameplayAction controllerNewInput)
    {
        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.NormalJumping &&
            (controllerNewInput & GameplayAction.Shoot) != GameplayAction.None)
            samus.TransitionShot.Hand(SamusTransitionShot.FromPose(samus));
    }

    /// <summary>
    /// Applies a same-facing input/fallback transition within <see cref="SamusMovementType.NormalJumping"/> or
    /// <see cref="SamusMovementType.Falling"/> without replacing the live velocities: aiming, firing or
    /// releasing forward, including the horizontal gun-extended jumping and falling poses.
    /// </summary>
    /// <remarks>
    /// The input transitions that name this setup change between poses of one radius; the compact straight-down poses,
    /// whose change needs the expansion collision probe, name <see cref="ApplyCompactAerialTransition"/> instead.
    /// </remarks>
    public static void ApplySameFacingAerialTransition(SamusState samus, SamusPoseId targetPose)
    {
        SamusPoseId sourcePose = samus.Pose;
        SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(samus);
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        // Both normal-jump and falling targets run their movement setup.
        // Preserve base velocity, but derive acceleration mode from extra dash speed.
        if (sourcePose != samus.Pose)
        {
            // The normal-jump setup also consumes stored shine on eligible poses.
            // This includes releasing forward midair, not just the initial ground jump.
            if (TryBeginShinesparkWindup(samus, targetPose, previousMovementType))
                return;
            InitializeOrdinaryAerialAcceleration(samus);
        }
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Applies same-facing transitions where either endpoint is a compact straight-down
    /// jumping or falling pose, preserving live velocity while reproducing pose-change collision.
    /// </summary>
    /// <remarks>
    /// Entering the compact radius from the jump's larger one requires no collision work in the larger-radius check.
    /// Leaving it probes every newly occupied pixel above and below. If both initial
    /// directions are blocked, the crouch selector selects ordinary crouch; if a compensating shift's
    /// second probe is blocked, the collision exit retains the source. Neither case installs the target.
    /// </remarks>
    public static void ApplyCompactAerialTransition(
        SamusState samus,
        SamusTerrain terrain,
        SamusPoseId targetPose,
        int frameCounter)
    {
        Ensure.NotNull(terrain);

        // The input transitions that name this setup change within one facing's normal jumps or falls.
        SamusPoseId sourcePose = samus.Pose;
        SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(sourcePose);
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
                terrain,
                targetPose,
                frameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }

        samus.Pose = targetPose;
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, centerAdjustment);
        if (sourcePose != samus.Pose)
        {
            // Pose collision uses prospective geometry; the live radius belongs to
            // the current frame's input phase and changes only at the next input phase.
            // Successful expansion still runs the normal-jump setup. A stored
            // shine may replace the requested aim pose; collision rejection above must
            // not consume the charge or apply the windup's one-pixel adjustment.
            if (TryBeginShinesparkWindup(samus, targetPose, previousMovementType))
                return;
            InitializeOrdinaryAerialAcceleration(samus);
        }
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Kills Samus's run speed when her input chooses no new pose while she falls. Cancel the
    /// dash component after movement, preserving the base speed and acceleration mode.
    /// It is the falling poses' own no-transition setup, so no new-fall Y reset is needed.
    /// </summary>
    public static void StopRunWhenFallingWithoutTransition(SamusState samus)
    {
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        samus.HorizontalSpeed.ExtraRunSpeed = default;
    }

}
