using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresDoorInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresDoorInstructionProgramDefinitions))]
internal abstract class CeresDoorInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => CeresDoorInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresDoorInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => CeresDoorInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => CeresDoorInstructionProgramDefinitions.Bank;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => CeresDoorInstructionProgramDefinitions.PresentationWord(index).Address;
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresDoorInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresDoorInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
