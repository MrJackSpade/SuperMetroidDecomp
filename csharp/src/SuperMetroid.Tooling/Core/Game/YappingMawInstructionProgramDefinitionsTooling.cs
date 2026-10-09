using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YappingMawInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YappingMawInstructionProgramDefinitions))]
internal abstract class YappingMawInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics words across eight attacks and the directional cooldown programs.</summary>
    public static int MechanicsWordCount => YappingMawInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves one flattened attack-or-cooldown mechanics position to its native address and value.</summary>
    /// <param name="index">Zero-based position across the eight attack lists followed by cooldown sequences.</param>
    /// <returns>The bank-$A8 instruction address and compiled timing, callback, or control value.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the mechanics-word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => YappingMawInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of compiled spritemap selectors for attack and cooldown poses.</summary>
    public static int PresentationWordCount => YappingMawInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address containing one attack or cooldown spritemap selector.</summary>
    /// <param name="index">Zero-based position among the compiled presentation operands.</param>
    /// <returns>The bank-$A8 address of the selected visual operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation-word sequence.</exception>
    public static ushort PresentationWordAddress(int index) => YappingMawInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute bank-$A8 address is either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to attack or cooldown mechanics; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < YappingMawInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort word = YappingMawInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (offset == word || offset == word + 1) return true;
        }
        return false;
    }
}
