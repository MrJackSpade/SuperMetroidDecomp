using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The names room palette files had before #1363, which carried their cartridge source.</summary>
internal static class RoomTilesetFileNames
{
    /// <summary>Each room palette's old file name and its current name.</summary>
    internal static IReadOnlyDictionary<string, string> LegacyPaletteNames() => RoomTilesetLegacyFileNameDefinitions.Palettes.ToDictionary(
        pair => pair.Value, pair => RoomStaticPaletteFormat.FileName(pair.Key));
}
