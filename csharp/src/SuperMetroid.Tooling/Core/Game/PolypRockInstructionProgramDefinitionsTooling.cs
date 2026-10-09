using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PolypRockInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PolypRockInstructionProgramDefinitions))]
internal abstract class PolypRockInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled instruction words for the static rock pose and sleep command.</summary>
    public static int MechanicsWordCount => PolypRockInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns one compiled mechanics word in native address order.</summary>
    /// <param name="index">Zero-based ordinal: zero is the initial timed pose and one is the sleep command.</param>
    /// <returns>The native address and value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The ordinal is outside the two-word program.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => PolypRockInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Exposes the single live spritemap operand to catalog tooling.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PolypRockInstructionProgramDefinitions.PresentationWord;

    /// <summary>Tests whether a full address identifies either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full banked address to classify.</param>
    /// <returns><see langword="true"/> when the address is in the projectile-program bank and belongs to a mechanics word.</returns>
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
