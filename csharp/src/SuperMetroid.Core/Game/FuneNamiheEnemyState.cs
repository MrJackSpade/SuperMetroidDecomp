using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge function pointers stored in Fune/Namihe variable B (<c>$0FAA,x</c>).
/// The two one-byte RTS targets are retained separately because they are observable native
/// state: Fune and Namihe deliberately park on different addresses while mouth animation
/// bytecode owns the active portion of their attack cycle.
/// </summary>
public enum FuneNamiheEnemyFunction : ushort
{
    /// <summary>Function_Fune_WaitForTimer, $A8:9737: increments the wrapping cooldown before a signed comparison with parameter one's high-byte target; on expiry it clears the counter, selects active mouth bytecode, and parks at $978E.</summary>
    FuneWaitForCooldown = 0x9737,
    /// <summary>Function_Namihe_WaitForSamusToGetNear, $A8:975C: selects active mouth bytecode only when Samus's modular vertical distance is strictly below the whole-pixel parameter-two high-byte threshold, then parks at $978F without a cooldown.</summary>
    NamiheWaitForSamus = 0x975c,
    /// <summary>RTS_A8978E, $A8:978E: Fune's inert main-AI target while animation instructions own mouth motion, projectile/sound production, and the finish callback that restores its cooldown wait.</summary>
    FuneActivityNoOp = 0x978e,
    /// <summary>RTS_A8978F, $A8:978F: Namihe's separately observable inert main-AI target while animation bytecode owns the attack and eventually restores its Samus-proximity wait.</summary>
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
