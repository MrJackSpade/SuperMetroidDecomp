namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// An angle a random sample chooses (#2555): the least angle the choice makes, turned on by one step for each value the
/// field it reads can take above zero, so the field's values choose evenly spaced angles from the least on. A catalog
/// names the choice's field, least angle and step, and this type is the one place that turns a sample into such an angle.
/// </summary>
/// <remarks>
/// Read wherever a family draws a random angle, such as the start of a falling piece's sway, on the sample the draw takes.
/// Evidence: each draw's original masking and shifting of the random number into the angle's word, which the field and
/// step reproduce for every sample; the SMV replays compare the random value and every angle it drives on every update.
/// </remarks>
/// <param name="Field">The part of the sample the choice reads.</param>
/// <param name="Least">The angle the field's zero chooses.</param>
/// <param name="Step">How much further round each next value of the field chooses.</param>
public readonly partial record struct RandomAngle(RandomSampleField Field, RotationAngle Least, RotationAngle Step)
{
    /// <summary>The angle <paramref name="sample"/> chooses: the least, turned on by a step for each value of its field.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    public RotationAngle Of(int sample) => Least.TurnedBy(Step, Field.Of(sample));
}
