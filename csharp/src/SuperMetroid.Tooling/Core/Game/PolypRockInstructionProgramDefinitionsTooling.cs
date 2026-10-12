using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PolypRockInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PolypRockInstructionProgramDefinitions))]
internal abstract class PolypRockInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>The single-frame lava rock has a pose duration and sleep instruction in compiled control.</summary>
    public static int MechanicsWordCount => PolypRockInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes the one-frame hold or terminal sleep word at $86:BBD5-BBD9.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => PolypRockInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Identifies the interleaved spritemap operand at $86:BBD7 as presentation data.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PolypRockInstructionProgramDefinitions.PresentationWord;
    /// <summary>Recognizes the two-byte lava-rock duration and sleep words in bank $86, excluding its sole visual operand.</summary>
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
