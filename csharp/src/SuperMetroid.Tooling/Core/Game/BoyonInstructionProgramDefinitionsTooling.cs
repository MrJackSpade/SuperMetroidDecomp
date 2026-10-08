using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BoyonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BoyonInstructionProgramDefinitions))]
internal abstract class BoyonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 18;
    public static int PresentationWordCount => 10;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool bouncing = index >= 8;
        int word = bouncing ? index - 8 : index;
        int frames = bouncing ? 6 : 4;
        int offset = word < 2 ? 2 * word : word < frames + 2
            ? 4 + 4 * (word - 2) : 4 + 4 * frames + 2 * (word - frames - 2);
        ushort address = (ushort)((bouncing ? BoyonInstructionProgramDefinitions.Bouncing : BoyonInstructionProgramDefinitions.Idle) + offset);
        return new(address, BoyonInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 4 ? BoyonInstructionProgramDefinitions.Idle + 6 + 4 * index : BoyonInstructionProgramDefinitions.Bouncing + 6 + 4 * (index - 4));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = (ushort)address - BoyonInstructionProgramDefinitions.Idle;
        if ((uint)offset >= 56) return false;
        bool bouncing = offset >= 24;
        int local = bouncing ? offset - 24 : offset;
        int tail = bouncing ? 28 : 20;
        return local < 4 || local >= tail || local % 4 < 2;
    }
}
