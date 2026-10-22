namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>One editable RGB5 palette file per distinct graphics-set color source.</summary>
public static class RoomStaticPaletteFormat
{
    /// <summary>The supported room-palette JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The number of RGB5 colors in one room graphics-set palette source.</summary>
    public const int ColorCount = RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount / Bgr555.ByteCount;

    /// <summary>The editable JSON file name of a room palette.</summary>
    public static string FileName(SuperMetroid.Core.Rooms.LevelData.RoomPaletteSet set) =>
        $"room-palette-{SuperMetroid.Core.Assets.InstalledArtworkNames.Hyphenated(set.ToString())}.json";
}
