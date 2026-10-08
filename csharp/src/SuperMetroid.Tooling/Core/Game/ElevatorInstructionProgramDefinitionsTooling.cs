using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ElevatorInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ElevatorInstructionProgramDefinitions))]
internal abstract class ElevatorInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 4;
    public static int PresentationWordCount => 2;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(ElevatorInstructionProgramDefinitions.Loop + (index < 2 ? 4 * index : 8 + 2 * (index - 2)));
        return new(address, ElevatorInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(ElevatorInstructionProgramDefinitions.Loop + 2 + 4 * index);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int offset = unchecked((ushort)address) - ElevatorInstructionProgramDefinitions.Loop;
        return (uint)offset < 12 && (offset >= 8 || offset % 4 < 2);
    }
}
