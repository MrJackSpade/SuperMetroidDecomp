using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Named view of Waver's seven private words at <c>$0FA8-$0FB4</c>. Keeping the words in
/// their cartridge roles makes instruction-list changes and fixed-point motion inspectable
/// without leaking generic <c>VariableA</c> names into the translated AI.
/// </summary>
public sealed class WaverEnemyState
{
    /// <summary>Physical enemy slot retaining Waver's fixed-point motion and animation words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Waver's seven private words from its occupied enemy slot.</summary>
    internal WaverEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WaverProgram> Program { get; set; }

    /// <summary>Low 16 bits of the signed horizontal 16.16 displacement.</summary>
    public ushort XSubvelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>High 16 bits of the signed horizontal 16.16 displacement.</summary>
    public short XVelocity
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)value);
    }

    /// <summary>The instruction-list index currently installed in the common enemy slot.</summary>
    public WaverAnimationSelector CurrentInstructionListIndex
    {
        get => (WaverAnimationSelector)_slot.VariableC;
        internal set => _slot.VariableC = value.ToNativeWord();
    }

    /// <summary>Byte angle advanced by two per unobstructed frame.</summary>
    public ushort Angle
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Set by instruction <c>$A3:86E3</c> after the four-frame spin completes.</summary>
    public bool SpinFinished
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Requested list: bit zero selects facing, bit one selects the temporary spin family.
    /// </summary>
    public WaverAnimationSelector RequestedInstructionListIndex
    {
        get => (WaverAnimationSelector)_slot.VariableF;
        internal set => _slot.VariableF = value.ToNativeWord();
    }
}
