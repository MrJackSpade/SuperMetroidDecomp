using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Verification access to <see cref="ImportAreaMapPresentation"/> members production does not use.</summary>
internal static class InstalledAreaMapPresentationAccess
{
    /// <summary>Reads what the import's map presentation was read with.</summary>
    extension(ImportAreaMapPresentation)
    {
        /// <summary>The escape timer's characters the import's map presentation <paramref name="maps"/> was read with (#1636).</summary>
        /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
        internal static EscapeTimerCharacters EscapeTimerTilesOf(AreaMapPresentation maps) =>
            ImportAreaMapPresentation.Imported(maps, "escape timer characters").EscapeTimerTiles;
    }
}
