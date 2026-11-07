using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>
/// The import's reading of a <see cref="TorizoArtworkSwap"/>'s place among the fifty-two, by which it finds the page of
/// characters the swap's pictures are composed from and where the original game copies them.
/// </summary>
internal static class TorizoArtworkSwapOrdinals
{
    /// <summary>The place of a Torizo artwork swap.</summary>
    extension(TorizoArtworkSwap swap)
    {
        /// <summary>The swap's place among the fifty-two, from zero, in the order the body programs were authored.</summary>
        internal int Ordinal => swap.ordinal;
    }
}
