using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's airborne pose transitions: jumps, spin and wall jumps, grapple wall jumps and drops, Shinespark wind-up, aerial turns and aim.</summary>
internal static class SamusAerialPoseTransitions
{
    /// <summary>
    /// Shared momentum initialization in SamusFunc_F468_NormalJump ($91:F543) and
    /// SamusFunc_F468_Falling ($91:F60D). Base velocity survives; extra dash momentum
    /// alone selects deceleration mode two instead of ordinary acceleration.
    /// </summary>
    internal static void InitializeOrdinaryAerialAcceleration(SamusState samus)
    {
        samus.HorizontalSpeed.AccelerationMode =
            samus.HorizontalSpeed.ExtraRunSpeed.Whole != 0 || samus.HorizontalSpeed.ExtraRunSpeed.Fraction != 0
                ? SamusAccelerationMode.Decelerating
                : SamusAccelerationMode.Accelerating;
    }

    /// <summary>
    /// Applies a normal-jump body selected when aim or Fire cancels a spin, Space Jump,
    /// Screw Attack, or wall-jump pose. This is the shared
    /// <c>$19/$1A/$1B/$1C/$81-$84 -&gt; movement-type-$02</c> route through
    /// <c>$91:F404</c>, <c>$91:F543</c>, and <c>$91:FC66</c>.
    /// </summary>
    /// <remarks>
    /// The transition does not call <c>Make_Samus_Jump</c>: <c>$13/$14</c> are deliberately
    /// excluded by <c>$91:FC66</c>, so the live vertical velocity survives. Spin bodies are
    /// shorter than the resulting normal-jump body, however, so the shared changed-pose
    /// collision pass must still be allowed to reject the expansion under a low ceiling.
    /// Wall-jump tables `$91:A9EC/$AA12` use this very same route for their `$69-$6C` aim
    /// records and `$13/$14` Shot records; they do not have a separate wall-fire handler.
    /// </remarks>
    public static bool TryApplySpinOrWallJumpToNormalJumpTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        SnesButton controllerNewInput,
        ISamusRoomPlms? plms = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = samus.Pose;
        bool normalJumpTarget =
            SamusPoseMetadata.IsRightFacingNormalJumpPose(targetPose) ||
            SamusPoseMetadata.IsLeftFacingNormalJumpPose(targetPose);
        bool compactJumpSource = SamusPoseMetadata.IsSpinJumpPose(sourcePose) || SamusPoseMetadata.IsWallJumpPose(sourcePose);
        if (!compactJumpSource || !normalJumpTarget ||
            SamusPoseMetadata.ReadPoseXDirection(sourcePose) != SamusPoseMetadata.ReadPoseXDirection(targetPose))
        {
            // The six spin/Space/Screw tables and two wall-jump tables emit only same-facing
            // normal-jump records when aim or Shoot cancels rotation. A different movement
            // family is owned by the spin-direction or landing initializers instead.
            throw new InvalidOperationException(
                $"Compact-jump-to-normal-jump transition ${(int)sourcePose:X2} -> " +
                $"${(int)targetPose:X2} is not a same-facing retail route.");
        }

        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.RetainSource)
            return false;
        if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            // $91:FFA7 installs ordinary crouch when the expanded body is blocked above and
            // below. F433 still runs and sees the spin/wall-jump previous movement type.
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            if (samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
                samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
            return true;
        }

        samus.Pose = targetPose;
        // F404/F543 keep the source collision radius for the rest of this update.
        // Alpha's SetRadius publishes the new pose radius on the next update.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));

        // Aim may cancel the spin into a pose that consumes stored shine. Run that
        // branch of the common normal-jump initializer before ordinary acceleration.
        bool beganShinespark = TryBeginShinesparkWindup(samus, 
            targetPose, SamusPoseMetadata.IsWallJumpPose(sourcePose)
                ? SamusMovementType.WallJumping : SamusMovementType.SpinJumping);
        if (!beganShinespark)
            InitializeOrdinaryAerialAcceleration(samus);

        // SamusFunc_F433 reloads the ordinary suit palette whenever the previous movement
        // type was spin/wall-jump and Screw Attack is equipped, even if the visible source
        // happened to be generic spin or Space Jump art.
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();

        if (beganShinespark)
            return true;

        // $91:F5CF publishes the newly installed pose's shot direction on the exact Fire
        // edge that selected this record. The projectile producer consumes it in alpha.
        if ((controllerNewInput & SnesButton.X) != 0)
            samus.PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.NormalJumpPoseHandoffTag | SamusPoseMetadata.ReadShotDirection(samus)));

        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>Applies a normal-jump or falling turn, selecting aim-preserving art through $91:F9D6/$F9E0 after the shared larger-pose collision pass.</summary>
    /// <param name="samus">The Samus this reads.</param>
    /// <param name="level">Live room geometry used to admit the selected turn body's expansion.</param>
    /// <param name="genericTargetPose">Generic right-to-left or left-to-right turn pose for the current jumping/falling family; aim-specific art is selected from the source shot direction, not this argument.</param>
    /// <param name="nmiFrameCounter">Native NMI counter supplied to changed-pose block reactions.</param>
    /// <param name="plms">Optional live PLM owner for those collision reactions.</param>
    /// <returns>True when the aerial turn is installed; false when expansion retains the source or substitutes stable crouch, so failure does not guarantee unchanged state.</returns>
    /// <remarks>An accepted turn folds extra run speed into base turn momentum, preserves vertical velocity, adjusts the whole-pixel Y center if required, and initializes animation while retaining the source collision radius until the next alpha pass.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="level"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source/target movement families or facing metadata do not describe an admitted normal-jump/falling turn.</exception>
    /// <exception cref="InvalidDataException">The source pose's shot-direction metadata is outside zero through nine.</exception>
    public static bool TryApplyAerialTurn(SamusState samus, 

        RoomLevelData level,
        SamusPoseId genericTargetPose,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = samus.Pose;
        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        bool jumping = sourceMovementType == SamusMovementType.NormalJumping;
        bool falling = sourceMovementType == SamusMovementType.Falling;
        bool turnsLeft = genericTargetPose == (jumping
            ? SamusPoseId.TurningRightToLeftJumpPose
            : SamusPoseId.TurningRightToLeftFallingPose);
        bool turnsRight = genericTargetPose == (jumping
            ? SamusPoseId.TurningLeftToRightJumpPose
            : SamusPoseId.TurningLeftToRightFallingPose);
        if ((!jumping && !falling) || (!turnsLeft && !turnsRight))
        {
            throw new InvalidOperationException(
                $"Aerial turn ${(int)sourcePose:X2} -> ${(int)genericTargetPose:X2} is not a verified type-2/type-6 transition.");
        }

        // These are literal copies of `$91:F9D6` and `$91:F9E0`. Directions four and five
        // are not typos: compact down-aim sources reuse `$91/$92` or `$95/$96` according to
        // facing. Retaining all ten entries is essential for a debugger to preserve aim.
        byte shotDirection = SamusPoseMetadata.ReadShotDirection(samus);
        SamusPoseId selectedTurnPose = jumping
            ? shotDirection switch
            {
                0 => SamusPoseId.TurningRightToLeftJumpAimUpPose,
                1 => SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose,
                2 => SamusPoseId.TurningRightToLeftJumpPose,
                3 or 4 => SamusPoseId.TurningRightToLeftJumpAimDownPose,
                5 or 6 => SamusPoseId.TurningLeftToRightJumpAimDownPose,
                7 => SamusPoseId.TurningLeftToRightJumpPose,
                8 => SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose,
                9 => SamusPoseId.TurningLeftToRightJumpAimUpPose,
                _ => throw new InvalidDataException($"Jump pose ${(int)sourcePose:X2} has invalid shot direction ${shotDirection:X2}."),
            }
            : shotDirection switch
            {
                0 => SamusPoseId.TurningRightToLeftFallingAimUpPose,
                1 => SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose,
                2 => SamusPoseId.TurningRightToLeftFallingPose,
                3 or 4 => SamusPoseId.TurningRightToLeftFallingAimDownPose,
                5 or 6 => SamusPoseId.TurningLeftToRightFallingAimDownPose,
                7 => SamusPoseId.TurningLeftToRightFallingPose,
                8 => SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose,
                9 => SamusPoseId.TurningLeftToRightFallingAimUpPose,
                _ => throw new InvalidDataException($"Fall pose ${(int)sourcePose:X2} has invalid shot direction ${shotDirection:X2}."),
            };

        if ((turnsLeft && !SamusPoseMetadata.IsRightToLeftAerialTurnPose(selectedTurnPose)) ||
            (turnsRight && !SamusPoseMetadata.IsLeftToRightAerialTurnPose(selectedTurnPose)))
        {
            throw new InvalidOperationException(
                $"Aerial turn source ${(int)sourcePose:X2} has direction metadata inconsistent with ${(int)genericTargetPose:X2}.");
        }

        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            selectedTurnPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.RetainSource)
            return false;
        if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        // F404 resolves expansion before F433 dispatches the final movement type.
        // A crouch substitution never reaches the aerial-turn momentum initializer.
        SamusGroundedPoseTransitions.FoldExtraRunSpeedIntoBaseAndStartTurn(samus);
        samus.Pose = selectedTurnPose;
        // The changed-pose handler preserves the source radius until next alpha.
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Applies `$91:F624` when spin/wall-jump input selects the opposite spin pose. A true
    /// direction reversal folds extra speed into base speed and selects mode one, preserving
    /// old-world direction while the newly facing pose decelerates.
    /// </summary>
    public static void ApplySpinJumpDirectionTransition(SamusState samus, SamusPoseId targetPose)
    {
        if (!SamusPoseMetadata.IsSpinJumpPose(targetPose) ||
            (!SamusPoseMetadata.IsWallJumpPose(samus.Pose) && !SamusPoseMetadata.IsSpinJumpPose(samus.Pose)))
        {
            throw new InvalidOperationException(
                $"Spin direction transition ${(int)samus.Pose:X2} -> ${(int)targetPose:X2} is not verified.");
        }

        SamusFacingDirection oldDirection = SamusPoseMetadata.ReadFacingDirection(samus);
        SamusFacingDirection newDirection = SamusPoseMetadata.ReadFacingDirection(targetPose);
        if (oldDirection != newDirection)
        {
            SamusGroundedPoseTransitions.FoldExtraRunSpeedIntoBaseAndStartTurn(samus);

            // Unlike bank-$91's grounded/aerial turn initializers, `$91:F624` calls
            // `Samus_CancelSpeedBoost` immediately after folding the extra pair. Waiting
            // for another movement frame would leave `$0B3C` observably stale.
            samus.HorizontalSpeed.CancelRunningMomentum(oldDirection);
        }

        // Ordinary and wall-jump definition fallbacks publish generic `$19/$1A`, whereas
        // the dedicated `$81/$82` and `$1B/$1C` input tables can publish their already-
        // specialized opposite-facing record directly. Reduce either form back to the
        // generic direction before applying `$91:F624`'s live equipment priority. This is
        // observable when both bits are equipped: an `$81 -> $82` ROM record must remain
        // Screw art, while the same directional intent with Screw unequipped becomes Space
        // Jump art instead of trusting stale pose-table equipment state.
        SamusPoseId genericTarget = newDirection == SamusFacingDirection.Left ? SamusPoseId.SpinJumpLeftPose : SamusPoseId.SpinJumpRightPose;
        samus.Pose = SelectEquippedSpinPose(samus, genericTarget, start: SpinPoseStart.SkipFirstFrame);
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // InitializeSpinJump writes frame one, skipping the static first spin frame. This
        // is easy to miss because an ordinary ground jump initializes the pose elsewhere.
        SamusPresentation.InitializeAnimation(samus, initialFrame: 1);
    }


    /// <summary>
    /// Applies `$91:EABE`, solid-collision command five, and `$90:9949` after the block
    /// wall test succeeds. The launch values are read from the cartridge's dry-air tables.
    /// </summary>
    /// <param name="samus">The Samus this reads.</param>
    /// <param name="level">
    /// Live collision geometry supplied by gameplay. Null is reserved for isolated
    /// launch-value/particle diagnostics that explicitly model unobstructed space.
    /// </param>
    /// <param name="nmiFrameCounter">Native NMI counter passed to the changed-pose collision resolver when live room geometry is supplied.</param>
    /// <param name="plms">Optional PLM owner receiving changed-pose block reactions; unused by the unobstructed diagnostic path.</param>
    public static void ApplyWallJumpTrigger(SamusState samus, 

        RoomLevelData? level = null,
        int nmiFrameCounter = 0,
        ISamusRoomPlms? plms = null)
    {

        // Only the type-3 spin mover reaches `$91:EABE`. The cached movement type, not the
        // live pose, gates it: a drained pose stored without InitializeSamusPose_1 keeps
        // spinning and still wall-jumps.
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.SpinJumping)
            throw new InvalidOperationException(
                $"Wall-jump trigger requires cached spin movement, not ${(byte)SamusPoseMetadata.ReadMovementType(samus):X2}.");

        SamusPoseId targetPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose;
        if (level is not null)
        {
            // The accepted wall contact still passes through the ordinary changed-pose
            // collision dispatcher. Expanding a compact spin body beneath a ceiling can
            // move the center away from it, reject the expansion, or select crouch.
            LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
level, targetPose, nmiFrameCounter, plms, out int centerAdjustment);
            if (collision == LargerPoseCollisionOutcome.RetainSource)
                return;
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
            {
                samus.ApplyPoseChangeCollisionCrouchFallback(samus.Pose);
                return;
            }
            samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        }

        // `$91:F433` observes previous movement type three plus equipped Screw Attack and
        // immediately reloads the normal suit palette before `$83/$84` starts. The desktop
        // runtime performs palette writes later in the frame, so preserve that phase split.
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();

        samus.Pose = targetPose;
        // `$91:FDAE` leaves $0B00 at the spin body's radius; alpha's SetSamusRadius takes
        // the wall-jump body only next frame, so this frame's enemy and projectile
        // collision still use the compact spin radius.

        // `$91:F2D3` clears acceleration and the ordinary base-speed pair before
        // `$90:9949` installs the launch. It deliberately does *not* touch the extra-run
        // pair at `$0B42/$0B44` or the running-momentum flag at `$0B3C`: a wall jump made
        // out of a Dash therefore carries that speed into movement type $14.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;

        // SolidVerticalCollision_WallJumpTriggered queues library-three sound five with a
        // six-entry threshold after clearing the base-speed words and before pose setup.
        samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary3Sounds.WallJump, maximumQueued: 6);

        SamusAerialMovement.InitializeWallJump(samus);
        samus.LiquidPhysics.SpawnWallJumpDust(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }


    /// <summary>
    /// Applies the pose/launch half of <c>$9B:C9CE</c> after the grapple wall-grace probe
    /// has accepted a fresh Jump edge. This route intentionally reverses the contact pose:
    /// `$B8` launches through left-facing `$84`, while `$B9` launches through `$83`.
    /// </summary>
    /// <remarks>
    /// The eventual seven-pixel horizontal push remains the ordinary `$FB` animation-command
    /// path already implemented by <see cref="SamusPresentation.AnimateNoFx"/>. This method performs only the
    /// immediate bank-$9B cleanup, prospective-pose command six, and environment-selected
    /// `$90:9949` vertical launch that occur when the wall-jump function runs.
    /// </remarks>
    public static void ApplyGrappleWallJump(SamusState samus)
    {
        if (samus.Pose is not (SamusPoseId.GrappleWallContactLeftPose or SamusPoseId.GrappleWallContactRightPose))
        {
            throw new InvalidOperationException(
                $"Grapple wall jump requires contact pose $B8/$B9, not ${(int)samus.Pose:X2}.");
        }

        // `$9B:C9D5-$C9EB` tests the contact pose's X-direction byte, not its descriptive
        // wall side. Direction eight selects `$84`; direction four selects `$83`.
        samus.Pose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Right) ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // `$9B:C9CE` clears the ordinary base-speed pair, then the normal pose-transition
        // machinery reaches `$90:9949`. Like the non-grapple route, it leaves both the
        // extra-run pair and `$0B3C` intact, so Dash momentum survives this wall launch.
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.HorizontalSpeed.BaseSpeed = default;

        // `$9B:C9CE` uses generic QueueSound: library one, sound seven, maximum fifteen.
        // It also tears down the active beam flare so the wall-jump's charged-contact path
        // cannot inherit charge accumulated before grapple became active.
        samus.LiquidPhysics.QueueMovementSound(SamusGrappleRomData.Sounds.Stop.SoundEffect, SamusGrappleRomData.Sounds.Stop.MaximumQueued);
        samus.ProjectileFlareCounter = 0;

        SamusAerialMovement.InitializeWallJump(samus);
        samus.LiquidPhysics.SpawnWallJumpDust(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }


    /// <summary>
    /// Commits the pose selected by <c>GrappleBeamFunction_Dropped</c> at `$9B:C8C5`.
    /// The caller supplies the exact cartridge-table target and this method runs the shared
    /// block-only pose-expansion collision before clearing grapple fall momentum.
    /// </summary>
    /// <returns>
    /// True when the requested target fits; false when native pose-change collision retains
    /// the source or substitutes stable crouch.
    /// </returns>
    public static bool ApplyGrappleDropTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null,
        GrappleDropSpeed speed = GrappleDropSpeed.Cleared)
    {

        Ensure.NotNull(level);

        bool supportedTarget = targetPose is
            SamusPoseId.FacingRightNormalPose or SamusPoseId.FacingLeftNormalPose or
            SamusPoseId.StandingAimUpRightPose or SamusPoseId.StandingAimUpLeftPose or
            SamusPoseId.StandingAimDiagonalUpRightPose or SamusPoseId.StandingAimDiagonalUpLeftPose or
            SamusPoseId.StandingAimDiagonalDownRightPose or SamusPoseId.StandingAimDiagonalDownLeftPose or
            SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingLeftPose or
            SamusPoseId.CrouchingAimUpRightPose or SamusPoseId.CrouchingAimUpLeftPose or
            SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalUpLeftPose or
            SamusPoseId.CrouchingAimDiagonalDownRightPose or SamusPoseId.CrouchingAimDiagonalDownLeftPose;
        if (!supportedTarget)
        {
            // `$9B:C8C5` chooses from the complete twelve-entry dropped-pose table, whose
            // outputs are exactly the standing/aim/crouch set above. Other releases use
            // separate grapple wall-jump or swing-release handlers, not this API.
            throw new InvalidOperationException(
                $"Grapple drop transition ${(int)samus.Pose:X2} -> ${(int)targetPose:X2} is not a retail dropped-table route.");
        }

        // EnemyDeathAnimation requests this cleanup for a grapple-killed enemy even
        // when the beam never attached. Samus can therefore still be standing, aiming,
        // or jumping. Native C8C5 selects by radius/shot direction in those poses too;
        // only its output domain is restricted, not the source to connected-grapple art.
        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            samus.Pose = targetPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
            if (samus.Pose != samus.PoseHistory.PreviousPose)
                SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

        // `$9B:C95E-$C967` clears the two base-X and two Y-speed words regardless of the
        // pose-collision result. Extra run speed is not touched by this routine; an ordinary
        // connected grapple never creates it, so preserving the words is the literal rule.
        if (speed == GrappleDropSpeed.Cleared)
        {
            samus.HorizontalSpeed.BaseSpeed = default;
            samus.Kinematics.YSpeed = default;
        }
        return collision == LargerPoseCollisionOutcome.Allowed;
    }


    /// <summary>
    /// Applies `$90:9DBF-$90:9DC8` when an early spin frame finds the wall-jump chord.
    /// This does not launch Samus: it rewinds the ordinary spin sequence to frame `$0A`
    /// with a one-tick timer so the visible wall-contact pose reaches eligibility naturally.
    /// </summary>
    /// <remarks>
    /// `$90:9DB2` tests only for the Screw Attack poses. It runs for whatever pose holds the
    /// cached spin-jump movement type, including a drained pose stored without
    /// InitializeSamusPose_1 ($91:E571).
    /// </remarks>
    public static void ApplyWallContactAnimationRewind(SamusState samus)
    {
        samus.AnimationFrameTimer = WrappingCountdown.Start(1);

        // `$90:9D96-$90:9DA6` gives Screw Attack a longer pre-contact animation. Its first
        // eligible wall frame is 27, so contact rewinds to 26. Ordinary spin and Space Jump
        // both use the compact 10 -> 11 handoff.
        samus.AnimationFrame = SamusPoseMetadata.IsScrewAttackPose(samus.Pose)
            ? SamusWallContactAnimationDefinitions.ScrewAttackRewindFrame
            : SamusWallContactAnimationDefinitions.SpinJumpRewindFrame;
    }


    /// <summary>
    /// Applies equipment, liquid and start-sound selection from <c>SamusFunc_F468_SpinJump</c> at
    /// <c>$91:F624</c> to a generic transition-table target `$19/$1A`.
    /// </summary>
    internal static SamusPoseId SelectEquippedSpinPose(SamusState samus, SamusPoseId genericPose, SpinPoseStart start = SpinPoseStart.FirstFrame)
    {
        bool facingLeft = genericPose switch
        {
            SamusPoseId.SpinJumpRightPose => false,
            SamusPoseId.SpinJumpLeftPose => true,
            SamusPoseId.ForwardFacingPowerSuitPose or
                SamusPoseId.ForwardFacingSuitedPose or
                SamusPoseId.FacingRightNormalPose or
                SamusPoseId.FacingLeftNormalPose or
                SamusPoseId.MovingRightNormalPose or
                SamusPoseId.MovingLeftNormalPose or
                SamusPoseId.MovingRightGunExtendedPose or
                SamusPoseId.MovingLeftGunExtendedPose or
                SamusPoseId.UnusedPose65 or
                SamusPoseId.UnusedPose66 or
                SamusPoseId.MoonwalkFacingLeftPose or
                SamusPoseId.MoonwalkFacingRightPose or
                SamusPoseId.MoonwalkAimUpLeftPose or
                SamusPoseId.MoonwalkAimUpRightPose or
                SamusPoseId.MoonwalkAimDownLeftPose or
                SamusPoseId.MoonwalkAimDownRightPose or
                SamusPoseId.MoonwalkTurnJumpLeftPose or
                SamusPoseId.MoonwalkTurnJumpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
                SamusPoseId.ShinesparkWindupRightPose or
                SamusPoseId.ShinesparkWindupLeftPose or
                SamusPoseId.ShinesparkHorizontalRightPose or
                SamusPoseId.ShinesparkHorizontalLeftPose or
                SamusPoseId.ShinesparkVerticalRightPose or
                SamusPoseId.ShinesparkVerticalLeftPose or
                SamusPoseId.ShinesparkDiagonalRightPose or
                SamusPoseId.ShinesparkDiagonalLeftPose or
                SamusPoseId.CrystalFlashRightPose or
                SamusPoseId.CrystalFlashLeftPose or
                SamusPoseId.XrayingStandingRightPose or
                SamusPoseId.XrayingStandingLeftPose or
                SamusPoseId.DeathSequenceRightPose or
                SamusPoseId.DeathSequenceLeftPose or
                SamusPoseId.XrayingCrouchingRightPose or
                SamusPoseId.XrayingCrouchingLeftPose or
                SamusPoseId.UnusedPoseDb or
                SamusPoseId.UnusedPoseDc or
                SamusPoseId.UnusedPoseDd or
                SamusPoseId.UnusedPoseDe or
                SamusPoseId.UnusedPoseDf or
                SamusPoseId.DrainedCrouchingRightPose or
                SamusPoseId.DrainedCrouchingLeftPose or
                SamusPoseId.DrainedStandingRightPose or
                SamusPoseId.DrainedStandingLeftPose or
                SamusPoseId.RanIntoWallRightPose or
                SamusPoseId.RanIntoWallLeftPose or
                SamusPoseId.RanIntoWallAimUpRightPose or
                SamusPoseId.RanIntoWallAimUpLeftPose or
                SamusPoseId.RanIntoWallAimDownRightPose or
                SamusPoseId.RanIntoWallAimDownLeftPose or
                SamusPoseId.TurningRightToLeftPose or
                SamusPoseId.TurningLeftToRightPose or
                SamusPoseId.SpaceJumpRightPose or
                SamusPoseId.SpaceJumpLeftPose or
                SamusPoseId.ScrewAttackRightPose or
                SamusPoseId.ScrewAttackLeftPose or
                SamusPoseId.TurningRightToLeftJumpPose or
                SamusPoseId.TurningLeftToRightJumpPose or
                SamusPoseId.WallJumpRightPose or
                SamusPoseId.WallJumpLeftPose or
                SamusPoseId.GrappleSwingRightPose or
                SamusPoseId.GrappleSwingLeftPose or
                SamusPoseId.GrappleStandingRightPose or
                SamusPoseId.GrappleStandingLeftPose or
                SamusPoseId.GrappleStandingDownRightPose or
                SamusPoseId.GrappleStandingDownLeftPose or
                SamusPoseId.GrappleCrouchingRightPose or
                SamusPoseId.GrappleCrouchingLeftPose or
                SamusPoseId.GrappleCrouchingDownRightPose or
                SamusPoseId.GrappleCrouchingDownLeftPose or
                SamusPoseId.GrappleWallContactLeftPose or
                SamusPoseId.GrappleWallContactRightPose or
                SamusPoseId.DraygonGrabbedNeutralLeftPose or
                SamusPoseId.DraygonGrabbedAimUpLeftPose or
                SamusPoseId.DraygonGrabbedFiringLeftPose or
                SamusPoseId.DraygonGrabbedAimDownLeftPose or
                SamusPoseId.DraygonGrabbedMovingLeftPose or
                SamusPoseId.DraygonGrabbedNeutralRightPose or
                SamusPoseId.DraygonGrabbedAimUpRightPose or
                SamusPoseId.DraygonGrabbedFiringRightPose or
                SamusPoseId.DraygonGrabbedAimDownRightPose or
                SamusPoseId.DraygonGrabbedMovingRightPose or
                SamusPoseId.DamageBoostLeftPose or
                SamusPoseId.DamageBoostRightPose or
                SamusPoseId.KnockbackRightPose or
                SamusPoseId.KnockbackLeftPose or
                SamusPoseId.TurningRightToLeftFallingPose or
                SamusPoseId.TurningLeftToRightFallingPose or
                SamusPoseId.TurningRightToLeftJumpAimUpPose or
                SamusPoseId.TurningLeftToRightJumpAimUpPose or
                SamusPoseId.TurningRightToLeftJumpAimDownPose or
                SamusPoseId.TurningLeftToRightJumpAimDownPose or
                SamusPoseId.TurningRightToLeftFallingAimUpPose or
                SamusPoseId.TurningLeftToRightFallingAimUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDownPose or
                SamusPoseId.TurningLeftToRightFallingAimDownPose or
                SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
                SamusPoseId.MorphBallGroundRightPose or
                SamusPoseId.MorphBallMovingRightPose or
                SamusPoseId.MorphBallMovingLeftPose or
                SamusPoseId.MorphBallFallingRightPose or
                SamusPoseId.MorphBallFallingLeftPose or
                SamusPoseId.UnusedKnockbackRightPose or
                SamusPoseId.UnusedKnockbackLeftPose or
                SamusPoseId.MorphingTransitionRightPose or
                SamusPoseId.MorphingTransitionLeftPose or
                SamusPoseId.UnmorphingTransitionRightPose or
                SamusPoseId.UnmorphingTransitionLeftPose or
                SamusPoseId.MorphBallGroundLeftPose or
                SamusPoseId.SpringBallGroundRightPose or
                SamusPoseId.SpringBallGroundLeftPose or
                SamusPoseId.SpringBallMovingRightPose or
                SamusPoseId.SpringBallMovingLeftPose or
                SamusPoseId.SpringBallFallingRightPose or
                SamusPoseId.SpringBallFallingLeftPose or
                SamusPoseId.SpringBallJumpRightPose or
                SamusPoseId.SpringBallJumpLeftPose or
                SamusPoseId.FallingRightPose or
                SamusPoseId.FallingLeftPose or
                SamusPoseId.FallingGunExtendedRightPose or
                SamusPoseId.FallingGunExtendedLeftPose or
                SamusPoseId.StandingAimUpRightPose or
                SamusPoseId.StandingAimUpLeftPose or
                SamusPoseId.StandingAimDiagonalUpRightPose or
                SamusPoseId.StandingAimDiagonalUpLeftPose or
                SamusPoseId.StandingAimDiagonalDownRightPose or
                SamusPoseId.StandingAimDiagonalDownLeftPose or
                SamusPoseId.RunningAimUpRightPose or
                SamusPoseId.RunningAimUpLeftPose or
                SamusPoseId.RunningAimDiagonalUpRightPose or
                SamusPoseId.RunningAimDiagonalUpLeftPose or
                SamusPoseId.RunningAimDiagonalDownRightPose or
                SamusPoseId.RunningAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpGunExtendedRightPose or
                SamusPoseId.NormalJumpGunExtendedLeftPose or
                SamusPoseId.NormalJumpAimUpRightPose or
                SamusPoseId.NormalJumpAimUpLeftPose or
                SamusPoseId.NormalJumpAimDownRightPose or
                SamusPoseId.NormalJumpAimDownLeftPose or
                SamusPoseId.NormalJumpForwardRightPose or
                SamusPoseId.NormalJumpForwardLeftPose or
                SamusPoseId.NormalJumpTransitionAimUpRightPose or
                SamusPoseId.NormalJumpTransitionAimUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
                SamusPoseId.FallingAimUpRightPose or
                SamusPoseId.FallingAimUpLeftPose or
                SamusPoseId.FallingAimDownRightPose or
                SamusPoseId.FallingAimDownLeftPose or
                SamusPoseId.FallingAimDiagonalUpRightPose or
                SamusPoseId.FallingAimDiagonalUpLeftPose or
                SamusPoseId.FallingAimDiagonalDownRightPose or
                SamusPoseId.FallingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimDiagonalUpRightPose or
                SamusPoseId.CrouchingAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingAimDiagonalDownRightPose or
                SamusPoseId.CrouchingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimUpRightPose or
                SamusPoseId.CrouchingAimUpLeftPose or
                SamusPoseId.TurningRightToLeftCrouchingPose or
                SamusPoseId.TurningLeftToRightCrouchingPose or
                SamusPoseId.TurningRightToLeftAimUpPose or
                SamusPoseId.TurningLeftToRightAimUpPose or
                SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
                SamusPoseId.LandingAimUpRightPose or
                SamusPoseId.LandingAimUpLeftPose or
                SamusPoseId.LandingAimDiagonalUpRightPose or
                SamusPoseId.LandingAimDiagonalUpLeftPose or
                SamusPoseId.LandingAimDiagonalDownRightPose or
                SamusPoseId.LandingAimDiagonalDownLeftPose or
                SamusPoseId.FiringLandingRightPose or
                SamusPoseId.FiringLandingLeftPose or
                SamusPoseId.CrouchingTransitionAimUpRightPose or
                SamusPoseId.CrouchingTransitionAimUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.StandingTransitionAimUpRightPose or
                SamusPoseId.StandingTransitionAimUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalDownRightPose or
                SamusPoseId.StandingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingRightPose or
                SamusPoseId.CrouchingLeftPose or
                SamusPoseId.CrouchingTransitionRightPose or
                SamusPoseId.CrouchingTransitionLeftPose or
                SamusPoseId.StandingTransitionRightPose or
                SamusPoseId.StandingTransitionLeftPose or
                SamusPoseId.NeutralJumpTransitionRightPose or
                SamusPoseId.NeutralJumpTransitionLeftPose or
                SamusPoseId.NeutralJumpRightPose or
                SamusPoseId.NeutralJumpLeftPose or
                SamusPoseId.NormalLandingRightPose or
                SamusPoseId.NormalLandingLeftPose or
                SamusPoseId.SpinLandingRightPose or
                SamusPoseId.SpinLandingLeftPose or
                SamusPoseId.UnusedPose20 or
                SamusPoseId.UnusedPose21 or
                SamusPoseId.UnusedPose22 or
                SamusPoseId.UnusedPose23 or
                SamusPoseId.UnusedPose24 or
                SamusPoseId.UnusedPose39 or
                SamusPoseId.UnusedPose3A or
                SamusPoseId.UnusedPose3F or
                SamusPoseId.UnusedPose40 or
                SamusPoseId.UnusedPose42 or
                SamusPoseId.UnusedPose45 or
                SamusPoseId.UnusedPose46 or
                SamusPoseId.UnusedPose47 or
                SamusPoseId.UnusedPose48 or
                SamusPoseId.UnusedPose5B or
                SamusPoseId.UnusedPose5C or
                SamusPoseId.UnusedPose5D or
                SamusPoseId.UnusedPose5E or
                SamusPoseId.UnusedPose5F or
                SamusPoseId.UnusedPose60 or
                SamusPoseId.UnusedPose61 or
                SamusPoseId.UnusedPose62 or
                SamusPoseId.UnusedPose63 or
                SamusPoseId.UnusedPose64 or
                SamusPoseId.UnusedPoseAC or
                SamusPoseId.UnusedPoseAD or
                SamusPoseId.UnusedPoseAE or
                SamusPoseId.UnusedPoseAF or
                SamusPoseId.UnusedPoseB0 or
                SamusPoseId.UnusedPoseB1 or
                SamusPoseId.UnusedPoseC5 or
                SamusPoseId.UnusedPoseC6 => throw new ArgumentOutOfRangeException(
                nameof(genericPose),
                genericPose,
                "Equipment spin selection requires generic pose $19 or $1A."),
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => throw new ArgumentOutOfRangeException(
                nameof(genericPose),
                genericPose,
                "Equipment spin selection requires generic pose $19 or $1A."),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };

        // Full submersion returns before either equipment substitution or sound dispatch.
        // Gravity bypasses this test; water's disabled-physics option is handled by the
        // shared top-boundary query, including the native water-before-lava precedence.
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) && samus.LiquidPhysics.IsTopBoundarySubmerged(samus))
            return genericPose;

        // Native tests Screw Attack first. A save with both bits equipped therefore uses
        // `$81/$82`, not Space Jump art, while retaining Space Jump's repeat-jump physics.
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
        {
            if (start != SpinPoseStart.SkipFirstFrame)
                samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary1Sounds.ScrewAttack, maximumQueued: 6);
            return facingLeft ? SamusPoseId.ScrewAttackLeftPose : SamusPoseId.ScrewAttackRightPose;
        }
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump))
        {
            // Unlike ordinary/Screw Attack spins, native also queues this on reversal.
            samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary1Sounds.SpaceJump, maximumQueued: 6);
            return facingLeft ? SamusPoseId.SpaceJumpLeftPose : SamusPoseId.SpaceJumpRightPose;
        }
        if (start != SpinPoseStart.SkipFirstFrame && !samus.LiquidPhysics.CinematicFunctionActive)
            samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary1Sounds.SpinJump, maximumQueued: 6);
        return genericPose;
    }


    /// <summary>
    /// Applies <c>SamusFunc_F468_NormalJump</c> at <c>$91:F543</c> after a normal-jump
    /// transition has selected one of the six cartridge-approved launch bodies.
    /// </summary>
    /// <remarks>
    /// This deliberately receives the already-selected target instead of testing the
    /// transition art `$4B/$4C/$55-$5A`. Native does not consume stored shine when Jump is
    /// first pressed; it waits until command `$F8/$FD` installs `$4D/$4E/$15/$16/$69/$6A`.
    /// That delay is visible both to animation and to the 180-frame palette countdown.
    /// </remarks>
    internal static bool TryBeginShinesparkWindup(SamusState samus, 
        SamusPoseId targetPose,
        SamusMovementType previousMovementType)
    {
        bool right = targetPose is
            SamusPoseId.NeutralJumpRightPose or SamusPoseId.NormalJumpAimUpRightPose or
            SamusPoseId.NormalJumpAimDiagonalUpRightPose;
        bool left = targetPose is
            SamusPoseId.NeutralJumpLeftPose or SamusPoseId.NormalJumpAimUpLeftPose or
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose;
        if ((!right && !left) || SamusInputSnapshot.SharedShineTimer(samus) == 0)
        {
            return false;
        }

        samus.Pose = right ? SamusPoseId.ShinesparkWindupRightPose : SamusPoseId.ShinesparkWindupLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.Shinespark.BeginWindup(samus);

        // `$91:F580-$F595` checks the PREVIOUS movement type, lifts Samus one pixel and
        // stores the result in both Y words, so this frame's vertical scroll sees no motion.
        if (previousMovementType == SamusMovementType.NormalJumping)
        {
            samus.Kinematics.Position -= new Offset16<RoomPixels>(0, 1);
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
        }

        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Applies a `$C7/$C8 -> $C9-$CE` input-table match and installs its exact special
    /// movement handler. No ordinary jump velocity or grounded momentum routine runs.
    /// </summary>
    public static void ApplyShinesparkDirectionTransition(SamusState samus, SamusPoseId targetPose)
    {
        if (samus.Pose is not (SamusPoseId.ShinesparkWindupRightPose or SamusPoseId.ShinesparkWindupLeftPose))
        {
            throw new InvalidOperationException(
                $"Directional shinespark transition requires windup pose, not ${(int)samus.Pose:X2}.");
        }

        samus.Shinespark.BeginDirectionalLaunch(samus, targetPose);
    }


    /// <summary>
    /// Applies the verified ordinary-input jump transitions selected from the cartridge's
    /// bank-$91 table, including <c>HandleJumpTransition</c>'s call to
    /// <c>Make_Samus_Jump</c>. Only explicitly verified no-equipment/no-aim routes admitted
    /// by the current runtime are accepted, including interrupting any same-facing landing
    /// stream `$A4-$A7/$E0-$E7` with a fresh Jump edge.
    /// </summary>
    public static void ApplyOrdinaryJumpTransition(SamusState samus, 
        SamusPoseId targetPose,
        SnesButton controllerNewInput = SnesButton.None)
    {
        bool verified =
            SamusPoseMetadata.IsStandingGroundTurnToJumpTransition(samus.Pose, targetPose) ||
            new SamusPoseChange(samus.Pose, targetPose) is
            (SamusPoseId.FacingRightNormalPose or SamusPoseId.StandingAimUpRightPose or
                SamusPoseId.StandingAimDiagonalUpRightPose or SamusPoseId.StandingAimDiagonalDownRightPose,
             SamusPoseId.NeutralJumpTransitionRightPose) or
            (SamusPoseId.FacingLeftNormalPose or SamusPoseId.StandingAimUpLeftPose or
                SamusPoseId.StandingAimDiagonalUpLeftPose or SamusPoseId.StandingAimDiagonalDownLeftPose,
             SamusPoseId.NeutralJumpTransitionLeftPose) or
            (SamusPoseId.FacingRightNormalPose or SamusPoseId.StandingAimUpRightPose or
                SamusPoseId.StandingAimDiagonalUpRightPose or SamusPoseId.StandingAimDiagonalDownRightPose,
             SamusPoseId.NormalJumpTransitionAimUpRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose) or
            (SamusPoseId.FacingLeftNormalPose or SamusPoseId.StandingAimUpLeftPose or
                SamusPoseId.StandingAimDiagonalUpLeftPose or SamusPoseId.StandingAimDiagonalDownLeftPose,
             SamusPoseId.NormalJumpTransitionAimUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose) or
            (SamusPoseId.MovingRightNormalPose or SamusPoseId.MovingRightGunExtendedPose or SamusPoseId.RunningAimUpRightPose or
                SamusPoseId.RunningAimDiagonalUpRightPose or SamusPoseId.RunningAimDiagonalDownRightPose,
             SamusPoseId.SpinJumpRightPose) or
            (SamusPoseId.MovingLeftNormalPose or SamusPoseId.MovingLeftGunExtendedPose or SamusPoseId.RunningAimUpLeftPose or
                SamusPoseId.RunningAimDiagonalUpLeftPose or SamusPoseId.RunningAimDiagonalDownLeftPose,
             SamusPoseId.SpinJumpLeftPose) or
            // Turning-on-ground uses its own movement-type-$0E input handler, but a Jump
            // record still enters the ordinary spin-jump initializer. The turn pose has
            // already folded run momentum in `$91:F8D3`; `$91:F624` now owns the same jump
            // speed, radius, equipment substitution, and frame-zero setup as a run jump.
            // `$91:8142` keeps consulting the turning pose's live input table. A fresh
            // Jump with no horizontal direction selects neutral-transition `$4B/$4C`;
            // holding the completed facing selects spin `$19/$1A`. Both pairs enter the
            // same jump initializer and differ only in their cartridge-authored body.
            // `$91:AF98-$AFFF` can leave the moonwalk turn art early while the backward
            // direction remains held, or select `$4B/$4C` on a fresh Jump edge. Both
            // routes call the same dry-air jump initializer after changing pose.
            (SamusPoseId.MoonwalkTurnJumpLeftPose or SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimDownLeftPose,
             SamusPoseId.SpinJumpLeftPose or SamusPoseId.NeutralJumpTransitionLeftPose) or
            (SamusPoseId.MoonwalkTurnJumpRightPose or SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimDownRightPose,
             SamusPoseId.SpinJumpRightPose or SamusPoseId.NeutralJumpTransitionRightPose) or
            (SamusPoseId.RanIntoWallRightPose or SamusPoseId.RanIntoWallAimUpRightPose or SamusPoseId.RanIntoWallAimDownRightPose,
             SamusPoseId.NeutralJumpTransitionRightPose) or
            (SamusPoseId.RanIntoWallLeftPose or SamusPoseId.RanIntoWallAimUpLeftPose or SamusPoseId.RanIntoWallAimDownLeftPose,
             SamusPoseId.NeutralJumpTransitionLeftPose) ||
            // Every landing body shares its facing's ordinary standing input table. The
            // centralized predicate deliberately covers normal, spin, aimed, and firing
            // landings, while still rejecting a corrupt cross-facing `$4B/$4C` target.
            SamusPoseMetadata.IsLandingToNormalJumpTransition(samus.Pose, targetPose);
        if (!verified)
        {
            // The pairs above exhaust the active retail input-table records that invoke
            // the ordinary jump initializer. Movement-specific launches (damage boost,
            // grapple, bomb jump, wall jump, and Shinespark) own separate entry points.
            throw new InvalidOperationException(
                $"Ordinary jump transition ${(int)samus.Pose:X2} -> ${(int)targetPose:X2} is not an active retail route for this initializer.");
        }

        samus.Pose = targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose
            ? SelectEquippedSpinPose(samus, targetPose)
            : targetPose;
        // Pose dispatch happens after movement. The next alpha phase, including
        // the intro-demo alpha handler, owns publication of the new live radius.
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        SamusAerialMovement.InitializeJump(samus);

        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.NormalJumping &&
            (controllerNewInput & SnesButton.X) != 0)
        {
            // `$91:F5CF-$F5E6` runs only in the normal-jumping initializer. It reads the
            // newly installed pose's direction byte and adds `$8000`; spin-jump's separate
            // initializer never publishes this bridge even when Shoot and Jump share a frame.
            samus.PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.NormalJumpPoseHandoffTag | SamusPoseMetadata.ReadShotDirection(samus)));
        }
    }


    /// <summary>
    /// Ports `$90:EB20`, the unconditional current-state epilogue clear that follows the
    /// HUD/projectile handler. A failed cooldown/slot allocation must lose the bridge too;
    /// it is deliberately not retained until some later shot succeeds.
    /// </summary>
    public static void ClearPoseTransitionShotDirection(SamusState samus) => samus.PoseTransitionShotDirection = 0;


    /// <summary>
    /// <c>InitializeSamusPose_NormalJumping</c> ($91:F543) for a normal-jump pose already
    /// installed by a transition that reaches the ordinary initializer ($91:F433). Stored shine
    /// diverts the six shinespark-capable poses into a windup; otherwise retained extra run
    /// speed selects deceleration and a fresh Shoot press arms the transition shot.
    /// </summary>
    /// <returns>True when the initializer began a shinespark windup instead.</returns>
    internal static bool InitializeInstalledNormalJumpPose(SamusState samus, 
        SamusMovementType previousMovementType,
        SnesButton controllerNewInput)
    {
        if (TryBeginShinesparkWindup(samus, samus.Pose, previousMovementType))
            return true;
        InitializeOrdinaryAerialAcceleration(samus);
        PublishNormalJumpPoseShotDirection(samus, controllerNewInput);
        return false;
    }


    /// <summary>Publishes $91:F5CF-F5E6 after an accepted normal-jump pose initializer.</summary>
    internal static void PublishNormalJumpPoseShotDirection(SamusState samus, SnesButton controllerNewInput)
    {
        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.NormalJumping &&
            (controllerNewInput & SnesButton.X) != 0)
            samus.PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.NormalJumpPoseHandoffTag | SamusPoseMetadata.ReadShotDirection(samus)));
    }

    /// <summary>
    /// Applies a same-facing input/fallback transition within normal-jump type two or
    /// falling type six without replacing the live 16.16 velocity words. The horizontal
    /// gun-extended records `$13/$14/$67/$68` use this same native initialization seam;
    /// “aim” survives in the historical method name only because that was the first slice.
    /// </summary>
    public static void ApplyAerialAimTransition(SamusState samus, SamusPoseId targetPose)
    {
        if (SamusPoseMetadata.IsCompactAerialPose(samus.Pose) || SamusPoseMetadata.IsCompactAerialPose(targetPose))
        {
            // Compact poses change Samus's collision height. This overload intentionally
            // has no room data, so accepting the request would bypass the native expansion
            // collision probe; callers must select the collision-aware API explicitly.
            throw new InvalidOperationException(
                "Radius-ten aerial transitions require TryApplyCompactAerialTransition and active room collision data.");
        }
        if (!SamusPoseMetadata.IsSameFacingAerialAimFireOrForwardTransition(samus.Pose, targetPose))
        {
            // This helper models one same-facing ROM family, not the general pose dispatcher.
            // A cross-family request therefore violates the method contract rather than
            // identifying an untranslated member of the admitted family.
            throw new InvalidOperationException(
                $"Aerial aim/fire/forward transition ${(int)samus.Pose:X2} -> ${(int)targetPose:X2} is outside the same-facing family handled by this operation.");
        }

        SamusPoseId sourcePose = samus.Pose;
        SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(samus);
        ushort oldRadius = samus.Kinematics.Radius.Y;
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (samus.Kinematics.Radius.Y != oldRadius)
        {
            // The admitted family deliberately excludes compact straight-down `$17/$18`
            // and `$2D/$2E`; reaching a different radius proves a caller crossed that seam.
            // Every retail member admitted above has the same collision radius. A mismatch
            // can only come from inconsistent/corrupt pose metadata, not an omitted route.
            throw new InvalidDataException(
                $"Aerial pose ${(int)targetPose:X2} unexpectedly changes radius {oldRadius} -> {samus.Kinematics.Radius.Y} inside an equal-radius transition family.");
        }
        // Both normal-jump and falling targets run their movement initializer.
        // Preserve base velocity, but derive acceleration mode from extra dash speed.
        if (sourcePose != samus.Pose)
        {
            // The normal-jump initializer also consumes stored shine on eligible poses.
            // This includes releasing forward midair, not just the initial ground jump.
            if (TryBeginShinesparkWindup(samus, targetPose, previousMovementType))
                return;
            InitializeOrdinaryAerialAcceleration(samus);
        }
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
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
    public static bool TryApplyCompactAerialTransition(SamusState samus, 

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {

        Ensure.NotNull(level);

        bool rightJump = SamusPoseMetadata.IsRightFacingNormalJumpPose(samus.Pose) &&
            SamusPoseMetadata.IsRightFacingNormalJumpPose(targetPose);
        bool leftJump = SamusPoseMetadata.IsLeftFacingNormalJumpPose(samus.Pose) &&
            SamusPoseMetadata.IsLeftFacingNormalJumpPose(targetPose);
        bool rightFall = SamusPoseMetadata.IsRightFacingFallingPose(samus.Pose) &&
            SamusPoseMetadata.IsRightFacingFallingPose(targetPose);
        bool leftFall = SamusPoseMetadata.IsLeftFacingFallingPose(samus.Pose) &&
            SamusPoseMetadata.IsLeftFacingFallingPose(targetPose);
        if ((!rightJump && !leftJump && !rightFall && !leftFall) ||
            (!SamusPoseMetadata.IsCompactAerialPose(samus.Pose) && !SamusPoseMetadata.IsCompactAerialPose(targetPose)))
        {
            // This is the exact precondition of the collision-aware compact-pose helper.
            // Other aerial changes use ApplyAerialAimTransition or a movement-specific API.
            throw new InvalidOperationException(
                $"Compact aerial transition ${(int)samus.Pose:X2} -> ${(int)targetPose:X2} is not a same-family ROM route.");
        }

        SamusPoseId sourcePose = samus.Pose;
        LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        samus.Pose = targetPose;
        samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        if (sourcePose != samus.Pose)
        {
            // Pose collision uses prospective geometry; the live radius belongs to
            // the current frame's alpha and changes only at the next alpha.
            // Successful expansion still runs the normal-jump initializer. A stored
            // shine may replace the requested aim pose; collision rejection above must
            // not consume the charge or apply the windup's one-pixel adjustment.
            if (TryBeginShinesparkWindup(samus, targetPose,
                rightJump || leftJump ? SamusMovementType.NormalJumping : SamusMovementType.Falling))
                return true;
            InitializeOrdinaryAerialAcceleration(samus);
        }
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        return true;
    }


    /// <summary>
    /// Applies falling input-lookup failure's command eight ($91:EC8E). Cancel the
    /// dash component after movement, preserving the base speed and acceleration mode.
    /// The caller admits only a still-falling body, so no new-fall Y reset is needed.
    /// </summary>
    public static void ApplyFallingInputFallback(SamusState samus)
    {
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Falling)
            throw new InvalidOperationException("Falling input fallback requires a falling pose.");
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        samus.HorizontalSpeed.ExtraRunSpeed = default;
    }
}
