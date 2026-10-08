using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SciserInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SciserInstructionProgramDefinitions))]
internal abstract class SciserInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => SciserInstructionProgramDefinitions.SurfaceCount * 8;
    public static int PresentationWordCount => SciserInstructionProgramDefinitions.SurfaceCount * 4;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 8;
        int offset = word < 2 ? word * 2 : word < 6 ? 4 + (word - 2) * 4 : 20 + (word - 6) * 2;
        ushort address = (ushort)(SciserInstructionProgramDefinitions.UpsideRight + index / 8 * SciserInstructionProgramDefinitions.ProgramBytes + offset);
        return new(address, SciserInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(SciserInstructionProgramDefinitions.UpsideRight + index / 4 * SciserInstructionProgramDefinitions.ProgramBytes + 6 + index % 4 * 4);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int offset = (ushort)address - SciserInstructionProgramDefinitions.UpsideRight;
        return (uint)offset < SciserInstructionProgramDefinitions.SurfaceCount * SciserInstructionProgramDefinitions.ProgramBytes &&
            ((offset % SciserInstructionProgramDefinitions.ProgramBytes & ~1) is 0 or 2 or 4 or 8 or 12 or 16 or 20 or 22);
    }
}
