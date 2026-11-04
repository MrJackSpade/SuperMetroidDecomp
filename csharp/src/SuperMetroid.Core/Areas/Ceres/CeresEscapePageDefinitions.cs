using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The pages the Ceres self-destruct shows as its warning begins (#1406, #1463, #1513), one an update: the escape timer's
/// two pages and the warning text's five, the steps of the escape's artwork sequence, which the Ridley arena's backgrounds
/// also follow; then eight updates in which the game readies the warning text's five pages and the doors' three for the
/// backgrounds, which the arena does not show, so none of them shows.
/// </summary>
internal static class CeresEscapePageDefinitions
{
    /// <summary>The warning text's pages: four full pages, then a half page.</summary>
    internal const int WarningPageCount = 5;

    /// <summary>The timer-sprite list's pages: the escape timer's two, then the warning text's.</summary>
    internal const int ObjectPageCount = EscapeTextArtworkDefinitions.TimerPageCount + WarningPageCount;

    /// <summary>
    /// The updates the self-destruct spends on its background page list: one for each of the warning text's
    /// five pages and the doors' three, none of which shows.
    /// </summary>
    internal const int BackgroundPageUpdates = 8;
}
