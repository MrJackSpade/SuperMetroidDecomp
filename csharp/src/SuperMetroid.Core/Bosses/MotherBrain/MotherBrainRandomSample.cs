namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The room's random number as one Mother Brain update reads it without advancing the generator. Mother Brain's
/// choices test the number's sign, its low byte and its low bits, so the sample answers those questions by name; the
/// sixteen-bit range is the generator's own and is load-bearing, because each test reads a different part of the
/// same number.
/// </summary>
public readonly record struct MotherBrainRandomSample
{
    /// <summary>The number of distinct values the generator produces.</summary>
    private const int ValueCount = 65536;

    /// <summary>The value at and above which the sixteen-bit number reads as negative.</summary>
    private const int NegativeThreshold = ValueCount / 2;

    /// <summary>The number of values one low byte holds.</summary>
    private const int ByteValueCount = 256;

    /// <summary>Retains one generator value from zero through 65535.</summary>
    private MotherBrainRandomSample(int value) => Value = value;

    /// <summary>The sampled generator value, from zero through 65535.</summary>
    private int Value { get; }

    /// <summary>Samples the generator's current value exactly.</summary>
    public static MotherBrainRandomSample FromGenerator(ushort value) => new(value);

    /// <summary>Whether the number's top bit is set, so it reads as negative when treated as signed.</summary>
    public bool IsNegative => Value >= NegativeThreshold;

    /// <summary>The number's low byte, from zero through 255.</summary>
    public int LowByte => Value % ByteValueCount;

    /// <summary>The number's lowest <paramref name="bitCount"/> bits, from zero through two to that power minus one.</summary>
    public int LowBits(int bitCount) => Value % (1 << bitCount);

    /// <summary>Whether bit <paramref name="bit"/> (zero for the lowest) of the number is set.</summary>
    public bool HasBit(int bit) => (Value >> bit) % 2 != 0;
}
