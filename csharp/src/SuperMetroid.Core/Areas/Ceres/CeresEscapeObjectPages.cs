using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres escape's object looks (<see cref="CeresEscapePageDefinitions"/>) in the order the timer-sprite list shows them:
/// the escape timer's two pages, then the warning text's pages.
/// </summary>
internal static class CeresEscapeObjectPages
{
    /// <summary>
    /// The looks the timer-sprite list ($A6:C4CB) shows over the sprites' artwork, one an update, in its order: the escape
    /// timer's two pages, then the warning text's from its first.
    /// </summary>
    internal static IReadOnlyList<SpriteArtworkSource> ObjectPages { get; } =
    [
        .. Enum.GetValues<EscapeTimerTilePage>().Select(page => new InstalledArtworkCharacters(EscapeTimerTilePages.AssetOf(page))),
        .. Enumerable.Range(0, CeresEscapePageDefinitions.WarningPageCount).Select(page => new CeresWarningTextPage(page)),
    ];
}
