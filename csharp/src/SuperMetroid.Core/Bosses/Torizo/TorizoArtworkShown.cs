namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Which artwork swaps a Torizo has shown and in what order (#1406): each swap's place among all the swaps it has shown,
/// latest highest. Where two swaps show over the same part of the body, the one shown later is what its frames draw.
/// </summary>
internal sealed class TorizoArtworkShown
{
    /// <summary>Each swap's place among the swaps shown, by ordinal, or zero for a swap not shown.</summary>
    private readonly long[] _shownAt = new long[TorizoArtworkSwap.Count];

    /// <summary>The swaps shown so far, counting repeats.</summary>
    internal long ShownCount { get; private set; }

    /// <summary>Selects <paramref name="swap"/> at this update, after every swap shown before.</summary>
    internal void Show(TorizoArtworkSwap swap) => _shownAt[swap.Ordinal] = ++ShownCount;

    /// <summary>The place among the swaps shown at which <paramref name="swap"/> was last shown, or null if it never was.</summary>
    internal long? LastShown(TorizoArtworkSwap swap) => _shownAt[swap.Ordinal] is var place and > 0 ? place : null;
}
