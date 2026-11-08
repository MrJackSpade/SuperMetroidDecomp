using System.Collections.Immutable;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The bits of a random number a Torizo decision tests, each a single-bit field of the sample: it is taken when every one
/// is clear, so the number of bits sets its odds.
/// </summary>
/// <param name="Bits">The tested bits.</param>
internal sealed record TorizoRandomBits(ImmutableArray<RandomSampleField> Bits)
{
    /// <summary>The decision testing the bits at <paramref name="positions"/>.</summary>
    internal static TorizoRandomBits At(params int[] positions) =>
        new([.. positions.Select(position => new RandomSampleField(Ensure.BetweenInclusive(0, 15, position), 1))]);

    /// <summary>Whether <paramref name="random"/> has every tested bit clear.</summary>
    internal bool AllClear(int random) => Bits.All(bit => !bit.IsSetIn(random));
}
