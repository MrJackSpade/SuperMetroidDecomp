using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidArmInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidArmInstructionProgramDefinitions))]
internal abstract class KraidArmInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled timing and control words across Kraid arm's four instruction-list variants.</summary>
    public static int MechanicsWordCount => KraidArmInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a compiled timing or control word and its address in the arm instruction lists.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The address and expected value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidArmInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of interleaved extended-spritemap selector words across the four instruction-list variants.</summary>
    public static int PresentationWordCount => KraidArmInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of an interleaved presentation-selector operand.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The bank-local address of the selected selector word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => KraidArmInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an address identifies either byte of a compiled Kraid arm mechanics word.</summary>
    /// <param name="address">24-bit ROM address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A7; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidArmInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidArmInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
