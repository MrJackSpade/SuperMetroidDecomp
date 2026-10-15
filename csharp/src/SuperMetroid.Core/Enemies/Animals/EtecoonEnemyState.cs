using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Debugger-facing projection of the six native Etecoon variables. All values remain backed
/// by the common enemy slot, so a watch window shows the same wrapped words consumed by the
/// translated dispatcher and the cartridge instruction interpreter.
/// </summary>
public sealed class EtecoonEnemyState
{
    /// <summary>Physical enemy record whose variables A through F hold Etecoon's native state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view over the existing Etecoon slot without copying its variables.</summary>
    internal EtecoonEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<EtecoonProgram> Program { get; set; }

    /// <summary>
    /// Signed 16.16 velocity: the vertical whole and fraction words in variables A and B, the
    /// horizontal whole and fraction words in variables C and D.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableC, _slot.VariableD, _slot.VariableA, _slot.VariableB);
        internal set
        {
            _slot.VariableA = value.Y.WholeWord;
            _slot.VariableB = value.Y.Fraction;
            _slot.VariableC = value.X.WholeWord;
            _slot.VariableD = value.X.Fraction;
        }
    }

    /// <summary>Wrapped countdown in variable E.</summary>
    public NativeCountdown FunctionTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Bank-$A7 function address in variable F.</summary>
    public EtecoonAiFunction Function
    {
        get => (EtecoonAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
