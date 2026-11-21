namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A selection a random sample makes (#1461): the part of the sample one choice reads, a run of its binary places by
/// their position and width. A catalog names the field each choice reads, and this type is the one place that reads a
/// field from a sample.
/// </summary>
/// <remarks>
/// Read at every random choice's draw, through the named field for what it chooses. Evidence: each choice's original
/// masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays compare the random
/// value and every outcome it drives on every update.
/// </remarks>
/// <param name="Shift">The position of the field's lowest bit in the sample.</param>
/// <param name="Width">How many bits the field holds, 0 through 16; a field of no bits reads zero, a choice with one outcome.</param>
[EncodingOwner("random sample bit fields")]
[CompatibilityService("which part of the shared random number each random choice reads, exactly as the game's choices read it")]
public readonly record struct RandomSampleField(int Shift, int Width)
{
    /// <summary>The whole sample, all sixteen bits: the field a choice reads when it compares the random number itself (#1619).</summary>
    public static RandomSampleField Whole { get; } = new(0, 16);

    /// <summary>No part of the sample: a field with one outcome, zero, for a choice that does not vary.</summary>
    public static RandomSampleField Nothing { get; } = new(0, 0);

    /// <summary>The number of values each half of the sample can take: the ordinary roll and the second roll.</summary>
    private const int HalfOutcomes = 256;

    /// <summary>
    /// The random number's ordinary roll of <paramref name="outcomes"/> equally likely values (#2563): the roll most of the
    /// game's choices take, its lowest places, so two ordinary rolls of one random number agree on their common values.
    /// </summary>
    /// <param name="outcomes">How many values the roll takes, a power of two from 2 through 65536.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="outcomes"/> is not a power of two in that range.</exception>
    public static RandomSampleField OneOf(int outcomes) => new(0, WidthOf(outcomes, Whole.Outcomes));

    /// <summary>
    /// The random number's second roll of <paramref name="outcomes"/> equally likely values (#2563): a roll the game takes
    /// alongside the ordinary roll (<see cref="OneOf"/>) of the same random number, independent of it while the ordinary roll
    /// takes 256 values or fewer, as a choice of a height beside a choice of a column.
    /// </summary>
    /// <param name="outcomes">How many values the roll takes, a power of two from 2 through 256.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="outcomes"/> is not a power of two in that range.</exception>
    public static RandomSampleField SecondOf(int outcomes) => new(HalfWidth, WidthOf(outcomes, HalfOutcomes));

    /// <summary>
    /// Which of <paramref name="parts"/> equal parts of the random range the random number falls in (#2563), counted from the
    /// lowest: a choice the game took by the random number's size, as a chance of one in two taken by its upper half.
    /// </summary>
    /// <param name="parts">How many parts the range is split into, a power of two from 2 through 65536.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parts"/> is not a power of two in that range.</exception>
    public static RandomSampleField RangePart(int parts)
    {
        int width = WidthOf(parts, Whole.Outcomes);
        return new(Whole.Width - width, width);
    }

    /// <summary>The places of the sample each of its two halves holds.</summary>
    private static int HalfWidth => Whole.Width / 2;

    /// <summary>How many places a field of <paramref name="outcomes"/> values holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="outcomes"/> is not a power of two from 2 through <paramref name="most"/>.</exception>
    private static int WidthOf(int outcomes, int most)
    {
        if (outcomes < 2 || outcomes > most || !System.Numerics.BitOperations.IsPow2(outcomes))
            throw new ArgumentOutOfRangeException(nameof(outcomes), outcomes, $"A roll takes a power of two from 2 through {most} values.");
        return System.Numerics.BitOperations.Log2((uint)outcomes);
    }

    /// <summary>The field's value in <paramref name="sample"/>, 0 through <see cref="Outcomes"/> minus one.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    public int Of(int sample) => (sample >> Shift) & (Outcomes - 1);

    /// <summary>
    /// The field's value in <paramref name="sample"/> advanced <paramref name="steps"/> places, continuing past the field's
    /// last value from zero (#1619): the roll a choice reads when the game added a counter to the field it drew and
    /// kept the field's bits of the sum, so the counter moves the roll's outcome while the sample still decides it.
    /// </summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    /// <param name="steps">How far the counter advances the roll, zero or more.</param>
    public int OfAdvancedBy(int sample, int steps) => (Of(sample) + Ensure.AtLeastZero(steps)) & (Outcomes - 1);

    /// <summary>How many values the field can take.</summary>
    public int Outcomes => 1 << Width;
}

