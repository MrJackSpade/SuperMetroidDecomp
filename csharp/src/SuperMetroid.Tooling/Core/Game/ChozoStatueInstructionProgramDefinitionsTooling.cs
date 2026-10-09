using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ChozoStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChozoStatueInstructionProgramDefinitions))]
internal abstract class ChozoStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled Chozo statue mechanics words.</summary>
    public static int MechanicsWordCount => ChozoStatueInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a mechanics word from the compiled Chozo statue instruction program.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The native instruction address and compiled value at that position.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => ChozoStatueInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of spritemap operands extracted from the Chozo statue instruction program.</summary>
    public static int PresentationWordCount => ChozoStatueInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the native address of an extracted spritemap operand.</summary>
    /// <param name="index">Zero-based position in the presentation-operand sequence.</param>
    /// <returns>The operand's address in the program bank.</returns>
    public static ushort PresentationWordAddress(int index) => ChozoStatueInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Determines whether an address identifies either byte of a compiled mechanics word in bank AA.</summary>
    /// <param name="address">Full bus address to check.</param>
    /// <returns><see langword="true"/> for a mechanics-word byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ChozoStatueInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ChozoStatueInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
