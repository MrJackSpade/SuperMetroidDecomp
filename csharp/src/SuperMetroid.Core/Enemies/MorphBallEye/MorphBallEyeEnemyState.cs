using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Typed aliases for the four common enemy words used by retail enemy <c>$E6BF</c>.
/// The mount and eye body share a definition, but parameter two's sign selects their role.
/// </summary>
public sealed class MorphBallEyeEnemyState
{
    /// <summary>Native enemy slot whose variables C through F hold this eye's state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the eye's typed state over an existing enemy slot.</summary>
    internal MorphBallEyeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MorphBallEyeProgram> Program { get; set; }

    /// <summary>
    /// Native <c>Eye.activatedFlag</c> in variable C: one once the beam object has initialized, zero once the body
    /// starts closing. Nothing else is stored here; the beam keeps cycling its colours only while it is nonzero.
    /// </summary>
    public int ActivatedFlag
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = checked((ushort)value);
    }

    /// <summary>
    /// Zero-up, clockwise, 1/256-turn aim angle in native variable D. Only the low byte is
    /// meaningful, but the cartridge stores the complete accumulator result as a word.
    /// </summary>
    public SnesAngle Angle
    {
        get => SnesAngle.FromTableIndex(unchecked((byte)_slot.VariableD));
        internal set => _slot.VariableD = value.TableIndex;
    }

    /// <summary>Activation/deactivation countdown in native variable E.</summary>
    public NativeCountdown FunctionTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Indirect bank-$A8 function word in native variable F.</summary>
    public MorphBallEyeAiFunction Function
    {
        get => (MorphBallEyeAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
