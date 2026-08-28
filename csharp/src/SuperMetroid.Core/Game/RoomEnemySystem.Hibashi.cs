namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 function word stored in Hibashi variable A. The graphics part switches
/// between one inactive timer and one instruction-driven active eruption.
/// </summary>
public enum HibashiEnemyFunction : ushort
{
    Inactive = 0x902f,
    Active = 0x9062,
}

/// <summary>
/// Debugger-facing projection of Hibashi's shared variables and population aliases. Every
/// shipped pillar uses two consecutive records of definition $E07F: part zero owns art and
/// timing, while nonzero part one is the invisible collision body manipulated by part zero.
/// </summary>
public sealed class HibashiEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal HibashiEnemyState(RoomEnemySlot slot) => _slot = slot;

    public HibashiEnemyFunction Function
    {
        get => (HibashiEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort InactiveTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public ushort FinishedActivityFlag
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort SpawnYPosition
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native extension word $0FB4 aliases population init0/parameter one.</summary>
    public ushort InactiveTimerResetValue => _slot.Parameter1;

    /// <summary>Native extension word $0FB6 aliases population init1/parameter two.</summary>
    public ushort Part => _slot.Parameter2;
}

/// <summary>
/// Literal translation of Hibashi/fire pillar enemy <c>$E07F</c> from
/// <c>$A6:8CFB-$94D2</c>. The art owner never moves. Its ROM instruction commands move and
/// resize the following invisible slot, turning the eruption animation itself into the
/// damaging hitbox before both parts return to their inactive property state.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort HibashiDefinition = 0xe07f;

    private const ushort HibashiGraphicsInstructionList = 0x8d1b;
    private const ushort HibashiHitboxInstructionList = 0x8da9;
    private const ushort HibashiEruptionSoundEffect = 0x0061;
    private const int HibashiYOffsets = 0xa68dbb;
    private const int HibashiYRadiusTable = 0xa68de7;

    private readonly HibashiEnemyState?[] _hibashiStates =
        new HibashiEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for both graphics and hitbox records in all physical slots.</summary>
    public IReadOnlyList<HibashiEnemyState?> HibashiStates => _hibashiStates;

    /// <summary>Ports <c>InitAI_Hibashi</c> at <c>$A6:8FFC</c>.</summary>
    private void InitializeHibashi(RoomEnemySlot slot)
    {
        var state = new HibashiEnemyState(slot);
        _hibashiStates[slot.SlotIndex] = state;

        // Every part starts with the one-frame hitbox map. Only part zero replaces it with
        // the 17-frame eruption list and initializes the state-machine words below.
        slot.CurrentInstruction = HibashiHitboxInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        if (state.Part != 0)
            return;

        slot.CurrentInstruction = HibashiGraphicsInstructionList;
        state.Function = HibashiEnemyFunction.Inactive;
        state.SpawnYPosition = slot.YPosition;

        // The visible graphics record must never participate through its header radius.
        // Its following part receives the dynamic radius from instruction commands.
        slot.XRadius = 0;
    }

    /// <summary>Ports <c>MainAI_Hibashi</c> at <c>$A6:9023</c>.</summary>
    private void RunHibashiMain(RoomEnemySlot slot, HibashiEnemyState state)
    {
        if (state.Part != 0)
            return;

        switch (state.Function)
        {
            case HibashiEnemyFunction.Inactive:
                RunHibashiInactive(slot, state);
                return;

            case HibashiEnemyFunction.Active:
                RunHibashiActive(slot, state);
                return;

            default:
                throw new NotSupportedException(
                    $"Hibashi function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports <c>Function_Hibashi_Inactive</c> at <c>$A6:902F</c>.</summary>
    private void RunHibashiInactive(RoomEnemySlot graphics, HibashiEnemyState state)
    {
        // DEC/BPL means timer zero deliberately becomes $FFFF and erupts immediately. A
        // reset value N later produces N+1 inactive actor frames because zero is nonnegative.
        state.InactiveTimer = unchecked((ushort)(state.InactiveTimer - 1));
        if (!IsNegative16(state.InactiveTimer))
            return;

        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);
        state.Function = HibashiEnemyFunction.Active;
        state.FinishedActivityFlag = 0;
        graphics.InstructionTimer = 1;
        graphics.Timer = 0;
        graphics.CurrentInstruction = HibashiGraphicsInstructionList;

        // Property $0100 controls rendering only. Property $0400 controls admission to
        // Samus/projectile/grapple collision. The bottom remains visually invisible for the
        // entire eruption while becoming a live collision body here.
        graphics.Properties = graphics.Properties.Without(EnemyProperties.Invisible);
        hitbox.Properties = hitbox.Properties.Without(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Ports <c>Function_Hibashi_Active</c> at <c>$A6:9062</c>.</summary>
    private static void RunHibashiActive(RoomEnemySlot graphics, HibashiEnemyState state)
    {
        if (state.FinishedActivityFlag == 0)
            return;

        state.InactiveTimer = state.InactiveTimerResetValue;
        graphics.Properties = graphics.Properties.With(EnemyProperties.Invisible);
        state.Function = HibashiEnemyFunction.Inactive;
    }

    /// <summary>Ports instruction <c>$A6:8DAF</c>.</summary>
    private void PlayHibashiEruptionSound()
    {
        LastHibashiSoundEffect = HibashiEruptionSoundEffect;
    }

    /// <summary>
    /// Ports activity instructions $A6:8E13-$8F45. The 16 commands differ only by their
    /// direct word-table index; frame zero additionally restores the fixed eight-pixel X
    /// radius. Reading both tables from the cartridge keeps their shrinking final radii
    /// inspectable without duplicating ROM data as host constants.
    /// </summary>
    private void ApplyHibashiActivityFrame(RoomEnemySlot graphics, int frameIndex)
    {
        if ((uint)frameIndex >= 16)
            throw new ArgumentOutOfRangeException(nameof(frameIndex));

        HibashiEnemyState state = RequireHibashiState(graphics);
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);
        int tableOffset = frameIndex * 2;
        hitbox.YPosition = unchecked((ushort)(
            state.SpawnYPosition - ReadWord(_bus!, HibashiYOffsets + tableOffset)));
        hitbox.YRadius = ReadWord(_bus!, HibashiYRadiusTable + tableOffset);
        if (frameIndex == 0)
            hitbox.XRadius = 8;
    }

    /// <summary>Ports finish-activity instruction <c>$A6:8FD1</c>.</summary>
    private void FinishHibashiActivity(RoomEnemySlot graphics)
    {
        HibashiEnemyState state = RequireHibashiState(graphics);
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);

        state.FinishedActivityFlag = 1;
        hitbox.XRadius = 0;
        hitbox.YRadius = 0;
        graphics.YPosition = state.SpawnYPosition;
        graphics.Properties = graphics.Properties.With(EnemyProperties.Invisible);
        hitbox.Properties = hitbox.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>
    /// Resolves the native <c>Enemy[1]</c> operand. Retail populations always pair part zero
    /// with a following part-one record; reject only states that cannot represent that read.
    /// </summary>
    private RoomEnemySlot GetFollowingHibashiPart(RoomEnemySlot graphics)
    {
        if (graphics.SlotIndex >= MaximumEnemyCount - 1)
            throw new InvalidDataException("Hibashi graphics part has no following enemy slot.");
        return _slots[graphics.SlotIndex + 1];
    }

    private HibashiEnemyState RequireHibashiState(RoomEnemySlot slot) =>
        _hibashiStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Hibashi state.");
}
