namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A speed a random sample chooses (#2555): the slowest speed the choice makes, raised by one step for each value the field
/// it reads can take above zero, so the field's values choose evenly spaced speeds from the slowest up. A catalog names the
/// choice's field, slowest speed and step, and this type is the one place that turns a sample into such a speed.
/// </summary>
/// <remarks>
/// Read wherever a family draws a random speed, a fall, a drift or a jitter, on the sample the draw takes. Evidence: each
/// draw's original addition of a masked and shifted field of the random number to its slowest speed's word, which the
/// field and step reproduce for every sample; the SMV replays compare the random value and every speed it drives on every
/// update.
/// </remarks>
/// <param name="Field">The part of the sample the choice reads.</param>
/// <param name="Slowest">The speed the field's zero chooses.</param>
/// <param name="Step">How much faster each next value of the field chooses.</param>
public readonly partial record struct RandomSpeed(RandomSampleField Field, SignedEightEight Slowest, SignedEightEight Step)
{
    /// <summary>The speed <paramref name="sample"/> chooses: the slowest, raised by a step for each value of its field.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    public SignedEightEight Of(int sample) => Slowest + Step * Field.Of(sample);
}
