namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 function word stored in Wrecked Ship Spark variable B. The values are
/// deliberately the cartridge addresses: a debugger can compare the selected state with
/// <c>$0FAA,x</c> directly, and an accidental table overread remains visible as an unknown
/// native address instead of being coerced into a plausible host state.
/// </summary>
public enum SparkEnemyFunction : ushort
{
    AlwaysActive = 0xe694,
    IntermittentInactive = 0xe695,
    IntermittentActive = 0xe6b7,
    EmitFallingSparks = 0xe6dc,
}

/// <summary>
/// Debugger-facing projection of Spark's three private words. The actor uses common enemy
/// variable B for its indirect function, variable E for the population-supplied base time,
/// and variable F for the live countdown.
/// </summary>
public sealed class SparkEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal SparkEnemyState(RoomEnemySlot slot) => _slot = slot;

    public SparkEnemyFunction Function
    {
        get => (SparkEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    public ushort BaseFunctionTime
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort FunctionTimer
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}

/// <summary>
/// Literal translation of Wrecked Ship Spark enemy <c>$EA3F</c> from
/// <c>$A8:E587-$E7AA</c>. Population parameter one chooses an always-active body, an
/// intermittent tangible/intangible body, or a stationary emitter. Parameter two is the
/// base countdown; negative values request a fresh cartridge RNG delay.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort SparkDefinition = 0xea3f;
    internal const ushort SparkShotAi = 0xe70e;

    private const int SparkInstructionListTable = 0xa8e682;
    private const int SparkFunctionTable = 0xa8e688;
    private const ushort SparkFlickerOnInstructionList = 0xe5a7;
    private const ushort SparkFlickerOutInstructionList = 0xe5e5;

    private readonly SparkEnemyState?[] _sparkStates =
        new SparkEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Spark state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<SparkEnemyState?> SparkStates => _sparkStates;

    /// <summary>Ports <c>InitAI_Spark</c> at <c>$A8:E637</c>.</summary>
    private void InitializeSpark(RoomEnemySlot slot)
    {
        var state = new SparkEnemyState(slot);
        _sparkStates[slot.SlotIndex] = state;

        // Native masks the selector to two bits, but both tables contain only three words.
        // Reading the ROM directly preserves selector three's real adjacent-code overread;
        // a later main pass will report the exact nonsensical function if malformed/custom
        // population data actually selects it.
        int selectorOffset = (slot.Parameter1 & 3) * 2;
        state.Function = (SparkEnemyFunction)ReadWord(
            _bus!,
            SparkFunctionTable + selectorOffset);
        state.BaseFunctionTime = slot.Parameter2;
        SetSparkFunctionTimer(state, additionalTime: 0);

        slot.InstructionTimer = 1;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            SparkInstructionListTable + selectorOffset);
        slot.Timer = 0;

        if (_isAreaBossDefeated?.Invoke() ?? false)
            return;

        // This is intentionally *not* `ORA #$0100`. The retail instruction is the odd
        // absolute-memory form `ORA $0100`, noted as a bug in the disassembly. Preserve the
        // exact live WRAM dependency so stack-page contents, including a zero word, produce
        // the same properties as the cartridge instead of inventing forced invisibility.
        slot.Properties = unchecked((ushort)(
            slot.Properties | ReadWord(_bus!, 0x7e0100)));
    }

    /// <summary>Ports the indirect main dispatcher at <c>$A8:E68E</c>.</summary>
    private void RunSparkMain(RoomEnemySlot slot, SparkEnemyState state)
    {
        switch (state.Function)
        {
            case SparkEnemyFunction.AlwaysActive:
                return;

            case SparkEnemyFunction.IntermittentInactive:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                state.Function = SparkEnemyFunction.IntermittentActive;
                InstallSparkInstructionList(slot, SparkFlickerOnInstructionList);
                SetSparkFunctionTimer(state, additionalTime: 0);
                return;

            case SparkEnemyFunction.IntermittentActive:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                state.Function = SparkEnemyFunction.IntermittentInactive;
                InstallSparkInstructionList(slot, SparkFlickerOutInstructionList);
                SetSparkFunctionTimer(state, additionalTime: 8);
                return;

            case SparkEnemyFunction.EmitFallingSparks:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                SpawnFallingSpark(slot);
                SetSparkFunctionTimer(state, additionalTime: 0);
                return;

            default:
                throw new NotSupportedException(
                    $"Spark function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Reproduces <c>DEC; BEQ</c>. A zero countdown wraps to <c>$FFFF</c> and therefore does
    /// not expire for another 65,535 actor frames; it must not be treated as immediately due.
    /// </summary>
    private static bool DecrementSparkTimerAndTestExpired(SparkEnemyState state)
    {
        ushort decremented = unchecked((ushort)(state.FunctionTimer - 1));
        if (decremented == 0)
            return true;

        state.FunctionTimer = decremented;
        return false;
    }

    /// <summary>Ports <c>SetSparkFunctionTimer</c> at <c>$A8:E6F6</c>.</summary>
    private void SetSparkFunctionTimer(SparkEnemyState state, ushort additionalTime)
    {
        ushort baseTime = state.BaseFunctionTime;
        if (unchecked((short)baseTime) < 0)
        {
            Func<ushort> nextRandom = _nextRandom ?? throw new InvalidOperationException(
                "Spark randomized timing requires the shared cartridge RNG.");
            baseTime = unchecked((ushort)((nextRandom() & 0x003f) + 4));
        }

        state.FunctionTimer = unchecked((ushort)(baseTime + additionalTime));
    }

    private static void InstallSparkInstructionList(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = 1;
    }

    private SparkEnemyState RequireSparkState(RoomEnemySlot slot) =>
        _sparkStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Spark state.");
}
