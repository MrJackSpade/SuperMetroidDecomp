namespace SuperMetroid.Core.Hardware;

/// <summary>The SNES CPU's unsigned 16-bit dividend / 8-bit divisor operation.</summary>
public static class SnesUnsignedDivision
{
    /// <summary>
    /// Returns the quotient published at $4214 after writes to $4204-$4206.
    /// A zero divisor produces $FFFF, including when the dividend is also zero.
    /// Callers must preserve the widths of the actual register writes.
    /// </summary>
    public static ushort Quotient(ushort dividend, byte divisor) =>
        divisor == 0 ? ushort.MaxValue : (ushort)(dividend / divisor);
}
