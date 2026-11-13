namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The order each Torizo's body animations show their artwork swaps in (#1406, #1538), the order in which each swap's look
/// adds to the Torizo's artwork: the import composes every look's finished pictures over the looks before it. A swap a
/// animation comes round to again, as the crumbling and glowing loops do, shows its look again.
/// </summary>
internal static class TorizoArtworkSwapDefinitions
{
    /// <summary>
    /// Bomb Torizo's swaps: its statue crumbling as it wakes (swaps 13 to 20), then its gut blowing up (0 to 3), which its
    /// health reaches before its face blowing up (4 to 12).
    /// </summary>
    internal static IReadOnlyList<TorizoArtworkSwap> BombSequence { get; } =
        [.. Enumerable.Range(13, 8).Concat(Enumerable.Range(0, 13)).Select(ordinal => new TorizoArtworkSwap(ordinal))];

    /// <summary>
    /// Golden Torizo's swaps, in the order its animations show them: its awakening and crumbling (swaps 21 to 29), its egg
    /// release (30 to 43) and its eye beam's glow (44 to 51).
    /// </summary>
    internal static IReadOnlyList<TorizoArtworkSwap> GoldenSequence { get; } =
        [.. Enumerable.Range(21, TorizoArtworkSwap.Count - 21).Select(ordinal => new TorizoArtworkSwap(ordinal))];
}
