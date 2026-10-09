using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MagdolliteLavaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MagdolliteLavaInstructionProgramDefinitions))]
internal abstract class MagdolliteLavaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of mechanics words for the left/right poses and shot-triggered drop sequence.</summary>
    public static int MechanicsWordCount => MagdolliteLavaInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns one compiled mechanics word in the left, right, then shot-program ordering.</summary>
    /// <param name="index">Zero-based position among the facing-specific and shot mechanics words.</param>
    /// <returns>The native address and value of the selected delay or control instruction.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => MagdolliteLavaInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands used by the left and right lava poses.</summary>
    public static int PresentationWordCount => MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the address of one facing-specific spritemap pointer.</summary>
    /// <param name="index">Zero-based position selecting the left or right pose.</param>
    /// <returns>The bank-$86 address containing that pose's presentation operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the two presentation operands.</exception>
    public static ushort PresentationWordAddress(int index) => MagdolliteLavaInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute address identifies either byte of a compiled mechanics word in the projectile bank.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to the compiled mechanics sequence; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MagdolliteLavaInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = MagdolliteLavaInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
