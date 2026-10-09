namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Dragon's left/right rising and falling fireball loops.
/// Interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class DragonFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Rising_Left</c> at $86:B4BF.</summary>
    internal const ushort RisingLeft = 0xb4bf;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Rising_Right</c> at $86:B4CB.</summary>
    internal const ushort RisingRight = 0xb4cb;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Falling_Left</c> at $86:B4D7.</summary>
    internal const ushort FallingLeft = 0xb4d7;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Falling_Right</c> at $86:B4E3.</summary>
    internal const ushort FallingRight = 0xb4e3;

    /// <summary>Number of spritemap operands embedded in the four compiled fireball loops.</summary>
    public static int PresentationWordCount => 8;

    /// <summary>Gets the bank-$86 address of an operand that names an extracted fireball spritemap.</summary>
    /// <param name="index">Zero-based index into the fireball presentation operands.</param>
    /// <returns>The address of the selected spritemap operand in the compiled instruction lists.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation operand range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(RisingLeft + index / 2 * 12 + index % 2 * 4 + 2);
    }

    /// <summary>Reads a compiled control-flow or timing word from a fireball instruction list.</summary>
    /// <param name="address">Bank-$86 address of the mechanics word to resolve.</param>
    /// <returns>The compiled word, including loop targets and instruction durations.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value)) return value;
        throw new InvalidDataException(
            $"Dragon-fireball instruction mechanics pointer $86:{address:X4} is not compiled.");
    }

    /// <summary>Attempts to resolve a bank-$86 word used for fireball timing or instruction flow.</summary>
    /// <param name="address">Address to look up within the four rising and falling instruction loops.</param>
    /// <param name="value">Receives the resolved word when found; otherwise receives zero.</param>
    /// <returns><see langword="true"/> when the address contains a compiled mechanics word.</returns>
    internal static bool TryRead(int address, out ushort value)
    {
        int offset = address - RisingLeft;
        value = 0;
        if (offset < 0 || offset >= 48) return false;
        int step = offset % 12;
        value = step switch
        {
            0 or 4 => 5,
            8 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
            10 => (ushort)(RisingLeft + offset / 12 * 12),
            _ => 0,
        };
        return value != 0;
    }
}
