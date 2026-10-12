using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresBabyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresBabyInstructionProgramDefinitions))]
internal abstract class CeresBabyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => CeresBabyInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresBabyInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets the cartridge bank containing this instruction program.</summary>
    static int IDeclaredProgramBank.Bank => CeresBabyInstructionProgramDefinitions.Bank;
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
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
