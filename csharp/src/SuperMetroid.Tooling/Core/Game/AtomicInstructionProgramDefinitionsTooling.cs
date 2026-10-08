using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="AtomicInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(AtomicInstructionProgramDefinitions))]
internal abstract class AtomicInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    // Four loops: six duration/visual pairs followed by goto and its target.
    public static int MechanicsWordCount => 4 * 8;
    public static int PresentationWordCount => 4 * 6;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 8;
        ushort address = (ushort)(AtomicInstructionProgramDefinitions.UpRight + 28 * (index / 8) +
            (word < 6 ? 4 * word : 24 + 2 * (word - 6)));
        return new(address, AtomicInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(AtomicInstructionProgramDefinitions.UpRight + 28 * (index / 6) + 4 * (index % 6) + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        int offset = (ushort)address - AtomicInstructionProgramDefinitions.UpRight;
        if ((uint)offset >= 4 * 28) return false;
        int stage = offset % 28;
        return stage >= 24 || stage % 4 < 2;
    }
}
