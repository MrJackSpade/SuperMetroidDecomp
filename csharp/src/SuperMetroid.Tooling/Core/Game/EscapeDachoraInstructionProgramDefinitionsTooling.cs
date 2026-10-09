using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EscapeDachoraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EscapeDachoraInstructionProgramDefinitions))]
internal abstract class EscapeDachoraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics operands across the low-tide, high-tide, and departure programs.</summary>
    public static int MechanicsWordCount => EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets the expected value and address for one compiled escape-program mechanics operand.</summary>
    /// <param name="index">Zero-based index in the compiled mechanics-word sequence.</param>
    /// <returns>The address and value of the selected mechanics operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap-selector operands compiled for the direction runs and departure phases.</summary>
    public static int PresentationWordCount => EscapeDachoraInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-$B3 address of a compiled spritemap-selector operand.</summary>
    /// <param name="index">Zero-based index in the presentation-word sequence.</param>
    /// <returns>The address containing the selected visual operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address belongs to a compiled mechanics word for the escape Dachora.</summary>
    /// <param name="address">24-bit ROM address to classify; either byte of a mechanics word is accepted.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $B3; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
