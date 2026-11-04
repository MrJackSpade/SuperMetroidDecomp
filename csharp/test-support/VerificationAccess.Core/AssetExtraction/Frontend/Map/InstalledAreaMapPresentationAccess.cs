using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Verification access to <see cref="InstalledAreaMapPresentation"/> members production does not use.</summary>
internal static class InstalledAreaMapPresentationAccess
{
    /// <summary>Reads what an installed map presentation was read with.</summary>
    extension(InstalledAreaMapPresentation)
    {
        /// <summary>The escape timer's characters the installed map presentation <paramref name="maps"/> was read with.</summary>
        /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
        internal static EscapeTimerCharacters EscapeTimerTilesOf(AreaMapPresentation maps) =>
            (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.EscapeTimerTiles
            ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no escape timer characters.");
    }
}
