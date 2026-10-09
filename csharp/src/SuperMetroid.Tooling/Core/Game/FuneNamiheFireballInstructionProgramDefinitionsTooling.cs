using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FuneNamiheFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FuneNamiheFireballInstructionProgramDefinitions))]
internal abstract class FuneNamiheFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of spritemap operand words embedded across the two directional fireball lists.</summary>
    public static int PresentationWordCount => FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-$86 address of a presentation operand in left-then-right list order.</summary>
    /// <param name="index">Zero-based index among the six visual operands.</param>
    /// <returns>The native address whose value is supplied by presentation data.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the six presentation slots.</exception>
    public static ushort PresentationWordAddress(int index) => FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress(index);

    // Each facing has three five-tick frames followed by a jump to its start.
    /// <summary>Number of delay and loop-control words in the two directional programs.</summary>
    public static int MechanicsWordCount => 10;

    /// <summary>Returns an instruction word from the mechanics sequence for the left and right lists.</summary>
    /// <param name="index">Zero-based index excluding the spritemap operands.</param>
    /// <returns>The bank-$86 address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the ten mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int word = index % 5;
        ushort address = (ushort)(FuneNamiheFireballInstructionProgramDefinitions.Left + 16 * (index / 5) +
            (word < 3 ? 4 * word : 12 + 2 * (word - 3)));
        return new(address, FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether a full cartridge address refers to a byte in a compiled fireball mechanics word.</summary>
    /// <param name="address">24-bit address to classify.</param>
    /// <returns><see langword="true"/> for mechanics bytes in bank $86; presentation operands and other addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = unchecked((ushort)address) - FuneNamiheFireballInstructionProgramDefinitions.Left;
        return (uint)offset < 32 && (offset % 16 >= 12 || offset % 4 < 2);
    }
}
