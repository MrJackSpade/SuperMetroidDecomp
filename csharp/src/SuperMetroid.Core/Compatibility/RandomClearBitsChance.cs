using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A chance a random choice takes when every one of its bits of the sample is clear (#2320, #2479): the game tested single,
/// separate bits of its random number one after another and took the chance only when none was set, so a chance of two
/// bits is taken by one sample in four and one of four bits by one in sixteen. Each bit is a <see cref="RandomChance"/>
/// taken by its one-bit field's lowest value, and the chance is taken when every one of them is. A chance the game took on
/// a run of a field's values is a <see cref="RandomChance"/>; this one's bits are not a run of any field.
/// </summary>
/// <remarks>
/// Taken at every such choice, on the sample the choice reads, through the named chance its family's catalog gives for what
/// the choice decides. Evidence: each choice's original tests of its random word's bits, which the chance reproduces for
/// every one of the 65536 samples; the SMV replays compare the random value and every outcome it drives on every update.
/// </remarks>
public sealed partial class RandomClearBitsChance
{
    /// <summary>For each bit the chance needs clear, the chance its one-bit field reads zero.</summary>
    private readonly ImmutableArray<RandomChance> _bitsClear;

    /// <summary>Retains the chances each needed bit is clear.</summary>
    private RandomClearBitsChance(ImmutableArray<RandomChance> bitsClear) => _bitsClear = bitsClear;

    /// <summary>The chance taken when every one of <paramref name="bits"/> is clear in the sample.</summary>
    /// <param name="bits">The separate single bits of the sample the chance needs clear, two or more.</param>
    /// <exception cref="ArgumentException">Fewer than two bits are given, or a field given holds more than one bit.</exception>
    public static RandomClearBitsChance WhenAllClear(params ReadOnlySpan<RandomSampleField> bits)
    {
        if (bits.Length < 2)
            throw new ArgumentException($"A chance of clear bits needs two or more bits, not {bits.Length}; one bit is a RandomChance.", nameof(bits));

        ImmutableArray<RandomChance>.Builder bitsClear = ImmutableArray.CreateBuilder<RandomChance>(bits.Length);
        foreach (RandomSampleField bit in bits)
        {
            bitsClear.Add(bit.Width == 1
                ? RandomChance.Lowest(1, bit)
                : throw new ArgumentException($"A chance of clear bits reads single bits, not a {bit.Width}-bit field.", nameof(bits)));
        }

        return new(bitsClear.MoveToImmutable());
    }

    /// <summary>Whether the chance is taken on <paramref name="sample"/>: none of its bits is set.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    public bool IsTakenBy(int sample)
    {
        foreach (RandomChance bitClear in _bitsClear)
        {
            if (!bitClear.IsTakenBy(sample))
                return false;
        }

        return true;
    }
}
