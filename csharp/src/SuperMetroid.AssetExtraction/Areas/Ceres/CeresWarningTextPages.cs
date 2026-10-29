using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using static SuperMetroid.AssetExtraction.Areas.Ceres.CeresWarningTextPageDefinitions;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The installed characters each Ceres warning page look shows (#1406, #1463): consecutive full pages of the installed
/// warning text from its first byte, the last a half page.
/// </summary>
internal static class CeresWarningTextPages
{
    /// <summary>The byte of the installed warning text <paramref name="page"/> starts at.</summary>
    internal static int ByteOffset(CeresWarningTextPage page) => page.Page * PageCharacters * CharacterBytes;

    /// <summary>The bytes <paramref name="page"/> shows: a full page, or half of one for the last.</summary>
    internal static int ByteCount(CeresWarningTextPage page) =>
        (page.Page == CeresEscapePageDefinitions.WarningPageCount - 1 ? PageCharacters / 2 : PageCharacters) * CharacterBytes;
}
