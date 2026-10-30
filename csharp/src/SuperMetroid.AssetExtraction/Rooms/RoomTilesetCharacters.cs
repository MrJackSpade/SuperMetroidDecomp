using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The room characters a tileset's pictures are composed from (#1466), which only the import knows.</summary>
internal static class RoomTilesetCharacters
{
    /// <summary>A tileset's character sheet.</summary>
    extension(TilesetDefinition tileset)
    {
        /// <summary>The background character sheet the tileset uploads after the common room elements.</summary>
        internal RoomCharacterSet Characters => RoomTilesetCharacterDefinitions.CharacterSources[tileset.GraphicsSet];
    }
}
