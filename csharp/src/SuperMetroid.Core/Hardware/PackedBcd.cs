namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One packed binary-coded-decimal byte: a tens digit in the high nibble and a ones digit in the low nibble, so the
/// byte reads as its decimal value in hexadecimal (59 is stored as $59). The 65C816's decimal mode does arithmetic on
/// it digit by digit.
/// </summary>
/// <remarks>
/// This type is the single owner of the packed-BCD encoding and of the processor's decimal-mode subtraction over it,
/// including its borrow that wraps through 99 and its result for nondecimal nibbles; the byte it stores is that
/// encoding, and callers use <see cref="FromDecimal"/>, <see cref="Value"/> and <see cref="Subtract"/>.
/// </remarks>
/// <param name="Raw">The packed byte.</param>
[EncodingOwner("packed decimal timer digits")]
public readonly record struct PackedBcd(byte Raw)
{
    /// <summary>The number of values one nibble holds: a digit's place value in the packed byte.</summary>
    private const int NibbleBase = 16;

    /// <summary>Zero.</summary>
    public static PackedBcd Zero => default;

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
    public static PackedBcd FromDecimal(int value) => (uint)value <= 99
        ? new((byte)(value / 10 * NibbleBase + value % 10))
        : throw new ArgumentOutOfRangeException(nameof(value), value, "A packed-BCD byte holds 0 through 99.");

    /// <summary>
    /// The 65C816's 8-bit decimal-mode SBC with the carry set: this byte minus <paramref name="value"/>, reporting whether
    /// it borrowed. Like the processor it keeps the low eight result bits, so a borrow below zero wraps through 99.
    /// </summary>
    public PackedBcd Subtract(PackedBcd value, out bool borrowed)
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
