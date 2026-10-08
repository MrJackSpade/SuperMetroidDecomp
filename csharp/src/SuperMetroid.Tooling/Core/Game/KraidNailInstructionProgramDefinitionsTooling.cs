using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidNailInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidNailInstructionProgramDefinitions))]
internal abstract class KraidNailInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => KraidNailInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidNailInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => KraidNailInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Each frame's visual operand is two bytes after its duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= KraidNailInstructionProgramDefinitions.PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(KraidNailInstructionProgramDefinitions.Loop + 4 * index + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidNailInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidNailInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
