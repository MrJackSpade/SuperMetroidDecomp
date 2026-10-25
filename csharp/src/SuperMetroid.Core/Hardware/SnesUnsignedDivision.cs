namespace SuperMetroid.Core.Hardware;

/// <summary>The SNES CPU's unsigned 16-bit dividend / 8-bit divisor operation.</summary>
public static class SnesUnsignedDivision
{
    /// <summary>
    /// Returns the quotient published at $4214 after writes to $4204-$4206.
    /// A zero divisor produces $FFFF, including when the dividend is also zero.
    /// Callers must preserve the widths of the actual register writes: the dividend is a word, 0 through 65535, and
    /// the divisor a byte, 0 through 255.
    /// </summary>
    public static ushort Quotient(int dividend, int divisor)
    {
        Ensure.BetweenInclusive(0, ushort.MaxValue, dividend);
        return Ensure.BetweenInclusive(0, byte.MaxValue, divisor) == 0 ? ushort.MaxValue : (ushort)(dividend / divisor);
    }
}
