using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PolypRockInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PolypRockInstructionProgramDefinitions))]
internal abstract class PolypRockInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => PolypRockInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => PolypRockInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PolypRockInstructionProgramDefinitions.PresentationWord;
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
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
