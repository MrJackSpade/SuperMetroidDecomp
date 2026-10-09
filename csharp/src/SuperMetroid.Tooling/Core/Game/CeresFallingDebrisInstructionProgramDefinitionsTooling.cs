using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresFallingDebrisInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresFallingDebrisInstructionProgramDefinitions))]
internal abstract class CeresFallingDebrisInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of delay and terminal-control words in the compiled light and dark debris programs.</summary>
    public static int MechanicsWordCount => CeresFallingDebrisInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns the address and value for one of the four compiled mechanics words.</summary>
    /// <param name="index">Zero-based position across the light and dark one-tick-then-sleep programs.</param>
    /// <returns>The native instruction address and its compiled delay or sleep value.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the four mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresFallingDebrisInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap pointer operands, one for each falling-debris pose.</summary>
    public static int PresentationWordCount => CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the address of a light or dark pose's spritemap pointer.</summary>
    /// <param name="index">Zero-based position in the two-entry presentation sequence.</param>
    /// <returns>The native instruction address containing the selected pose's artwork pointer.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the two presentation operands.</exception>
    public static ushort PresentationWordAddress(int index) => CeresFallingDebrisInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Determines whether a bank-$86 byte belongs to one of the compiled mechanics words.</summary>
    /// <param name="address">Absolute SNES address to classify.</param>
    /// <returns><see langword="true"/> when the byte is either half of a compiled delay or sleep word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresFallingDebrisInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresFallingDebrisInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
