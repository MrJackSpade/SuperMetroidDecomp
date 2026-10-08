using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="LowerNorfairRioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(LowerNorfairRioInstructionProgramDefinitions))]
internal abstract class LowerNorfairRioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 51;
    public static int PresentationWordCount => 32;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int address = LowerNorfairRioInstructionProgramDefinitions.Idle; address < LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
        {
            if (!LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord((ushort)address) && index-- == 0)
                return new((ushort)address, LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
        }
        throw new IndexOutOfRangeException();
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        for (int address = LowerNorfairRioInstructionProgramDefinitions.Idle; address < LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
        {
            if (LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord((ushort)address) && index-- == 0)
                return (ushort)address;
        }
        throw new IndexOutOfRangeException();
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort word = (ushort)(address & ~1);
        return LowerNorfairRioInstructionProgramDefinitions.ProgramAt(word) != 0 && !LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord(word);
    }
}
