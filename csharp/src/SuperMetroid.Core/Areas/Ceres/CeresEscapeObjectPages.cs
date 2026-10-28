using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using static SuperMetroid.Core.Areas.Ceres.CeresEscapePageDefinitions;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres escape's object character pages (<see cref="CeresEscapePageDefinitions"/>) in the order the timer-sprite list shows
/// them: the escape timer's two pages, then the warning text's five, the last one half a page.
/// </summary>
internal static class CeresEscapeObjectPages
{
    /// <summary>
    /// The pages the timer-sprite list ($A6:C4CB) shows over the OBJ characters, one an update, in its order: the escape
    /// timer's two pages, then the warning text's five from its first character.
    /// </summary>
    internal static IReadOnlyList<SpriteArtworkSource> ObjectPages { get; } =
    [
        .. Enum.GetValues<EscapeTimerTilePage>().Select(page => new InstalledArtworkCharacters(EscapeTimerTilePages.AssetOf(page))),
        .. Enumerable.Range(0, WarningPageCount).Select(page =>
            new CeresEscapeTileCharacters(WarningTextPage, page * PageCharacters * CharacterBytes, WarningPageByteCount(page))),
    ];

    /// <summary>The bytes warning text page <paramref name="page"/> holds: a full page, or half of one for the last.</summary>
    private static int WarningPageByteCount(int page) =>
        (page == WarningPageCount - 1 ? PageCharacters / 2 : PageCharacters) * CharacterBytes;
}
