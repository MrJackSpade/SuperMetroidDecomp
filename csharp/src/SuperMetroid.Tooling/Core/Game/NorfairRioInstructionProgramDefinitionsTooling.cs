using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairRioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairRioInstructionProgramDefinitions))]
internal abstract class NorfairRioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled timing, callback, transition, and loop-control words for Rio and its flames.</summary>
    public static int MechanicsWordCount => NorfairRioInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves a flattened parent-or-flame mechanics position to its address and compiled value.</summary>
    /// <param name="index">Zero-based position across the idle, swoop-transition, flight, and flame programs.</param>
    /// <returns>The bank-$A2 instruction address and its timing, callback, or control value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the compiled mechanics sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairRioInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of interleaved spritemap operands that select extracted presentation frames.</summary>
    public static int PresentationWordCount => NorfairRioInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the address of one spritemap operand in the compiled program layout.</summary>
    /// <param name="index">Zero-based position among the parent and flame presentation operands.</param>
    /// <returns>The bank-$A2 address containing the selected spritemap pointer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index) => NorfairRioInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute bank-$A2 address is either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to the compiled mechanics layout; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NorfairRioInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NorfairRioInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
