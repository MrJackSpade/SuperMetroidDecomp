namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 function word stored in Hibashi variable A. The graphics part switches
/// between one inactive timer and one instruction-driven active eruption.
/// </summary>
public enum HibashiEnemyFunction : ushort
{
    /// <summary>$A6:902F, Function_Hibashi_Inactive: decrement the graphics owner's wait word and, on signed underflow, restart eruption art and enable the following invisible collision part.</summary>
    Inactive = 0x902f,
    /// <summary>$A6:9062, Function_Hibashi_Active: let instruction-driven art/hitbox commands run until the finish flag is set, then reload the population wait and return to inactive.</summary>
    Active = 0x9062,
}

/// <summary>
/// Debugger-facing projection of Hibashi's shared variables and population aliases. Every
/// shipped pillar uses two consecutive records of definition $E07F: part zero owns art and
/// timing, while nonzero part one is the invisible collision body manipulated by part zero.
/// </summary>
public sealed class HibashiEnemyState
{
    /// <summary>Population record whose native variables back this Hibashi state view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a state view over one Hibashi population record.</summary>
    /// <param name="slot">Enemy slot containing this part's shared variables and native parameters.</param>
    internal HibashiEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Graphics part's same-bank function word in common variable A, initialized to inactive; nonzero population parts do not dispatch this state machine.</summary>
    public HibashiEnemyFunction Function
    {
        get => (HibashiEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Raw countdown in common variable B, decremented once per inactive AI update; zero wraps to $FFFF and erupts immediately, while a nonnegative reset N waits N+1 updates.</summary>
    public ushort InactiveTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Animation-to-AI handshake in common variable C: cleared on eruption start, set to one by $A6:8FD1 after disabling the hitbox, and tested as nonzero by active AI.</summary>
    public ushort FinishedActivityFlag
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Graphics owner's whole-pixel room Y captured at initialization; activity frames place the next part's hitbox relative to it and the finish instruction restores the art owner to it.</summary>
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
    /// <summary>Per-slot Hibashi state views, populated when each Hibashi part is initialized.</summary>
    private readonly HibashiEnemyState?[] _hibashiStates =
        new HibashiEnemyState?[MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Hibashi</c> at <c>$A6:8FFC</c>.</summary>
    private void InitializeHibashi(RoomEnemySlot slot)
    {
        var state = new HibashiEnemyState(slot);
        _hibashiStates[slot.SlotIndex] = state;

        // Every part starts with the one-frame hitbox map. Only part zero replaces it with
        // the instruction-driven eruption list and initializes the state-machine words below.
        slot.CurrentInstruction = HibashiDefinitions.HitboxInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        if (state.Part != 0)
            return;

        slot.CurrentInstruction = HibashiDefinitions.GraphicsInstructionList;
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
                throw new InvalidDataException(
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
        graphics.CurrentInstruction = HibashiDefinitions.GraphicsInstructionList;

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
        LastHibashiSoundEffect = HibashiDefinitions.EruptionSoundEffect;
    }

    /// <summary>
    /// Ports activity instructions $A6:8E13-$8FBD. The 22 commands differ only by their
    /// direct activity index; frame zero additionally restores the fixed eight-pixel X
    /// radius.
    /// </summary>
    private void ApplyHibashiActivityFrame(RoomEnemySlot graphics, int frameIndex)
    {
        if ((uint)frameIndex >= 22)
            throw new ArgumentOutOfRangeException(nameof(frameIndex));

        LastHibashiActivityFrameIndex = frameIndex;
        HibashiEnemyState state = RequireHibashiState(graphics);
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);
        HibashiActivityDefinition frame = HibashiDefinitions.ActivityFrame(frameIndex);
        hitbox.YPosition = unchecked((ushort)(
            state.SpawnYPosition - frame.YOffset));
        hitbox.YRadius = frame.YRadius;
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

    /// <summary>Gets the initialized Hibashi state associated with an enemy slot.</summary>
    /// <param name="slot">Hibashi slot whose state is required.</param>
    /// <returns>The state view created for the slot during Hibashi initialization.</returns>
    /// <exception cref="InvalidOperationException">The slot has not been initialized as Hibashi.</exception>
    private HibashiEnemyState RequireHibashiState(RoomEnemySlot slot) =>
        _hibashiStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Hibashi state.");
}
