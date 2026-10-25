namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>One editable RGB5 palette file per distinct graphics-set color source.</summary>
public static class RoomStaticPaletteFormat
{
    /// <summary>The supported room-palette JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The background colours a room's palette selects: eight sixteen-colour rows.</summary>
    public const int ColorCount = 128;

    /// <summary>The bytes of a room palette's packed colours, two per colour.</summary>
    public const int ByteCount = ColorCount * Bgr555.ByteCount;

    /// <summary>The editable JSON file name of a room palette.</summary>
    public static string FileName(SuperMetroid.Core.Rooms.LevelData.RoomPaletteSet set) =>
        $"room-palette-{SuperMetroid.Core.Assets.InstalledArtworkNames.Hyphenated(set.ToString())}.json";
}
