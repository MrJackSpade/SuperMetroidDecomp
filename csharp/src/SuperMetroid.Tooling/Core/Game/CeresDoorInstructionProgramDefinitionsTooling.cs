using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresDoorInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresDoorInstructionProgramDefinitions))]
internal abstract class CeresDoorInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int MechanicsWordCount => CeresDoorInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresDoorInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => CeresDoorInstructionProgramDefinitions.PresentationWordCount;
    static int IDeclaredProgramBank.Bank => CeresDoorInstructionProgramDefinitions.Bank;
    public static ushort PresentationWordAddress(int index) => CeresDoorInstructionProgramDefinitions.PresentationWord(index).Address;
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
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
