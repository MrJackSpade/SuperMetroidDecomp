namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$A8 function words stored by the Evir composite actors.</summary>
public enum EvirAiFunction : ushort
{
    HandleBodyOrArms = 0x8922,
    ProjectileIdle = 0x8a34,
    ProjectileMoving = 0x8a3b,
    ProjectileRegenerating = 0x8a78,
}

/// <summary>
/// Typed view of the common and extended enemy RAM used by Evir (also called Mini-Draygon
/// in older source annotations). The cartridge builds each creature from three consecutive
/// physical enemy records: body, arms, and a reusable aimed projectile.
/// </summary>
public sealed class EvirEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal EvirEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Zero for left and one for right; native common variable B.</summary>
    public ushort FacingDirection
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Bank-$A8 indirect function word in native common variable C.</summary>
    public EvirAiFunction Function
    {
        get => (EvirAiFunction)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Body bobbing countdown in native common variable E.</summary>
    public ushort MovementTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Regeneration X offset in native common variable F.</summary>
    public ushort RegenerationXOffset
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    // The remaining words live in the parallel $7E:7800 extended-enemy array. Keeping them
    // on this state object preserves their native ownership without polluting the common
    // 64-byte RoomEnemySlot abstraction with one family's aliases.
    public ushort MovementDirection { get; internal set; }
    public ushort InstalledInstructionList { get; internal set; }
    public ushort RequestedInstructionList { get; internal set; }
    public short DownVelocity { get; internal set; }
    public ushort DownSubvelocity { get; internal set; }
    public short UpVelocity { get; internal set; }
    public ushort UpSubvelocity { get; internal set; }
    public short XVelocity { get; internal set; }
    public ushort XSubvelocity { get; internal set; }
    public short YVelocity { get; internal set; }
    public ushort YSubvelocity { get; internal set; }
    public ushort MovingFlag { get; internal set; }
    public ushort RegenerationFlag { get; internal set; }
}

/// <summary>
/// Literal translation of retail enemies $E63F/$E67F from $A8:8687-$8B58. Evir is a
/// deliberately coupled three-slot actor: the arms follow and face with the body, while the
/// projectile aims at Samus, flies until it is 256 pixels beyond the viewport, then grows
/// back out of the mouth through ROM animation bytecode.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort EvirDefinition = 0xe63f;
    internal const ushort EvirProjectileDefinition = 0xe67f;

    private const ushort EvirBodyLeftInstruction = 0x86a7;
    private const ushort EvirArmsLeftInstruction = 0x86c3;
    private const ushort EvirBodyRightInstruction = 0x870b;
    private const ushort EvirArmsRightInstruction = 0x8727;
    private const ushort EvirProjectileNormalInstruction = 0x876f;
    private const ushort EvirProjectileRegenerationInstruction = 0x8775;
    private const ushort EvirTouchAi = 0x8b06;
    private const ushort EvirPowerBombAi = 0x8b0c;
    private const ushort EvirShotAi = 0x8b12;
    private const ushort EvirSpitSound = 0x005e;
    private const ushort EvirActivationDistance = 0x0080;
    private const ushort EvirProjectileSpeed = 4;
    private const ushort EvirFarOffscreenDistance = 0x0100;

    private readonly EvirEnemyState?[] _evirStates =
        new EvirEnemyState?[MaximumEnemyCount];

    /// <summary>Typed native state for body, arms, and projectile records.</summary>
    public IReadOnlyList<EvirEnemyState?> EvirStates => _evirStates;

    /// <summary>Most recent library-two spit sound requested during this enemy frame.</summary>
    public ushort? LastEvirSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Evir</c> at $A8:87E0.</summary>
    private void InitializeEvir(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new EvirEnemyState(slot);
        _evirStates[slot.SlotIndex] = state;

        if (slot.Parameter1 != 0)
        {
            // The second record is not a second creature. Its nonzero parameter selects the
            // arms role, which always reads facing and position from the immediately prior
            // body record. Retail gives it layer four so the two maps compose correctly.
            PositionEvirArms(slot, state);
            slot.Layer = 4;
        }
        else
        {
            SetEvirBodyFacing(slot, state, samus);

            // Parameter two's low byte indexes two adjacent signed 16.16 entries in the
            // shared linear-speed table: downward first, then upward. The high byte is the
            // steady-state number of frames between direction changes.
            ushort speedOffset = unchecked((ushort)((byte)slot.Parameter2 * 8));
            (state.DownVelocity, state.DownSubvelocity) = ReadLinearEnemySpeed(speedOffset);
            (state.UpVelocity, state.UpSubvelocity) = ReadLinearEnemySpeed(
                unchecked((ushort)(speedOffset + 4)));

            // $A8:8811 accidentally indexes Enemy.init1 with Y (the speed-table byte offset)
            // rather than X. Both retail layouts make that aliased byte zero, so the first
            // bobbing half-cycle turns immediately; later cycles use the body's high byte.
            state.MovementTimer = 0;
        }

        state.MovementDirection = 0;
        state.InstalledInstructionList = 0;
        state.Function = EvirAiFunction.HandleBodyOrArms;
    }

    /// <summary>Ports <c>InitAI_EvirProjectile</c> at $A8:88B0.</summary>
    private void InitializeEvirProjectile(RoomEnemySlot slot)
    {
        var state = new EvirEnemyState(slot)
        {
            RequestedInstructionList = EvirProjectileNormalInstruction,
            InstalledInstructionList = 0,
            RegenerationFlag = 0,
            MovingFlag = 0,
            Function = EvirAiFunction.ProjectileIdle,
            RegenerationXOffset = 0,
        };
        _evirStates[slot.SlotIndex] = state;
        InstallEvirInstruction(slot, state);

        RoomEnemySlot body = RequireEvirRelativeSlot(slot, -2, EvirDefinition, "projectile body");
        // The projectile definition has no graphics-set entry of its own. Native copies the
        // body's resolved palette/tile indexes so the projectile map addresses the Evir art.
        slot.PaletteIndex = body.PaletteIndex;
        slot.VramTilesIndex = body.VramTilesIndex;
        ResetEvirProjectilePosition(slot, state);

        // Native clears installed-list after having installed the normal list. That oddity
        // forces the first main-AI request to refresh the bytecode timer exactly once.
        state.InstalledInstructionList = 0;
    }

    private void RunEvirMain(RoomEnemySlot slot, EvirEnemyState state, SamusState? samus)
    {
        if (state.Function != EvirAiFunction.HandleBodyOrArms)
        {
            throw new NotSupportedException(
                $"Evir body/arms function $A8:{(ushort)state.Function:X4} is not translated.");
        }

        if (slot.Parameter1 != 0)
        {
            PositionEvirArms(slot, state);
            return;
        }

        RoomEnemySlot projectile = RequireEvirRelativeSlot(
            slot,
            2,
            EvirProjectileDefinition,
            "body projectile");
        EvirEnemyState projectileState = RequireEvirState(projectile);

        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing.
        if (projectileState.RegenerationFlag == 0)
            SetEvirBodyFacing(slot, state, samus);

        int displacement = state.MovementDirection == 0
            ? ComposeEvirFixed(state.UpVelocity, state.UpSubvelocity)
            : ComposeEvirFixed(state.DownVelocity, state.DownSubvelocity);
        (slot.YPosition, slot.YSubposition) = AddEvirPosition(
            slot.YPosition,
            slot.YSubposition,
            displacement);

        // DEC/BPL means zero expires immediately into $FFFF. Every later reset uses the
        // unshifted high parameter byte, despite initialization's unrelated aliasing bug.
        state.MovementTimer = unchecked((ushort)(state.MovementTimer - 1));
        if ((state.MovementTimer & 0x8000) != 0)
        {
            state.MovementTimer = unchecked((byte)(slot.Parameter2 >> 8));
            state.MovementDirection ^= 1;
        }
    }

    private void RunEvirProjectileMain(
        RoomEnemySlot slot,
        EvirEnemyState state,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY)
    {
        if (slot.FrozenTimer == 0)
        {
            if (state.MovingFlag != 0)
            {
                RequestEvirInstruction(slot, state, EvirProjectileNormalInstruction);
            }
            else if (state.RegenerationFlag != 0)
            {
                RequestEvirInstruction(slot, state, EvirProjectileRegenerationInstruction);
            }
            else
            {
                TryLaunchEvirProjectile(slot, state, samus);
            }
        }

        switch (state.Function)
        {
            case EvirAiFunction.ProjectileIdle:
                ResetEvirProjectilePosition(slot, state);
                break;
            case EvirAiFunction.ProjectileMoving:
                RunMovingEvirProjectile(slot, state, cameraX, cameraY);
                break;
            case EvirAiFunction.ProjectileRegenerating:
                RunRegeneratingEvirProjectile(slot, state);
                break;
            default:
                throw new NotSupportedException(
                    $"Evir projectile function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void SetEvirBodyFacing(
        RoomEnemySlot body,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir facing AI requires the active Samus actor.");
        state.FacingDirection = unchecked((short)(activeSamus.XPosition - body.XPosition)) < 0
            ? (ushort)0
            : (ushort)1;
        RequestEvirInstruction(
            body,
            state,
            state.FacingDirection == 0
                ? EvirBodyLeftInstruction
                : EvirBodyRightInstruction);
    }

    private void PositionEvirArms(RoomEnemySlot arms, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(arms, -1, EvirDefinition, "arms body");
        EvirEnemyState bodyState = RequireEvirState(body);
        state.FacingDirection = bodyState.FacingDirection;
        arms.XPosition = unchecked((ushort)(
            body.XPosition + (state.FacingDirection == 0 ? -4 : 4)));
        arms.YPosition = unchecked((ushort)(body.YPosition + 10));
        RequestEvirInstruction(
            arms,
            state,
            state.FacingDirection == 0
                ? EvirArmsLeftInstruction
                : EvirArmsRightInstruction);
    }

    private void ResetEvirProjectilePosition(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EvirDefinition,
            "projectile body");
        EvirEnemyState bodyState = RequireEvirState(body);
        state.FacingDirection = bodyState.FacingDirection;
        projectile.XPosition = unchecked((ushort)(
            body.XPosition + (state.FacingDirection == 0 ? -4 : 4)));
        projectile.YPosition = unchecked((ushort)(body.YPosition + 18));
    }

    private void TryLaunchEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir projectile aiming requires the active Samus actor.");
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EvirDefinition,
            "projectile body");
        if (!IsWithinStrictModularDistance(
                activeSamus.XPosition,
                body.XPosition,
                EvirActivationDistance))
        {
            return;
        }

        // CalculateAngleOfSamusFromEnemy returns zero up/clockwise. The two native angle
        // transforms below feed the buggy table multipliers and produce signed 16.16 speed.
        byte cartridgeAngle = CalculateCartridgeAngle(
            unchecked((short)(activeSamus.XPosition - projectile.XPosition)),
            unchecked((short)(activeSamus.YPosition - projectile.YPosition)));
        byte transformedAngle = unchecked((byte)-(byte)(cartridgeAngle - 0x40));
        (state.XVelocity, state.XSubvelocity) = ReadEightBitCosineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        (state.YVelocity, state.YSubvelocity) = ReadEightBitNegativeSineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        RequestEvirInstruction(projectile, state, EvirProjectileNormalInstruction);
        state.MovingFlag = 1;
        state.Function = EvirAiFunction.ProjectileMoving;
    }

    private void RunMovingEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        StartEvirRegenerationIfFarOffscreen(projectile, state, cameraX, cameraY);
        (projectile.XPosition, projectile.XSubposition) = AddEvirPosition(
            projectile.XPosition,
            projectile.XSubposition,
            ComposeEvirFixed(state.XVelocity, state.XSubvelocity));
        (projectile.YPosition, projectile.YSubposition) = AddEvirPosition(
            projectile.YPosition,
            projectile.YSubposition,
            ComposeEvirFixed(state.YVelocity, state.YSubvelocity));
    }

    private void StartEvirRegenerationIfFarOffscreen(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        bool farOffscreen =
            IsNegative16(projectile.XPosition + EvirFarOffscreenDistance - cameraX) ||
            IsNegative16(cameraX + 256 + EvirFarOffscreenDistance - projectile.XPosition) ||
            IsNegative16(projectile.YPosition + EvirFarOffscreenDistance - cameraY) ||
            IsNegative16(cameraY + 256 + EvirFarOffscreenDistance - projectile.YPosition);
        if (!farOffscreen)
            return;

        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EvirDefinition,
            "projectile body");
        if (body.FrozenTimer != 0)
            return;

        state.MovingFlag = 0;
        state.RegenerationFlag = 1;
        state.Function = EvirAiFunction.ProjectileRegenerating;
        RequestEvirInstruction(projectile, state, EvirProjectileRegenerationInstruction);
    }

    private void RunRegeneratingEvirProjectile(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EvirDefinition,
            "projectile body");
        if (body.FrozenTimer != 0)
            return;

        if (state.RegenerationFlag == 0)
        {
            RequestEvirInstruction(projectile, state, EvirProjectileNormalInstruction);
            state.MovingFlag = 0;
            state.Function = EvirAiFunction.ProjectileIdle;
            return;
        }

        ResetEvirProjectilePosition(projectile, state);
        projectile.XPosition = unchecked((ushort)(
            projectile.XPosition + state.RegenerationXOffset));
    }

    /// <summary>Instruction $A8:878F.</summary>
    private void QueueEvirSpitSound() => LastEvirSoundEffect = EvirSpitSound;

    /// <summary>Instruction $A8:879B.</summary>
    private void SetInitialEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationXOffset = state.FacingDirection != 0
            ? unchecked((ushort)-8)
            : (ushort)8;

    /// <summary>Instruction $A8:87B6.</summary>
    private void AdvanceEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationXOffset = unchecked((ushort)(
            state.RegenerationXOffset + (state.FacingDirection != 0 ? 1 : -1)));

    /// <summary>Instruction $A8:87CB.</summary>
    private static void FinishEvirRegeneration(EvirEnemyState state)
    {
        state.RegenerationFlag = 0;
        state.MovingFlag = 0;
        state.Function = EvirAiFunction.ProjectileIdle;
    }

    private static int ComposeEvirFixed(short whole, ushort fraction) =>
        unchecked((whole << 16) | fraction);

    private static (ushort Position, ushort Subposition) AddEvirPosition(
        ushort position,
        ushort subposition,
        int displacement)
    {
        int fixedPosition = unchecked((position << 16) | subposition);
        fixedPosition = unchecked(fixedPosition + displacement);
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    private void RequestEvirInstruction(
        RoomEnemySlot slot,
        EvirEnemyState state,
        ushort instructionList)
    {
        state.RequestedInstructionList = instructionList;
        InstallEvirInstruction(slot, state);
    }

    private static void InstallEvirInstruction(RoomEnemySlot slot, EvirEnemyState state)
    {
        if (state.RequestedInstructionList == state.InstalledInstructionList)
            return;
        slot.CurrentInstruction = state.RequestedInstructionList;
        state.InstalledInstructionList = state.RequestedInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot RequireEvirRelativeSlot(
        RoomEnemySlot owner,
        int relativeSlot,
        ushort expectedDefinition,
        string relationship)
    {
        int targetIndex = owner.SlotIndex + relativeSlot;
        if ((uint)targetIndex >= _slots.Length ||
            _slots[targetIndex].EnemyDefinitionPointer != expectedDefinition)
        {
            throw new InvalidDataException(
                $"Evir {relationship} expected definition ${expectedDefinition:X4} " +
                $"at relative slot {relativeSlot:+#;-#;0} from {owner.SlotIndex}.");
        }
        return _slots[targetIndex];
    }

    private EvirEnemyState RequireEvirState(RoomEnemySlot slot) =>
        _evirStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Evir state.");

    /// <summary>
    /// Ports $A8:8B16 after common touch/shot/power-bomb damage. Death removes the two
    /// dependent records; freezing propagates to arms and to a projectile unless it is
    /// already in flight, matching the native moving-function exception.
    /// </summary>
    private void ResolveEvirCombatAfterCommon(RoomEnemySlot body)
    {
        RoomEnemySlot arms = RequireEvirRelativeSlot(body, 1, EvirDefinition, "body arms");
        RoomEnemySlot projectile = RequireEvirRelativeSlot(
            body,
            2,
            EvirProjectileDefinition,
            "body projectile");
        if (body.Health == 0)
        {
            arms.Properties = arms.Properties.With(EnemyProperties.Deleted);
            projectile.Properties = projectile.Properties.With(EnemyProperties.Deleted);
        }

        if (body.FrozenTimer == 0)
            return;
        arms.FrozenTimer = body.FrozenTimer;
        arms.AiHandlerBits = unchecked((ushort)(arms.AiHandlerBits | 0x0004));
        if (RequireEvirState(projectile).Function != EvirAiFunction.ProjectileMoving)
        {
            projectile.FrozenTimer = body.FrozenTimer;
            projectile.AiHandlerBits = unchecked((ushort)(projectile.AiHandlerBits | 0x0004));
        }
    }
}
