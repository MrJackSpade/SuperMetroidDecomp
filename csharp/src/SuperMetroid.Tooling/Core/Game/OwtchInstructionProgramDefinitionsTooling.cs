using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="OwtchInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(OwtchInstructionProgramDefinitions))]
internal abstract class OwtchInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 12;
    public static int PresentationWordCount => 6;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 6;
        int offset = word == 0 ? 0 : word <= 4 ? 2 + (word - 1) * 4 : 16;
        ushort address = (ushort)(OwtchInstructionProgramDefinitions.MovingLeft + index / 6 * OwtchInstructionProgramDefinitions.ProgramBytes + offset);
        return new(address, OwtchInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(OwtchInstructionProgramDefinitions.MovingLeft + index / 3 * OwtchInstructionProgramDefinitions.ProgramBytes + 4 + index % 3 * 4);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = (ushort)address - OwtchInstructionProgramDefinitions.MovingLeft;
        return (uint)offset < 2 * OwtchInstructionProgramDefinitions.ProgramBytes &&
            ((offset % OwtchInstructionProgramDefinitions.ProgramBytes & ~1) is 0 or 2 or 6 or 10 or 14 or 16);
    }
}
