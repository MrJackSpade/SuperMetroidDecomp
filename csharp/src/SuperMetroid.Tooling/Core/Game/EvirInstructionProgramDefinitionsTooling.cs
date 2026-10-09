using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EvirInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EvirInstructionProgramDefinitions))]
internal abstract class EvirInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words across Evir's body, arms, and projectile programs.</summary>
    public static int MechanicsWordCount => EvirInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns one fixed timing, callback, timer, or loop-control word.</summary>
    /// <param name="index">Zero-based index across both facing loops and the projectile programs.</param>
    /// <returns>The address and value of the indexed mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => EvirInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of live spritemap operands interleaved with the control words.</summary>
    public static int PresentationWordCount => EvirInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$A8 address of an interleaved live spritemap operand.</summary>
    /// <param name="index">Zero-based operand index across the body, arms, and projectile programs.</param>
    /// <returns>Address of the indexed presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation operands.</exception>
    public static ushort PresentationWordAddress(int index) => EvirInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a bank-$A8 address names either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics-word byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EvirInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EvirInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
