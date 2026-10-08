using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresBabyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresBabyInstructionProgramDefinitions))]
internal abstract class CeresBabyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int MechanicsWordCount => CeresBabyInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresBabyInstructionProgramDefinitions.MechanicsWord(index);
    static int IDeclaredProgramBank.Bank => CeresBabyInstructionProgramDefinitions.Bank;
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
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
