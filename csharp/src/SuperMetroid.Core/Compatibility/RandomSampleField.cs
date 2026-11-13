namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A selection a random sample makes (#1461): the bits of the sample one choice reads, by their position and width.
/// The cartridge draws a choice by masking its random word; a catalog names the field the choice reads instead of the
/// mask, and this type is the one place that reads a field from a sample.
/// </summary>
/// <remarks>
/// Read at every random choice's draw, through the named field for what it chooses. Evidence: each choice's original
/// masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays compare the random
/// value and every outcome it drives on every update.
/// </remarks>
/// <param name="Shift">The position of the field's lowest bit in the sample.</param>
/// <param name="Width">How many bits the field holds, 0 through 16; a field of no bits reads zero, a choice with one outcome.</param>
[EncodingOwner("random sample bit fields")]
[CompatibilityService("which bits of the shared random sample each choice reads, exactly as the original masked and shifted its random word")]
public readonly record struct RandomSampleField(int Shift, int Width)
{
    /// <summary>The whole sample, all sixteen bits: the field a choice reads when it compares the random word itself (#1619).</summary>
    public static RandomSampleField Whole { get; } = new(0, 16);

    /// <summary>The field's value in <paramref name="sample"/>, 0 through <see cref="Outcomes"/> minus one.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    public int Of(int sample) => (sample >> Shift) & (Outcomes - 1);

    /// <summary>
    /// The field's value in <paramref name="sample"/> advanced <paramref name="steps"/> places, continuing past the field's
    /// last value from zero (#1619): the roll a choice reads when the original added a counter to the field it drew and
    /// kept the field's bits of the sum, so the counter moves the roll's outcome while the sample still decides it.
    /// </summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    /// <param name="steps">How far the counter advances the roll, zero or more.</param>
    public int OfAdvancedBy(int sample, int steps) => (Of(sample) + Ensure.AtLeastZero(steps)) & (Outcomes - 1);

    /// <summary>How many values the field can take.</summary>
    public int Outcomes => 1 << Width;

    /// <summary>Whether the field, a single bit, is set in <paramref name="sample"/>.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    /// <exception cref="InvalidOperationException">The field holds more than one bit.</exception>
    public bool IsSetIn(int sample) => Width == 1
        ? Of(sample) != 0
        : throw new InvalidOperationException($"A {Width}-bit random field is a choice, not a flag.");
}

