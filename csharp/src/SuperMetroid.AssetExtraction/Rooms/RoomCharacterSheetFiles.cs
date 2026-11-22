using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed file names of the room character sheets the rooms' pictures are composed from (#1466).</summary>
public static partial class RoomCharacterSheetFiles
{
    /// <summary>Editable PNG filename for the common room elements (CRE) character stream shared by room graphics sets.</summary>
    public const string CreFileName = "room-characters-cre.png";

    /// <summary>The editable PNG file name of a character sheet; graphics sets that share a sheet share one PNG.</summary>
    [InstalledKey]
    public static string FileName(RoomCharacterSet set) =>
        $"room-characters-{InstalledArtworkNames.Hyphenated(RoomCharacterSetKeys.Installed.Of(set))}.png";
}
