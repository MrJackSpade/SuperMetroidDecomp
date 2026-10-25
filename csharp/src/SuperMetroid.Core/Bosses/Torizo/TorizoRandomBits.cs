namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The bits of a random number a Torizo decision tests: it is taken when every one is clear, so the number of bits sets
/// its odds.
/// </summary>
/// <param name="Mask">The tested bits.</param>
internal readonly record struct TorizoRandomBits(int Mask)
{
    /// <summary>The decision testing the bits at <paramref name="positions"/>.</summary>
    internal static TorizoRandomBits At(params int[] positions)
    {
        int mask = 0;
        foreach (int position in positions)
            mask |= 1 << Ensure.BetweenInclusive(0, 15, position);
        return new(mask);
    }

    /// <summary>Whether <paramref name="random"/> has every tested bit clear.</summary>
    internal bool AllClear(int random) => (random & Mask) == 0;
}
