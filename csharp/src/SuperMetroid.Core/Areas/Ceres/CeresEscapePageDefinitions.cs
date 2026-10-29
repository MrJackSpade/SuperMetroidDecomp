namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The pages the Ceres self-destruct shows as its warning begins (#1406, #1463), one an update ($A6:C4CB, $A6:C4FE): the
/// escape timer's two pages and the warning text's five as looks over the sprites' artwork, which the Ridley arena's
/// backgrounds also read; then eight updates in which the cartridge copies the warning text's five pages and the doors'
/// three to the background characters, which the arena does not read, so none of them shows.
/// </summary>
internal static class CeresEscapePageDefinitions
{
    /// <summary>The warning text's pages: four full pages, then a half page.</summary>
    internal const int WarningPageCount = 5;

    /// <summary>
    /// The updates the self-destruct spends on its background page list ($A6:C4FE): one for each of the warning text's
    /// five pages and the doors' three, none of which shows.
    /// </summary>
    internal const int BackgroundPageUpdates = 8;
}
