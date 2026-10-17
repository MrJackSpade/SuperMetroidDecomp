using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Typed debugger view of all six private words used by the shared bank-$A8 actor.
/// Keeping this as a view over <see cref="RoomEnemySlot"/> preserves the actual WRAM aliasing
/// and avoids maintaining a second, subtly divergent copy of the enemy state.
/// </summary>
public sealed class FuneNamiheEnemyState
{
    /// <summary>Physical enemy record whose variables A through F hold the shared actor state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view without copying Fune or Namihe's native variable words.</summary>
    internal FuneNamiheEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<FuneNamiheProgram> Program { get; set; }

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
    /// Variable D: the complete low nibble of population parameter one, 0 through 15. Zero selects Fune;
    /// every nonzero value selects Namihe, exactly as the native BEQ test does. The slot word holds the
    /// value exactly, so aliased reads of slot memory see the native word.
    /// </summary>
    public int VariantIndex
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = checked((ushort)value);
    }

    /// <summary>
    /// Variable E: Fune's incrementing cooldown count of waiting updates, compared with
    /// <see cref="CooldownTime"/> and reset to zero when the wait ends. The slot word holds the
    /// count exactly; the waiting routine's increment wraps it at the word boundary.
    /// </summary>
    public int CooldownTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = checked((ushort)value);
    }

    /// <summary>Variable F: the number of updates Fune waits, initialized from parameter one's high byte.</summary>
    public int CooldownTime
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = checked((ushort)value);
    }

    /// <summary>The shared initializer treats any nonzero variant nibble as Namihe.</summary>
    public bool IsNamihe => VariantIndex != 0;
}
