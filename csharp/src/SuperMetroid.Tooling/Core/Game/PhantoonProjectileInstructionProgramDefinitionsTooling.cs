using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PhantoonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PhantoonProjectileInstructionProgramDefinitions))]
internal abstract class PhantoonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled flame timing, repetition, control-flow, and deletion words.</summary>
    public static int MechanicsWordCount => PhantoonProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves an ordinal in the compiled flame-program mechanics table to its address and value.</summary>
    /// <param name="index">Zero-based position across the starting and destroyable flame instruction lists.</param>
    /// <returns>The native instruction address and its timer, repetition, or control operand.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the compiled mechanics table.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => PhantoonProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands that select separately installed Phantoon flame artwork.</summary>
    public static int PresentationWordCount => PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of one spritemap operand in the flame-program ordering.</summary>
    /// <param name="index">Zero-based position among the compiled presentation operands.</param>
    /// <returns>The bank-$86 address containing the selected artwork pointer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the compiled presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index) => PhantoonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute projectile-bank address is either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to compiled timing or control data; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PhantoonProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PhantoonProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
