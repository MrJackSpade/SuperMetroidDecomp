using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairLavaJumperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairLavaJumperInstructionProgramDefinitions))]
internal abstract class NorfairLavaJumperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled timing and control words in the hidden, jump, and follower programs.</summary>
    public static int MechanicsWordCount => NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a compiled timing or control word and its address in the parent or follower programs.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The address and expected value of the selected mechanics word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of interleaved spritemap operands exposed for the hidden, jump, and follower visuals.</summary>
    public static int PresentationWordCount => NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of an interleaved spritemap operand in a parent or follower instruction list.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The bank-local address of the selected visual operand.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an address identifies either byte of a compiled Norfair lava-jumper mechanics word.</summary>
    /// <param name="address">24-bit ROM address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A2; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
