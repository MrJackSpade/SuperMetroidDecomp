namespace SuperMetroid.Core.Progression;

/// <summary>
/// Two decimal digits of the escape timer packed in one byte (#1420): a tens digit in the high nibble and a ones digit
/// in the low nibble, so the byte reads as its decimal value in hexadecimal (59 is stored as $59). The timer counts
/// down digit by digit.
/// </summary>
/// <remarks>
/// This type is the single owner of the packed-decimal form and of the digit-by-digit subtraction over it, including
/// its borrow that wraps through 99 and its result for nondecimal nibbles, which the timer shows and tests; the byte it
/// stores is that form, and callers use <see cref="FromDecimal"/>, <see cref="Value"/> and <see cref="Subtract"/>.
/// </remarks>
/// <param name="Raw">The packed byte.</param>
[EncodingOwner("packed decimal timer digits")]
public readonly record struct PackedDecimal(byte Raw)
{
    /// <summary>The number of values one nibble holds: a digit's place value in the packed byte.</summary>
    private const int NibbleBase = 16;

    /// <summary>Zero.</summary>
    public static PackedDecimal Zero => default;

    /// <summary>The tens digit, the high nibble.</summary>
    public int Tens => Raw / NibbleBase;

    /// <summary>The ones digit, the low nibble.</summary>
    public int Ones => Raw % NibbleBase;

    /// <summary>Whether both nibbles are decimal digits.</summary>
    public bool IsValid => Tens <= 9 && Ones <= 9;

    /// <summary>The decimal value of a valid packed byte.</summary>
    public int Value => IsValid ? Tens * 10 + Ones
        : throw new InvalidOperationException($"Packed-BCD byte ${Raw:X2} holds a nondecimal digit.");

    /// <summary>Packs the decimal <paramref name="value"/>, 0 through 99.</summary>
    public static PackedDecimal FromDecimal(int value) => (uint)value <= 99
        ? new((byte)(value / 10 * NibbleBase + value % 10))
        : throw new ArgumentOutOfRangeException(nameof(value), value, "A packed-BCD byte holds 0 through 99.");

    /// <summary>
    /// This value minus <paramref name="value"/>, digit by digit with a decimal correction, reporting whether it
    /// borrowed. The result keeps eight bits, so a borrow below zero wraps through 99.
    /// </summary>
    public PackedDecimal Subtract(PackedDecimal value, out bool borrowed)
    {
        // Low digit with the implicit input carry, then the processor's decimal correction.
        int result = Ones + (NibbleBase - 1 - value.Ones) + 1;
        if (result < NibbleBase)
        {
            int correctedLow = result - 6;
            result = correctedLow & (correctedLow < 0 ? NibbleBase - 1 : 2 * NibbleBase - 1);
        }

        // High digit; a decimal borrow subtracts six from the upper digit. Values may go negative here, and the final
        // byte keeps the processor's low eight result bits.
        result = Tens * NibbleBase + (NibbleBase - 1 - value.Tens) * NibbleBase + result;
        if (result < NibbleBase * NibbleBase)
            result -= 6 * NibbleBase;

        borrowed = result < NibbleBase * NibbleBase;
        return new(unchecked((byte)result));
    }
}
