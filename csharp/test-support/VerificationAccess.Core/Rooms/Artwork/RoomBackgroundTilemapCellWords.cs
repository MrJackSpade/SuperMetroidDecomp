using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Editable tilemap cells written from and read as the cartridge's packed tilemap entries through the codec, so fixtures
/// can author cells by their entries and checks can compare a document's cells with the cartridge's words.
/// </summary>
internal static class RoomBackgroundTilemapCellWords
{
    /// <summary>The editable cell that places the tile packed tilemap entry <paramref name="word"/> places.</summary>
    internal static RoomBackgroundTilemapCell Cell(int word) => Cell(BackgroundTileWordBytes.Tile(word));

    /// <summary>The editable cells that place <paramref name="tiles"/>, in order.</summary>
    internal static RoomBackgroundTilemapCell[] Cells(ReadOnlySpan<BackgroundTile> tiles)
    {
        var cells = new RoomBackgroundTilemapCell[tiles.Length];
        for (int index = 0; index < cells.Length; index++)
            cells[index] = Cell(tiles[index]);
        return cells;
    }

    /// <summary>The editable cell that places <paramref name="tile"/>.</summary>
    internal static RoomBackgroundTilemapCell Cell(BackgroundTile tile)
    {
        return new RoomBackgroundTilemapCell
        {
            TileColumn = tile.Tile % RoomBackgroundTilemapFormat.TileColumns,
            TileRow = tile.Tile / RoomBackgroundTilemapFormat.TileColumns,
            Palette = tile.ColourVariant,
            Priority = tile.HighPriority,
            FlipX = tile.FlipHorizontally,
            FlipY = tile.FlipVertically,
        };
    }

    /// <summary>The editable cells that place the tiles packed tilemap entries <paramref name="words"/> place, in order.</summary>
    internal static RoomBackgroundTilemapCell[] Cells(params int[] words) => [.. words.Select(word => Cell(word))];

    /// <summary>A cell's cartridge view.</summary>
    extension(RoomBackgroundTilemapCell cell)
    {
        /// <summary>The packed tilemap entry of the tile this cell places.</summary>
        /// <exception cref="InvalidDataException">The cell's tile or palette lies outside the schema.</exception>
        internal ushort PackedWord => (cell.Placement() ?? throw new InvalidDataException("The cell places no tile.")).PackedWord;
    }
}
