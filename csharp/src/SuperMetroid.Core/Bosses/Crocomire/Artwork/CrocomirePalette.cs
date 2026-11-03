using SuperMetroid.Core.Assets;
using Band = SuperMetroid.Core.Bosses.Crocomire.Artwork.CrocomireColorCatalog.Band;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Crocomire's five material colour transfers: how a colour resolves from its edit or the calculated stock paint, how a
/// transfer is installed and how the installed colours are identified.
/// </summary>
internal static class CrocomirePalette
{
    /// <summary>Returns an edited color or calculates the corresponding stock paint.</summary>
    internal static RgbColor Get(Dictionary<CrocomireColorKey, RgbColor> edits, Band band, int index)
    {
        if ((uint)index >= CrocomireColorCatalog.Counts[band]) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new CrocomireColorKey(band, index), out RgbColor edited)
            ? edited
            : CrocomireColorCatalog.StockPaint[band](index);
    }
}
