using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresBabyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresBabyInstructionProgramDefinitions))]
internal abstract class CeresBabyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts callback, branch, and frame-duration words in the mixed Ceres Baby program.</summary>
    public static int MechanicsWordCount => CeresBabyInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes native address/value pairs for Baby control-flow verification.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresBabyInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Places the Ceres Baby's mixed program in native bank $A6.</summary>
    static int IDeclaredProgramBank.Bank => CeresBabyInstructionProgramDefinitions.Bank;
    /// <summary>Recognizes the two bytes of each compiled bank-$A6 control word, excluding draw selectors.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresBabyInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresBabyInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
