using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The room characters, block table and palette a tileset's pictures are composed from (#1466, #1616), which only the
/// import knows.
/// </summary>
internal static class RoomTilesetSets
{
    /// <summary>A tileset's character sheet, block table and palette.</summary>
    extension(TilesetDefinition tileset)
    {
        /// <summary>The background character sheet the tileset uploads after the common room elements.</summary>
        internal RoomCharacterSet Characters => RoomTilesetCharacterDefinitions.CharacterSources[tileset.GraphicsSet];

        /// <summary>The block table the tileset's rooms are built from.</summary>
        internal RoomBlockSet Blocks => RoomTilesetTableDefinitions.BlockSources[tileset.GraphicsSet];

        /// <summary>The background palette; its first 128 colours are loaded with the room.</summary>
        internal RoomPaletteSet Palette => RoomTilesetTableDefinitions.PaletteSources[tileset.GraphicsSet];
    }
}
