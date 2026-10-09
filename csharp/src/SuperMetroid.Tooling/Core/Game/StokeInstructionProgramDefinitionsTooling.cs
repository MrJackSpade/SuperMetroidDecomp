using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="StokeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(StokeInstructionProgramDefinitions))]
internal abstract class StokeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics entries across Stoke's left/right movement and attack programs.</summary>
    public static int MechanicsWordCount => StokeInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves an indexed movement or attack entry to its instruction address and encoded value.</summary>
    /// <param name="index">Zero-based index across Stoke's compiled mechanics entries.</param>
    /// <returns>The native instruction address and value for the selected entry.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => StokeInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands embedded in Stoke's movement and attack programs.</summary>
    public static int PresentationWordCount => 12;

    /// <summary>Calculates the bank-relative address of an embedded movement or attack spritemap operand.</summary>
    /// <param name="index">Zero-based index across the presentation operands for both facing directions.</param>
    /// <returns>The native instruction address containing the operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation operand range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int local = index % 6;
        ushort move = index < 6 ? StokeInstructionProgramDefinitions.MovingLeft : StokeInstructionProgramDefinitions.MovingRight;
        ushort attack = index < 6 ? StokeInstructionProgramDefinitions.AttackingLeft : StokeInstructionProgramDefinitions.AttackingRight;
        return (ushort)(local < 4 ? move + 4 + 4 * local : attack + 2 + 8 * (local - 4));
    }

    /// <summary>Tests whether a full SNES address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full address in the SNES address space.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word in bank $A2; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < StokeInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = StokeInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
