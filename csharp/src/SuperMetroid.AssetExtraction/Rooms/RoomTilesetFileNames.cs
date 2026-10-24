using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The names room character, block and palette files had before #1363, which carried their cartridge source.</summary>
internal static class RoomTilesetFileNames
{
    /// <summary>Each character sheet's old file name and its current name.</summary>
    internal static IReadOnlyDictionary<string, string> LegacyCharacterNames() => RoomTilesetLegacyFileNameDefinitions.Characters.ToDictionary(
        pair => pair.Value, pair => RoomCharacterAtlasFormat.FileName(pair.Key));

    /// <summary>Each block table's old file name and its current name.</summary>
    internal static IReadOnlyDictionary<string, string> LegacyBlockNames() => RoomTilesetLegacyFileNameDefinitions.Blocks.ToDictionary(
        pair => pair.Value, pair => RoomMetatileFormat.FileName(pair.Key));

    /// <summary>Each room palette's old file name and its current name.</summary>
    internal static IReadOnlyDictionary<string, string> LegacyPaletteNames() => RoomTilesetLegacyFileNameDefinitions.Palettes.ToDictionary(
        pair => pair.Value, pair => RoomStaticPaletteFormat.FileName(pair.Key));
}
