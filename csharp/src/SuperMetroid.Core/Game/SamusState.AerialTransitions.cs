using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Aerial turns, wall jumps, grapple drops, and shinespark/jump pose transitions.
/// </summary>
public sealed partial class SamusState
{
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
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter,
        ushort controllerNewInput,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        byte sourcePose = Pose;
        bool normalJumpTarget =
            IsRightFacingNormalJumpPose(targetPose) ||
            IsLeftFacingNormalJumpPose(targetPose);
        bool compactJumpSource = IsSpinJumpPose(sourcePose) || IsWallJumpPose(sourcePose);
        if (!compactJumpSource || !normalJumpTarget ||
            ReadPoseXDirection(bus, sourcePose) != ReadPoseXDirection(bus, targetPose))
        {
            // The six spin/Space/Screw tables and two wall-jump tables emit only same-facing
            // normal-jump records when aim or Shoot cancels rotation. A different movement
            // family is owned by the spin-direction or landing initializers instead.
            throw new InvalidOperationException(
                $"Compact-jump-to-normal-jump transition ${sourcePose:X2} -> " +
                $"${targetPose:X2} is not a same-facing retail route.");
        }

        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
            return false;

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));

        // InitializeSamusPose_NormalJumping chooses acceleration mode two only while some
        // stored extra-run speed remains. It never clears either speed pair on this route.
        HorizontalSpeed.AccelerationMode =
            HorizontalSpeed.ExtraRunSpeed != 0 || HorizontalSpeed.ExtraRunSubspeed != 0
                ? (ushort)2
                : (ushort)0;

        // SamusFunc_F433 reloads the ordinary suit palette whenever the previous movement
        // type was spin/wall-jump and Screw Attack is equipped, even if the visible source
        // happened to be generic spin or Space Jump art.
        if (EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            HorizontalSpeed.RequestNormalSuitPaletteRestore();

        // $91:F5CF publishes the newly installed pose's shot direction on the exact Fire
        // edge that selected this record. The projectile producer consumes it in alpha.
        if ((controllerNewInput & (ushort)SnesButton.X) != 0)
            PoseTransitionShotDirection = unchecked((ushort)(0x8000 | ReadShotDirection(bus)));

        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Compatibility name for the `$13/$14` Fire subset. Keeping this narrow wrapper makes
    /// existing focused tests readable while all input-table exits share one native handler.
    /// </summary>
    public bool TryApplySpinToNormalJumpFireTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter,
        ushort controllerNewInput,
        RoomPlmSystem? plms = null)
    {
        if (targetPose is not (SamusPoseIds.NormalJumpGunExtendedRightPose or SamusPoseIds.NormalJumpGunExtendedLeftPose))
        {
            throw new InvalidOperationException(
                $"Spin-fire compatibility route requires pose $13/$14, not ${targetPose:X2}.");
        }
        return TryApplySpinOrWallJumpToNormalJumpTransition(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            controllerNewInput,
            plms);
    }

    public bool TryApplyAerialTurn(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte genericTargetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        byte sourcePose = Pose;
        SamusMovementType sourceMovementType = ReadMovementType(bus);
        bool jumping = sourceMovementType == SamusMovementType.NormalJumping;
        bool falling = sourceMovementType == SamusMovementType.Falling;
        bool turnsLeft = genericTargetPose == (jumping
            ? SamusPoseIds.TurningRightToLeftJumpPose
            : SamusPoseIds.TurningRightToLeftFallingPose);
        bool turnsRight = genericTargetPose == (jumping
            ? SamusPoseIds.TurningLeftToRightJumpPose
            : SamusPoseIds.TurningLeftToRightFallingPose);
        if ((!jumping && !falling) || (!turnsLeft && !turnsRight))
        {
            throw new InvalidOperationException(
                $"Aerial turn ${sourcePose:X2} -> ${genericTargetPose:X2} is not a verified type-2/type-6 transition.");
        }

        // These are literal copies of `$91:F9D6` and `$91:F9E0`. Directions four and five
        // are not typos: compact down-aim sources reuse `$91/$92` or `$95/$96` according to
        // facing. Retaining all ten entries is essential for a debugger to preserve aim.
        byte shotDirection = ReadShotDirection(bus);
        byte selectedTurnPose = jumping
            ? shotDirection switch
            {
                0 => SamusPoseIds.TurningRightToLeftJumpAimUpPose,
                1 => SamusPoseIds.TurningRightToLeftJumpAimDiagonalUpPose,
                2 => SamusPoseIds.TurningRightToLeftJumpPose,
                3 or 4 => SamusPoseIds.TurningRightToLeftJumpAimDownPose,
                5 or 6 => SamusPoseIds.TurningLeftToRightJumpAimDownPose,
                7 => SamusPoseIds.TurningLeftToRightJumpPose,
                8 => SamusPoseIds.TurningLeftToRightJumpAimDiagonalUpPose,
                9 => SamusPoseIds.TurningLeftToRightJumpAimUpPose,
                _ => throw new InvalidDataException($"Jump pose ${sourcePose:X2} has invalid shot direction ${shotDirection:X2}."),
            }
            : shotDirection switch
            {
                0 => SamusPoseIds.TurningRightToLeftFallingAimUpPose,
                1 => SamusPoseIds.TurningRightToLeftFallingAimDiagonalUpPose,
                2 => SamusPoseIds.TurningRightToLeftFallingPose,
                3 or 4 => SamusPoseIds.TurningRightToLeftFallingAimDownPose,
                5 or 6 => SamusPoseIds.TurningLeftToRightFallingAimDownPose,
                7 => SamusPoseIds.TurningLeftToRightFallingPose,
                8 => SamusPoseIds.TurningLeftToRightFallingAimDiagonalUpPose,
                9 => SamusPoseIds.TurningLeftToRightFallingAimUpPose,
                _ => throw new InvalidDataException($"Fall pose ${sourcePose:X2} has invalid shot direction ${shotDirection:X2}."),
            };

        if ((turnsLeft && !IsRightToLeftAerialTurnPose(selectedTurnPose)) ||
            (turnsRight && !IsLeftToRightAerialTurnPose(selectedTurnPose)))
        {
            throw new InvalidOperationException(
                $"Aerial turn source ${sourcePose:X2} has direction metadata inconsistent with ${genericTargetPose:X2}.");
        }

        // `$91:F952/$91:F98A` run this momentum conversion before the shared pose-change
        // collision resolver. Consequently even a cramped compact-pose rejection consumes
        // extra run speed; moving it after the probe would produce observably different WRAM.
        FoldExtraRunSpeedIntoBaseAndStartTurn();

        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            selectedTurnPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.RetainSource)
            return false;
        if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = selectedTurnPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies `$91:F624` when spin/wall-jump input selects the opposite spin pose. A true
    /// direction reversal folds extra speed into base speed and selects mode one, preserving
    /// old-world direction while the newly facing pose decelerates.
    /// </summary>
    public void ApplySpinJumpDirectionTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsSpinJumpPose(targetPose) ||
            (!IsWallJumpPose(Pose) && !IsSpinJumpPose(Pose)))
        {
            throw new InvalidOperationException(
                $"Spin direction transition ${Pose:X2} -> ${targetPose:X2} is not verified.");
        }

        byte oldDirection = ReadPoseXDirection(bus);
        byte newDirection = ReadPoseXDirection(bus, targetPose);
        if (oldDirection != newDirection)
        {
            FoldExtraRunSpeedIntoBaseAndStartTurn();

            // Unlike bank-$91's grounded/aerial turn initializers, `$91:F624` calls
            // `Samus_CancelSpeedBoost` immediately after folding the extra pair. Waiting
            // for another movement frame would leave `$0B3C` observably stale.
            HorizontalSpeed.CancelRunningMomentum(oldDirection);
        }

        // Ordinary and wall-jump definition fallbacks publish generic `$19/$1A`, whereas
        // the dedicated `$81/$82` and `$1B/$1C` input tables can publish their already-
        // specialized opposite-facing record directly. Reduce either form back to the
        // generic direction before applying `$91:F624`'s live equipment priority. This is
        // observable when both bits are equipped: an `$81 -> $82` ROM record must remain
        // Screw art, while the same directional intent with Screw unequipped becomes Space
        // Jump art instead of trusting stale pose-table equipment state.
        byte genericTarget = newDirection == 4 ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose;
        Pose = SelectEquippedSpinPose(genericTarget);
        RefreshCollisionRadii(bus);

        // InitializeSpinJump writes frame one, skipping the static first spin frame. This
        // is easy to miss because an ordinary ground jump initializes the pose elsewhere.
        InitializeAnimation(bus, initialFrame: 1);
    }

    /// <summary>
    /// Applies `$91:EABE`, solid-collision command five, and `$90:9949` after the block
    /// wall test succeeds. The launch values are read from the cartridge's dry-air tables.
    /// </summary>
    public void ApplyWallJumpTrigger(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsSpinJumpPose(Pose))
            throw new InvalidOperationException($"Wall-jump trigger requires spin pose, not ${Pose:X2}.");

        // `$91:F433` observes previous movement type three plus equipped Screw Attack and
        // immediately reloads the normal suit palette before `$83/$84` starts. The desktop
        // runtime performs palette writes later in the frame, so preserve that phase split.
        if (EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            HorizontalSpeed.RequestNormalSuitPaletteRestore();

        Pose = IsFacingLeft(bus) ? SamusPoseIds.WallJumpLeftPose : SamusPoseIds.WallJumpRightPose;
        RefreshCollisionRadii(bus);

        // `$91:F2D3` clears acceleration and the ordinary base-speed pair before
        // `$90:9949` installs the launch. It deliberately does *not* touch the extra-run
        // pair at `$0B42/$0B44` or the running-momentum flag at `$0B3C`: a wall jump made
        // out of a Dash therefore carries that speed into movement type $14.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;

        // SolidVerticalCollision_WallJumpTriggered queues library-three sound five with a
        // six-entry threshold after clearing the base-speed words and before pose setup.
        LiquidPhysics.QueueMovementSound(library: 3, soundId: 0x05, maximumQueued: 6);

        SamusAerialMovement.InitializeWallJump(bus, this);
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies the pose/launch half of <c>$9B:C9CE</c> after the grapple wall-grace probe
    /// has accepted a fresh Jump edge. This route intentionally reverses the contact pose:
    /// `$B8` launches through left-facing `$84`, while `$B9` launches through `$83`.
    /// </summary>
    /// <remarks>
    /// The eventual seven-pixel horizontal push remains the ordinary `$FB` animation-command
    /// path already implemented by <see cref="AnimateNoFx"/>. This method performs only the
    /// immediate bank-$9B cleanup, prospective-pose command six, and environment-selected
    /// `$90:9949` vertical launch that occur when the wall-jump function runs.
    /// </remarks>
    public void ApplyGrappleWallJump(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose is not (SamusPoseIds.GrappleWallContactLeftPose or SamusPoseIds.GrappleWallContactRightPose))
        {
            throw new InvalidOperationException(
                $"Grapple wall jump requires contact pose $B8/$B9, not ${Pose:X2}.");
        }

        // `$9B:C9D5-$C9EB` tests the contact pose's X-direction byte, not its descriptive
        // wall side. Direction eight selects `$84`; direction four selects `$83`.
        Pose = IsFacingRight(bus) ? SamusPoseIds.WallJumpLeftPose : SamusPoseIds.WallJumpRightPose;
        RefreshCollisionRadii(bus);

        // `$9B:C9CE` clears the ordinary base-speed pair, then the normal pose-transition
        // machinery reaches `$90:9949`. Like the non-grapple route, it leaves both the
        // extra-run pair and `$0B3C` intact, so Dash momentum survives this wall launch.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;

        // `$9B:C9CE` uses generic QueueSound: library one, sound seven, maximum fifteen.
        // It also tears down the active beam flare so the wall-jump's charged-contact path
        // cannot inherit charge accumulated before grapple became active.
        LiquidPhysics.QueueMovementSound(library: 1, soundId: 0x07, maximumQueued: 15);
        ProjectileFlareCounter = 0;

        SamusAerialMovement.InitializeWallJump(bus, this);
        InitializeAnimation(bus, initialFrame: 0);
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
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool supportedSource = Pose is
            SamusPoseIds.GrappleSwingRightPose or SamusPoseIds.GrappleSwingLeftPose or
            SamusPoseIds.GrappleStandingRightPose or SamusPoseIds.GrappleStandingLeftPose or
            SamusPoseIds.GrappleStandingDownRightPose or SamusPoseIds.GrappleStandingDownLeftPose or
            SamusPoseIds.GrappleCrouchingRightPose or SamusPoseIds.GrappleCrouchingLeftPose or
            SamusPoseIds.GrappleCrouchingDownRightPose or SamusPoseIds.GrappleCrouchingDownLeftPose or
            SamusPoseIds.GrappleWallContactLeftPose or SamusPoseIds.GrappleWallContactRightPose;
        bool supportedTarget = targetPose is
            SamusPoseIds.FacingRightNormalPose or SamusPoseIds.FacingLeftNormalPose or
            SamusPoseIds.StandingAimUpRightPose or SamusPoseIds.StandingAimUpLeftPose or
            SamusPoseIds.StandingAimDiagonalUpRightPose or SamusPoseIds.StandingAimDiagonalUpLeftPose or
            SamusPoseIds.StandingAimDiagonalDownRightPose or SamusPoseIds.StandingAimDiagonalDownLeftPose or
            SamusPoseIds.CrouchingRightPose or SamusPoseIds.CrouchingLeftPose or
            SamusPoseIds.CrouchingAimUpRightPose or SamusPoseIds.CrouchingAimUpLeftPose or
            SamusPoseIds.CrouchingAimDiagonalUpRightPose or SamusPoseIds.CrouchingAimDiagonalUpLeftPose or
            SamusPoseIds.CrouchingAimDiagonalDownRightPose or SamusPoseIds.CrouchingAimDiagonalDownLeftPose;
        if (!supportedSource || !supportedTarget)
        {
            // `$9B:C8C5` chooses from the complete twelve-entry dropped-pose table, whose
            // outputs are exactly the standing/aim/crouch set above. Other releases use
            // separate grapple wall-jump or swing-release handlers, not this API.
            throw new InvalidOperationException(
                $"Grapple drop transition ${Pose:X2} -> ${targetPose:X2} is not a retail dropped-table route.");
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            plms,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            Pose = targetPose;
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
            InitializeAnimation(bus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
        }

        // `$9B:C95E-$C967` clears the two base-X and two Y-speed words regardless of the
        // pose-collision result. Extra run speed is not touched by this routine; an ordinary
        // connected grapple never creates it, so preserving the words is the literal rule.
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Applies `$90:9DBF-$90:9DC8` when an early spin frame finds the wall-jump chord.
    /// This does not launch Samus: it rewinds the ordinary spin sequence to frame `$0A`
    /// with a one-tick timer so the visible wall-contact pose reaches eligibility naturally.
    /// </summary>
    public void ApplyWallContactAnimationRewind()
    {
        if (!IsSpinJumpPose(Pose))
            throw new InvalidOperationException($"Wall-contact rewind requires spin pose, not ${Pose:X2}.");
        AnimationFrameTimer = 1;

        // `$90:9D96-$90:9DA6` gives Screw Attack a longer pre-contact animation. Its first
        // eligible wall frame is 27, so contact rewinds to 26. Ordinary spin and Space Jump
        // both use the compact 10 -> 11 handoff.
        AnimationFrame = IsScrewAttackPose(Pose) ? (ushort)0x1a : (ushort)0x0a;
    }

    /// <summary>
    /// Exact 16.16 ADC equivalent shared by bank-$91's grounded, aerial, and spin-turn
    /// initializers. It deliberately permits word overflow, matching the 65816 registers.
    /// </summary>
    private void FoldExtraRunSpeedIntoBaseAndStartTurn()
    {
        SamusHorizontalSpeedState speed = HorizontalSpeed;
        uint combinedSpeed = unchecked(speed.BaseFixed +
            ((uint)speed.ExtraRunSpeed << 16) + speed.ExtraRunSubspeed);
        speed.BaseSpeed = unchecked((ushort)(combinedSpeed >> 16));
        speed.BaseSubspeed = unchecked((ushort)combinedSpeed);
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.AccelerationMode = 1;
    }

    /// <summary>
    /// Applies the dry-room equipment half of <c>SamusFunc_F468_SpinJump</c> at
    /// <c>$91:F624</c> to a generic transition-table target `$19/$1A`.
    /// </summary>
    private byte SelectEquippedSpinPose(byte genericPose)
    {
        bool facingLeft = genericPose switch
        {
            SamusPoseIds.SpinJumpRightPose => false,
            SamusPoseIds.SpinJumpLeftPose => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(genericPose),
                genericPose,
                "Equipment spin selection requires generic pose $19 or $1A."),
        };

        // Native tests Screw Attack first. A save with both bits equipped therefore uses
        // `$81/$82`, not Space Jump art, while retaining Space Jump's repeat-jump physics.
        if (EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
            return facingLeft ? SamusPoseIds.ScrewAttackLeftPose : SamusPoseIds.ScrewAttackRightPose;
        if (EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump))
            return facingLeft ? SamusPoseIds.SpaceJumpLeftPose : SamusPoseIds.SpaceJumpRightPose;
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
        ISnesAddressSpace bus,
        byte targetPose,
        SamusMovementType previousMovementType)
    {
        bool right = targetPose is
            SamusPoseIds.NeutralJumpRightPose or SamusPoseIds.NormalJumpAimUpRightPose or
            SamusPoseIds.NormalJumpAimDiagonalUpRightPose;
        bool left = targetPose is
            SamusPoseIds.NeutralJumpLeftPose or SamusPoseIds.NormalJumpAimUpLeftPose or
            SamusPoseIds.NormalJumpAimDiagonalUpLeftPose;
        if ((!right && !left) || Shinespark.ShineTimer == 0 ||
            Shinespark.Phase != ShinesparkPhase.Stored)
        {
            return false;
        }

        Pose = right ? SamusPoseIds.ShinesparkWindupRightPose : SamusPoseIds.ShinesparkWindupLeftPose;
        RefreshCollisionRadii(bus);
        Shinespark.BeginWindup(this);

        // `$91:F56B-$F575` checks the PREVIOUS movement type and adjusts both current and
        // previous Y words by one. The host camera captures its previous point outside this
        // object, so only the live word is written here; the same-frame camera delta remains
        // one pixel and the following frame starts from the corrected coordinate.
        if (previousMovementType == SamusMovementType.NormalJumping)
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 1));

        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies a `$C7/$C8 -> $C9-$CE` input-table match and installs its exact special
    /// movement handler. No ordinary jump velocity or grounded momentum routine runs.
    /// </summary>
    public void ApplyShinesparkDirectionTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose is not (SamusPoseIds.ShinesparkWindupRightPose or SamusPoseIds.ShinesparkWindupLeftPose))
        {
            throw new InvalidOperationException(
                $"Directional shinespark transition requires windup pose, not ${Pose:X2}.");
        }

        Shinespark.BeginDirectionalLaunch(bus, this, targetPose);
    }

    /// <summary>
    /// Applies the verified ordinary-input jump transitions selected from the cartridge's
    /// bank-$91 table, including <c>HandleJumpTransition</c>'s call to
    /// <c>Make_Samus_Jump</c>. Only explicitly verified no-equipment/no-aim routes admitted
    /// by the current runtime are accepted, including interrupting any same-facing landing
    /// stream `$A4-$A7/$E0-$E7` with a fresh Jump edge.
    /// </summary>
    public void ApplyOrdinaryJumpTransition(
        ISnesAddressSpace bus,
        byte targetPose,
        ushort controllerNewInput = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool verified =
            SamusState.IsStandingGroundTurnToJumpTransition(Pose, targetPose) ||
            (Pose, targetPose) is
            (SamusPoseIds.FacingRightNormalPose or SamusPoseIds.StandingAimUpRightPose or
                SamusPoseIds.StandingAimDiagonalUpRightPose or SamusPoseIds.StandingAimDiagonalDownRightPose,
             SamusPoseIds.NeutralJumpTransitionRightPose) or
            (SamusPoseIds.FacingLeftNormalPose or SamusPoseIds.StandingAimUpLeftPose or
                SamusPoseIds.StandingAimDiagonalUpLeftPose or SamusPoseIds.StandingAimDiagonalDownLeftPose,
             SamusPoseIds.NeutralJumpTransitionLeftPose) or
            (SamusPoseIds.FacingRightNormalPose or SamusPoseIds.StandingAimUpRightPose or
                SamusPoseIds.StandingAimDiagonalUpRightPose or SamusPoseIds.StandingAimDiagonalDownRightPose,
             SamusPoseIds.NormalJumpTransitionAimUpRightPose or
                SamusPoseIds.NormalJumpTransitionAimDiagonalUpRightPose or
                SamusPoseIds.NormalJumpTransitionAimDiagonalDownRightPose) or
            (SamusPoseIds.FacingLeftNormalPose or SamusPoseIds.StandingAimUpLeftPose or
                SamusPoseIds.StandingAimDiagonalUpLeftPose or SamusPoseIds.StandingAimDiagonalDownLeftPose,
             SamusPoseIds.NormalJumpTransitionAimUpLeftPose or
                SamusPoseIds.NormalJumpTransitionAimDiagonalUpLeftPose or
                SamusPoseIds.NormalJumpTransitionAimDiagonalDownLeftPose) or
            (SamusPoseIds.MovingRightNormalPose or SamusPoseIds.MovingRightGunExtendedPose or SamusPoseIds.RunningAimUpRightPose or
                SamusPoseIds.RunningAimDiagonalUpRightPose or SamusPoseIds.RunningAimDiagonalDownRightPose,
             SamusPoseIds.SpinJumpRightPose) or
            (SamusPoseIds.MovingLeftNormalPose or SamusPoseIds.MovingLeftGunExtendedPose or SamusPoseIds.RunningAimUpLeftPose or
                SamusPoseIds.RunningAimDiagonalUpLeftPose or SamusPoseIds.RunningAimDiagonalDownLeftPose,
             SamusPoseIds.SpinJumpLeftPose) or
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
            (SamusPoseIds.MoonwalkTurnJumpLeftPose or SamusPoseIds.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseIds.MoonwalkTurnJumpAimDownLeftPose,
             SamusPoseIds.SpinJumpLeftPose or SamusPoseIds.NeutralJumpTransitionLeftPose) or
            (SamusPoseIds.MoonwalkTurnJumpRightPose or SamusPoseIds.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseIds.MoonwalkTurnJumpAimDownRightPose,
             SamusPoseIds.SpinJumpRightPose or SamusPoseIds.NeutralJumpTransitionRightPose) or
            (SamusPoseIds.RanIntoWallRightPose or SamusPoseIds.RanIntoWallAimUpRightPose or SamusPoseIds.RanIntoWallAimDownRightPose,
             SamusPoseIds.NeutralJumpTransitionRightPose) or
            (SamusPoseIds.RanIntoWallLeftPose or SamusPoseIds.RanIntoWallAimUpLeftPose or SamusPoseIds.RanIntoWallAimDownLeftPose,
             SamusPoseIds.NeutralJumpTransitionLeftPose) ||
            // Every landing body shares its facing's ordinary standing input table. The
            // centralized predicate deliberately covers normal, spin, aimed, and firing
            // landings, while still rejecting a corrupt cross-facing `$4B/$4C` target.
            IsLandingToNormalJumpTransition(Pose, targetPose);
        if (!verified)
        {
            // The pairs above exhaust the active retail input-table records that invoke
            // the ordinary jump initializer. Movement-specific launches (damage boost,
            // grapple, bomb jump, wall jump, and Shinespark) own separate entry points.
            throw new InvalidOperationException(
                $"Ordinary jump transition ${Pose:X2} -> ${targetPose:X2} is not an active retail route for this initializer.");
        }

        Pose = targetPose is SamusPoseIds.SpinJumpRightPose or SamusPoseIds.SpinJumpLeftPose
            ? SelectEquippedSpinPose(targetPose)
            : targetPose;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);
        SamusAerialMovement.InitializeJump(bus, this);

        if (ReadMovementType(bus) == SamusMovementType.NormalJumping &&
            (controllerNewInput & (ushort)SnesButton.X) != 0)
        {
            // `$91:F5CF-$F5E6` runs only in the normal-jumping initializer. It reads the
            // newly installed pose's direction byte and adds `$8000`; spin-jump's separate
            // initializer never publishes this bridge even when Shoot and Jump share a frame.
            PoseTransitionShotDirection = unchecked((ushort)(0x8000 | ReadShotDirection(bus)));
        }
    }

    /// <summary>
    /// Ports `$90:EB20`, the unconditional current-state epilogue clear that follows the
    /// HUD/projectile handler. A failed cooldown/slot allocation must lose the bridge too;
    /// it is deliberately not retained until some later shot succeeds.
    /// </summary>
    public void ClearPoseTransitionShotDirection() => PoseTransitionShotDirection = 0;

}
