using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="LowerNorfairRioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(LowerNorfairRioInstructionProgramDefinitions))]
internal abstract class LowerNorfairRioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 51;
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => 32;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
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
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
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
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort word = (ushort)(address & ~1);
        return LowerNorfairRioInstructionProgramDefinitions.ProgramAt(word) is not null && !LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord(word);
    }
}
