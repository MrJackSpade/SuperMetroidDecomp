using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's pose transitions: grounded, aerial, posture and morph-ball changes, turn and wall-jump handling, and the collision checks a larger new pose must pass, applied to the Samus state it owns.</summary>
internal sealed class SamusPoseTransitions
{
    /// <summary>The live Samus state whose pose, speed, and collision radius transitions are applied.</summary>
    private readonly SamusState _samus;

    /// <summary>Binds pose transitions to one Samus instance so failed transitions preserve its current state.</summary>
    internal SamusPoseTransitions(SamusState owner)
    {
        _samus = owner;
    }
    /// <summary>
    /// Shared momentum initialization in SamusFunc_F468_NormalJump ($91:F543) and
    /// SamusFunc_F468_Falling ($91:F60D). Base velocity survives; extra dash momentum
    /// alone selects deceleration mode two instead of ordinary acceleration.
    /// </summary>
    private void InitializeOrdinaryAerialAcceleration()
    {
        _samus.HorizontalSpeed.AccelerationMode =
            _samus.HorizontalSpeed.ExtraRunSpeed != 0 || _samus.HorizontalSpeed.ExtraRunSubspeed != 0
                ? SamusHorizontalAccelerationModes.Decelerating
                : SamusHorizontalAccelerationModes.Accelerating;
    }

    /// <summary>
    /// Installs the lookup-failure pose of an aimed crouching turn when every input is
    /// released ($91:81DB -> $91:82D9). Definition byte two is `$28` for all six records,
    /// so the turn is abandoned for left-facing crouch art regardless of its direction.
    /// Because the pose changes, $91:EBEE skips command two; only $91:F404 runs.
    /// </summary>
    public void ApplyAimedCrouchingTurnInputFallback(SamusPoseId targetPose)
    {
        if (!SamusPoseMetadata.IsAimedCrouchingTurnPose(_samus.Pose))
            throw new InvalidOperationException($"Aimed crouching turn fallback requires a type-$17 crouching turn, not ${(int)_samus.Pose:X2}.");
        if (!SamusPoseMetadata.IsRightFacingCrouchingPose(targetPose) && !SamusPoseMetadata.IsLeftFacingCrouchingPose(targetPose))
            throw new InvalidDataException($"Aimed crouching turn ${(int)_samus.Pose:X2} names non-crouching fallback ${(int)targetPose:X2}.");
        ApplySimpleGroundedPoseChange(_samus.Pose, targetPose, "Aimed crouching turn release");
    }

    /// <summary>
    /// Applies prospective pose change command two ($91:ECD0) when a turn's input lookup
    /// fails and the pose definition retains the current pose. Movement types $0E, $17 and
    /// $18 all select command two in $91:8332. This stops reverse acceleration without
    /// restarting the unfinished turn animation or erasing velocity words.
    /// </summary>
    public void ApplyTurnInputFallback(SamusMovementType movementType)
    {
        if (movementType is not (SamusMovementType.TurningOnGround or
            SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling))
            throw new InvalidOperationException($"Turn fallback requires a turning movement type, not ${(byte)movementType:X2}.");
        _samus.HorizontalSpeed.AccelerationMode = SamusHorizontalAccelerationModes.Accelerating;
        _samus.HorizontalSpeed.CancelRunningMomentum(_samus.PoseMetadata.ReadFacingDirection());
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
    public bool TryApplySpinOrWallJumpToNormalJumpTransition(

        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        SnesButton controllerNewInput,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = _samus.Pose;
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

        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

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
            _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            if (_samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
                _samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
            return true;
        }

        _samus.Pose = targetPose;
        // F404/F543 keep the source collision radius for the rest of this update.
        // Alpha's SetRadius publishes the new pose radius on the next update.
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));

        // Aim may cancel the spin into a pose that consumes stored shine. Run that
        // branch of the common normal-jump initializer before ordinary acceleration.
        bool beganShinespark = TryBeginShinesparkWindup(
            targetPose, SamusPoseMetadata.IsWallJumpPose(sourcePose)
                ? SamusMovementType.WallJumping : SamusMovementType.SpinJumping);
        if (!beganShinespark)
            InitializeOrdinaryAerialAcceleration();

        // SamusFunc_F433 reloads the ordinary suit palette whenever the previous movement
        // type was spin/wall-jump and Screw Attack is equipped, even if the visible source
        // happened to be generic spin or Space Jump art.
        if (_samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            _samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();

        if (beganShinespark)
            return true;

        // $91:F5CF publishes the newly installed pose's shot direction on the exact Fire
        // edge that selected this record. The projectile producer consumes it in alpha.
        if ((controllerNewInput & SnesButton.X) != 0)
            _samus.PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.NormalJumpPoseHandoffTag | _samus.PoseMetadata.ReadShotDirection()));

        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        return true;
    }

    /// <summary>Applies a normal-jump or falling turn, selecting aim-preserving art through $91:F9D6/$F9E0 after the shared larger-pose collision pass.</summary>
    /// <param name="level">Live room geometry used to admit the selected turn body's expansion.</param>
    /// <param name="genericTargetPose">Generic right-to-left or left-to-right turn pose for the current jumping/falling family; aim-specific art is selected from the source shot direction, not this argument.</param>
    /// <param name="nmiFrameCounter">Native NMI counter supplied to changed-pose block reactions.</param>
    /// <param name="plms">Optional live PLM owner for those collision reactions.</param>
    /// <returns>True when the aerial turn is installed; false when expansion retains the source or substitutes stable crouch, so failure does not guarantee unchanged state.</returns>
    /// <remarks>An accepted turn folds extra run speed into base turn momentum, preserves vertical velocity, adjusts the whole-pixel Y center if required, and initializes animation while retaining the source collision radius until the next alpha pass.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="level"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source/target movement families or facing metadata do not describe an admitted normal-jump/falling turn.</exception>
    /// <exception cref="InvalidDataException">The source pose's shot-direction metadata is outside zero through nine.</exception>
    public bool TryApplyAerialTurn(

        RoomLevelData level,
        SamusPoseId genericTargetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = _samus.Pose;
        SamusMovementType sourceMovementType = _samus.PoseMetadata.ReadMovementType();
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
        byte shotDirection = _samus.PoseMetadata.ReadShotDirection();
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

        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

            level,
            selectedTurnPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.RetainSource)
            return false;
        if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        // F404 resolves expansion before F433 dispatches the final movement type.
        // A crouch substitution never reaches the aerial-turn momentum initializer.
        FoldExtraRunSpeedIntoBaseAndStartTurn();
        _samus.Pose = selectedTurnPose;
        // The changed-pose handler preserves the source radius until next alpha.
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies `$91:F624` when spin/wall-jump input selects the opposite spin pose. A true
    /// direction reversal folds extra speed into base speed and selects mode one, preserving
    /// old-world direction while the newly facing pose decelerates.
    /// </summary>
    public void ApplySpinJumpDirectionTransition(SamusPoseId targetPose)
    {
        if (!SamusPoseMetadata.IsSpinJumpPose(targetPose) ||
            (!SamusPoseMetadata.IsWallJumpPose(_samus.Pose) && !SamusPoseMetadata.IsSpinJumpPose(_samus.Pose)))
        {
            throw new InvalidOperationException(
                $"Spin direction transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not verified.");
        }

        SamusFacingDirection oldDirection = _samus.PoseMetadata.ReadFacingDirection();
        SamusFacingDirection newDirection = SamusPoseMetadata.ReadFacingDirection(targetPose);
        if (oldDirection != newDirection)
        {
            FoldExtraRunSpeedIntoBaseAndStartTurn();

            // Unlike bank-$91's grounded/aerial turn initializers, `$91:F624` calls
            // `Samus_CancelSpeedBoost` immediately after folding the extra pair. Waiting
            // for another movement frame would leave `$0B3C` observably stale.
            _samus.HorizontalSpeed.CancelRunningMomentum(oldDirection);
        }

        // Ordinary and wall-jump definition fallbacks publish generic `$19/$1A`, whereas
        // the dedicated `$81/$82` and `$1B/$1C` input tables can publish their already-
        // specialized opposite-facing record directly. Reduce either form back to the
        // generic direction before applying `$91:F624`'s live equipment priority. This is
        // observable when both bits are equipped: an `$81 -> $82` ROM record must remain
        // Screw art, while the same directional intent with Screw unequipped becomes Space
        // Jump art instead of trusting stale pose-table equipment state.
        SamusPoseId genericTarget = newDirection == SamusFacingDirection.Left ? SamusPoseId.SpinJumpLeftPose : SamusPoseId.SpinJumpRightPose;
        _samus.Pose = SelectEquippedSpinPose(genericTarget, skipFirstAnimationFrame: true);
        _samus.PoseMetadata.RefreshCollisionRadii();

        // InitializeSpinJump writes frame one, skipping the static first spin frame. This
        // is easy to miss because an ordinary ground jump initializes the pose elsewhere.
        _samus.Presentation.InitializeAnimation(initialFrame: 1);
    }

    /// <summary>
    /// Applies `$91:EABE`, solid-collision command five, and `$90:9949` after the block
    /// wall test succeeds. The launch values are read from the cartridge's dry-air tables.
    /// </summary>
    /// <param name="level">
    /// Live collision geometry supplied by gameplay. Null is reserved for isolated
    /// launch-value/particle diagnostics that explicitly model unobstructed space.
    /// </param>
    /// <param name="nmiFrameCounter">Native NMI counter passed to the changed-pose collision resolver when live room geometry is supplied.</param>
    /// <param name="plms">Optional PLM owner receiving changed-pose block reactions; unused by the unobstructed diagnostic path.</param>
    public void ApplyWallJumpTrigger(

        RoomLevelData? level = null,
        ushort nmiFrameCounter = 0,
        RoomPlmSystem? plms = null)
    {

        // Only the type-3 spin mover reaches `$91:EABE`. The cached movement type, not the
        // live pose, gates it: a drained pose stored without InitializeSamusPose_1 keeps
        // spinning and still wall-jumps.
        if (_samus.PoseMetadata.ReadMovementType() != SamusMovementType.SpinJumping)
            throw new InvalidOperationException(
                $"Wall-jump trigger requires cached spin movement, not ${(byte)_samus.PoseMetadata.ReadMovementType():X2}.");

        SamusPoseId targetPose = _samus.PoseMetadata.IsFacingLeft() ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose;
        if (level is not null)
        {
            // The accepted wall contact still passes through the ordinary changed-pose
            // collision dispatcher. Expanding a compact spin body beneath a ceiling can
            // move the center away from it, reject the expansion, or select crouch.
            LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(
level, targetPose, nmiFrameCounter, plms, out int centerAdjustment);
            if (collision == LargerPoseCollisionOutcome.RetainSource)
                return;
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
            {
                _samus.ApplyPoseChangeCollisionCrouchFallback(_samus.Pose);
                return;
            }
            _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        }

        // `$91:F433` observes previous movement type three plus equipped Screw Attack and
        // immediately reloads the normal suit palette before `$83/$84` starts. The desktop
        // runtime performs palette writes later in the frame, so preserve that phase split.
        if (_samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            _samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();

        _samus.Pose = targetPose;
        // `$91:FDAE` leaves $0B00 at the spin body's radius; alpha's SetSamusRadius takes
        // the wall-jump body only next frame, so this frame's enemy and projectile
        // collision still use the compact spin radius.

        // `$91:F2D3` clears acceleration and the ordinary base-speed pair before
        // `$90:9949` installs the launch. It deliberately does *not* touch the extra-run
        // pair at `$0B42/$0B44` or the running-momentum flag at `$0B3C`: a wall jump made
        // out of a Dash therefore carries that speed into movement type $14.
        _samus.HorizontalSpeed.AccelerationMode = 0;
        _samus.HorizontalSpeed.BaseSpeed = 0;
        _samus.HorizontalSpeed.BaseSubspeed = 0;

        // SolidVerticalCollision_WallJumpTriggered queues library-three sound five with a
        // six-entry threshold after clearing the base-speed words and before pose setup.
        _samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary3Sounds.WallJump, maximumQueued: 6);

        SamusAerialMovement.InitializeWallJump(_samus);
        _samus.LiquidPhysics.SpawnWallJumpDust(_samus);
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
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
    public void ApplyGrappleWallJump()
    {
        if (_samus.Pose is not (SamusPoseId.GrappleWallContactLeftPose or SamusPoseId.GrappleWallContactRightPose))
        {
            throw new InvalidOperationException(
                $"Grapple wall jump requires contact pose $B8/$B9, not ${(int)_samus.Pose:X2}.");
        }

        // `$9B:C9D5-$C9EB` tests the contact pose's X-direction byte, not its descriptive
        // wall side. Direction eight selects `$84`; direction four selects `$83`.
        _samus.Pose = _samus.PoseMetadata.IsFacingRight() ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose;
        _samus.PoseMetadata.RefreshCollisionRadii();

        // `$9B:C9CE` clears the ordinary base-speed pair, then the normal pose-transition
        // machinery reaches `$90:9949`. Like the non-grapple route, it leaves both the
        // extra-run pair and `$0B3C` intact, so Dash momentum survives this wall launch.
        _samus.HorizontalSpeed.AccelerationMode = 0;
        _samus.HorizontalSpeed.BaseSpeed = 0;
        _samus.HorizontalSpeed.BaseSubspeed = 0;

        // `$9B:C9CE` uses generic QueueSound: library one, sound seven, maximum fifteen.
        // It also tears down the active beam flare so the wall-jump's charged-contact path
        // cannot inherit charge accumulated before grapple became active.
        _samus.LiquidPhysics.QueueMovementSound(SamusGrappleRomData.Sounds.Stop.SoundEffect, SamusGrappleRomData.Sounds.Stop.MaximumQueued);
        _samus.ProjectileFlareCounter = 0;

        SamusAerialMovement.InitializeWallJump(_samus);
        _samus.LiquidPhysics.SpawnWallJumpDust(_samus);
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
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
    public bool ApplyGrappleDropTransition(

        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null,
        bool clearMovementSpeed = true)
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
                $"Grapple drop transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a retail dropped-table route.");
        }

        // EnemyDeathAnimation requests this cleanup for a grapple-killed enemy even
        // when the beam never attached. Samus can therefore still be standing, aiming,
        // or jumping. Native C8C5 selects by radius/shot direction in those poses too;
        // only its output domain is restricted, not the source to connected-grapple art.
        SamusPoseId sourcePose = _samus.Pose;
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            _samus.Pose = targetPose;
            _samus.PoseMetadata.RefreshCollisionRadii();
            _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
            if (_samus.Pose != _samus.PoseHistory.PreviousPose)
                _samus.Presentation.InitializeAnimation(initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

        // `$9B:C95E-$C967` clears the two base-X and two Y-speed words regardless of the
        // pose-collision result. Extra run speed is not touched by this routine; an ordinary
        // connected grapple never creates it, so preserving the words is the literal rule.
        if (clearMovementSpeed)
        {
            _samus.HorizontalSpeed.BaseSpeed = 0;
            _samus.HorizontalSpeed.BaseSubspeed = 0;
            _samus.Kinematics.YSpeed = 0;
            _samus.Kinematics.YSubspeed = 0;
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
    public void ApplyWallContactAnimationRewind()
    {
        _samus.AnimationFrameTimer = NativeCountdown.Start(1);

        // `$90:9D96-$90:9DA6` gives Screw Attack a longer pre-contact animation. Its first
        // eligible wall frame is 27, so contact rewinds to 26. Ordinary spin and Space Jump
        // both use the compact 10 -> 11 handoff.
        _samus.AnimationFrame = SamusPoseMetadata.IsScrewAttackPose(_samus.Pose)
            ? SamusWallContactAnimationDefinitions.ScrewAttackRewindFrame
            : SamusWallContactAnimationDefinitions.SpinJumpRewindFrame;
    }

    /// <summary>
    /// Exact 16.16 ADC equivalent shared by bank-$91's grounded, aerial, and spin-turn
    /// initializers. It deliberately permits word overflow, matching the 65816 registers.
    /// </summary>
    private void FoldExtraRunSpeedIntoBaseAndStartTurn()
    {
        SamusHorizontalSpeedState speed = _samus.HorizontalSpeed;
        uint combinedSpeed = unchecked(speed.BaseFixed +
            (SnesSixteenSixteen.FromWhole((uint)speed.ExtraRunSpeed)) + speed.ExtraRunSubspeed);
        speed.BaseSpeed = unchecked((ushort)(SnesSixteenSixteen.WholeOf(combinedSpeed)));
        speed.BaseSubspeed = unchecked((ushort)combinedSpeed);
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.AccelerationMode = 1;
    }

    /// <summary>
    /// Applies equipment, liquid and start-sound selection from <c>SamusFunc_F468_SpinJump</c> at
    /// <c>$91:F624</c> to a generic transition-table target `$19/$1A`.
    /// </summary>
    private SamusPoseId SelectEquippedSpinPose(SamusPoseId genericPose, bool skipFirstAnimationFrame = false)
    {
        bool facingLeft = genericPose switch
        {
            SamusPoseId.SpinJumpRightPose => false,
            SamusPoseId.SpinJumpLeftPose => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(genericPose),
                genericPose,
                "Equipment spin selection requires generic pose $19 or $1A."),
        };

        // Full submersion returns before either equipment substitution or sound dispatch.
        // Gravity bypasses this test; water's disabled-physics option is handled by the
        // shared top-boundary query, including the native water-before-lava precedence.
        if (!_samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) && _samus.LiquidPhysics.IsTopBoundarySubmerged(_samus))
            return genericPose;

        // Native tests Screw Attack first. A save with both bits equipped therefore uses
        // `$81/$82`, not Space Jump art, while retaining Space Jump's repeat-jump physics.
        if (_samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
        {
            if (!skipFirstAnimationFrame)
                _samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary1Sounds.ScrewAttack, maximumQueued: 6);
            return facingLeft ? SamusPoseId.ScrewAttackLeftPose : SamusPoseId.ScrewAttackRightPose;
        }
        if (_samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump))
        {
            // Unlike ordinary/Screw Attack spins, native also queues this on reversal.
            _samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary1Sounds.SpaceJump, maximumQueued: 6);
            return facingLeft ? SamusPoseId.SpaceJumpLeftPose : SamusPoseId.SpaceJumpRightPose;
        }
        if (!skipFirstAnimationFrame && !_samus.LiquidPhysics.CinematicFunctionActive)
            _samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary1Sounds.SpinJump, maximumQueued: 6);
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
    private bool TryBeginShinesparkWindup(
        SamusPoseId targetPose,
        SamusMovementType previousMovementType)
    {
        bool right = targetPose is
            SamusPoseId.NeutralJumpRightPose or SamusPoseId.NormalJumpAimUpRightPose or
            SamusPoseId.NormalJumpAimDiagonalUpRightPose;
        bool left = targetPose is
            SamusPoseId.NeutralJumpLeftPose or SamusPoseId.NormalJumpAimUpLeftPose or
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose;
        if ((!right && !left) || _samus.InputSnapshot.SharedShineTimer == 0)
        {
            return false;
        }

        _samus.Pose = right ? SamusPoseId.ShinesparkWindupRightPose : SamusPoseId.ShinesparkWindupLeftPose;
        _samus.PoseMetadata.RefreshCollisionRadii();
        _samus.Shinespark.BeginWindup(_samus);

        // `$91:F580-$F595` checks the PREVIOUS movement type, lifts Samus one pixel and
        // stores the result in both Y words, so this frame's vertical scroll sees no motion.
        if (previousMovementType == SamusMovementType.NormalJumping)
        {
            _samus.Kinematics.Position -= new Offset16<RoomPixels>(0, 1);
            _samus.WritePreviousYPosition(_samus.Kinematics.Position.Y.Whole);
        }

        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies a `$C7/$C8 -> $C9-$CE` input-table match and installs its exact special
    /// movement handler. No ordinary jump velocity or grounded momentum routine runs.
    /// </summary>
    public void ApplyShinesparkDirectionTransition(SamusPoseId targetPose)
    {
        if (_samus.Pose is not (SamusPoseId.ShinesparkWindupRightPose or SamusPoseId.ShinesparkWindupLeftPose))
        {
            throw new InvalidOperationException(
                $"Directional shinespark transition requires windup pose, not ${(int)_samus.Pose:X2}.");
        }

        _samus.Shinespark.BeginDirectionalLaunch(_samus, targetPose);
    }

    /// <summary>
    /// Applies the verified ordinary-input jump transitions selected from the cartridge's
    /// bank-$91 table, including <c>HandleJumpTransition</c>'s call to
    /// <c>Make_Samus_Jump</c>. Only explicitly verified no-equipment/no-aim routes admitted
    /// by the current runtime are accepted, including interrupting any same-facing landing
    /// stream `$A4-$A7/$E0-$E7` with a fresh Jump edge.
    /// </summary>
    public void ApplyOrdinaryJumpTransition(
        SamusPoseId targetPose,
        SnesButton controllerNewInput = SnesButton.None)
    {
        bool verified =
            SamusPoseMetadata.IsStandingGroundTurnToJumpTransition(_samus.Pose, targetPose) ||
            (_samus.Pose, targetPose) is
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
            SamusPoseMetadata.IsLandingToNormalJumpTransition(_samus.Pose, targetPose);
        if (!verified)
        {
            // The pairs above exhaust the active retail input-table records that invoke
            // the ordinary jump initializer. Movement-specific launches (damage boost,
            // grapple, bomb jump, wall jump, and Shinespark) own separate entry points.
            throw new InvalidOperationException(
                $"Ordinary jump transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not an active retail route for this initializer.");
        }

        _samus.Pose = targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose
            ? SelectEquippedSpinPose(targetPose)
            : targetPose;
        // Pose dispatch happens after movement. The next alpha phase, including
        // the intro-demo alpha handler, owns publication of the new live radius.
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        SamusAerialMovement.InitializeJump(_samus);

        if (_samus.PoseMetadata.ReadMovementType() == SamusMovementType.NormalJumping &&
            (controllerNewInput & SnesButton.X) != 0)
        {
            // `$91:F5CF-$F5E6` runs only in the normal-jumping initializer. It reads the
            // newly installed pose's direction byte and adds `$8000`; spin-jump's separate
            // initializer never publishes this bridge even when Shoot and Jump share a frame.
            _samus.PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.NormalJumpPoseHandoffTag | _samus.PoseMetadata.ReadShotDirection()));
        }
    }

    /// <summary>
    /// Ports `$90:EB20`, the unconditional current-state epilogue clear that follows the
    /// HUD/projectile handler. A failed cooldown/slot allocation must lose the bridge too;
    /// it is deliberately not retained until some later shot succeeds.
    /// </summary>
    public void ClearPoseTransitionShotDirection() => _samus.PoseTransitionShotDirection = 0;

    /// <summary>
    /// <c>InitializeSamusPose_NormalJumping</c> ($91:F543) for a normal-jump pose already
    /// installed by a transition that reaches the ordinary initializer ($91:F433). Stored shine
    /// diverts the six shinespark-capable poses into a windup; otherwise retained extra run
    /// speed selects deceleration and a fresh Shoot press arms the transition shot.
    /// </summary>
    /// <returns>True when the initializer began a shinespark windup instead.</returns>
    internal bool InitializeInstalledNormalJumpPose(
        SamusMovementType previousMovementType,
        SnesButton controllerNewInput)
    {
        if (TryBeginShinesparkWindup(_samus.Pose, previousMovementType))
            return true;
        InitializeOrdinaryAerialAcceleration();
        PublishNormalJumpPoseShotDirection(controllerNewInput);
        return false;
    }

    /// <summary>Publishes $91:F5CF-F5E6 after an accepted normal-jump pose initializer.</summary>
    internal void PublishNormalJumpPoseShotDirection(SnesButton controllerNewInput)
    {
        if (_samus.PoseMetadata.ReadMovementType() == SamusMovementType.NormalJumping &&
            (controllerNewInput & SnesButton.X) != 0)
            _samus.PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.NormalJumpPoseHandoffTag | _samus.PoseMetadata.ReadShotDirection()));
    }
    /// <summary>
    /// Applies a same-facing input/fallback transition within normal-jump type two or
    /// falling type six without replacing the live 16.16 velocity words. The horizontal
    /// gun-extended records `$13/$14/$67/$68` use this same native initialization seam;
    /// “aim” survives in the historical method name only because that was the first slice.
    /// </summary>
    public void ApplyAerialAimTransition(SamusPoseId targetPose)
    {
        if (SamusPoseMetadata.IsCompactAerialPose(_samus.Pose) || SamusPoseMetadata.IsCompactAerialPose(targetPose))
        {
            // Compact poses change Samus's collision height. This overload intentionally
            // has no room data, so accepting the request would bypass the native expansion
            // collision probe; callers must select the collision-aware API explicitly.
            throw new InvalidOperationException(
                "Radius-ten aerial transitions require TryApplyCompactAerialTransition and active room collision data.");
        }
        if (!SamusPoseMetadata.IsSameFacingAerialAimFireOrForwardTransition(_samus.Pose, targetPose))
        {
            // This helper models one same-facing ROM family, not the general pose dispatcher.
            // A cross-family request therefore violates the method contract rather than
            // identifying an untranslated member of the admitted family.
            throw new InvalidOperationException(
                $"Aerial aim/fire/forward transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is outside the same-facing family handled by this operation.");
        }

        SamusPoseId sourcePose = _samus.Pose;
        SamusMovementType previousMovementType = _samus.PoseMetadata.ReadMovementType();
        ushort oldRadius = _samus.Kinematics.Radius.Y;
        _samus.Pose = targetPose;
        _samus.PoseMetadata.RefreshCollisionRadii();
        if (_samus.Kinematics.Radius.Y != oldRadius)
        {
            // The admitted family deliberately excludes compact straight-down `$17/$18`
            // and `$2D/$2E`; reaching a different radius proves a caller crossed that seam.
            // Every retail member admitted above has the same collision radius. A mismatch
            // can only come from inconsistent/corrupt pose metadata, not an omitted route.
            throw new InvalidDataException(
                $"Aerial pose ${(int)targetPose:X2} unexpectedly changes radius {oldRadius} -> {_samus.Kinematics.Radius.Y} inside an equal-radius transition family.");
        }
        // Both normal-jump and falling targets run their movement initializer.
        // Preserve base velocity, but derive acceleration mode from extra dash speed.
        if (sourcePose != _samus.Pose)
        {
            // The normal-jump initializer also consumes stored shine on eligible poses.
            // This includes releasing forward midair, not just the initial ground jump.
            if (TryBeginShinesparkWindup(targetPose, previousMovementType))
                return;
            InitializeOrdinaryAerialAcceleration();
        }
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
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
    public bool TryApplyCompactAerialTransition(

        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        bool rightJump = SamusPoseMetadata.IsRightFacingNormalJumpPose(_samus.Pose) &&
            SamusPoseMetadata.IsRightFacingNormalJumpPose(targetPose);
        bool leftJump = SamusPoseMetadata.IsLeftFacingNormalJumpPose(_samus.Pose) &&
            SamusPoseMetadata.IsLeftFacingNormalJumpPose(targetPose);
        bool rightFall = SamusPoseMetadata.IsRightFacingFallingPose(_samus.Pose) &&
            SamusPoseMetadata.IsRightFacingFallingPose(targetPose);
        bool leftFall = SamusPoseMetadata.IsLeftFacingFallingPose(_samus.Pose) &&
            SamusPoseMetadata.IsLeftFacingFallingPose(targetPose);
        if ((!rightJump && !leftJump && !rightFall && !leftFall) ||
            (!SamusPoseMetadata.IsCompactAerialPose(_samus.Pose) && !SamusPoseMetadata.IsCompactAerialPose(targetPose)))
        {
            // This is the exact precondition of the collision-aware compact-pose helper.
            // Other aerial changes use ApplyAerialAimTransition or a movement-specific API.
            throw new InvalidOperationException(
                $"Compact aerial transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a same-family ROM route.");
        }

        SamusPoseId sourcePose = _samus.Pose;
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        _samus.Pose = targetPose;
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        if (sourcePose != _samus.Pose)
        {
            // Pose collision uses prospective geometry; the live radius belongs to
            // the current frame's alpha and changes only at the next alpha.
            // Successful expansion still runs the normal-jump initializer. A stored
            // shine may replace the requested aim pose; collision rejection above must
            // not consume the charge or apply the windup's one-pixel adjustment.
            if (TryBeginShinesparkWindup(targetPose,
                rightJump || leftJump ? SamusMovementType.NormalJumping : SamusMovementType.Falling))
                return true;
            InitializeOrdinaryAerialAcceleration();
        }
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies falling input-lookup failure's command eight ($91:EC8E). Cancel the
    /// dash component after movement, preserving the base speed and acceleration mode.
    /// The caller admits only a still-falling body, so no new-fall Y reset is needed.
    /// </summary>
    public void ApplyFallingInputFallback()
    {
        if (_samus.PoseMetadata.ReadMovementType() != SamusMovementType.Falling)
            throw new InvalidOperationException("Falling input fallback requires a falling pose.");
        _samus.HorizontalSpeed.CancelRunningMomentum(_samus.PoseMetadata.ReadFacingDirection());
        _samus.HorizontalSpeed.ExtraRunSpeed = 0;
        _samus.HorizontalSpeed.ExtraRunSubspeed = 0;
    }

    /// <summary>
    /// Applies a same-facing arm-body transition within the movement-type-zero standing
    /// family and movement-type-one running family. Besides aimed bodies, this includes the
    /// real `$0B/$0C` horizontal-fire records.
    /// </summary>
    /// <remarks>
    /// All admitted records have radius 21. If both endpoints are running, `$91:F50C` writes
    /// `$8000` to the new-frame word and `$91:FB5C` preserves the current frame and timer;
    /// this matters when firing midway through the ten-frame run cycle. A transition between
    /// movement types initializes frame zero normally. Movement type changes take effect on
    /// the following frame because pose transitions occur after movement.
    /// </remarks>
    public void ApplyGroundedAimTransition(SamusPoseId targetPose)
    {
        bool sourceRight = SamusPoseMetadata.IsRightFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsRightFacingRunningPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(_samus.Pose);
        bool targetRight = SamusPoseMetadata.IsRightFacingStandingPose(targetPose) || SamusPoseMetadata.IsRightFacingRunningPose(targetPose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(targetPose);
        bool sourceLeft = SamusPoseMetadata.IsLeftFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(_samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(_samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingLandingPose(_samus.Pose);
        bool targetLeft = SamusPoseMetadata.IsLeftFacingStandingPose(targetPose) || SamusPoseMetadata.IsLeftFacingRunningPose(targetPose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(targetPose);
        bool sameRightFamily = sourceRight && targetRight;
        bool sameLeftFamily = sourceLeft && targetLeft;
        bool sameRightCrouch = SamusPoseMetadata.IsRightFacingCrouchingPose(_samus.Pose) &&
            SamusPoseMetadata.IsRightFacingCrouchingPose(targetPose);
        bool sameLeftCrouch = SamusPoseMetadata.IsLeftFacingCrouchingPose(_samus.Pose) &&
            SamusPoseMetadata.IsLeftFacingCrouchingPose(targetPose);
        if (!sameRightFamily && !sameLeftFamily && !sameRightCrouch && !sameLeftCrouch)
        {
            // The native input tables never feed a facing reversal through this arm-only
            // transition seam; reversal has its own turn-pose initialization path.
            throw new InvalidOperationException(
                $"Grounded aim transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is outside this same-facing pose family.");
        }
        if (!SamusPoseMetadata.IsGroundedAimPose(_samus.Pose) && !SamusPoseMetadata.IsGroundedAimPose(targetPose) &&
            !SamusPoseMetadata.IsGunExtendedPose(_samus.Pose) && !SamusPoseMetadata.IsGunExtendedPose(targetPose))
        {
            throw new InvalidOperationException(
                "Grounded arm transition requires an aimed or gun-extended source/target pose.");
        }

        SamusPoseId sourcePose = _samus.Pose;
        bool preservesRunningAnimation =
            (SamusPoseMetadata.IsRightFacingRunningPose(sourcePose) && SamusPoseMetadata.IsRightFacingRunningPose(targetPose)) ||
            (SamusPoseMetadata.IsLeftFacingRunningPose(sourcePose) && SamusPoseMetadata.IsLeftFacingRunningPose(targetPose));
        if (!preservesRunningAnimation)
        {
            // This is `$91:F404/$91:FB08`'s ordinary target installation. Replacing Pose
            // directly would omit radius refresh, delay-list selection, and command cleanup.
            ApplySimpleGroundedPoseChange(sourcePose, targetPose, "Grounded arm");
            return;
        }

        // `$91:F50C-$F51A` sees previous movement type one and publishes `$8000`. The BMI
        // at `$91:FB5C` exits before touching frame, timer, or delay-buffer state. Every live
        // running arm pose points to the same `$91:B20A` list, so the cached list address is
        // already exact and must not be rebound or reset here.
        ushort oldRadius = _samus.Kinematics.Radius.Y;
        _samus.Pose = targetPose;
        _samus.PoseMetadata.RefreshCollisionRadii();
        if (_samus.Kinematics.Radius.Y != oldRadius)
        {
            throw new InvalidDataException(
                $"Running arm pose ${(int)targetPose:X2} unexpectedly changed radius {oldRadius} -> {_samus.Kinematics.Radius.Y}.");
        }
    }

    /// <summary>
    /// Implements `$91:EB56-$91:EB87`'s ten-entry shot-direction selector. The input pose
    /// may be the current running pose after a killed-X-speed collision or the prospective
    /// running pose tested by the one-pixel arm-pump probe.
    /// </summary>
    public static SamusPoseId SelectRanIntoWallPose(SamusPoseId sourcePose)
    {
        byte shotDirection = SamusPoseMetadata.ReadShotDirection(sourcePose);
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
                $"Pose ${(int)sourcePose:X2} has invalid shot-direction byte ${shotDirection:X2}."),
        };
    }

    /// <summary>
    /// Ports <c>CheckIfProspectivePoseRunsIntoAWall</c> at <c>$91:EADE</c>.
    /// </summary>
    /// <remarks>
    /// A current type-one collision maps the current pose immediately. Otherwise, only a
    /// prospective type-one pose is interesting: native first probes solid enemies one pixel
    /// ahead in the CURRENT pose direction and maps the prospective pose without moving when
    /// one is hit. Otherwise it moves Samus that pixel, retains the move when clear, and maps
    /// the prospective pose's shot direction when a block stops it. The retained move is the
    /// retail arm-pump bug.
    /// </remarks>
    public SamusPoseId? CheckProspectiveRunningPoseForWall(

        RoomLevelData level,
        SamusPoseId? prospectivePose,
        bool currentXSpeedKilledByBlock,
        out BlockMoveResult? onePixelProbe,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        onePixelProbe = null;

        if (currentXSpeedKilledByBlock && _samus.PoseMetadata.ReadMovementKind() == SamusMovementType.Running)
            return SelectRanIntoWallPose(_samus.Pose);

        if (prospectivePose is not { } target ||
            SamusPoseMetadata.ReadMovementKind(target) != SamusMovementType.Running)
            return null;

        bool facingLeft = _samus.PoseMetadata.IsFacingLeft();
        if (_samus.Kinematics.InteractiveEnemies.Count != 0)
        {
            // $91:EB05-$EB3C: Samus_vs_SolidEnemy_CollisionDetection with $12.$14 = 1.0000
            // and the facing direction stored as the collision movement direction.
            SamusCollisionDirection direction = facingLeft
                ? SamusCollisionDirection.Left
                : SamusCollisionDirection.Right;
            SolidEnemyCollisionResult enemyProbe = SamusSolidEnemyCollision.Probe(
                _samus.Kinematics, _samus.Kinematics.InteractiveEnemies, direction, distance: 1, distanceSubposition: 0);
            _samus.Kinematics.RecordSolidEnemyCollision(direction, enemyProbe.EnemyIndex);
            if (enemyProbe.Collided)
                return SelectRanIntoWallPose(target);
        }

        int onePixelForward = facingLeft
            ? -(1 << 16)
            : 1 << 16;
        onePixelProbe = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, _samus.Kinematics, new HorizontalBlockMove { Displacement = onePixelForward, AlignToSlopeAfterMovement = false });
        return onePixelProbe.Value.Collided
            ? SelectRanIntoWallPose(target)
            : null;
    }

    /// <summary>
    /// Installs the pose selected when `$91:EADE` finds a block wall. This deliberately
    /// accepts standing/running/landing sources as well as an existing wall-stop source:
    /// `$91:EADE` can test a prospective run before that run is installed, and changing aim
    /// while still pressing into the wall proposes another running pose for the same probe.
    /// A moonwalk that proposes a run is probed too: `$91:EB05` moves one pixel along the
    /// moonwalk pose's facing, but `$91:EB4E` picks the wall pose from the proposed run's
    /// direction, so a moonwalk facing right can stop in the left-facing wall pose.
    /// </summary>
    public void ApplyRanIntoWallPoseChange(SamusPoseId targetPose)
    {
        bool moonwalking = SamusPoseMetadata.IsMoonwalkingPose(_samus.Pose);
        bool rightSource = SamusPoseMetadata.IsRightFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsRightFacingRunningPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(_samus.Pose) || SamusPoseMetadata.IsRightFacingLandingPose(_samus.Pose) || moonwalking;
        bool leftSource = SamusPoseMetadata.IsLeftFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(_samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingLandingPose(_samus.Pose) || moonwalking;
        bool rightRoute = rightSource &&
            (SamusPoseMetadata.IsRightFacingRanIntoWallPose(targetPose) || targetPose == SamusPoseId.StandingAimUpRightPose);
        bool leftRoute = leftSource &&
            (SamusPoseMetadata.IsLeftFacingRanIntoWallPose(targetPose) || targetPose == SamusPoseId.StandingAimUpLeftPose);
        if (!rightRoute && !leftRoute)
        {
            // Only block-backed ran-into-wall records call this exact initializer. Treat a
            // different source/target pair as misuse of the specialized operation.
            throw new InvalidOperationException(
                $"Ran-into-wall pose change ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a block-backed retail route.");
        }

        ApplySimpleGroundedPoseChange(_samus.Pose, targetPose, "Ran into wall");
    }

    /// <summary>Applies `$89/$8A/$CF-$D2`'s same-facing exit to a type-one running pose.</summary>
    public void ApplyRanIntoWallToRunning(SamusPoseId targetPose)
    {
        bool rightRoute = SamusPoseMetadata.IsRightFacingRanIntoWallPose(_samus.Pose) &&
            SamusPoseMetadata.IsRightFacingRunningPose(targetPose);
        bool leftRoute = SamusPoseMetadata.IsLeftFacingRanIntoWallPose(_samus.Pose) &&
            SamusPoseMetadata.IsLeftFacingRunningPose(targetPose);
        if (!rightRoute && !leftRoute)
        {
            // Running exits are a strict same-facing subset of the ran-into-wall table.
            throw new InvalidOperationException(
                $"Ran-into-wall running exit ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a retail route.");
        }

        ApplySimpleGroundedPoseChange(_samus.Pose, targetPose, "Ran into wall to running");
    }

    /// <summary>
    /// Applies the movement-type-$10 option gate and stable-pose changes surrounding
    /// <c>InitializeSamusPose_Moonwalking</c> at <c>$91:F88C</c>.
    /// </summary>
    /// <remarks>
    /// Standing transition tables always publish a moonwalk candidate when Shoot and the
    /// backward direction are held. The native settings word decides what that candidate
    /// means: enabled retains `$49/$4A/$75-$78`; disabled substitutes ordinary `$25/$26`
    /// turn art. Once active, aim changes and the direct return to forward running are
    /// ordinary radius-21 pose installations with no invented velocity adjustment.
    /// </remarks>
    public void ApplyMoonwalkPoseChange(
        SamusPoseId targetPose,
        bool moonwalkEnabled)
    {

        bool sourceStandingRight = SamusPoseMetadata.IsRightFacingStandingPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(_samus.Pose);
        bool sourceStandingLeft = SamusPoseMetadata.IsLeftFacingStandingPose(_samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(_samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingLandingPose(_samus.Pose);
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
            // `$91:F893-$F8A9` tests the candidate pose-X byte. `$4A/$76/$78` store four
            // and become right-to-left `$25`; `$49/$75/$77` store eight and become `$26`.
            ApplyGroundedTurn(
                targetVisualRight ? SamusPoseId.TurningRightToLeftPose : SamusPoseId.TurningLeftToRightPose);
            return;
        }

        bool sameVisualFamily =
            (SamusPoseMetadata.IsMoonwalkingFacingRightPose(_samus.Pose) && targetVisualRight) ||
            (SamusPoseMetadata.IsMoonwalkingFacingLeftPose(_samus.Pose) && targetVisualLeft);
        bool exitsToForwardRun =
            (SamusPoseMetadata.IsMoonwalkingFacingRightPose(_samus.Pose) && targetPose == SamusPoseId.MovingRightNormalPose) ||
            (SamusPoseMetadata.IsMoonwalkingFacingLeftPose(_samus.Pose) && targetPose == SamusPoseId.MovingLeftNormalPose);
        bool exitsToStandingFallback =
            (SamusPoseMetadata.IsMoonwalkingFacingRightPose(_samus.Pose) && SamusPoseMetadata.IsRightFacingStandingPose(targetPose)) ||
            (SamusPoseMetadata.IsMoonwalkingFacingLeftPose(_samus.Pose) && SamusPoseMetadata.IsLeftFacingStandingPose(targetPose));
        if ((!entering || !moonwalkEnabled) && !sameVisualFamily &&
            !exitsToForwardRun && !exitsToStandingFallback)
        {
            // The complete stable moonwalk table is represented by the predicates above;
            // reaching this branch means the caller selected the wrong pose-change API.
            throw new InvalidOperationException(
                $"Moonwalk pose change ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a stable retail route.");
        }

        ApplySimpleGroundedPoseChange(_samus.Pose, targetPose, "Moonwalk");
    }

    /// <summary>
    /// Applies `$91:F8D3-$91:F950` when a stable moonwalk pose reverses while Jump is held.
    /// The six requested `$BF-$C4` records are not airborne yet: they retain the grounded
    /// turn movement handler while their three visible frames play.
    /// </summary>
    public void ApplyMoonwalkTurnJump(SamusPoseId targetPose)
    {

        SamusPoseId expectedTarget = _samus.PoseMetadata.ReadShotDirection() switch
        {
            1 => SamusPoseId.MoonwalkTurnJumpAimUpLeftPose,
            2 => SamusPoseId.MoonwalkTurnJumpLeftPose,
            3 => SamusPoseId.MoonwalkTurnJumpAimDownLeftPose,
            6 => SamusPoseId.MoonwalkTurnJumpAimDownRightPose,
            7 => SamusPoseId.MoonwalkTurnJumpRightPose,
            8 => SamusPoseId.MoonwalkTurnJumpAimUpRightPose,
            // Stable moonwalk poses publish only the six directions enumerated above.
            // Another value means WRAM/pose metadata is inconsistent, not missing logic.
            _ => throw new InvalidDataException(
                $"Moonwalk shot direction ${_samus.PoseMetadata.ReadShotDirection():X2} is invalid for a stable moonwalk pose."),
        };
        SamusPoseId inputTableTarget = _samus.PoseMetadata.IsFacingLeft()
            ? SamusPoseId.MoonwalkTurnJumpLeftPose : SamusPoseId.MoonwalkTurnJumpRightPose;
        if (!SamusPoseMetadata.IsMoonwalkingPose(_samus.Pose) || targetPose != expectedTarget && targetPose != inputTableTarget)
        {
            // Input requests the un-aimed turn; the pose initializer then substitutes
            // the old muzzle's aimed turn. Already-resolved diagnostic callers are valid too.
            throw new InvalidOperationException(
                $"Moonwalk turn/jump ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a retail route.");
        }

        // Preserve the old muzzle direction before installing turn art. The next alpha
        // uses the tag to admit the turning HUD producer and forcibly release charge.
        byte sourceShotDirection = _samus.PoseMetadata.ReadShotDirection();
        FoldExtraRunSpeedIntoBaseAndStartTurn();
        ApplySimpleGroundedPoseChange(_samus.Pose, expectedTarget, "Moonwalk turn/jump");
        _samus.PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.MoonwalkPoseHandoffTag | sourceShotDirection));
    }

    /// <summary>
    /// Applies the verified ordinary transition from standing-right pose $01 to running-
    /// right pose $09 at the end-of-frame bank-$91 transition seam.
    /// </summary>
    /// <remarks>
    /// This is intentionally not a general pose setter. For this transition, momentum
    /// routine index zero is a no-op, both poses have radius 21, and pose $09 uses the
    /// already-supported default render/tile paths. Other transitions can change momentum,
    /// align radii, invoke collision, or run specialized handlers and remain rejected.
    /// </remarks>
    public void ApplyStandingRightToRunningRight()
    {
        if (_samus.Pose != SamusPoseId.FacingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Standing-right to running-right transition requires pose $01, not ${(int)_samus.Pose:X2}.");
        }

        _samus.Pose = SamusPoseId.MovingRightNormalPose;
        _samus.PoseMetadata.RefreshCollisionRadii();

        // $91:F404 -> $91:FB08 resets animation frame zero and loads pose $09's first
        // delay byte after SamusFunc_F433 refreshes pose direction/movement metadata.
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
    }

    /// <summary>
    /// Applies the verified no-button fallback from running-right pose $09 to standing-right
    /// pose $01 after ordinary running momentum has decelerated to zero.
    /// </summary>
    public void ApplyRunningRightToStandingRight()
    {
        if (_samus.Pose != SamusPoseId.MovingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Running-right to standing-right transition requires pose $09, not ${(int)_samus.Pose:X2}.");
        }

        _samus.Pose = SamusPoseId.FacingRightNormalPose;
        _samus.PoseMetadata.RefreshCollisionRadii();

        // Pose $09's new-pose-unless-buttons byte is $01. Once Samus_Pose_Func2 selects
        // momentum index two, `$91:ECD0` cancels `$0B3C/$0B3E` but deliberately leaves the
        // numeric extra pair intact. The following standing movement pass consumes that
        // final no-base-speed displacement before clearing every X-motion word.
        _samus.HorizontalSpeed.CancelRunningMomentum(_samus.PoseMetadata.ReadFacingDirection());
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
    }

    /// <summary>Applies the ordinary grounded $02 -> $0A start-moving-left transition.</summary>
    public void ApplyStandingLeftToRunningLeft() =>
        ApplySimpleGroundedPoseChange(
            SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingLeftNormalPose,
            "Standing-left to running-left");

    /// <summary>Applies the no-button grounded $0A -> $02 fallback.</summary>
    public void ApplyRunningLeftToStandingLeft()
    {
        ApplySimpleGroundedPoseChange(
            SamusPoseId.MovingLeftNormalPose,
            SamusPoseId.FacingLeftNormalPose,
            "Running-left to standing-left");

        // This is the mirrored `$91:ECD0` momentum-index-two route used by `$09 -> $01`.
        // ExtraRunSpeed/Subspeed remain available to standing's ordered movement/clear pass.
        _samus.HorizontalSpeed.CancelRunningMomentum(_samus.PoseMetadata.ReadFacingDirection());
    }

    /// <summary>
    /// Applies the shared standing/landing transition-table route from any right-facing
    /// landing (including firing `$E6`) to running right `$09`, or from the mirrored left
    /// family (including `$E7`) to `$0A`. Landing movement has already cleared momentum in
    /// this frame; the new running pose begins accelerating on the next frame.
    /// </summary>
    public void ApplyLandingToRunning(SamusPoseId targetPose)
    {
        bool right = SamusPoseMetadata.IsRightFacingLandingPose(_samus.Pose) &&
            targetPose == SamusPoseId.MovingRightNormalPose;
        bool left = SamusPoseMetadata.IsLeftFacingLandingPose(_samus.Pose) &&
            targetPose == SamusPoseId.MovingLeftNormalPose;
        if (!right && !left)
        {
            throw new InvalidOperationException(
                $"Landing-to-run transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not verified.");
        }

        ApplySimpleGroundedPoseChange(_samus.Pose, targetPose, "Landing-to-run");
    }

    /// <summary>
    /// Applies bank-$91's grounded turn initialization at <c>$91:F8D3</c>. Input tables
    /// first publish generic `$25/$26`; the initializer indexes the previous pose's shot
    /// direction through `$91:F9C2` and may replace it with `$8B-$8E/$9C/$9D`.
    /// </summary>
    public void ApplyGroundedTurn(SamusPoseId targetPose, SnesButton controllerInput = SnesButton.None)
    {

        // `$91:F8D3-$F8DB` gives the two front-view poses a deliberately unusual route.
        // Their pose-X direction is zero, so they cannot honestly be classified as either
        // a right-facing or left-facing source. Native code instead skips the shot-direction
        // lookup entirely when the previous pose is `$00/$9B`, retaining the generic
        // `$25/$26` selected by that front-view pose's input transition table.
        bool wasForwardFacing = SamusPoseMetadata.IsForwardFacingPose(_samus.Pose);

        // Crouching transition tables publish `$43/$44` directly, whereas every admitted
        // standing/running/landing table publishes `$25/$26`. The initializer still uses
        // previous movement type five to choose the full crouched aim-preserving table.
        SamusMovementType sourceMovementType = _samus.PoseMetadata.ReadMovementType();
        bool wasCrouching = sourceMovementType == SamusMovementType.Crouching;
        bool wasMoonwalking = sourceMovementType == SamusMovementType.Moonwalking;

        // `$91:F8F3-$F903` reads the PREVIOUS moonwalk pose, not the selected turn pose.
        // Preserve the complete source byte now so the low-nibble projectile direction is
        // still available after ApplySimpleGroundedPoseChange replaces `Pose` below.
        byte moonwalkSourceShotDirection = wasMoonwalking
            ? _samus.PoseMetadata.ReadShotDirection()
            : (byte)0;

        bool rightSource = SamusPoseMetadata.IsRightFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsRightFacingRunningPose(_samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkingFacingRightPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRanIntoWallPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingCrouchingPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(_samus.Pose);
        bool leftSource = SamusPoseMetadata.IsLeftFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(_samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkingFacingLeftPose(_samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingRanIntoWallPose(_samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingCrouchingPose(_samus.Pose) ||
            SamusPoseMetadata.IsLeftFacingLandingPose(_samus.Pose);
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
                $"Grounded turn ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a verified transition.");
        }

        // F8F9 checks held Jump, not a fresh press. Releasing Shoot from a buffered
        // Moonwalk selects a generic turn in the input table, then reaches this same
        // turn/jump initializer without a new Jump edge.
        if (wasMoonwalking && (controllerInput & SnesButton.A) != 0)
        {
            ApplyMoonwalkTurnJump(turnsLeft
                ? SamusPoseId.MoonwalkTurnJumpLeftPose : SamusPoseId.MoonwalkTurnJumpRightPose);
            return;
        }

        // `$91:F8D3` reads the PREVIOUS pose record before it installs the final turn art.
        // Previous movement type five selects `$91:F9CC`; every other admitted source uses
        // `$91:F9C2`. Both tables have ten entries, but shot directions four/five belong to
        // the compact straight-down family that remains untranslated.
        SamusPoseId selectedTurnPose;
        if (wasForwardFacing)
        {
            // This is the literal early branch to `$91:F931`: keep the transition-table
            // result. Reading byte three from `$00/$9B` here would manufacture an aim/facing
            // direction that the cartridge explicitly declines to inspect.
            selectedTurnPose = targetPose;
        }
        else
        {
            byte shotDirection = _samus.PoseMetadata.ReadShotDirection();
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
                    // directions 4/5 (or any value outside the native eight-entry set).
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
                $"Grounded turn source ${(int)_samus.Pose:X2} has direction metadata inconsistent with target ${(int)targetPose:X2}.");
        }

        SamusHorizontalSpeedState speed = _samus.HorizontalSpeed;

        // $91:F931-$91:F941 folds the run-button/speed-booster component into base speed
        // with two independent 16-bit ADCs and their carry. Use one wrapping 16.16 add to
        // preserve exactly that pair of operations, including overflow at the high word.
        uint combinedSpeed = unchecked(speed.BaseFixed +
            (SnesSixteenSixteen.FromWhole((uint)speed.ExtraRunSpeed)) + speed.ExtraRunSubspeed);
        speed.BaseSpeed = unchecked((ushort)(SnesSixteenSixteen.WholeOf(combinedSpeed)));
        speed.BaseSubspeed = unchecked((ushort)combinedSpeed);

        // The native handler consumes the extra component and selects acceleration mode
        // one. $90:8EA9 interprets that mode as “move opposite the NEW facing direction”
        // while $90:9A7E decelerates, which is how reversal preserves old momentum.
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.AccelerationMode = 1;

        ApplySimpleGroundedPoseChange(_samus.Pose, selectedTurnPose, "Grounded turn");

        if (wasMoonwalking)
        {
            // The `$0100` tag makes HUD handler `$90:DD74` treat the one-frame turning
            // state as shooting. `$90:BA5F` later masks it away and consumes only this
            // source pose's direction byte when a beam slot can actually be initialized.
            _samus.PoseTransitionShotDirection = unchecked((ushort)(SamusProjectileRomData.MoonwalkPoseHandoffTag | moonwalkSourceShotDirection));
        }
    }
    /// <summary>
    /// Runs UpdateSamusPose's shared expansion check before the interrupted hurt
    /// pose is installed. Crouch-to-hurt must keep the larger body above its floor.
    /// </summary>
    internal bool TryResolveKnockbackPoseCollision(
RoomLevelData level, SamusPoseId targetPose,
        ushort nmiFrameCounter, RoomPlmSystem? plms)
    {
        SamusPoseId sourcePose = _samus.Pose;
        LargerPoseCollisionOutcome outcome = _samus.ResolveLargerPoseCollision(
level, targetPose, nmiFrameCounter, plms, out int adjustment);
        if (outcome != LargerPoseCollisionOutcome.Allowed)
        {
            if (outcome == LargerPoseCollisionOutcome.CrouchFallback)
                _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)adjustment));
        return true;
    }
    /// <summary>Applies the crouch input tables' direct same-facing exits ($27/$71/$73/$85 to $01, or $28/$72/$74/$86 to $02) after larger-pose collision admits the standing body.</summary>
    /// <param name="level">Live room block geometry used by the shared pose-expansion resolver.</param>
    /// <param name="targetPose">Final standing-right $01 or standing-left $02 pose matching the current crouch facing; animated stand-up and cross-facing routes are not accepted.</param>
    /// <param name="nmiFrameCounter">Native NMI counter supplied to changed-pose block reactions.</param>
    /// <param name="plms">Optional live PLM owner receiving those collision reactions.</param>
    /// <returns>True when the final standing pose and animation are installed; false when collision retains the source or substitutes stable crouch, which may change an aimed crouch even on failure.</returns>
    /// <remarks>These six-byte input records bypass animated $F7–$FC stand-up poses. Accepted expansion applies the whole-pixel Y adjustment but retains the crouching radius until the next alpha pass, matching $91:FDAE's previous-pose radius publication.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="level"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source and target are not a same-facing crouch-to-final-standing input-table route.</exception>
    public bool TryApplyDirectCrouchToStandingTransition(

        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        bool rightRoute = SamusPoseMetadata.IsRightFacingCrouchingPose(_samus.Pose) &&
            targetPose == SamusPoseId.FacingRightNormalPose;
        bool leftRoute = SamusPoseMetadata.IsLeftFacingCrouchingPose(_samus.Pose) &&
            targetPose == SamusPoseId.FacingLeftNormalPose;
        if (!rightRoute && !leftRoute)
        {
            // This entry point is the exact `$27/$28 -> $01/$02` direct-exit route.
            // Animated stand-ups and cross-facing changes have separate initializers.
            throw new InvalidOperationException(
                $"Direct crouch exit ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a ROM-table route.");
        }

        SamusPoseId sourcePose = _samus.Pose;
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // $91:FFA7 does not restore an aimed crouch when the attempted standing body
            // is boxed in. It selects the ordinary stable crouch from facing metadata.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        _samus.Pose = targetPose;
        // `$91:FDAE` explicitly publishes the previous pose's radius while probing the
        // enlargement, and this direct transition does not call Samus_SetRadius afterward.
        // Ordinary alpha publishes the standing radius at the start of the next frame.
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies the crouching table's `$4B/$4C` jump entry through the native ordering:
    /// enlarge radius 16 -> 19 with `$91:FDAE`, initialise movement type two, run the
    /// `$91:FC66` crouch-only Y adjustment, then call `Make_Samus_Jump`.
    /// </summary>
    /// <remarks>
    /// `$91:FC7D` tests the literal previous pose, not merely movement type five. Therefore
    /// only ordinary `$27/$28` subtract ten extra pixels; aimed `$71-$74/$85/$86` still
    /// jump, but retain only the collision resolver's bottom-alignment adjustment.
    /// </remarks>
    /// <returns>
    /// False when expansion is impossible. Simultaneous initial hits select ordinary stable
    /// crouch; rejection by a compensating opposite-side probe retains the source pose.
    /// </returns>
    public bool TryApplyCrouchJumpTransition(

        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        bool rightRoute = SamusPoseMetadata.IsRightFacingCrouchingPose(_samus.Pose) &&
            targetPose == SamusPoseId.NeutralJumpTransitionRightPose;
        bool leftRoute = SamusPoseMetadata.IsLeftFacingCrouchingPose(_samus.Pose) &&
            targetPose == SamusPoseId.NeutralJumpTransitionLeftPose;
        if (!rightRoute && !leftRoute)
        {
            // Only the two neutral `$4B/$4C` transition records enter this routine; aimed
            // crouches deliberately reuse the same facing-selected target.
            throw new InvalidOperationException(
                $"Crouch jump ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a ROM-table route.");
        }

        SamusPoseId sourcePose = _samus.Pose;
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        _samus.Pose = targetPose;
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));

        // Alpha owns the new radius next frame; collision correction and jump setup
        // below must not publish it during this frame's pose-commit stage.
        // HandleJumpTransition_NormalJumping at $91:FC7D performs this after pose
        // initialization/collision but before Make_Samus_Jump. It writes only current Y;
        // the desktop state has no separately exposed PreviousYPosition word to mirror.
        if (sourcePose is SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingLeftPose)
            _samus.Kinematics.Position -= new Offset16<RoomPixels>(0, 10);

        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        SamusAerialMovement.InitializeJump(_samus);
        return true;
    }

    /// <summary>
    /// Applies the ordinary or aimed crouch-start/stand-start transition records,
    /// including command seven's bottom alignment and the larger-radius collision branch
    /// from <c>HandlePoseChangeCollision</c>. The admitted target set is exactly
    /// `$35/$36/$3B/$3C/$F1-$FC`.
    /// </summary>
    /// <returns>
    /// False only when simultaneous floor and ceiling collision leave no verified room to
    /// expand from radius 16 to radius 21; native behavior then retains the crouching pose.
    /// </returns>
    public bool TryApplyPostureTransition(

        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        bool startsCrouchingRight =
            (SamusPoseMetadata.IsRightFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsRightFacingRunningPose(_samus.Pose) ||
             SamusPoseMetadata.IsMoonwalkingFacingRightPose(_samus.Pose) ||
             SamusPoseMetadata.IsRightFacingRanIntoWallPose(_samus.Pose) ||
             SamusPoseMetadata.IsRightFacingLandingPose(_samus.Pose)) &&
            targetPose is
                SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionAimUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose;
        bool startsCrouchingLeft =
            (SamusPoseMetadata.IsLeftFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(_samus.Pose) ||
             SamusPoseMetadata.IsMoonwalkingFacingLeftPose(_samus.Pose) ||
             SamusPoseMetadata.IsLeftFacingRanIntoWallPose(_samus.Pose) ||
             SamusPoseMetadata.IsLeftFacingLandingPose(_samus.Pose)) &&
            targetPose is
                SamusPoseId.CrouchingTransitionLeftPose or SamusPoseId.CrouchingTransitionAimUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose;
        bool startsCrouching = startsCrouchingRight || startsCrouchingLeft;
        bool startsStandingRight = SamusPoseMetadata.IsRightFacingCrouchingPose(_samus.Pose) &&
            targetPose is
                SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionAimUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalDownRightPose;
        bool startsStandingLeft = SamusPoseMetadata.IsLeftFacingCrouchingPose(_samus.Pose) &&
            targetPose is
                SamusPoseId.StandingTransitionLeftPose or SamusPoseId.StandingTransitionAimUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalDownLeftPose;
        bool startsStanding = startsStandingRight || startsStandingLeft;
        if (!startsCrouching && !startsStanding)
        {
            // `$91:F7B0/$F7D3` admit exactly the crouch/stand records enumerated above.
            throw new InvalidOperationException(
                $"Posture transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is outside the crouch/stand ROM-table family.");
        }

        if (startsCrouching)
        {
            // `Samus_CrouchTrans` at `$91:F7B0` samples the high stage byte before later
            // posture movement can cancel running momentum. A stage-four crouch therefore
            // banks 180 palette-handler ticks even though the following stable crouch has
            // no horizontal Speed Booster state of its own.
            if (_samus.Shinespark.TryStoreFromSpeedBooster(_samus.HorizontalSpeed.SpeedBoostCounter))
            {
                // The successful native store replaces the shared palette handler
                // and timer even if an interrupted Flash was keeping them alive.
                _samus.CrystalFlash.RelinquishPaletteHandler();
                // Reserve Mode keeps only X-Ray's HDMA object. Crouch storage replaces
                // shared palette handler eight with the stored-shine handler one.
                _samus.Xray.RelinquishPaletteHandler();
            }

            _samus.Pose = targetPose;
            _samus.PoseMetadata.RefreshCollisionRadii();

            // Prospective command seven reads five from $91:ED36, installs the target's
            // radius 16, then probes those five pixels down through $94:96AB. On level
            // ground the bottom boundary is unchanged; descending a slope, the probe meets
            // the surface first and the shorter clipped distance is what moves center Y.
            BlockMoveResult alignment = _samus.ProbeChangedPoseVertical(
level, SnesSixteenSixteen.FromWhole(SamusPostureDefinitions.CrouchEntryDownwardPixels),
                (nmiFrameCounter & 1) == 0, targetPose, plms);
            _samus.Kinematics.Position = _samus.Kinematics.Position.WithWholeY(unchecked((ushort)(
                _samus.Kinematics.Position.Y.Whole + (SnesSixteenSixteen.WholeOf(alignment.AcceptedDisplacement)))));
            // Command seven publishes the aligned whole Y before scrolling; the
            // posture change itself must not become camera movement.
            _samus.WritePreviousYPosition(_samus.Kinematics.Position.Y.Whole);
            _samus.Presentation.InitializeAnimation(initialFrame: 0);
            return true;
        }

        // Expanding from crouch radius 16 to standing-transition radius 21 invokes the
        // common pose-change collision routine before initialization. The shared helper
        // reads the target radius from this ROM rather than assuming the ordinary value 21.
        SamusPoseId sourcePose = _samus.Pose;
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

                level,
                targetPose,
                nmiFrameCounter,
                plms,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        _samus.Pose = targetPose;
        // `$91:FDAE` restores the source radius before returning. The prospective
        // standing-transition geometry was used only by the collision probes above;
        // ordinary alpha publishes the target radius at the start of the next frame.
        // Keeping that one-frame handoff is observable when the crouched body is resting
        // on a frozen enemy inside a ceiling, as in the Botwoon-pipe Mochtroid clip.
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies the ordinary Morph-Ball entry and exit records selected by the crouch and
    /// ball transition tables. This includes item gating, command-seven bottom alignment,
    /// and the block-only expansion collision used when unmorphing.
    /// </summary>
    /// <remarks>
    /// `$37/$38` shrink the current humanoid radius to seven and command seven moves center
    /// Y down by the table's nine-pixel request, clipped against room blocks using the
    /// new radius. This is not the source/target radius difference. `$3D/$3E` expand 7 to 16 through
    /// `$91:FDAE`; floor collision normally moves center up nine. If both sides constrain a
    /// radius-seven body, `$91:FFA7` rejects the target and keeps Samus morphed.
    /// </remarks>
    public bool TryApplyMorphTransition(

        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        // `$91:F7CE` does not require movement type five. Falling straight-down `$2D/$2E`
        // and spin-jump input tables can select the same `$37/$38` initializer. The literal
        // previous-movement-type test below even has a dedicated type-three side effect.
        // Validate only the facing-preserving relationship guaranteed by the ROM table.
        byte sourceDirection = _samus.PoseMetadata.ReadPoseXDirection();
        bool startsMorphingRight = sourceDirection == 8 &&
            targetPose == SamusPoseId.MorphingTransitionRightPose;
        bool startsMorphingLeft = sourceDirection == 4 &&
            targetPose == SamusPoseId.MorphingTransitionLeftPose;
        bool startsMorphing = startsMorphingRight || startsMorphingLeft;
        bool startsUnmorphingRight =
            _samus.Pose is SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallMovingRightPose or
                SamusPoseId.MorphBallFallingRightPose or SamusPoseId.SpringBallGroundRightPose or
                SamusPoseId.SpringBallMovingRightPose or SamusPoseId.SpringBallFallingRightPose or
                SamusPoseId.SpringBallJumpRightPose &&
            targetPose == SamusPoseId.UnmorphingTransitionRightPose;
        bool startsUnmorphingLeft =
            _samus.Pose is SamusPoseId.MorphBallGroundLeftPose or SamusPoseId.MorphBallMovingLeftPose or
                SamusPoseId.MorphBallFallingLeftPose or SamusPoseId.SpringBallGroundLeftPose or
                SamusPoseId.SpringBallMovingLeftPose or SamusPoseId.SpringBallFallingLeftPose or
                SamusPoseId.SpringBallJumpLeftPose &&
            targetPose == SamusPoseId.UnmorphingTransitionLeftPose;
        bool startsUnmorphing = startsUnmorphingRight || startsUnmorphingLeft;
        if (!startsMorphing && !startsUnmorphing)
        {
            // Morph and unmorph use a closed, facing-preserving table. Other ball pose
            // changes are handled by ApplyMorphBallPoseChange and must not enter here.
            throw new InvalidOperationException(
                $"Morph transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not a ROM-table route.");
        }

        if (startsMorphing)
        {
            // InitializeSamusPose_MorphingTransition at $91:F7CE restores PreviousPose and
            // returns carry set when bit $0004 is absent. No radius, animation, or position
            // write from the rejected prospective pose survives that native frame.
            if (!_samus.EquippedItems.HasAny(SamusEquipmentFlags.MorphBall))
                return false;

            SamusMovementType previousMovementType = _samus.PoseMetadata.ReadMovementType();
            ushort previousRadius = _samus.Kinematics.Radius.Y;
            _samus.Pose = targetPose;
            _samus.PoseMetadata.RefreshCollisionRadii();
            if (_samus.Kinematics.Radius.Y > previousRadius)
            {
                throw new InvalidDataException(
                    $"Morph entry ${(int)targetPose:X2} expanded radius {previousRadius} -> {_samus.Kinematics.Radius.Y}.");
            }
            BlockMoveResult alignment = _samus.ProbeChangedPoseVertical(
level, SnesSixteenSixteen.FromWhole(SamusPostureDefinitions.MorphEntryDownwardPixels),
                (nmiFrameCounter & 1) == 0, targetPose, plms);
            _samus.Kinematics.Position = _samus.Kinematics.Position.WithWholeY(unchecked((ushort)(
                _samus.Kinematics.Position.Y.Whole + (SnesSixteenSixteen.WholeOf(alignment.AcceptedDisplacement)))));
            // Command seven ($91:ED0E) replaces the previous whole-Y checkpoint
            // after alignment, so the camera does not count pose displacement as motion.
            _samus.WritePreviousYPosition(_samus.Kinematics.Position.Y.Whole);

            // `$91:F7D6-$F7E4` deliberately recognizes a spin-jump source and forces mode
            // two so the compact body retains decelerating aerial momentum after morphing.
            if (previousMovementType == SamusMovementType.SpinJumping)
                _samus.HorizontalSpeed.AccelerationMode = 2;

            // This is `$91:F7E7`, not the arm-cannon flare counter. Bomb-spread charging is
            // invalid once the body has entered either ordinary or Spring Ball form.
            _samus.BombSpreadChargeTimeoutCounter = 0;

            // Prospective command seven also cancels an active bounce before starting the
            // transition. Ordinary crouch entry normally sees zero, but retaining the
            // literal writes makes externally stimulated debugger states deterministic.
            CancelBounceForPostureTransition();
            _samus.Presentation.InitializeAnimation(initialFrame: 0);
            return true;
        }

        SamusPoseId sourcePose = _samus.Pose;
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // A morph source can only produce RetainSource here. Keep the defensive branch
            // explicit so a future caller cannot accidentally apply non-morph crouch fallback.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        _samus.Pose = targetPose;
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        _samus.WritePreviousYPosition(_samus.Kinematics.Position.Y.Whole);
        // Unmorph uses the same prospective command seven as morph. Its zero
        // alignment-table entry does not bypass the subsequent bounce cancellation.
        // The prospective collision probes used the target radius, but the live
        // word remains unchanged until alpha's Samus_SetRadius next frame. This
        // matters to the later enemy-projectile pickup overlap tests.
        CancelBounceForPostureTransition();
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        return true;
    }

    /// <summary>Shared bounce cancellation in native prospective command seven.</summary>
    private void CancelBounceForPostureTransition()
    {
        if (_samus.MorphBallBounceState == SamusBallBounceState.None)
            return;
        _samus.MorphBallBounceState = SamusBallBounceState.None;
        _samus.Kinematics.YSubspeed = 0;
        _samus.Kinematics.YSpeed = 0;
        _samus.Kinematics.YDirection = SamusVerticalDirection.None;
    }

    /// <summary>
    /// Applies a same-facing falling pose selected while the `$3D/$3E` unmorph animation
    /// is still active, without replacing the live aerial velocity.
    /// </summary>
    /// <remarks>
    /// The cartridge's unmorph input table remains active before animation command `$FD`
    /// reaches its normal `$27/$28` crouch endpoint. Up, aim, and Fire input can therefore
    /// interrupt the transition with an ordinary movement-type-six falling record. This is
    /// not a fresh jump or an animation-completion shortcut: it enters the shared bank-$91
    /// changed-pose initializer, makes room for the target body's potentially larger radius,
    /// and preserves the existing X/Y speed words that describe Samus's current fall.
    /// </remarks>
    public bool TryApplyUnmorphToFallingTransition(

        RoomLevelData level,
        SamusPoseId targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = _samus.Pose;
        bool rightFacingRoute = sourcePose == SamusPoseId.UnmorphingTransitionRightPose &&
            SamusPoseMetadata.IsRightFacingFallingPose(targetPose);
        bool leftFacingRoute = sourcePose == SamusPoseId.UnmorphingTransitionLeftPose &&
            SamusPoseMetadata.IsLeftFacingFallingPose(targetPose);
        if (!rightFacingRoute && !leftFacingRoute)
        {
            // `$91:A3F6/$91:A476` expose only the falling family matching the active
            // unmorph body's direction. Rejecting every other pairing keeps this helper
            // from silently becoming a catch-all for unrelated movement initializers.
            throw new InvalidOperationException(
                $"Unmorph-to-falling transition ${(int)sourcePose:X2} -> ${(int)targetPose:X2} is not a same-facing retail route.");
        }

        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // Radius sixteen is already non-ball geometry. If both sides constrain an
            // attempted full-height falling body, `$91:FFA7` selects stable crouch just as
            // it does for aimed crouch and compact-aerial expansion. A failed compensating
            // probe instead retains the visible unmorph frame and all live speed words.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return false;
        }

        _samus.Pose = targetPose;
        _samus.PoseMetadata.RefreshCollisionRadii();
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));

        // InitializeAnimation resets only the target art stream. In particular, do not call
        // Make_Samus_Jump or clear YDirection/YSpeed/YSubspeed: the input record can occur
        // halfway through an actual fall, as it does when leaving Parlor's Morph Ball tunnel.
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Installs a stable ordinary Morph-Ball pose while preserving the shared eight-frame
    /// rolling animation exactly as <c>InitializeSamusPose_MorphBall</c> requests.
    /// </summary>
    public void ApplyMorphBallPoseChange(SamusPoseId targetPose)
    {
        bool sourceSupported = SamusPoseMetadata.IsStableBallPose(_samus.Pose);
        bool targetSupported = SamusPoseMetadata.IsStableBallPose(targetPose);
        if (!sourceSupported || !targetSupported)
        {
            throw new InvalidOperationException(
                $"Stable Morph-Ball transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} requires stable ball endpoints.");
        }

        SamusFacingDirection previousDirection = _samus.PoseMetadata.ReadFacingDirection();
        int previousDelayList = _samus.AnimationDelayListAddress;
        _samus.Pose = targetPose;
        _samus.PoseMetadata.RefreshCollisionRadii();

        // All ordinary ball poses point at `$91:B378`. Native writes $8000 to the new-frame
        // selector, causing `$91:FB5C` to return without changing frame OR timer. Assert the
        // table identity instead of depending on that retail-data fact silently.
        int targetDelayList = _samus.Presentation.ResolveAnimationDelayList();
        if (previousDelayList != targetDelayList)
        {
            throw new InvalidDataException(
                $"Morph-Ball transition selected mismatched animation lists ${previousDelayList:X6}/${targetDelayList:X6}.");
        }

        SamusFacingDirection currentDirection = _samus.PoseMetadata.ReadFacingDirection();
        bool reversed = (previousDirection == SamusFacingDirection.Right && currentDirection == SamusFacingDirection.Left) ||
            (previousDirection == SamusFacingDirection.Left && currentDirection == SamusFacingDirection.Right);
        if (reversed)
        {
            // `$91:FA32-$91:FA52` folds run momentum into base speed with one 16-bit carry,
            // clears the extra words, and selects mode one so displacement initially keeps
            // travelling in the old direction while the ball decelerates through its turn.
            uint combined = unchecked(_samus.HorizontalSpeed.BaseFixed +
                (SnesSixteenSixteen.FromWhole((uint)_samus.HorizontalSpeed.ExtraRunSpeed)) +
                _samus.HorizontalSpeed.ExtraRunSubspeed);
            _samus.HorizontalSpeed.BaseSpeed = SnesFixedPosition.WholeOf(unchecked((int)combined));
            _samus.HorizontalSpeed.BaseSubspeed = unchecked((ushort)combined);
            // `$91:FA4C` invokes `Samus_CancelSpeedBoost` between the 16.16 fold and the
            // explicit extra-word clear. Preserve that ordering even for ordinary Dash.
            _samus.HorizontalSpeed.CancelRunningMomentum(currentDirection);
            _samus.HorizontalSpeed.ExtraRunSpeed = 0;
            _samus.HorizontalSpeed.ExtraRunSubspeed = 0;
            _samus.HorizontalSpeed.AccelerationMode = 1;
        }
    }

    /// <summary>
    /// Applies the winning solid-ceiling transition at $91:EFDF. Call after higher
    /// priority animation and hit interruptions, not from the collision scan.
    /// </summary>
    public void ApplySolidCeilingCollision()
    {
        _samus.Kinematics.YSpeed = 0;
        _samus.Kinematics.YSubspeed = 0;
        _samus.Kinematics.YDirection = SamusVerticalDirection.Down;
    }

    /// <summary>
    /// Resolves an ordinary Morph-Ball downward collision through `$91:EA07` and
    /// `$91:F1FC`, including both automatic rebounds and the final grounded pose.
    /// </summary>
    /// <returns>True only when the collision ends grounded; false means another rebound.</returns>
    public bool ApplyMorphBallLanding()
    {
        if (!SamusPoseMetadata.IsAirborneMorphBallPose(_samus.Pose) && !SamusPoseMetadata.IsGroundedMorphBallPose(_samus.Pose))
            throw new InvalidOperationException($"Morph-Ball landing requires airborne pose $31/$32, not ${(int)_samus.Pose:X2}.");

        // The cartridge tests the sign of a word subtraction, not an unsigned
        // magnitude. Moonfall's negative velocity therefore lands without a rebound.
        if (_samus.MorphBallBounceState == SamusBallBounceState.None &&
            unchecked((short)(_samus.Kinematics.YSpeed - SamusMovementRomData.FirstBallBounceMinimumSpeed)) >= 0)
        {
            _samus.MorphBallBounceState = SamusBallBounceState.FirstRebound;
            _samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            _samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed;
            _samus.Kinematics.YSubspeed = SamusVerticalMotionDefinitions.BallBounceSubspeed;
            return false;
        }

        if (_samus.MorphBallBounceState == SamusBallBounceState.FirstRebound)
        {
            _samus.MorphBallBounceState = SamusBallBounceState.SecondRebound;
            _samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            _samus.Kinematics.YSpeed = unchecked((ushort)(SamusVerticalMotionDefinitions.BallBounceSpeed - 1));
            _samus.Kinematics.YSubspeed = SamusVerticalMotionDefinitions.BallBounceSubspeed;
            return false;
        }

        _samus.MorphBallBounceState = SamusBallBounceState.None;
        _samus.Kinematics.YDirection = SamusVerticalDirection.None;
        _samus.Kinematics.YSpeed = 0;
        _samus.Kinematics.YSubspeed = 0;
        SamusPoseId groundedPose = _samus.PoseMetadata.IsFacingLeft()
            ? SamusPoseId.MorphBallGroundLeftPose
            : SamusPoseId.MorphBallGroundRightPose;
        ApplyMorphBallPoseChange(groundedPose);
        // $91:F02D-$F033 clears base momentum only when the bounce handler returns
        // carry clear. Both airborne rebounds above retain their horizontal speed.
        _samus.HorizontalSpeed.AccelerationMode = 0;
        _samus.HorizontalSpeed.BaseSpeed = 0;
        _samus.HorizontalSpeed.BaseSubspeed = 0;
        return true;
    }

    /// <summary>
    /// Resolves `$91:F25E` for Spring Ball. Holding Jump immediately calls the same
    /// cartridge-backed jump initializer as a grounded Spring Ball press; otherwise the
    /// low bounce byte advances through two rebounds while high byte `$0600` records the
    /// native Spring Ball bounce family.
    /// </summary>
    public bool ApplySpringBallLanding(SnesButton controllerInput)
    {
        if (!SamusPoseMetadata.IsAirborneSpringBallPose(_samus.Pose) && !SamusPoseMetadata.IsGroundedSpringBallPose(_samus.Pose))
            throw new InvalidOperationException($"Spring-Ball landing requires pose $7D-$80, not ${(int)_samus.Pose:X2}.");

        if ((controllerInput & SnesButton.A) != 0)
        {
            _samus.MorphBallBounceState = SamusBallBounceState.None;
            SamusAerialMovement.InitializeJump(_samus);
            // Native landing relaunch only initializes velocity. It retains the
            // current airborne pose/animation, unlike a fresh grounded Jump input.
            // Changing pose here also selects a different next-frame movement path.
            return false;
        }

        // $91:F26E dispatches on the low (rebound) byte only, so an ordinary-ball
        // rebound left over from a Spring Ball toggle selects the same rebound step.
        if (_samus.MorphBallBounceState == SamusBallBounceState.None &&
            unchecked((short)(_samus.Kinematics.YSpeed - SamusMovementRomData.FirstBallBounceMinimumSpeed)) >= 0)
        {
            _samus.MorphBallBounceState = SamusBallBounceState.SpringBallFirstRebound;
            _samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            _samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.BallBounceSpeed;
            _samus.Kinematics.YSubspeed = SamusVerticalMotionDefinitions.BallBounceSubspeed;
            return false;
        }

        if (_samus.MorphBallBounceState is SamusBallBounceState.FirstRebound or SamusBallBounceState.SpringBallFirstRebound)
        {
            _samus.MorphBallBounceState = SamusBallBounceState.SpringBallSecondRebound;
            _samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            _samus.Kinematics.YSpeed = unchecked((ushort)(SamusVerticalMotionDefinitions.BallBounceSpeed - 1));
            _samus.Kinematics.YSubspeed = SamusVerticalMotionDefinitions.BallBounceSubspeed;
            return false;
        }

        _samus.MorphBallBounceState = SamusBallBounceState.None;
        _samus.Kinematics.YDirection = SamusVerticalDirection.None;
        _samus.Kinematics.YSpeed = 0;
        _samus.Kinematics.YSubspeed = 0;
        SamusPoseId groundPose = _samus.PoseMetadata.IsFacingLeft()
            ? SamusPoseId.SpringBallGroundLeftPose
            : SamusPoseId.SpringBallGroundRightPose;
        ApplyMorphBallPoseChange(groundPose);
        // The common landing caller clears base speed when the bounce routine
        // returns carry clear. Spring Ball shares this with ordinary ball landing.
        _samus.HorizontalSpeed.AccelerationMode = 0;
        _samus.HorizontalSpeed.BaseSpeed = 0;
        _samus.HorizontalSpeed.BaseSubspeed = 0;
        return true;
    }

    /// <summary>
    /// Applies `$91:FC18` when grounded Spring Ball `$79/$7A` changes to `$7F/$80`.
    /// Moving-ground poses first pass through the same table-selected target, but the
    /// initializer only launches when the previous movement type was exactly `$11`.
    /// </summary>
    public void ApplySpringBallJump(SamusPoseId targetPose)
    {
        bool validTarget = targetPose is SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose;
        if (!SamusPoseMetadata.IsGroundedSpringBallPose(_samus.Pose) || !validTarget)
            throw new InvalidOperationException(
                $"Spring-Ball jump ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is outside the grounded-to-airborne Spring-Ball route.");

        ApplyMorphBallPoseChange(targetPose);
        _samus.MorphBallBounceState = SamusBallBounceState.None;
        SamusAerialMovement.InitializeJump(_samus);
    }

    /// <summary>
    /// Applies `$91:E8F2` after a bomb-jump frame moved down without a floor. Only the pose
    /// changes: the airborne ball's initializer leaves vertical speed for the next
    /// handler frame, whose underflow test turns the jump downward. Movement types whose
    /// `$90:E65A` entry is "no change" keep their pose.
    /// </summary>
    public void ApplyBombJumpFallingPose()
    {
        bool left = _samus.PoseMetadata.IsFacingLeft();
        if (SamusPoseMetadata.IsGroundedSpringBallPose(_samus.Pose))
            ApplyMorphBallPoseChange(left ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.SpringBallFallingRightPose);
        else if (SamusPoseMetadata.IsGroundedMorphBallPose(_samus.Pose))
            ApplyMorphBallPoseChange(left ? SamusPoseId.MorphBallFallingLeftPose : SamusPoseId.MorphBallFallingRightPose);
        else if (_samus.PoseMetadata.ReadMovementType() is SamusMovementType.Standing or SamusMovementType.Running or
                 SamusMovementType.Crouching or SamusMovementType.Moonwalking or SamusMovementType.RanIntoWall)
            throw new InvalidOperationException(
                $"Bomb-jump falling result for humanoid pose ${(int)_samus.Pose:X2} ($91:E8F2 airborne) is not translated.");
    }

    /// <summary>
    /// Applies `$91:E8F2`'s type-four walk-off endpoint, retaining the rolling animation
    /// while changing to ordinary airborne pose `$31/$32` and starting dry-air gravity.
    /// </summary>
    public void ApplyMorphBallWalkOff()
    {
        bool springBall = SamusPoseMetadata.IsGroundedSpringBallPose(_samus.Pose);
        if (!SamusPoseMetadata.IsGroundedMorphBallPose(_samus.Pose) && !springBall)
            throw new InvalidOperationException($"Morph-Ball walk-off requires grounded pose, not ${(int)_samus.Pose:X2}.");

        SamusPoseId fallingPose = _samus.PoseMetadata.IsFacingLeft()
            ? springBall ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.MorphBallFallingLeftPose
            : springBall ? SamusPoseId.SpringBallFallingRightPose : SamusPoseId.MorphBallFallingRightPose;
        _samus.Kinematics.YSpeed = 0;
        _samus.Kinematics.YSubspeed = 0;
        _samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        SamusAerialMovement.ConfigureEnvironmentGravity(_samus);
        ApplyMorphBallPoseChange(fallingPose);
    }

    /// <summary>
    /// Installs the ordinary or aimed falling pose selected by <c>$91:E8F2</c> when a
    /// grounded movement probe finds no floor. The collision command clears vertical
    /// speed and starts downward gravity before the pose is drawn.
    /// </summary>
    public void ApplyWalkedOffFloorTransition(
RoomLevelData level, SamusPoseId targetPose,
        ushort nmiFrameCounter = 0, RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        bool supportedSource =
            SamusPoseMetadata.IsRightFacingStandingPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingStandingPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingLandingPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingLandingPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingRunningPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(_samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkingPose(_samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkTurnJumpPose(_samus.Pose) ||
            _samus.Pose is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose ||
            SamusPoseMetadata.IsRanIntoWallPose(_samus.Pose) ||
            SamusPoseMetadata.IsRightFacingCrouchingPose(_samus.Pose) || SamusPoseMetadata.IsLeftFacingCrouchingPose(_samus.Pose);
        SamusPoseId expectedTarget = SelectFallingPoseForCurrentAim();
        if (!supportedSource || targetPose != expectedTarget)
        {
            // The caller computes the target from this pose's ROM metadata immediately
            // before invoking us. A mismatch is an inconsistent request, not missing logic.
            throw new InvalidOperationException(
                $"Walk-off transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} does not match the pose's ROM-selected falling target ${(int)expectedTarget:X2}.");
        }

        // A crouched or compact source can grow when falling is selected. Native
        // F404 runs the same pose-expansion collision as an input-driven transition,
        // including its fractional clamp. Skipping it embeds the new body in the
        // floor after a near-ground unmorph. Rejection also skips command five.
        SamusPoseId sourcePose = _samus.Pose;
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(
level, targetPose, nmiFrameCounter, plms, out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
            return;
        }
        _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
        // Command five's no-floor branch ($91:EFEF) preserves an ascending
        // velocity, even if a special handler left it on a standing pose. Crystal
        // Flash can do exactly that; unconditional reset shortens its return arc.
        if (_samus.Kinematics.YDirection != SamusVerticalDirection.Up)
        {
            _samus.MorphBallBounceState = SamusBallBounceState.None;
            _samus.Kinematics.YSpeed = 0;
            _samus.Kinematics.YSubspeed = 0;
            _samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }
        SamusAerialMovement.ConfigureEnvironmentGravity(_samus);
        _samus.Pose = targetPose;
        // Alpha publishes the falling radius next frame; pose commit retains the
        // radius used by this frame's grounded movement and collision probes.
        // SamusFunc_F433 dispatches the new falling movement type through $91:F60D
        // before command five initializes the downward state. That initializer derives
        // the mode from extra dash speed; it must not retain the grounded release mode.
        // Base speed itself survives until the next aerial movement routine consumes it.
        InitializeOrdinaryAerialAcceleration();
        _samus.Presentation.InitializeAnimation(initialFrame: 0);
    }

    /// <summary>
    /// Ports the facing lookup used by `$91:E8F2` for a grounded walk-off.
    /// </summary>
    public SamusPoseId SelectFallingPoseForCurrentAim()
    {
        // PSP_Falling indexes its pair with PoseXDirection, not the muzzle direction.
        // Moonwalk's visual facing opposes this physics byte; aim input is reconsidered
        // by the next frame's input handler after this ordinary falling pose is installed.
        return _samus.PoseMetadata.IsFacingLeft() ? SamusPoseId.FallingLeftPose : SamusPoseId.FallingRightPose;
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
    public void ApplyAerialLanding(
        bool wasSpinning,
        SnesButton controllerInput = SnesButton.None)
    {
        SamusPoseId sourcePose = _samus.Pose;
        bool leavingScrewAttack = SamusPoseMetadata.IsScrewAttackPose(sourcePose);
        SamusPoseId targetPose = SelectAerialLandingPose(wasSpinning, controllerInput);

        ushort oldRadius = _samus.Kinematics.Radius.Y;
        _samus.Pose = targetPose;
        _samus.PoseMetadata.RefreshCollisionRadii();
        if (_samus.Kinematics.Radius.Y > oldRadius)
        {
            ushort difference = unchecked((ushort)(_samus.Kinematics.Radius.Y - oldRadius));
            _samus.Kinematics.Position -= new Offset16<RoomPixels>(0, unchecked((short)difference));
        }

        ApplyAerialLandingCollisionCommand(leavingScrewAttack);
        _samus.Presentation.InitializeAnimation(initialFrame: StandingPoseInitialFrame(sourcePose, targetPose));
    }

    /// <summary>
    /// Applies a normal or spinning landing through the cartridge's shared prospective-pose
    /// collision handler before committing the taller landing body.
    /// </summary>
    /// <returns>
    /// True when the selected landing pose fit. False means native collision retained the
    /// source pose or selected its stable-crouch fallback; native skips the landing command.
    /// </returns>
    public bool TryApplyAerialLanding(

        RoomLevelData level,
        bool wasSpinning,
        SnesButton controllerInput,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);

        SamusPoseId sourcePose = _samus.Pose;
        bool leavingScrewAttack = SamusPoseMetadata.IsScrewAttackPose(sourcePose);
        SamusPoseId targetPose = SelectAerialLandingPose(wasSpinning, controllerInput);
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            _samus.Pose = targetPose;
            // Correct the center now, but retain the movement frame's live radius
            // until alpha, including when landing on a frozen enemy.
            _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
            _samus.Presentation.InitializeAnimation(initialFrame: StandingPoseInitialFrame(sourcePose, targetPose));
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            // `$91:FFA7` deliberately chooses stable crouch for a non-Morph source when
            // both the floor and ceiling reject the full landing body. This is what lets
            // a spin jump land in a 32-pixel passage without embedding Samus in its ceiling.
            _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

        // F404 compares the requested pose with the post-collision pose. A rejected
        // expansion or crouch substitution returns carry and skips command five; this
        // is not an accepted landing and must retain the vertical motion words.
        if (collision == LargerPoseCollisionOutcome.Allowed)
            ApplyAerialLandingCollisionCommand(leavingScrewAttack);
        else if (leavingScrewAttack)
            // Palette restoration belongs to F433, before the command carry gate.
            _samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Keeps the gun raised on frame one when both standing poses aim straight up;
    /// other standing transitions begin at frame zero.
    /// </summary>
    private static ushort StandingPoseInitialFrame(SamusPoseId sourcePose, SamusPoseId targetPose)
    {
        // $91:F4DC keeps the gun raised when both the old and new standing
        // initializer's poses aim straight up, including landing and its completion.
        return SamusPoseMetadata.ReadMovementType(targetPose) == SamusMovementType.Standing &&
            SamusPoseMetadata.ReadShotDirection(sourcePose) is 0 or 9 &&
            SamusPoseMetadata.ReadShotDirection(targetPose) is 0 or 9 ? (ushort)1 : (ushort)0;
    }

    /// <summary>Chooses <c>$91:E95D</c>'s prospective landing pose.</summary>
    private SamusPoseId SelectAerialLandingPose(
        bool wasSpinning,
        SnesButton controllerInput)
    {
        byte direction = _samus.PoseMetadata.ReadPoseXDirection();
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
        return _samus.PoseMetadata.ReadShotDirection() switch
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
    private void ApplyAerialLandingCollisionCommand(bool leavingScrewAttack)
    {
        // $91:F1EC installs the same one-shot input handler used by F8.
        // Its short held-Jump window is evaluated next alpha, not during landing.
        if (!_samus.InputLocked) _samus.InputSnapshot.AutoJumpInputPending = true;
        _samus.HorizontalSpeed.AccelerationMode = 0;
        _samus.HorizontalSpeed.BaseSpeed = 0;
        _samus.HorizontalSpeed.BaseSubspeed = 0;
        _samus.Kinematics.YSpeed = 0;
        _samus.Kinematics.YSubspeed = 0;

        // `$91:F433` reloads the normal suit palette whenever a pose change leaves spin or
        // wall-jump movement while Screw Attack is equipped. The host palette copy occurs
        // at the later `$91:D6F7` phase, so publish that same deferred work here.
        if (leavingScrewAttack)
            _samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        _samus.Kinematics.YDirection = SamusVerticalDirection.None;
    }

    /// <summary>
    /// Applies `$91:E9F3` directions four/five when radius-ten straight-down Samus lands.
    /// Both entries select ordinary `$A4/$A5`; the 10 -> 21 expansion still runs the full
    /// block pose-change collision resolver; command five clears motion only if accepted.
    /// </summary>
    public bool TryApplyCompactAerialLanding(

        RoomLevelData level,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        if (!SamusPoseMetadata.IsCompactAerialPose(_samus.Pose))
            throw new InvalidOperationException($"Compact landing requires pose $17/$18/$2D/$2E, not ${(int)_samus.Pose:X2}.");

        SamusPoseId sourcePose = _samus.Pose;
        SamusPoseId targetPose = _samus.PoseMetadata.ReadShotDirection() switch
        {
            4 => SamusPoseId.NormalLandingRightPose,
            5 => SamusPoseId.NormalLandingLeftPose,
            byte shotDirection => throw new InvalidOperationException(
                $"Compact pose ${(int)_samus.Pose:X2} has unexpected shot direction ${shotDirection:X2}."),
        };
        LargerPoseCollisionOutcome collision = _samus.ResolveLargerPoseCollision(

            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            _samus.Pose = targetPose;
            // $91:FDAE leaves $0B00 at the compact body's radius: this frame's enemy and
            // projectile collision still use it, and alpha's SetSamusRadius takes the
            // landing body only next frame, as for an ordinary landing.
            _samus.Kinematics.Position += new Offset16<RoomPixels>(0, unchecked((short)centerAdjustment));
            _samus.Presentation.InitializeAnimation(initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            _samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
        }

        // Like ordinary landings, F404's changed-pose result gates command five.
        // A collision-selected crouch is not allowed to clear velocity or arm autojump.
        if (collision == LargerPoseCollisionOutcome.Allowed)
            ApplyAerialLandingCollisionCommand(leavingScrewAttack: false);
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Consumes command $FD/$F8's command-three pose operand for every animation route in
    /// the current grounded/ordinary-air slice.
    /// </summary>
    public bool ApplyPendingVerifiedAnimationTransition()
    {
        if (_samus.PendingTransitionalPose is not SamusPoseId targetPose)
            return false;

        // Native pose initialization computes its own local delay without replacing
        // the buffer published by this frame's FX pass. Command three adds that live
        // buffer once more after installing the new animation.
        ushort commandAnimationBuffer = _samus.AnimationFrameBuffer;

        bool verified = (_samus.Pose, targetPose) is
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
                $"Animation transition ${(int)_samus.Pose:X2} -> ${(int)targetPose:X2} is not an active retail animation-command route.");
        }

        bool startsMoonwalkJump = SamusPoseMetadata.IsMoonwalkTurnJumpPose(_samus.Pose) &&
            targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose;
        SamusPoseId sourcePose = _samus.Pose;
        SamusMovementType previousMovementType = SamusPoseMetadata.ReadMovementType(sourcePose);

        // `$91:F404` runs movement-type initialization after the animation command has
        // selected the new normal-jump pose but before it initializes that pose's frame.
        // A live stored shine replaces the target with `$C7/$C8`, so never briefly seed
        // `$4D/$4E/$15/$16/$69/$6A` animation state in this branch.
        bool beganShinespark = TryBeginShinesparkWindup(
            targetPose,
            previousMovementType);
        if (!beganShinespark)
        {
            // Moonwalk turn lists end in literal `$F8,$19/$1A`. That operand is still fed
            // through movement-type initialization, so equipment substitution occurs here
            // just as it does for an input-table jump from ordinary running.
            SamusPoseId installedPose = startsMoonwalkJump
                ? SelectEquippedSpinPose(targetPose)
                : targetPose;
            // Animation-owned pose changes still run the target movement initializer.
            // In particular an aerial turn must stop decelerating in the old direction
            // when its final normal-jump/falling pose is installed, even if water made
            // the turn finish before base speed reached zero. Do not clear that speed.
            if (SamusPoseMetadata.ReadMovementType(installedPose) is SamusMovementType.NormalJumping or SamusMovementType.Falling)
                InitializeOrdinaryAerialAcceleration();
            ApplySimpleGroundedPoseChange(sourcePose, installedPose, "Animation command",
                refreshRadius: !SamusPoseMetadata.IsAerialTurnPose(sourcePose));
        }
        if (sourcePose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or
            SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
        {
            // The actor-owned release command has now reached its ROM-authored normal pose.
            // Clear only the host handler marker; pose initialization above already owns the
            // same radius and animation writes as native `$91:F404/$91:FB08`.
            _samus.Drained.CompleteRelease();
        }
        // Super-special command three does not run FBBB/FC99. An input-table
        // transition out of the same turn art DOES initialize jump velocity; keeping
        // those two routes distinct preserves both ordinary jumping and Moonfall.
        _samus.MorphBallBounceState = SamusBallBounceState.None;
        _samus.AnimationFrameTimer = NativeCountdown.FromRaw(unchecked((ushort)(_samus.AnimationFrameTimer.RawValue + commandAnimationBuffer)));
        return true;
    }

    /// <summary>
    /// Installs one already-validated grounded pose and runs the common $91:F404/$91:FB08
    /// metadata/radius work, initializing frame zero only for a changed pose.
    /// </summary>
    private void ApplySimpleGroundedPoseChange(
        SamusPoseId expectedPose,
        SamusPoseId targetPose,
        string transitionName,
        bool refreshRadius = true)
    {
        if (_samus.Pose != expectedPose)
        {
            throw new InvalidOperationException(
                $"{transitionName} requires pose ${(int)expectedPose:X2}, not ${(int)_samus.Pose:X2}.");
        }

        _samus.Pose = targetPose;
        // Aerial turn completion runs F433 without SetRadius; its source body
        // remains active until next alpha, including compact down-aim targets.
        if (refreshRadius)
            _samus.PoseMetadata.RefreshCollisionRadii();
        // $91:FB64-$FB67 retains the frame/timer when the ordinary pose is
        // unchanged. A prospective run rejected by the wall probe can resolve
        // back to the current wall-stop pose on every held-input frame.
        if (expectedPose != targetPose)
            _samus.Presentation.InitializeAnimation(initialFrame: StandingPoseInitialFrame(expectedPose, targetPose));
    }
}
