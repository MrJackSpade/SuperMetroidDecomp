namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A chance a random choice takes when every one of its bits of the sample is clear (#2320): the game tested single,
/// separate bits of its random number one after another and took the chance only when none was set, so a chance of two
/// bits is taken by one sample in four. A chance the game took on a run of a field's values is a
/// <see cref="RandomChance"/>; this one's bits are not a run of any field.
/// </summary>
/// <remarks>
/// Taken at every such choice, on the sample the choice reads, through the named chance its family's catalog gives for what
/// the choice decides. Evidence: each choice's original tests of its random word's bits, which the chance reproduces for
/// every one of the 65536 samples; the SMV replays compare the random value and every outcome it drives on every update.
/// </remarks>
/// <param name="First">One bit the chance needs clear.</param>
/// <param name="Second">The other bit the chance needs clear.</param>
[CompatibilityService("which random samples take a chance the original took by testing separate bits of its random word for zero")]
public readonly record struct RandomClearBitsChance(RandomSampleField First, RandomSampleField Second)
{
    /// <summary>Whether the chance is taken on <paramref name="sample"/>: neither of its bits is set.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    /// <exception cref="InvalidOperationException">A bit of the chance is a field of more than one bit.</exception>
    public bool IsTakenBy(int sample) => !First.IsSetIn(sample) && !Second.IsSetIn(sample);
}
