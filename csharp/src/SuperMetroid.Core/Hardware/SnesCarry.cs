namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The carry out of a native addition (#223): bits above an 8- or 16-bit register after a sum computed in a wider
/// host integer. Routines add it into the next byte or word, as <c>ADC</c> consumes the carry flag.
/// </summary>
[SnesEncodingOwner]
public static class SnesCarry
{
    /// <summary>The carry (or borrow, when negative) out of a sixteen-bit word addition.</summary>
    public static int OutOfWord(int sum) => sum >> 16;

    /// <inheritdoc cref="OutOfWord(int)"/>
    public static uint OutOfWord(uint sum) => sum >> 16;

    /// <summary>The carry (or borrow, when negative) out of an eight-bit addition.</summary>
    public static int OutOfByte(int sum) => sum >> 8;
}
