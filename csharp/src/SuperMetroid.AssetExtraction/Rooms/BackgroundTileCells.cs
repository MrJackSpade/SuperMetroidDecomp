using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Writes a placed background tile as the editable cell installed tilemap documents store: its tile split into a
/// 32-column character-sheet column and row, its colour variant, priority and mirroring.
/// </summary>
internal static class BackgroundTileCells
{
    /// <summary>The editable cell that places <paramref name="tile"/>.</summary>
    internal static RoomBackgroundTilemapCell Of(BackgroundTile tile) => new()
    {
        TileColumn = tile.Tile % RoomBackgroundTilemapFormat.TileColumns,
        TileRow = tile.Tile / RoomBackgroundTilemapFormat.TileColumns,
        Palette = tile.ColourVariant,
        Priority = tile.HighPriority,
        FlipX = tile.FlipHorizontally,
        FlipY = tile.FlipVertically,
    };

    /// <summary>The editable cell that places the tile a cartridge tilemap entry places.</summary>
    internal static RoomBackgroundTilemapCell Of(int word) => Of(BackgroundTileWords.ToTile(word));
}
