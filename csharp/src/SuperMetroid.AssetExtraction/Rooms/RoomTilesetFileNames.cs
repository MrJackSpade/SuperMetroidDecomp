using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The names room character, block and palette files had before #1363, which carried their cartridge source.</summary>
internal static class RoomTilesetFileNames
{
    /// <summary>Each character sheet's old file name and its current name.</summary>
    internal static IReadOnlyDictionary<string, string> LegacyCharacterNames() => RoomTilesetSources.Characters.ToDictionary(
        pair => $"room-characters-{pair.Value:X6}.png", pair => RoomCharacterAtlasFormat.FileName(pair.Key));

    /// <summary>Each block table's old file name and its current name.</summary>
    internal static IReadOnlyDictionary<string, string> LegacyBlockNames() => RoomTilesetSources.Blocks.ToDictionary(
        pair => $"room-blocks-{pair.Value:X6}.json", pair => RoomMetatileFormat.FileName(pair.Key));

    /// <summary>Each room palette's old file name and its current name.</summary>
    internal static IReadOnlyDictionary<string, string> LegacyPaletteNames() => RoomTilesetSources.Palettes.ToDictionary(
        pair => $"room-palette-{pair.Value:X6}.json", pair => RoomStaticPaletteFormat.FileName(pair.Key));
}
