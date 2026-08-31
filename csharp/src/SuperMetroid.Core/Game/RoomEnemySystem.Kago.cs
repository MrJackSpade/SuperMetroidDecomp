namespace SuperMetroid.Core.Game;

/// <summary>
/// The two indirect main-AI destinations used by Kago definition <c>$E7FF</c>. Keeping the
/// ROM pointers as enum values makes a debugger watch directly comparable with variable A.
/// </summary>
public enum KagoEnemyFunction : ushort
{
    InstallNoOp = 0xab7b,
    NoOp = 0xab81,
}

/// <summary>
/// Typed view of Kago's ordinary variables plus its bank-$7E:7808 hit counter. The latter
/// is genuinely outside the common 64-byte enemy record, so it remains actor-owned state
/// instead of being disguised as an unrelated generic slot word.
/// </summary>
public sealed class KagoEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal KagoEnemyState(RoomEnemySlot slot) => _slot = slot;

    public KagoEnemyFunction Function
    {
        get => (KagoEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: zero for the ten-frame loop, one for the three-frame loop.</summary>
    public bool UsesFastAnimation
    {
        get => _slot.VariableB != 0;
        internal set => _slot.VariableB = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Extra enemy word four. The ROM decrements first and dies only when the signed result
    /// is negative, so population value ten permits eleven accepted shots.
    /// </summary>
    public ushort HitCounter { get; internal set; }

    /// <summary>Variable F: set after Kago requests death animation variant four.</summary>
    public bool DeathAnimationStarted
    {
        get => _slot.VariableF != 0;
        internal set => _slot.VariableF = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Number of bank-$86 Kago bugs successfully allocated from this shell.</summary>
    public int SpawnedBugCount { get; internal set; }
}

/// <summary>
/// Literal translation of stationary Kago definition <c>$E7FF</c>. The shell's main AI is
/// intentionally a one-frame hand-off to a no-op; its custom shot callback owns the visible
/// acceleration, earthquake, hit counter, death request, and bug spawn.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort KagoDefinition = 0xe7ff;
    internal const ushort KagoShotAi = 0xab83;

    private const ushort KagoSlowInstructionList = 0xab1e;
    private const ushort KagoFastInstructionList = 0xab32;

    private readonly KagoEnemyState?[] _kagoStates =
        new KagoEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<KagoEnemyState?> KagoStates => _kagoStates;

    /// <summary>Ports <c>Kago_Init</c> at <c>$A8:AB46</c>.</summary>
    private void InitializeKago(RoomEnemySlot slot)
    {
        var state = new KagoEnemyState(slot);
        _kagoStates[slot.SlotIndex] = state;

        // Kago's population word already carries the other authored property bits. Native
        // init ORs only $2000 so the common instruction interpreter animates the shell.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.VariableE = 0;
        slot.CurrentInstruction = KagoSlowInstructionList;
        state.Function = KagoEnemyFunction.InstallNoOp;
        state.DeathAnimationStarted = false;
        state.HitCounter = slot.Parameter1;
    }

    /// <summary>Ports <c>Kago_Main</c> and its one useful indirect target.</summary>
    private static void RunKagoMain(KagoEnemyState state)
    {
        switch (state.Function)
        {
            case KagoEnemyFunction.InstallNoOp:
                state.Function = KagoEnemyFunction.NoOp;
                return;
            case KagoEnemyFunction.NoOp:
                return;
            default:
                throw new InvalidDataException(
                    $"Kago main function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Ports the Kago-owned tail of <c>Kago_Shot</c> at <c>$A8:AB83</c>. The caller has
    /// already executed common normal-enemy shot AI, exactly matching the leading JSR in
    /// the cartridge routine.
    /// </summary>
    private void ResolveKagoShotAfterCommon(RoomEnemySlot slot, KagoEnemyState state)
    {
        EarthquakeType = 2;
        EarthquakeTimer = 16;

        if (!state.UsesFastAnimation)
        {
            state.UsesFastAnimation = true;
            slot.CurrentInstruction = KagoFastInstructionList;
            slot.InstructionTimer = 1;
        }

        state.HitCounter = unchecked((ushort)(state.HitCounter - 1));
        if (unchecked((short)state.HitCounter) < 0)
        {
            // Common shot AI can theoretically kill the 1600-health shell first with a
            // debug-edited projectile. Do not count the same native death request twice.
            if (!slot.Properties.HasAny(EnemyProperties.Deleted))
            {
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
            }
            state.DeathAnimationStarted = true;
        }

        // The ROM spawns even on the lethal eleventh callback, after requesting Kago's
        // death animation. Pool exhaustion is silent and therefore does not roll back the
        // counter, earthquake, or animation transition.
        if (SpawnKagoBug(slot))
            state.SpawnedBugCount++;
    }

    private KagoEnemyState RequireKagoState(RoomEnemySlot slot) =>
        _kagoStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Kago state.");
}
