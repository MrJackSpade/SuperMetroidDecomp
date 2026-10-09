using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PolypInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PolypInstructionProgramDefinitions))]
internal abstract class PolypInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the two mechanics words in Polyp's one-tick stationary program.</summary>
    public static int MechanicsWordCount => PolypInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns Polyp's duration word or the following shared sleep command.</summary>
    /// <param name="index">Zero-based mechanics-word position: zero is the duration and one is the sleep command.</param>
    /// <returns>The native address and value of the selected word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the two-word program.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => PolypInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets Polyp's sole operand selecting its installed stationary spritemap.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PolypInstructionProgramDefinitions.PresentationWord;

    /// <summary>Tests whether a full SNES address is one of Polyp's compiled duration or sleep-command bytes.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> for bytes at $A2:B51A–B51B or $A2:B51E–B51F; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is 0xb51a or 0xb51b or 0xb51e or 0xb51f;
    }
}
