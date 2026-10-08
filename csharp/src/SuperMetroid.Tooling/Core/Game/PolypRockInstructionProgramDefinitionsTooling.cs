using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PolypRockInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PolypRockInstructionProgramDefinitions))]
internal abstract class PolypRockInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => PolypRockInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => PolypRockInstructionProgramDefinitions.MechanicsWord(index);
    static ushort ISinglePresentationOperand.PresentationWord => PolypRockInstructionProgramDefinitions.PresentationWord;
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PolypRockInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PolypRockInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
