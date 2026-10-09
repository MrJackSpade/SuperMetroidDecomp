using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadTourianCorpseInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadTourianCorpseInstructionProgramDefinitions))]
internal abstract class DeadTourianCorpseInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of duration and sleep words compiled for the corpse instruction lists.</summary>
    public static int MechanicsWordCount => DeadTourianCorpseInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns one indexed duration or terminal sleep word from the compiled corpse programs.</summary>
    /// <param name="index">Zero-based position in the combined mechanics-word sequence.</param>
    /// <returns>The bank-local address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => DeadTourianCorpseInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Returns the spritemap operand address in the selected six-byte corpse instruction list.</summary>
    /// <param name="index">Zero-based corpse instruction-list index.</param>
    /// <returns>Bank-local address of that list's presentation-owned spritemap word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index does not identify a compiled corpse list.</exception>
    internal static ushort PresentationWordAddress(int index) => (ushort)(DeadTourianCorpseInstructionProgramDefinitions.Program(index)+2);

    /// <summary>Tests whether a full bus address identifies either byte of a compiled corpse mechanics word.</summary>
    /// <param name="address">Full 24-bit bus address to classify.</param>
    /// <returns><see langword="true"/> for a duration or sleep-command byte in bank $A9.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < DeadTourianCorpseInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = DeadTourianCorpseInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
