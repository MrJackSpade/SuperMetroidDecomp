using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NoobTubeProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NoobTubeProjectileInstructionProgramDefinitions))]
internal abstract class NoobTubeProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the count of compiled timer, control-flow, and pre-instruction words across the crack, shards, and released bubbles.</summary>
    public static int MechanicsWordCount => NoobTubeProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a mechanics word by its ordinal in the flattened projectile-program sequence.</summary>
    /// <param name="index">Zero-based position in the combined mechanics-word sequence.</param>
    /// <returns>The bank-relative address and compiled value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => NoobTubeProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the count of extracted spritemap-selector operands across the projectile programs.</summary>
    public static int PresentationWordCount => NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-relative address of an extracted spritemap operand by its selector ordinal.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The address of the selected operand within its native instruction program.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index) => NoobTubeProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Determines whether an enemy-projectile-bank address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit SNES address to check.</param>
    /// <returns><see langword="true"/> when the address is a compiled mechanics byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NoobTubeProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NoobTubeProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
