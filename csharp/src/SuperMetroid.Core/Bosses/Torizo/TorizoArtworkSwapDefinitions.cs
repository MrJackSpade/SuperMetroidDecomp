namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The order each Torizo's body programs show their artwork swaps in (#1406, #1513): the steps of its artwork sequence,
/// whose pre-assembled pictures the installed sheets hold. A swap a program comes round to again, as the crumbling and
/// glowing loops do, takes its first step again.
/// </summary>
internal static class TorizoArtworkSwapDefinitions
{
    /// <summary>
    /// Bomb Torizo's steps: its statue crumbling as it wakes (swaps 13 to 20), then its gut blowing up (0 to 3), which its
    /// health reaches before its face blowing up (4 to 12).
    /// </summary>
    internal static IReadOnlyList<TorizoArtworkSwap> BombSequence { get; } =
        [.. Enumerable.Range(13, 8).Concat(Enumerable.Range(0, 13)).Select(ordinal => new TorizoArtworkSwap(ordinal))];

    /// <summary>
    /// Golden Torizo's steps, in the order its programs show them: its awakening and crumbling (swaps 21 to 29), its egg
    /// release (30 to 43) and its eye beam's glow (44 to 51).
    /// </summary>
    internal static IReadOnlyList<TorizoArtworkSwap> GoldenSequence { get; } =
        [.. Enumerable.Range(21, TorizoArtworkSwap.Count - 21).Select(ordinal => new TorizoArtworkSwap(ordinal))];
}
