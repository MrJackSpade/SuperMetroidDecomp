using SuperMetroid.Core.Areas.Ceres;
using static SuperMetroid.AssetExtraction.Areas.Ceres.CeresWarningTextPageDefinitions;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The installed characters each Ceres warning page shows (#1406, #1463, #1513): consecutive full pages of the installed
/// warning text from its first byte, the last a half page.
/// </summary>
internal static class CeresWarningTextPages
{
    /// <summary>The byte of the installed warning text warning page <paramref name="page"/> starts at.</summary>
    internal static int ByteOffset(int page) => page * PageCharacters * CharacterBytes;

    /// <summary>The bytes warning page <paramref name="page"/> shows: a full page, or half of one for the last.</summary>
    internal static int ByteCount(int page) =>
        (page == CeresEscapePageDefinitions.WarningPageCount - 1 ? PageCharacters / 2 : PageCharacters) * CharacterBytes;
}
