using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EscapeEtecoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EscapeEtecoonInstructionProgramDefinitions))]
internal abstract class EscapeEtecoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of timing and control operands compiled for the escape-sequence Etecoons.</summary>
    public static int MechanicsWordCount => EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves a mechanics-word ordinal to its native address and compiled operand.</summary>
    /// <param name="index">Zero-based ordinal across the compiled low-tide, high-tide, escape, stationary, and gratitude programs.</param>
    /// <returns>The bank-local address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The ordinal is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands resolved through installed presentation artwork.</summary>
    public static int PresentationWordCount => EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of an editable spritemap operand by ordinal.</summary>
    /// <param name="index">Zero-based ordinal among the presentation words.</param>
    /// <returns>The bank-local address of the selected operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The ordinal is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => EscapeEtecoonInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a full SNES address refers to either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full 24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> for a byte in a compiled bank-$B3 mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1)
                return true;
        }
        return false;
    }
}
