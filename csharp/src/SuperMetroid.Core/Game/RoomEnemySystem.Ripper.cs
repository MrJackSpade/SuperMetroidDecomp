using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed view of the common Ripper-family motion words plus GRipper's two private patrol
/// bounds. The three species reuse the same native variables C/D/E for fractional speed,
/// signed whole speed, and byte offset into the bank-$A0 linear-speed table.
/// </summary>
public sealed class RipperVariantEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _minimumXPositions;
    private readonly ushort[] _maximumXPositions;

    internal RipperVariantEnemyState(
        RoomEnemySlot slot,
        ushort[] minimumXPositions,
        ushort[] maximumXPositions)
    {
        _slot = slot;
        _minimumXPositions = minimumXPositions;
        _maximumXPositions = maximumXPositions;
    }

    /// <summary>Common variable C: low/fractional word of signed 16.16 X velocity.</summary>
    public ushort XSubvelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Common variable D: signed whole-pixel word of 16.16 X velocity.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Common variable E: byte offset into <c>$A0:8187</c>.</summary>
    public ushort SpeedTableByteOffset
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>GRipper extra word $00: exclusive left patrol reversal threshold.</summary>
    public ushort MinimumXPosition
    {
        get => _minimumXPositions[_slot.SlotIndex];
        internal set => _minimumXPositions[_slot.SlotIndex] = value;
    }

    /// <summary>GRipper extra word $01: inclusive right patrol reversal threshold.</summary>
    public ushort MaximumXPosition
    {
        get => _maximumXPositions[_slot.SlotIndex];
        internal set => _maximumXPositions[_slot.SlotIndex] = value;
    }
}

/// <summary>
/// Literal translation of enemy $D47F (Ripper) from bank $A2. This is intentionally kept
/// separate from the room scheduler: the same common speed table and terrain mover are used
/// by many later enemy families, while these list pointers and reversal rules belong only to
/// the Ripper actor.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort GRipperDefinition = 0xd3ff;
    internal const ushort Ripper2Definition = 0xd43f;
    internal const ushort RipperDefinition = 0xd47f;

    internal const ushort GRipperRipper2ShotAi = EnemyAiCodePointers.BankA2.GRipperRipper2Shot;

    private const ushort GRipperMovingLeftInstruction = 0xe19b;
    private const ushort GRipperMovingRightInstruction = 0xe1af;
    private const ushort Ripper2MovingRightInstruction = 0xe2e0;
    private const ushort Ripper2MovingLeftInstruction = 0xe2f4;
    private const ushort GRipperRipper2FrozenFacingLeftSpritemap = 0xe43f;
    private const ushort GRipperRipper2FrozenFacingRightSpritemap = 0xe44b;

    private const ushort RipperMovingRightInstruction = 0xe477;
    private const ushort RipperMovingLeftInstruction = 0xe48b;
    private const int CommonLinearEnemySpeedTable = 0xa28187;

    private readonly ushort[] _ripperVariantMinimumXPositions =
        new ushort[MaximumEnemyCount];
    private readonly ushort[] _ripperVariantMaximumXPositions =
        new ushort[MaximumEnemyCount];
    private readonly RipperVariantEnemyState?[] _ripperVariantStates =
        new RipperVariantEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for GRipper and Ripper II physical slots.</summary>
    public IReadOnlyList<RipperVariantEnemyState?> RipperVariantStates =>
        _ripperVariantStates;

    private void ResetRipperVariantRoomState()
    {
        Array.Clear(_ripperVariantMinimumXPositions);
        Array.Clear(_ripperVariantMaximumXPositions);
        Array.Clear(_ripperVariantStates);
    }

    /// <summary>Ports <c>InitAI_GRipper</c> at $A2:E1D3.</summary>
    private void InitializeGRipper(RoomEnemySlot slot)
    {
        var state = CreateRipperVariantState(slot);

        // GRipper overloads the population's initialization-list word as a packed speed
        // selector. Only its low byte reaches the eight-byte speed-table multiplication.
        // The peculiar BIT #$FEFF direction test is preserved literally: every retail word
        // with a nonzero speed field selects the positive pair, even when bit eight is set.
        ushort packedSelector = slot.CurrentInstruction;
        state.SpeedTableByteOffset = unchecked((ushort)((packedSelector & 0x00ff) * 8));
        bool selectsPositiveVelocity = (packedSelector & 0xfeff) != 0;
        LoadRipperVelocity(slot, selectsPositiveVelocity);
        SetRipperInstructionList(
            slot,
            unchecked((short)state.XVelocity) < 0
                ? GRipperMovingLeftInstruction
                : GRipperMovingRightInstruction);

        // Unlike the wall-collision helper, patrol-bound reversal does not clamp the actor
        // back to either endpoint. The one-frame fractional overshoot is native and becomes
        // visible again when the direction reverses on the next pass.
        state.MinimumXPosition = slot.Parameter1;
        state.MaximumXPosition = slot.Parameter2;
    }

    /// <summary>Ports <c>MainAI_GRipper</c> at $A2:E221.</summary>
    private void RunGRipperMain(
        RoomEnemySlot slot,
        RipperVariantEnemyState state,
        RoomLevelData? level)
    {
        RequireRipperTerrain(level, "GRipper");
        int displacement = unchecked(((short)state.XVelocity << 16) | state.XSubvelocity);
        bool reverse = MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level!,
            slot,
            displacement);
        if (!reverse && unchecked((short)state.XVelocity) < 0)
        {
            reverse = unchecked((short)(slot.XPosition - state.MinimumXPosition)) < 0;
        }
        if (!reverse && unchecked((short)state.XVelocity) >= 0)
        {
            reverse = unchecked((short)(slot.XPosition - state.MaximumXPosition)) >= 0;
        }
        if (reverse)
        {
            ReverseRipperVariant(
                slot,
                positiveInstruction: GRipperMovingRightInstruction,
                negativeInstruction: GRipperMovingLeftInstruction);
        }
    }

    /// <summary>Ports <c>InitAI_Ripper2</c> at $A2:E318.</summary>
    private void InitializeRipper2(RoomEnemySlot slot)
    {
        RipperVariantEnemyState state = CreateRipperVariantState(slot);
        state.SpeedTableByteOffset = unchecked((ushort)(slot.Parameter1 * 8));

        // Ripper II's list labels appear inverted relative to the displayed shared maps.
        // Keep the literal ROM pointers instead of normalizing their names: init1 zero uses
        // the negative speed pair with $E2E0, while nonzero uses positive speed with $E2F4.
        bool movingPositive = slot.Parameter2 != 0;
        LoadRipperVelocity(slot, movingPositive);
        SetRipperInstructionList(
            slot,
            movingPositive ? Ripper2MovingLeftInstruction : Ripper2MovingRightInstruction);
    }

    /// <summary>Ports <c>MainAI_Ripper2</c> at $A2:E353.</summary>
    private void RunRipper2Main(RoomEnemySlot slot, RoomLevelData? level)
    {
        RequireRipperTerrain(level, "Ripper II");
        int displacement = unchecked(((short)slot.VariableD << 16) | slot.VariableC);
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level!, slot, displacement))
            return;

        ReverseRipperVariant(
            slot,
            positiveInstruction: Ripper2MovingLeftInstruction,
            negativeInstruction: Ripper2MovingRightInstruction);
    }

    /// <summary>Ports <c>Ripper_Init</c> at $A2:E49F.</summary>
    private void InitializeRipper(RoomEnemySlot slot)
    {
        // Population parameter two is the initial facing/direction selector. A nonzero word
        // chooses the positive table pair and the right-facing list; zero chooses the
        // negative pair and left-facing list.
        bool movingRight = slot.Parameter2 != 0;
        slot.CurrentInstruction = movingRight
            ? RipperMovingRightInstruction
            : RipperMovingLeftInstruction;

        // Each logical speed occupies eight bytes in the common table:
        //   +0 signed pixel velocity, +2 subpixel velocity,
        //   +4 negated pixel velocity, +6 negated subpixel velocity.
        // Variable E retains the byte offset because the reversal routine reuses it.
        slot.VariableE = unchecked((ushort)(slot.Parameter1 * 8));
        LoadRipperVelocity(slot, movingRight);
    }

    /// <summary>Ports <c>Ripper_Main</c> at $A2:E4DA.</summary>
    private void RunRipperMain(RoomEnemySlot slot, RoomLevelData? level)
    {
        RequireRipperTerrain(level, "Ripper");

        // $A0:C6AB consumes a signed 16.16 displacement whose high and low words are kept
        // in Variables D/C. On collision the native helper aligns the actor flush with the
        // wall before Ripper swaps direction and animation.
        int displacement = unchecked(((short)slot.VariableD << 16) | slot.VariableC);
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level!, slot, displacement))
            return;

        bool wasMovingLeft = (short)slot.VariableD < 0;
        bool nowMovingRight = wasMovingLeft;
        LoadRipperVelocity(slot, nowMovingRight);
        slot.CurrentInstruction = nowMovingRight
            ? RipperMovingRightInstruction
            : RipperMovingLeftInstruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private void LoadRipperVelocity(RoomEnemySlot slot, bool movingRight)
    {
        int address = CommonLinearEnemySpeedTable + slot.VariableE + (movingRight ? 0 : 4);
        slot.VariableD = ReadWord(_bus!, address);
        slot.VariableC = ReadWord(_bus!, address + 2);
    }

    /// <summary>
    /// Ports the common post-shot tail at $A2:E3AD. Normal shot AI owns health, death,
    /// impact, and freeze timers; this family-specific tail only pins the direction-specific
    /// two-piece frozen map when the accepted projectile actually froze the actor.
    /// </summary>
    private static void ResolveGRipperRipper2ShotAfterCommon(RoomEnemySlot slot)
    {
        if (slot.FrozenTimer == 0)
            return;
        slot.SpritemapPointer = unchecked((short)slot.VariableD) < 0
            ? GRipperRipper2FrozenFacingLeftSpritemap
            : GRipperRipper2FrozenFacingRightSpritemap;
    }

    private RipperVariantEnemyState CreateRipperVariantState(RoomEnemySlot slot)
    {
        var state = new RipperVariantEnemyState(
            slot,
            _ripperVariantMinimumXPositions,
            _ripperVariantMaximumXPositions);
        _ripperVariantStates[slot.SlotIndex] = state;
        return state;
    }

    private static void RequireRipperTerrain(RoomLevelData? level, string species)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                $"{species} movement requires the active room collision allocation.");
        }
    }

    private void ReverseRipperVariant(
        RoomEnemySlot slot,
        ushort positiveInstruction,
        ushort negativeInstruction)
    {
        bool wasMovingNegative = unchecked((short)slot.VariableD) < 0;
        LoadRipperVelocity(slot, movingRight: wasMovingNegative);
        SetRipperInstructionList(
            slot,
            wasMovingNegative ? positiveInstruction : negativeInstruction);
    }

    private static void SetRipperInstructionList(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RipperVariantEnemyState RequireRipperVariantState(RoomEnemySlot slot) =>
        _ripperVariantStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized GRipper/Ripper II state.");
}
