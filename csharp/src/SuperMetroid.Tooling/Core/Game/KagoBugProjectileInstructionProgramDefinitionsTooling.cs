using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KagoBugProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KagoBugProjectileInstructionProgramDefinitions))]
internal abstract class KagoBugProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of mechanics words compiled for Kago bug landing, falling, jumping, and shot behavior.</summary>
    public static int MechanicsWordCount => KagoBugProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves one position in the compiled mechanics sequence to its native address and value.</summary>
    /// <param name="index">Zero-based position across the landed, falling, jump, and shot programs.</param>
    /// <returns>The instruction address and compiled delay, control, or callback operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => KagoBugProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands embedded in the compiled Kago bug projectile programs.</summary>
    public static int PresentationWordCount => KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of one spritemap operand in the compiled program order.</summary>
    /// <param name="index">Zero-based position in the presentation-operand sequence.</param>
    /// <returns>The bank-$86 address containing the selected spritemap pointer.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation sequence.</exception>
    public static ushort PresentationWordAddress(int index) => KagoBugProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute bank-$86 address is either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to mechanics data; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KagoBugProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KagoBugProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
