using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal dry-air translation of normal knockback and its damage-boost escape route.
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
    private const int DryAirInitialYSpeed = 0x909ee9;
    private const int DryAirInitialYSubspeed = 0x909eef;

    /// <summary>
    /// Consumes the ordinary non-morph branch of special prospective command one.
    /// </summary>
    /// <param name="knockbackXDirection">
    /// Bank-$A0's `$0A54`: zero means move left, one means move right.
    /// </param>
    public static void Start(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput,
        ushort knockbackXDirection)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (knockbackXDirection > 1)
            throw new ArgumentOutOfRangeException(nameof(knockbackXDirection));
        if (samus.KnockbackActive || samus.KnockbackDirection != 0)
            throw new InvalidOperationException("Normal knockback is already active or pending.");

        byte sourceMovementType = samus.ReadMovementType(bus);
        if (sourceMovementType is not (0 or 1 or 2 or 3 or 5 or 6 or 0x0d or 0x10 or 0x14 or 0x15))
        {
            throw new NotSupportedException(
                $"Normal knockback start from movement type ${sourceMovementType:X2} is not translated.");
        }

        // `$90:DEFA` chooses hurt art from the OLD pose's direction. The art faces the same
        // direction as the source; `$91:EDB0` independently decides physical knockback.
        bool facingLeft = SamusState.ReadPoseXDirection(bus, samus.Pose) == 4;
        samus.Pose = facingLeft ? SamusState.KnockbackLeftPose : SamusState.KnockbackRightPose;
        samus.RefreshCollisionRadii(bus);

        // The “down” variants are selected only by holding the source pose's forward bit
        // on the command-one frame. Horizontal direction always comes from `$0A54`.
        bool forwardHeld = facingLeft
            ? (controllerInput & (ushort)SnesButton.Left) != 0
            : (controllerInput & (ushort)SnesButton.Right) != 0;
        samus.KnockbackXDirection = knockbackXDirection;
        samus.KnockbackDirection = knockbackXDirection == 0
            ? forwardHeld ? (ushort)4 : (ushort)1
            : forwardHeld ? (ushort)5 : (ushort)2;

        // Enemy and enemy-projectile collision both write five. The first bank-$A0 hurt-
        // timer pass following the hit decrements it; keeping the native five in state lets
        // callers and debugger watches see the actual published value.
        samus.KnockbackTimer = 5;
        samus.KnockbackActive = true;

        // `$90:99D6` dry-air table index zero. As elsewhere in this port, values are read
        // from the user's ROM so PAL/modified constants cannot be silently replaced.
        samus.Kinematics.YSpeed = ReadWord(bus, DryAirInitialYSpeed);
        samus.Kinematics.YSubspeed = ReadWord(bus, DryAirInitialYSubspeed);
        samus.Kinematics.YDirection = 1;
        SamusAerialMovement.ConfigureDryAirGravity(bus, samus);
        samus.InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>Executes one `$90:DF38` special movement-handler frame.</summary>
    public static KnockbackMovementResult Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
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
            return EndToFalling(bus, samus);

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectNormalAirSpeedTable();
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType: 0x0a);
        int requestedX = samus.KnockbackXDirection == 0
            ? speed.CalculateLeftDisplacement(baseSpeed)
            : speed.CalculateRightDisplacement(baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedX);

        BlockMoveResult vertical = samus.KnockbackDirection is 1 or 2
            ? MoveUpWithGravity(bus, level, samus, nmiFrameCounter)
            : MoveDownWithoutSpeedCalculation(bus, level, samus, nmiFrameCounter);

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

        // Gameplay state eight calls `$A0:9169` near the end of its frame, after Samus,
        // enemies, camera, drawing, room ASM, and game-time work. Publish that same next-
        // frame timer value only after the special handler has consumed this frame.
        samus.KnockbackTimer--;

        return new KnockbackMovementResult(horizontal, vertical, Ended: false);
    }

    /// <summary>
    /// Applies the only cross-family transition in `$91:A8E4/$91:A8EC`: Jump plus the
    /// direction opposite the knockback pose enters `$50/$4F` and restores normal movement.
    /// </summary>
    public static void ApplyDamageBoostTransition(
        ISnesAddressSpace bus,
        SamusState samus,
        byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        bool valid = (samus.Pose, targetPose) is
            (SamusState.KnockbackRightPose, SamusState.DamageBoostRightPose) or
            (SamusState.KnockbackLeftPose, SamusState.DamageBoostLeftPose);
        if (!valid)
        {
            throw new InvalidOperationException(
                $"Damage boost ${samus.Pose:X2} -> ${targetPose:X2} is not a retail transition.");
        }

        // Normal pose input `$91:8113` notices movement type changed away from `$0A`, calls
        // Make_Samus_Jump, and clears the knockback timer. The damage-boost initializer at
        // `$91:F8AE` then restores the normal movement handler. The jump call reinitializes
        // Y speed; this is why a damage boost gets a fresh normal jump arc rather than merely
        // inheriting the four-frame hurt arc.
        samus.Pose = targetPose;
        samus.RefreshCollisionRadii(bus);
        SamusAerialMovement.InitializeDryAirJump(bus, samus);
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

    private static KnockbackMovementResult EndToFalling(ISnesAddressSpace bus, SamusState samus)
    {
        // `$90:DE57` chooses `$29/$2A`, command one restores the normal movement handler,
        // and the current vertical velocity survives for ordinary falling next frame.
        samus.Pose = SamusState.ReadPoseXDirection(bus, samus.Pose) == 4
            ? SamusState.FallingLeftPose
            : SamusState.FallingRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.KnockbackDirection = 0;
        samus.KnockbackActive = false;
        samus.InitializeAnimation(bus, initialFrame: 0);
        return new KnockbackMovementResult(null, null, Ended: true);
    }

    private static BlockMoveResult MoveUpWithGravity(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        SamusKinematicsState state = samus.Kinematics;
        uint oldSpeed = state.VerticalSpeedFixed;
        uint acceleration = Compose(state.YAcceleration, state.YSubacceleration);
        SetVerticalSpeed(state, unchecked(oldSpeed - acceleration));
        return SamusBlockCollision.MoveVertical(
            bus,
            level,
            state,
            unchecked(-(int)oldSpeed),
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
    }

    private static BlockMoveResult MoveDownWithoutSpeedCalculation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        // With no external displacement and no slope adjustment, `$90:923F` copies the
        // total horizontal fractional word and adds one to its whole word. That deliberate
        // coupling attempts to keep grounded motion in contact with descending slopes.
        uint requested = Compose(
            unchecked((ushort)(samus.HorizontalSpeed.TotalSpeed + 1)),
            samus.HorizontalSpeed.TotalSubspeed);
        return SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            unchecked((int)requested),
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
    }

    private static void SetVerticalSpeed(SamusKinematicsState state, uint speed)
    {
        state.YSpeed = unchecked((ushort)(speed >> 16));
        state.YSubspeed = unchecked((ushort)speed);
    }

    private static uint Compose(ushort high, ushort low) => ((uint)high << 16) | low;

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Collision and lifetime result from one `$90:DF38` frame.</summary>
public readonly record struct KnockbackMovementResult(
    BlockMoveResult? Horizontal,
    BlockMoveResult? Vertical,
    bool Ended);
