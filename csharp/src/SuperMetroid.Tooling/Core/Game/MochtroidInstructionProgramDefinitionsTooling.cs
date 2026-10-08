using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MochtroidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MochtroidInstructionProgramDefinitions))]
internal abstract class MochtroidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 12;
    public static int PresentationWordCount => 8;
    /// <summary>Each loop displays four timed records followed by Goto and its target; all records calculate on demand.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int program = index / 6;
        int record = index % 6;
        ushort address = (ushort)(MochtroidInstructionProgramDefinitions.FreeFlight + 20 * program + (record < 4 ? 4 * record : 16 + 2 * (record - 4)));
        return new(address, MochtroidInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(MochtroidInstructionProgramDefinitions.FreeFlight + 20 * (index / 4) + 4 * (index % 4) + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        int offset = unchecked((ushort)address) - MochtroidInstructionProgramDefinitions.FreeFlight;
        if (offset < 0 || offset >= 40)
            return false;
        int local = offset % 20;
        return local >= 16 || (local & 3) < 2;
    }
}
