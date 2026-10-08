using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BoulderInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BoulderInstructionProgramDefinitions))]
internal abstract class BoulderInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 20;
    public static int PresentationWordCount => 16;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 10;
        ushort address = (ushort)(BoulderInstructionProgramDefinitions.Left + 36 * (index / 10) + (word < 8 ? 4 * word : 32 + 2 * (word - 8)));
        return new(address, BoulderInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(BoulderInstructionProgramDefinitions.Left + 36 * (index / 8) + 4 * (index % 8) + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000) return false;
        int offset = (ushort)address - BoulderInstructionProgramDefinitions.Left;
        if ((uint)offset >= 72) return false;
        int stage = offset % 36;
        return stage >= 32 || stage % 4 < 2;
    }
}
