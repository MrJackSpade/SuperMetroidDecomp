namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The carry and borrow of byte and word arithmetic (#223, #1420): the bits above a byte or word after a sum computed
/// in a wider integer, which the game adds into the next byte or word of a multi-part value. A fraction that
/// overflows moves its whole part, and a chained addition or subtraction passes its carry or borrow on to the next
/// part, exactly as the original arithmetic did.
/// </summary>
[CompatibilityService("additions and subtractions whose carry or borrow out of a byte or word moves the next part of a multi-part value, so a fraction's overflow advances the whole part")]
[EncodingOwner("word addition carry")]
public static class WordCarry
{
    /// <summary>The carry (or borrow, when negative) out of a sixteen-bit word addition.</summary>
    public static int OutOfWord(int sum) => sum >> 16;

    /// <inheritdoc cref="OutOfWord(int)"/>
    public static uint OutOfWord(uint sum) => sum >> 16;

    /// <summary>The carry (or borrow, when negative) out of an eight-bit addition.</summary>
    public static int OutOfByte(int sum) => sum >> 8;

    /// <summary>
    /// Sixteen-bit <c>ADC</c>: adds <paramref name="addend"/> and the incoming carry, and leaves the carry out of the
    /// word in <paramref name="carry"/> for the next addition.
    /// </summary>
    public static ushort AddWithCarry(ushort value, ushort addend, ref bool carry)
    {
        int sum = value + addend + (carry ? 1 : 0);
        carry = sum > ushort.MaxValue;
        return unchecked((ushort)sum);
    }

    /// <summary>The carry a sixteen-bit <c>CMP</c> leaves: set when <paramref name="value"/> is at least <paramref name="operand"/> unsigned.</summary>
    public static bool AfterCompare(ushort value, ushort operand) => value >= operand;

    /// <summary>
    /// Sixteen-bit <c>SBC</c>: subtracts <paramref name="subtrahend"/> and a borrow when the incoming carry is clear,
    /// and leaves carry set when no borrow occurred, as the 65816 does.
    /// </summary>
    public static ushort SubtractWithBorrow(ushort value, ushort subtrahend, ref bool carry)
    {
        int difference = value - subtrahend - (carry ? 0 : 1);
        carry = difference >= 0;
        return unchecked((ushort)difference);
    }
}
