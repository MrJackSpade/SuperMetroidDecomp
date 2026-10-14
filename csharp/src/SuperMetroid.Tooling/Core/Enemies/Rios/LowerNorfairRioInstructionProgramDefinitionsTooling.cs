using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Development-tool members of <see cref="LowerNorfairRioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(LowerNorfairRioInstructionProgramDefinitions))]
internal abstract class LowerNorfairRioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Rio instruction words excluding drawing operands before the adjacent movement data.</summary>
    public static int MechanicsWordCount => 51;
    /// <summary>Rio drawing operands in the instruction region before movement definitions.</summary>
    public static int PresentationWordCount => 32;
    /// <summary>Walks the Rio instruction region and reads the indexed non-presentation word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int address = (ushort)LowerNorfairRioProgram.Idle; address < LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
        {
            if (!LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord((ushort)address) && index-- == 0)
                return new((ushort)address, LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
        }
        throw new IndexOutOfRangeException();
    }
    /// <summary>Walks the same region to locate the indexed drawing operand.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        for (int address = (ushort)LowerNorfairRioProgram.Idle; address < LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
        {
            if (LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord((ushort)address) && index-- == 0)
                return (ushort)address;
        }
        throw new IndexOutOfRangeException();
    }
    /// <summary>Checks that a bank-$A2 word belongs to a Rio program and is not presentation data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort word = (ushort)(address & ~1);
        return LowerNorfairRioInstructionProgramDefinitions.ProgramAt(word) is not null && !LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord(word);
    }
}
