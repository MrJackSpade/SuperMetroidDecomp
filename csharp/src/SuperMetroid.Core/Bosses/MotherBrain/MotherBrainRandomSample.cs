namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The room's random number as one Mother Brain update reads it without advancing the generator. Mother Brain's
/// choices test the number's sign, its low byte and its named fields, so the sample answers those questions by name; the
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
    public static MotherBrainRandomSample FromGenerator(int value) => new(Ensure.BetweenInclusive(0, ValueCount - 1, value));

    /// <summary>Whether the number's top bit is set, so it reads as negative when treated as signed.</summary>
    public bool IsNegative => Value >= NegativeThreshold;

    /// <summary>The number's low byte, from zero through 255.</summary>
    public int LowByte => Value % ByteValueCount;

    /// <summary>The value <paramref name="field"/> reads from the number.</summary>
    /// <param name="field">The choice the number makes.</param>
    internal int Read(RandomSampleField field) => field.Of(Value);

    /// <summary>Whether <paramref name="field"/>, a single bit, is set in the number.</summary>
    /// <param name="field">The single-bit choice the number makes.</param>
    internal bool IsSet(RandomSampleField field) => field.IsSetIn(Value);
}
