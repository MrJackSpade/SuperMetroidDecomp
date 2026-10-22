using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of room tileset artwork: where each sheet, table and palette is stored on the cartridge.</summary>
internal static class RoomTilesetNative
{
    /// <summary>A tileset's cartridge sources.</summary>
    extension(TilesetDefinition tileset)
    {
        /// <summary>Where the tileset's block table is stored.</summary>
        internal int BlockDefinitionsAddress => RoomTilesetSources.Blocks[tileset.Blocks];

        /// <summary>Where the tileset's character sheet is stored.</summary>
        internal int CharacterAddress => RoomTilesetSources.Characters[tileset.Characters];

        /// <summary>Where the tileset's palette is stored.</summary>
        internal int PaletteAddress => RoomTilesetSources.Palettes[tileset.Palette];
    }

    /// <summary>The character sheet stored at <paramref name="address"/>.</summary>
    internal static RoomCharacterSet CharacterSetAt(int address) =>
        RoomTilesetSources.Characters.Single(pair => pair.Value == address).Key;

    /// <summary>The block table stored at <paramref name="address"/>.</summary>
    internal static RoomBlockSet BlockSetAt(int address) =>
        RoomTilesetSources.Blocks.Single(pair => pair.Value == address).Key;

    /// <summary>The palette stored at <paramref name="address"/>.</summary>
    internal static RoomPaletteSet PaletteSetAt(int address) =>
        RoomTilesetSources.Palettes.Single(pair => pair.Value == address).Key;
}
