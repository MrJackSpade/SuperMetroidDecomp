using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RioInstructionProgramDefinitions))]
internal abstract class RioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of mechanics words across Rio's pose records and instruction-list control tails.</summary>
    public static int MechanicsWordCount => RioInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns the compiled address and duration or control operand at a mechanics-sequence index.</summary>
    /// <param name="index">Zero-based ordinal among the idle, transition, swoop, and cooldown mechanics words.</param>
    /// <returns>The bank-relative instruction address and encoded mechanics value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => RioInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of timed-pose spritemap operands across Rio's idle, transition, swoop, and cooldown lists.</summary>
    public static int PresentationWordCount => RioInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps a pose ordinal to the address of its spritemap operand.</summary>
    /// <param name="index">Zero-based ordinal across all pose records in the compiled instruction lists.</param>
    public static ushort PresentationWordAddress(int index) => RioInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address belongs to a compiled mechanics word in bank $A2.</summary>
    /// <param name="address">24-bit address to test against Rio's instruction programs.</param>
    /// <returns><see langword="true"/> when the address identifies either byte of a mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < RioInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = RioInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
