using SuperMetroid.Core.Areas.Ceres.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The character pages the Ceres self-destruct shows as its warning begins (#1406), one an update ($A6:C4CB, $A6:C4FE):
/// the escape timer's two pages and the warning text's five over the OBJ characters, which the Ridley arena's
/// backgrounds also read; then eight updates in which the cartridge copies the warning text's five pages and the doors'
/// three to the background characters, which the arena does not read, so none of them shows.
/// </summary>
internal static class CeresEscapePageDefinitions
{
    /// <summary>The bytes one four-bit character holds.</summary>
    internal const int CharacterBytes = 32;

    /// <summary>The characters one full page holds.</summary>
    internal const int PageCharacters = 16;

    /// <summary>The warning text's pages: four full pages, then a half page.</summary>
    internal const int WarningPageCount = 5;

    /// <summary>The installed Ceres escape character page holding the warning text (<see cref="CeresEscapeTileCharacters.Page"/>).</summary>
    internal const int WarningTextPage = 0;

    /// <summary>
    /// The updates the self-destruct spends on its background page list ($A6:C4FE): one for each of the warning text's
    /// five pages and the doors' three, none of which shows.
    /// </summary>
    internal const int BackgroundPageUpdates = 8;
}
