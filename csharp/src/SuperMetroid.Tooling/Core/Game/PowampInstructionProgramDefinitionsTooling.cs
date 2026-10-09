using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PowampInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PowampInstructionProgramDefinitions))]
internal abstract class PowampInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled timing and control words across the two body loops and two balloon transitions.</summary>
    public static int MechanicsWordCount => PowampInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets the address and compiled timing or control value of one Powamp mechanics word.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The address and expected value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => PowampInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of interleaved spritemap-selector operands in the body and balloon programs.</summary>
    public static int PresentationWordCount => PowampInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-local address of an interleaved spritemap-selector operand.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The address of the selected visual operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => PowampInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an address identifies either byte of a compiled Powamp mechanics word.</summary>
    /// <param name="address">24-bit ROM address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A8; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PowampInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PowampInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
