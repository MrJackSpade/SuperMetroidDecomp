using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresDoorInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresDoorInstructionProgramDefinitions))]
internal abstract class CeresDoorInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts Ceres door timing and control words compiled into the player host.</summary>
    public static int MechanicsWordCount => CeresDoorInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes an indexed Ceres door control word for extraction parity checks.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresDoorInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Ceres door frame selectors that map to installed presentation art.</summary>
    public static int PresentationWordCount => CeresDoorInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Identifies bank $A6 as the source of the Ceres door instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => CeresDoorInstructionProgramDefinitions.Bank;
    /// <summary>Locates the address of a Ceres door visual selector without reading its value as mechanics.</summary>
    public static ushort PresentationWordAddress(int index) => CeresDoorInstructionProgramDefinitions.PresentationWord(index).Address;
    /// <summary>Matches either byte of compiled Ceres door control in bank $A6 while excluding visual selectors.</summary>
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
