using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YappingMawBodyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YappingMawBodyProjectileInstructionProgramDefinitions))]
internal abstract class YappingMawBodyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of duration and sleep-control words across the down- and up-facing body-link programs.</summary>
    public static int MechanicsWordCount => YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns one facing's pose-duration or terminal sleep word from the compiled program sequence.</summary>
    /// <param name="index">Zero-based ordinal among the two words for each of the two facings.</param>
    /// <returns>The instruction address and encoded duration or sleep opcode.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of sprite operands selecting artwork for the two body-link poses.</summary>
    public static int PresentationWordCount => YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps a facing's pose ordinal to the address of its sprite presentation operand.</summary>
    /// <param name="index">Zero-based pose index, ordered facing down then facing up.</param>
    public static ushort PresentationWordAddress(int index) => YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address belongs to a compiled body-link mechanics word in bank $86.</summary>
    /// <param name="address">24-bit address to test against the down- and up-facing programs.</param>
    /// <returns><see langword="true"/> when the address identifies either byte of a mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
