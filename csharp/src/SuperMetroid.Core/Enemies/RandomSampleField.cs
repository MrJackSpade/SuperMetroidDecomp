namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A selection a random sample makes (#1461): the bits of the sample one choice reads, by their position and width.
/// The cartridge draws a choice by masking its random word; a catalog names the field the choice reads instead of the
/// mask, and this type is the one place that reads a field from a sample.
/// </summary>
/// <param name="Shift">The position of the field's lowest bit in the sample.</param>
/// <param name="Width">How many bits the field holds, 1 through 16.</param>
[EncodingOwner("random sample bit fields")]
public readonly record struct RandomSampleField(int Shift, int Width)
{
    /// <summary>The field's value in <paramref name="sample"/>, 0 through <see cref="Outcomes"/> minus one.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    public int Of(int sample) => (sample >> Shift) & (Outcomes - 1);

    /// <summary>How many values the field can take.</summary>
    public int Outcomes => 1 << Width;

    /// <summary>Whether the field, a single bit, is set in <paramref name="sample"/>.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    /// <exception cref="InvalidOperationException">The field holds more than one bit.</exception>
    public bool IsSetIn(int sample) => Width == 1
        ? Of(sample) != 0
        : throw new InvalidOperationException($"A {Width}-bit random field is a choice, not a flag.");
}

/// <summary>Draws named selections from the enemy random-number word (#1461).</summary>
public static class RandomSampleFieldSelections
{
    /// <summary>Advances the random word and returns the value <paramref name="field"/> reads from the new sample.</summary>
    /// <param name="random">The random-number word.</param>
    /// <param name="field">The selection drawn.</param>
    public static int Next(this IEnemyRandomNumbers random, RandomSampleField field) =>
        field.Of(Ensure.NotNull(random).Generate());
}
