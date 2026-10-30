using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// Reads the room effects' BG3 pages as the cartridge's little-endian tilemap words, and builds catalogs whose documents
/// carry edited pages, so verification can compare pages with the cartridge's transfers.
/// </summary>
internal static class RoomFxLayer3TilemapFixtures
{
    /// <summary>The cartridge transfer bytes of a catalog's pages.</summary>
    extension(RoomFxLayer3Pages catalog)
    {
        /// <summary>The little-endian tilemap words of the page <paramref name="type"/> shows.</summary>
        internal byte[] ResolveBytes(RoomFxType type) => BackgroundTileWordBytes.Of(catalog.Resolve(type).Span);
    }

    /// <summary>A catalog loaded from a document holding <paramref name="stock"/>'s pages with <paramref name="type"/>'s replaced by <paramref name="page"/>.</summary>
    /// <exception cref="InvalidDataException">The edited page is not a complete page.</exception>
    internal static RoomFxLayer3Pages WithPage(RoomFxLayer3Pages stock, RoomFxType type, ReadOnlySpan<BackgroundTile> page)
    {
        var pages = RoomFxLayer3TilemapFormat.Types.ToDictionary(each => each.ToString(),
            each => Cells(stock.Resolve(each).Span));
        pages[type.ToString()] = Cells(page);
        using var json = new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
            new RoomFxLayer3TilemapDocument { Version = RoomFxLayer3TilemapFormat.Version, Pages = pages },
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
        return RoomFxLayer3Pages.Load(json);
    }

    /// <summary>The document cells that place <paramref name="tiles"/>.</summary>
    private static RoomBackgroundTilemapCell[] Cells(ReadOnlySpan<BackgroundTile> tiles)
    {
        var cells = new RoomBackgroundTilemapCell[tiles.Length];
        for (int index = 0; index < cells.Length; index++)
        {
            BackgroundTile tile = tiles[index];
            cells[index] = new RoomBackgroundTilemapCell
            {
                TileColumn = tile.Tile % RoomBackgroundTilemapFormat.TileColumns,
                TileRow = tile.Tile / RoomBackgroundTilemapFormat.TileColumns,
                Palette = tile.ColourVariant,
                Priority = tile.HighPriority,
                FlipX = tile.FlipHorizontally,
                FlipY = tile.FlipVertically,
            };
        }
        return cells;
    }
}
