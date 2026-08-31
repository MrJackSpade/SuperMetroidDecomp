namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge function pointers stored in Fune/Namihe variable B (<c>$0FAA,x</c>).
/// The two one-byte RTS targets are retained separately because they are observable native
/// state: Fune and Namihe deliberately park on different addresses while mouth animation
/// bytecode owns the active portion of their attack cycle.
/// </summary>
public enum FuneNamiheEnemyFunction : ushort
{
    FuneWaitForCooldown = 0x9737,
    NamiheWaitForSamus = 0x975c,
    FuneActivityNoOp = 0x978e,
    NamiheActivityNoOp = 0x978f,
}

/// <summary>
/// Typed debugger view of all six private words used by the shared bank-$A8 actor.
/// Keeping this as a view over <see cref="RoomEnemySlot"/> preserves the actual WRAM aliasing
/// and avoids maintaining a second, subtly divergent copy of the enemy state.
/// </summary>
public sealed class FuneNamiheEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal FuneNamiheEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Variable A: pointer into the eight-entry table at $A8:96D3, not directly to an
    /// animation list. Subtracting four selects active art; adding four restores idle art.
    /// </summary>
    public ushort InstructionListPointerTableCursor
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Variable B: indirect main-AI function.</summary>
    public FuneNamiheEnemyFunction Function
    {
        get => (FuneNamiheEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Variable C: strict vertical wake distance used only by Namihe.</summary>
    public ushort YProximity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>
    /// Variable D: the complete low nibble of population parameter one. Zero selects Fune;
    /// every nonzero value selects Namihe, exactly as the native BEQ test does.
    /// </summary>
    public ushort VariantIndex
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable E: Fune's incrementing cooldown counter.</summary>
    public ushort CooldownTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Variable F: cooldown target from parameter one's high byte.</summary>
    public ushort CooldownTime
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>The shared initializer treats any nonzero variant nibble as Namihe.</summary>
    public bool IsNamihe => VariantIndex != 0;
}

/// <summary>
/// Literal actor translation for Fune $A0:E6FF and Namihe $A0:E73F. These stationary vents
/// do not move themselves: main AI decides when to switch from idle to active bytecode,
/// while instruction opcodes open the mouth, create the projectile, play sound, and hand
/// control back to the correct wait function.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort FuneDefinition = 0xe6ff;
    internal const ushort NamiheDefinition = 0xe73f;

    private const ushort FuneIdleLeftPointerTableEntry = 0x96d7;
    private const ushort NamiheIdleLeftPointerTableEntry = 0x96df;
    private const ushort ActivePointerTableDelta = 4;
    private const ushort FacingRightPointerTableDelta = 2;
    private const ushort FuneNamiheSpitSound = 0x001f;

    private readonly FuneNamiheEnemyState?[] _funeNamiheStates =
        new FuneNamiheEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for all physical slots occupied by Fune or Namihe.</summary>
    public IReadOnlyList<FuneNamiheEnemyState?> FuneNamiheStates => _funeNamiheStates;

    /// <summary>
    /// Most recent library-two spit sound requested by opcode $A8:9625. Audio mixing is an
    /// outer frontend concern, so the actor publishes the exact sound number for that seam.
    /// </summary>
    public ushort? LastFuneNamiheSoundEffect { get; private set; }

    private static bool IsFuneNamiheDefinition(ushort definitionPointer) =>
        definitionPointer is FuneDefinition or NamiheDefinition;

    /// <summary>Ports shared initializer <c>$A8:96E3</c>.</summary>
    private void InitializeFuneNamihe(RoomEnemySlot slot)
    {
        FuneNamiheEnemyState state = new(slot)
        {
            InstructionListPointerTableCursor = FuneIdleLeftPointerTableEntry,
            Function = FuneNamiheEnemyFunction.FuneWaitForCooldown,
            VariantIndex = unchecked((ushort)(slot.Parameter1 & 0x000f)),
            YProximity = unchecked((byte)(slot.Parameter2 >> 8)),
            CooldownTime = unchecked((byte)(slot.Parameter1 >> 8)),
            CooldownTimer = 0,
        };

        // Species is encoded in the population parameter rather than inferred from the
        // enemy header. This sounds redundant, but preserving it lets malformed/custom ROM
        // populations exhibit the same cross-species behavior as the retail routine.
        if (state.IsNamihe)
        {
            state.InstructionListPointerTableCursor = NamiheIdleLeftPointerTableEntry;
            state.Function = FuneNamiheEnemyFunction.NamiheWaitForSamus;
        }

        // Any bit in parameter one's upper low-byte nibble selects right-facing art. The
        // code does not mask to a single direction flag before making this decision.
        if ((slot.Parameter1 & 0x00f0) != 0)
        {
            state.InstructionListPointerTableCursor = unchecked((ushort)(
                state.InstructionListPointerTableCursor + FacingRightPointerTableDelta));
        }

        _funeNamiheStates[slot.SlotIndex] = state;
        InstallFuneNamiheInstructionList(slot, state);
    }

    /// <summary>Ports shared main dispatcher <c>$A8:9730</c>.</summary>
    private void RunFuneNamiheMain(
        RoomEnemySlot slot,
        FuneNamiheEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case FuneNamiheEnemyFunction.FuneWaitForCooldown:
                // INC occurs before CMP. BMI tests the signed subtraction result rather
                // than performing an unsigned branch; ordinary room parameters are small,
                // but the distinction remains visible at the $8000 wrap boundary.
                state.CooldownTimer = unchecked((ushort)(state.CooldownTimer + 1));
                if (unchecked((short)(state.CooldownTimer - state.CooldownTime)) < 0)
                    return;

                state.InstructionListPointerTableCursor = unchecked((ushort)(
                    state.InstructionListPointerTableCursor - ActivePointerTableDelta));
                InstallFuneNamiheInstructionList(slot, state);
                state.Function = FuneNamiheEnemyFunction.FuneActivityNoOp;
                state.CooldownTimer = 0;
                return;

            case FuneNamiheEnemyFunction.NamiheWaitForSamus:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Namihe proximity AI requires the active Samus actor.");
                }

                // IsSamusWithinAPixelRowsOfEnemy takes the modular absolute difference and
                // uses a strict comparison. Equality at the configured distance stays idle.
                if (!IsWithinStrictModularDistance(
                        samus.YPosition,
                        slot.YPosition,
                        state.YProximity))
                {
                    return;
                }

                state.InstructionListPointerTableCursor = unchecked((ushort)(
                    state.InstructionListPointerTableCursor - ActivePointerTableDelta));
                InstallFuneNamiheInstructionList(slot, state);
                state.Function = FuneNamiheEnemyFunction.NamiheActivityNoOp;
                return;

            case FuneNamiheEnemyFunction.FuneActivityNoOp:
            case FuneNamiheEnemyFunction.NamiheActivityNoOp:
                // The active animation's finish opcode is the only owner allowed to leave
                // these RTS states. Main AI intentionally does nothing until that command.
                return;

            default:
                throw new InvalidDataException(
                    $"Fune/Namihe function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Models <c>SetFuneNamiheInstList</c>: the state word points to a ROM word which in
    /// turn points to the actual bytecode list. Both instruction timers are reset together.
    /// </summary>
    private void InstallFuneNamiheInstructionList(
        RoomEnemySlot slot,
        FuneNamiheEnemyState state)
    {
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = ReadFuneNamiheInstructionList(
            state.InstructionListPointerTableCursor);
    }

    /// <summary>
    /// The table lives at $A8:96D3; callers carry a same-bank cursor. This overload makes
    /// that two-stage address explicit without allowing a host addition to cross banks.
    /// </summary>
    private ushort ReadFuneNamiheInstructionList(ushort tableCursor) =>
        ReadWord(_bus!, 0xa80000 | tableCursor);

    /// <summary>Instruction $A8:9625.</summary>
    private void QueueFuneNamiheSpitSound() =>
        LastFuneNamiheSoundEffect = FuneNamiheSpitSound;

    /// <summary>Instructions $A8:9695/$96B4, which differ only by address.</summary>
    private void FinishFuneNamiheActivity(FuneNamiheEnemyState state)
    {
        state.InstructionListPointerTableCursor = unchecked((ushort)(
            state.InstructionListPointerTableCursor + ActivePointerTableDelta));
        state.Function = state.IsNamihe
            ? FuneNamiheEnemyFunction.NamiheWaitForSamus
            : FuneNamiheEnemyFunction.FuneWaitForCooldown;
    }

    private FuneNamiheEnemyState RequireFuneNamiheState(RoomEnemySlot slot) =>
        _funeNamiheStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Fune/Namihe state.");
}
