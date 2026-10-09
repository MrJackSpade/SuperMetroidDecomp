using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SpacePirateProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SpacePirateProjectileInstructionProgramDefinitions))]
internal abstract class SpacePirateProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of mechanics words across the four laser and claw instruction lists.</summary>
    public static int MechanicsWordCount => SpacePirateProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves one flattened laser-or-claw mechanics position to its native address and value.</summary>
    /// <param name="index">Zero-based position across left/right laser lists followed by left/right claw lists.</param>
    /// <returns>The bank-relative address and compiled timing or control value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the mechanics-word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => SpacePirateProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands across the left/right laser and claw lists.</summary>
    public static int PresentationWordCount => SpacePirateProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native instruction address containing one laser or claw spritemap operand.</summary>
    /// <param name="index">Zero-based position across the laser operands followed by the claw operands.</param>
    /// <returns>The address of the selected presentation pointer word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the presentation-word sequence.</exception>
    public static ushort PresentationWordAddress(int index) => SpacePirateProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute projectile-bank address is either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to laser or claw mechanics; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SpacePirateProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SpacePirateProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
