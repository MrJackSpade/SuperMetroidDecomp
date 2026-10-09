namespace SuperMetroid.Core.Game;

/// <summary>
/// The two indirect main-AI destinations used by Kago definition <c>$E7FF</c>. Keeping the
/// ROM pointers as enum values makes a debugger watch directly comparable with variable A.
/// </summary>
public enum KagoEnemyFunction : ushort
{
    /// <summary>Function_Kago_Nothing, $A8:AB7B: the first main-AI call only replaces variable A with its $AB81 return label; shell animation and shot-triggered effects remain independently owned.</summary>
    InstallNoOp = 0xab7b,
    /// <summary>Function_Kago_Nothing.return, $A8:AB81: persistent inert RTL target after the initial handoff; the instruction interpreter animates the stationary shell and its shot callback controls damage response and bug spawning.</summary>
    NoOp = 0xab81,
}

/// <summary>
/// Typed view of Kago's ordinary variables plus its bank-$7E:7808 hit counter. The latter
/// is genuinely outside the common 64-byte enemy record, so it remains actor-owned state
/// instead of being disguised as an unrelated generic slot word.
/// </summary>
public sealed class KagoEnemyState
{
    /// <summary>Common enemy-record storage from which this typed state reads and writes Kago's variable fields.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Associates Kago's typed state with the common enemy record for its physical slot.</summary>
    /// <param name="slot">Initialized enemy slot whose variables back the Kago-specific fields.</param>
    internal KagoEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Common variable A ($7E:0FA8 plus physical slot offset), holding the native bank-$A8 indirect main-AI word; initialized to $AB7B and changed to $AB81 on the first AI update.</summary>
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
    /// <summary>Native enemy-definition pointer for the stationary Kago shell in bank $A8.</summary>
    internal const ushort KagoDefinition = 0xe7ff;
    /// <summary>Native shot-response callback pointer used after common projectile hit handling.</summary>
    internal const ushort KagoShotAi = EnemyAiCodePointers.BankA8.KagoShot;

    /// <summary>Typed Kago state indexed by physical enemy slot until each shell is removed.</summary>
    private readonly KagoEnemyState?[] _kagoStates =
        new KagoEnemyState?[MaximumEnemyCount];

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
        slot.CurrentInstruction = KagoInstructionProgramDefinitions.Slow;
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
            slot.CurrentInstruction = KagoInstructionProgramDefinitions.Fast;
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

    /// <summary>Gets the typed Kago state established when this shell was initialized.</summary>
    /// <param name="slot">Enemy slot whose Kago-specific state is required.</param>
    /// <returns>The state associated with <paramref name="slot"/>.</returns>
    /// <exception cref="InvalidOperationException">The slot has not been initialized as a Kago.</exception>
    private KagoEnemyState RequireKagoState(RoomEnemySlot slot) =>
        _kagoStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Kago state.");
}
