namespace SuperMetroid.Core.Progression;

/// <summary>
/// The cartridge's shared random state (WRAM <c>$05E5</c>): one sixteen-bit value every random consumer draws from.
/// Advancing it and exchanging its two bytes are its only operations; a sample is the state read as an int.
/// </summary>
public readonly record struct SharedRandomState
{
    /// <summary>Holds a state from 0 through 65535.</summary>
    public SharedRandomState(int value) => Value = Ensure.BetweenInclusive(0, ushort.MaxValue, value);

    /// <summary>The state as a sample, 0 through 65535.</summary>
    public int Value { get; }

    /// <summary>
    /// The state after one advance, exactly as <c>$80:8111</c> computes it: the two byte multiplies, the high-byte
    /// addition's carry and the final sixteen-bit wrapping addition.
    /// </summary>
    public SharedRandomState Next()
    {
        // The ROM performs two separate 8x8 multiplies through the SNES hardware
        // multiplier ($4202/$4203). This first product is retained as a 16-bit word.
        int lowByteProduct = SnesLittleEndian.LowByte(Value) * 5;

        // The second hardware multiplication is read through an 8-bit accumulator, so
        // only its low result byte participates directly in the next addition.
        int highByteProductLow = unchecked((byte)(SnesLittleEndian.HighByte(Value) * 5));

        // SEC makes the addition start with carry=1. It modifies the high byte of the
        // first product. The ninth bit is retained separately as the processor carry.
        int highByteSum = SnesLittleEndian.HighByte(unchecked((ushort)lowByteProduct)) + highByteProductLow + 1;

        // STA $02,S replaces the saved product's high byte but, naturally, stores only
        // eight bits. Reconstruct that post-STA stack word explicitly.
        int savedProduct = SnesLittleEndian.Word(unchecked((byte)lowByteProduct), unchecked((byte)highByteSum));

        // After PLA restores 16-bit A, ADC #$0011 consumes the carry left by the 8-bit
        // addition. That extra carry is the reason this is only *roughly* r*5+$0111.
        int result = savedProduct + 17 + SnesCarry.OutOfByte(highByteSum);

        // A 65C816 accumulator wraps at 16 bits.
        return new SharedRandomState(SnesLittleEndian.LowWord(result));
    }

    /// <summary>The state with its two bytes exchanged, as the lava and acid FX routine stores it back.</summary>
    public SharedRandomState WithBytesSwapped() =>
        new(SnesLittleEndian.SwapBytes(checked((ushort)Value)));
}
