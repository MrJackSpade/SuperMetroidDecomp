using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ElevatorInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ElevatorInstructionProgramDefinitions))]
internal abstract class ElevatorInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 4;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => 2;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(ElevatorInstructionProgramDefinitions.Loop + (index < 2 ? 4 * index : 8 + 2 * (index - 2)));
        return new(address, ElevatorInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(ElevatorInstructionProgramDefinitions.Loop + 2 + 4 * index);
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
        int offset = unchecked((ushort)address) - ElevatorInstructionProgramDefinitions.Loop;
        return (uint)offset < 12 && (offset >= 8 || offset % 4 < 2);
    }
}
