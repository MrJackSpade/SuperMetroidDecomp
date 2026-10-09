namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for both directional Fune/Namihe fireball programs. Their
/// interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class FuneNamiheFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_NamiFuneFireball_Left</c> at $86:DE96.</summary>
    internal const ushort Left = 0xde96;

    /// <summary><c>InstList_EnemyProjectile_NamiFuneFireball_Right</c> at $86:DEA6.</summary>
    internal const ushort Right = 0xdea6;

    /// <summary>Number of editable spritemap operands across the left- and right-facing instruction lists.</summary>
    public static int PresentationWordCount => 6;

    /// <summary>Maps an ordinal to the address of one editable spritemap operand in the two fireball lists.</summary>
    /// <param name="index">Zero-based operand ordinal across the left list followed by the right list.</param>
    /// <returns>The bank-$86 address of the selected spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The ordinal is not within <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Left + 16 * (index / 3) + 4 * (index % 3) + 2);
    }

    /// <summary>Determines whether an address names one of the six editable spritemap operands.</summary>
    /// <param name="address">Bank-$86 address to classify.</param>
    /// <returns><see langword="true"/> only for a presentation operand in either directional list.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Left + 2);
        return (uint)offset < 32 && offset % 16 < 12 && offset % 4 == 0;
    }
    /// <summary>Returns the compiled wait duration, goto opcode, or loop target for a non-presentation word address.</summary>
    /// <param name="address">Bank-$86 address of an instruction word in either fireball list.</param>
    /// <returns>The native mechanics word stored at that position in the instruction program.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Left;
        if ((uint)offset < 32)
        {
            int stage = offset % 16;
            if (stage < 12 && stage % 4 == 0)
                return 5;
            if (stage == 12)
                return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
            if (stage == 14)
                return (ushort)(address - 14);
        }
        throw new InvalidDataException(
            $"Fune/Namihe fireball instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
    }
}
