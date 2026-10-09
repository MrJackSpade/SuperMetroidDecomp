using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoExplosionInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoExplosionInstructionProgramDefinitions))]
internal abstract class TorizoExplosionInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words across the low-health, death, and smoke programs.</summary>
    public static int MechanicsWordCount => TorizoExplosionInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a compiled Torizo explosion opcode, operand, or pose duration.</summary>
    /// <param name="index">Zero-based position in the concatenated mechanics-word layout.</param>
    /// <returns>The bank-$86 address and value of the selected word.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the 53 mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => TorizoExplosionInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of spritemap operand addresses supplied by extracted explosion artwork.</summary>
    public static int PresentationWordCount => TorizoExplosionInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$86 address reserved for one extracted explosion spritemap operand.</summary>
    /// <param name="index">Zero-based position among the presentation operands.</param>
    /// <returns>Address populated by the installed explosion artwork.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the presentation operand range.</exception>
    public static ushort PresentationWordAddress(int index) => TorizoExplosionInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a bank-$86 byte belongs to a compiled mechanics word rather than presentation data.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TorizoExplosionInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TorizoExplosionInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
