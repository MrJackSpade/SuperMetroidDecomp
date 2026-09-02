using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal air/water/lava translation of humanoid and morphed knockback plus the
/// humanoid-only damage-boost escape route.
/// </summary>
/// <remarks>
/// This code follows `$90:DDE9-$90:DF98`, `$90:99D6`, and `$91:ED4E-$91:EE26`.
/// Enemy damage calculation is deliberately not invented here: a caller supplies only the
/// collision result that bank `$A0` would have published, namely whether the source lies to
/// Samus's left or right. Every pose, speed-table entry, timer value, and subsequent input
/// transition is then taken from the original cartridge model.
/// </remarks>
public static class SamusKnockbackMovement
{
    // The selector routine is `$90:99D6`, but its two three-word data arrays live later in
    // bank $90 at `$9EE9/$9EEF` (named exactly that way in the disassembly): whole speeds
    // `{5,2,2}` followed by subspeeds `{0,0,0}`. Do not infer data placement from the C
    // decompiler's declaration order; `$99CA/$99D0` are executable bytes/data belonging to
    // the preceding routine and produce enormous bogus hurt velocities when read as tables.
    private const int InitialYSpeedTable = 0x909ee9;
    private const int InitialYSubspeedTable = 0x909eef;

    /// <summary>
    /// Consumes special prospective command one after bank `$90` has admitted either the
    /// normal `$53/$54` hurt-pose branch or the pose-preserving Morph/Spring Ball branch.
    /// </summary>
    /// <param name="knockbackXDirection">
    /// Bank-$A0's `$0A54`: zero means move left, one means move right.
    /// </param>
    /// <returns>
    /// True when the movement-type table installs the special knockback handler. False for
    /// native interrupt-suppression entries such as turning, grapple, and shinespark.
    /// </returns>
    public static bool Start(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput,
        ushort knockbackXDirection,
        ushort knockbackTimer = 5)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (knockbackXDirection > 1)
            throw new ArgumentOutOfRangeException(nameof(knockbackXDirection));
        if (knockbackTimer == 0)
            throw new ArgumentOutOfRangeException(nameof(knockbackTimer));
        if (samus.KnockbackActive || samus.KnockbackDirection != 0)
            throw new InvalidOperationException("Normal knockback is already active or pending.");

        byte sourceMovementType = samus.ReadMovementType(bus);
        bool morphed = sourceMovementType is 4 or 8 or 9 or 0x11 or 0x12 or 0x13;
        bool humanoid = sourceMovementType is 0 or 1 or 2 or 3 or 5 or 6 or 0x0d or
            0x10 or 0x14 or 0x15;
        bool unusedMovementSeven = sourceMovementType == 7;
        bool suppressesKnockback = sourceMovementType is
            0x0a or 0x0b or 0x0c or 0x0e or 0x0f or 0x16 or 0x17 or 0x18 or 0x19 or
            0x1a or 0x1b;
        if (!morphed && !humanoid && !unusedMovementSeven && !suppressesKnockback)
        {
            // Bank `$90:DDE9` contains exactly 28 entries, indexed by the low movement byte.
            // A larger value cannot name another native behavior; it means the caller supplied
            // corrupt pose metadata rather than an untranslated movement family.
            throw new InvalidDataException(
                $"Movement type ${sourceMovementType:X2} lies outside the 28-entry knockback table.");
        }

        // Enemy contact has already established `$18AA` before `$90:DDE9` selects a pose.
        // Turning, grapple, current knockback, damage boost, and special type-$1B bodies all
        // return carry clear: they retain the timer/flicker but do not install `$90:DF38`.
        samus.KnockbackTimer = knockbackTimer;
        if (suppressesKnockback)
            return false;

        // `$90:DEFA` replaces an ordinary body with `$53/$54`. `$90:DF15`, by contrast,
        // republishes the exact Morph/Spring Ball pose. Because UpdateSamusPose sees no
        // pose change in that branch, the rolling animation frame and timer survive.
        bool facingLeft = SamusState.IsFacingLeft(bus, samus.Pose);
        if (humanoid)
        {
            samus.Pose = facingLeft
                ? SamusState.KnockbackLeftPose
                : SamusState.KnockbackRightPose;
            samus.RefreshCollisionRadii(bus);
        }
        else if (unusedMovementSeven)
        {
            // `$90:DF1D-$DF37` is dead in retail play but fully specified: unlike the ball
            // path it installs `$33/$34`, then continues through the same command-one hurt
            // initializer. This preserves the actual table instead of turning an unused
            // native arm into an exception.
            samus.Pose = facingLeft
                ? SamusState.UnusedKnockbackLeftPose
                : SamusState.UnusedKnockbackRightPose;
            samus.RefreshCollisionRadii(bus);
            humanoid = true;
        }

        // `$91:EE27` deliberately ignores both the damage-source side and controller input
        // when choosing a ball's vertical branch: right-facing ball art always gets up-right
        // direction two, and left-facing art gets up-left direction one. Humanoid `$91:EDB0`
        // retains the unusual forward-held selection of down-left/down-right directions.
        samus.KnockbackXDirection = knockbackXDirection;
        if (morphed)
        {
            samus.KnockbackDirection = facingLeft ? (ushort)1 : (ushort)2;
        }
        else
        {
            bool forwardHeld = facingLeft
                ? (controllerInput & (ushort)SnesButton.Left) != 0
                : (controllerInput & (ushort)SnesButton.Right) != 0;
            samus.KnockbackDirection = knockbackXDirection == 0
                ? forwardHeld ? (ushort)4 : (ushort)1
                : forwardHeld ? (ushort)5 : (ushort)2;
        }

        // Enemy and enemy-projectile collision normally publish five, which is the default
        // supplied above. The intro Rinka deliberately publishes eleven at `$8B:B918`;
        // accepting the producer-owned value here prevents this shared initializer from
        // shortening that visibly longer scripted reaction to ordinary gameplay timing.
        samus.KnockbackActive = true;

        // The remainder of `$91:ED4E` runs for both pointer-table families. A pending bomb
        // jump cannot coexist with hurt movement, and shinespark/Screw contact damage is
        // cancelled before the special movement handler begins.
        samus.BombJumpDirection = 0;
        samus.HorizontalSpeed.ContactDamageIndex = 0;

        // `$91:ED63-$91:ED66` writes one to the shared hurt-flash counter after cancelling
        // bomb-jump/contact damage. This is an independent 60-call palette/audio lifetime,
        // not the five-frame `$18AA` knockback timer established above.
        samus.HurtFlashCounter = 1;

        // `$90:99D6` indexes air/water/lava by zero/two/four after the exact bottom-edge
        // and Gravity-Suit checks. Values remain live ROM reads for regional/modded builds.
        int liquidOffset = samus.LiquidPhysics.DetermineMovementMedium(samus) * 2;
        samus.Kinematics.YSpeed = ReadWord(bus, InitialYSpeedTable + liquidOffset);
        samus.Kinematics.YSubspeed = ReadWord(bus, InitialYSubspeedTable + liquidOffset);
        samus.Kinematics.YDirection = 1;
        SamusAerialMovement.ConfigureEnvironmentGravity(bus, samus);
        if (humanoid)
            samus.InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>Executes one `$90:DF38` special movement-handler frame.</summary>
    public static KnockbackMovementResult Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!samus.KnockbackActive)
            throw new InvalidOperationException("Knockback movement requires the special handler to be active.");
        if (samus.KnockbackDirection is not (1 or 2 or 4 or 5))
            throw new InvalidOperationException($"Invalid knockback direction ${samus.KnockbackDirection:X4}.");

        // `$90:DE20` observes the value left by the previous frame's final `$A0:9169`
        // timer pass. Zero ends type-$0A knockback before beta can move it again; a value
        // of one therefore still owns this movement frame and becomes zero afterward.
        if (samus.KnockbackTimer == 0)
        {
            // Only `$53/$54` are movement type `$0A`; `$90:DE20` gives that family a new
            // `$29/$2A` pose. A ball retains whatever Morph/Spring pose is current and goes
            // directly to the shared command-one cleanup at `$91:F31D`.
            return samus.ReadMovementType(bus) == 0x0a
                ? FinishHumanoidToFalling(bus, samus)
                : EndWithoutPoseChange(samus);
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType: 0x0a);
        int requestedX = samus.KnockbackXDirection == 0
            ? speed.CalculateLeftDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed)
            : speed.CalculateRightDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedX,
            plms: plms);

        BlockMoveResult vertical = samus.KnockbackDirection is 1 or 2
            ? MoveWithSharedVerticalSpeedCalculation(bus, level, samus, nmiFrameCounter, plms)
            : MoveDownWithoutSpeedCalculation(bus, level, samus, nmiFrameCounter, plms);

        // `$91:F010` observes these words inside downward collision, before `$90:DF6E`
        // clears them. Preserve the impact snapshot in the typed result so the runtime can
        // execute the shared landing-presentation routine at the same logical seam.
        bool landed = samus.KnockbackDirection is 4 or 5 && vertical.Collided;
        ushort impactYSpeed = samus.Kinematics.YSpeed;
        ushort impactYSubspeed = samus.Kinematics.YSubspeed;

        if (vertical.Collided)
        {
            // `$90:DF6E` is reached only after the vertical helper, so horizontal wall
            // contact by itself does not perform these writes. Bottom alignment is already
            // represented by bank-$94's accepted displacement against the current radius.
            speed.AccelerationMode = 0;
            speed.BaseSpeed = 0;
            speed.BaseSubspeed = 0;
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YDirection = 0;
        }

        return new KnockbackMovementResult(
            horizontal,
            vertical,
            Ended: false,
            Landed: landed,
            impactYSpeed,
            impactYSubspeed);
    }

    /// <summary>
    /// Applies the only cross-family transition in `$91:A8E4/$91:A8EC`: Jump plus the
    /// direction opposite the knockback pose enters `$50/$4F` and restores normal movement.
    /// </summary>
    public static void ApplyDamageBoostTransition(
        ISnesAddressSpace bus,
        SamusState samus,
        byte sourcePose,
        byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        // Alpha chooses the input record from the pose present at frame start. Beta then
        // runs before UpdateSamusPose and can exhaust the hurt timer, replacing `$53/$54`
        // with falling `$29/$2A` on that same frame. Validate the captured dispatch pose,
        // not the now-mutable Samus.Pose, or a completely genuine last-frame damage boost
        // is incorrectly rejected as `$29/$2A -> $50/$4F`.
        bool valid = (sourcePose, targetPose) is
            (SamusState.KnockbackRightPose, SamusState.DamageBoostRightPose) or
            (SamusState.KnockbackLeftPose, SamusState.DamageBoostLeftPose);
        if (!valid)
        {
            throw new InvalidOperationException(
                $"Damage boost ${sourcePose:X2} -> ${targetPose:X2} is not a retail transition.");
        }

        // Normal pose input `$91:8113` notices movement type changed away from `$0A`, calls
        // Make_Samus_Jump, and clears the knockback timer. The damage-boost initializer at
        // `$91:F8AE` then restores the normal movement handler. The jump call reinitializes
        // Y speed; this is why a damage boost gets a fresh normal jump arc rather than merely
        // inheriting the four-frame hurt arc.
        samus.Pose = targetPose;
        samus.RefreshCollisionRadii(bus);
        SamusAerialMovement.InitializeJump(bus, samus);
        samus.KnockbackTimer = 0;
        samus.KnockbackDirection = 0;
        samus.KnockbackActive = false;
        samus.InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>Installs a same-family target selected by the two `$91:A3F6/A40A` tables.</summary>
    public static void ApplyDamageBoostPoseTransition(
        ISnesAddressSpace bus,
        SamusState samus,
        byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        bool valid = (samus.Pose, targetPose) is
            (SamusState.DamageBoostLeftPose,
                SamusState.NeutralJumpLeftPose or SamusState.NormalJumpForwardLeftPose) or
            (SamusState.DamageBoostRightPose,
                SamusState.NeutralJumpRightPose or SamusState.NormalJumpForwardRightPose);
        if (!valid)
        {
            throw new InvalidOperationException(
                $"Damage-boost exit ${samus.Pose:X2} -> ${targetPose:X2} is not in the ROM table.");
        }

        // Both target families have the same radius. HandlePoseChange nevertheless runs
        // their ordinary initializer; it preserves the live 16.16 jump velocity and starts
        // the selected target's delay stream at frame zero.
        samus.Pose = targetPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Consumes the humanoid arm of `$90:DE20-$DE73` and `$91:F31D`: select the
    /// facing-preserving falling pose, align its shorter body to the old feet, and restore
    /// the normal movement handler's state.
    /// </summary>
    /// <remarks>
    /// Ordinary damage reaches this through <see cref="Step"/> when `$18AA` expires. Ceres
    /// Ridley's `$90:E1FD/$E21C` wall-collision handoff reaches the same cartridge cleanup
    /// after restoring the normal handler. Keeping that second caller here is important:
    /// Ceres does not invent a standing pose, and the optional `$53/$54` damage-boost input
    /// table is not the only way a neutral player can regain control after the shove.
    /// </remarks>
    public static KnockbackMovementResult FinishHumanoidToFalling(
        ISnesAddressSpace bus,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (samus.Pose is not (SamusState.KnockbackRightPose or SamusState.KnockbackLeftPose))
        {
            throw new InvalidOperationException(
                $"Humanoid knockback completion requires pose $53/$54, not ${samus.Pose:X2}.");
        }

        // `$90:DE57` chooses `$29/$2A`. After the ordinary pose-change initializer has
        // installed radius 19, command one `$91:F31D` aligns the new body bottom to the old
        // radius-21 hurt body. Thus the center moves down two pixels before velocity clears.
        ushort previousRadius = samus.Kinematics.YRadius;
        samus.Pose = SamusState.IsFacingLeft(bus, samus.Pose)
            ? SamusState.FallingLeftPose
            : SamusState.FallingRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.Kinematics.YPosition = unchecked((ushort)(
            samus.Kinematics.YPosition + previousRadius - samus.Kinematics.YRadius));
        samus.InitializeAnimation(bus, initialFrame: 0);
        return FinishKnockback(samus);
    }

    private static KnockbackMovementResult EndWithoutPoseChange(SamusState samus)
    {
        // `$90:DE40` republishes the current pose, so UpdateSamusPose neither reloads its
        // radii nor resets its animation. This is the detail that keeps an interrupted
        // rolling Morph/Spring Ball visually continuous across the five hurt frames.
        return FinishKnockback(samus);
    }

    private static KnockbackMovementResult FinishKnockback(SamusState samus)
    {
        // Exact `$91:F31D` cleanup shared by humanoid and morphed completion. The falling
        // flag has no independent host field yet; Y-direction two is its movement-visible
        // publication and is consumed by every translated normal/ball dispatcher.
        samus.KnockbackDirection = 0;
        samus.KnockbackActive = false;
        samus.MorphBallBounceState = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YDirection = 2;
        return new KnockbackMovementResult(null, null, Ended: true);
    }

    private static BlockMoveResult MoveWithSharedVerticalSpeedCalculation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms)
    {
        // `$90:DF53` calls the exact same `$90:90E2` vertical routine as ordinary aerial
        // movement, so reuse that translated owner instead of maintaining a subtly narrower
        // copy. The retail Rinka timer expires while Samus is still rising; her reported
        // midair hang came from failing to dispatch ordinary falling after that expiry, not
        // from this calculation. Sharing the routine still preserves the real direction-
        // reversal behavior for any longer knockback stimulus without inventing a new path.
        return SamusAerialMovement.StepVerticalWithSpeedCalculations(
            bus,
            level,
            samus,
            nmiFrameCounter,
            out _,
            out _,
            plms: plms);
    }

    private static BlockMoveResult MoveDownWithoutSpeedCalculation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms)
    {
        // `$90:923F` gives nonzero external Y priority over its normal total-X-plus-one
        // slope-following probe. This down-knockback path consumes the exact same helper.
        int requested = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);
        return SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            requested,
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            plms: plms);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Collision and lifetime result from one `$90:DF38` frame.</summary>
public readonly record struct KnockbackMovementResult(
    BlockMoveResult? Horizontal,
    BlockMoveResult? Vertical,
    bool Ended,
    bool Landed = false,
    ushort ImpactYSpeed = 0,
    ushort ImpactYSubspeed = 0);
