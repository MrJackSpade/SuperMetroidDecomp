using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="StokeProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(StokeProjectileInstructionProgramDefinitions))]
internal abstract class StokeProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of delay and control words compiled from the two-frame Stoke projectile loop.</summary>
    public static int MechanicsWordCount => StokeProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets one loop-control entry by its ordinal in the compiled sequence.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The bank-relative address and value of the selected delay, branch, or loop word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => StokeProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of frame-selector operands interleaved in the projectile loop.</summary>
    public static int PresentationWordCount => StokeProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-relative address of a frame-selector operand by its zero-based position.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The native address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index) => StokeProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Determines whether a projectile-bank address points to either byte of a compiled loop-control word.</summary>
    /// <param name="address">24-bit SNES address to check.</param>
    /// <returns><see langword="true"/> when the address is a compiled mechanics byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < StokeProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = StokeProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
