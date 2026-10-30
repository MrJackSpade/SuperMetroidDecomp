using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The four-row Japanese subtitle staging map, as ordered editable BG tile references.</summary>
public sealed class IntroFinalLineTilemap
{
    /// <summary>Edited map tiles when any cell differs from the native formula; null keeps stock generation.</summary>
    internal readonly BackgroundTile[]? suppliedTiles;

    /// <summary>Keeps edited tiles only when the supplied map differs from the stock subtitle layout.</summary>
    private IntroFinalLineTilemap(BackgroundTile[] tiles)
    {
        for (int index = 0; index < tiles.Length; index++)
        {
            if (tiles[index] == CalculateTile(index)) continue;
            suppliedTiles = tiles;
            break;
        }
    }

    /// <summary>Native cell order: 32 columns in each of four consecutive rows.</summary>
    public ReadOnlyMemory<BackgroundTile> Tiles
    {
        get
        {
            if (suppliedTiles is not null) return suppliedTiles;
            var output = new BackgroundTile[IntroFinalLineTilemapFormat.CellCount];
            for (int index = 0; index < output.Length; index++) output[index] = CalculateTile(index);
            return output;
        }
    }

    /// <summary>Calculates the native margin or subtitle glyph tile for one of the 128 BG3 cells.</summary>
    private static BackgroundTile CalculateTile(int index)
    {
        int column = index % IntroFinalLineTilemapFormat.Columns;
        int row = index / IntroFinalLineTilemapFormat.Columns;
        int margin = IntroFinalLineTilemapFormat.MarginColumns;
        int interiorColumns = IntroFinalLineTilemapFormat.Columns - 2 * margin;
        if (column < margin || column >= margin + interiorColumns) return IntroFinalLineTilemapFormat.MarginTile;
        // Native $8B:8D84 places lower halves 0300 bytes after upper halves; each group contains two text lines.
        int tileStripRow = 2 * (row & 1) + (row >> 1);
        BackgroundTile first = IntroFinalLineTilemapFormat.FirstInteriorTile;
        return first with { Tile = first.Tile + tileStripRow * interiorColumns + column - margin };
    }
    /// <summary>Validates and compiles 128 ordered character references into the four-row BG3 subtitle map corresponding to $8B:A72B-A82A; exact stock tiles remain formula-derived.</summary>
    /// <param name="json">Caller-owned stream containing the supported editable tile-reference document.</param>
    /// <returns>The subtitle staging map, including editable margins, palette, priority, and flip attributes.</returns>
    /// <exception cref="InvalidDataException">The document schema, cell count, or character/palette selectors are invalid.</exception>
    public static IntroFinalLineTilemap Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroFinalLineTilemapDocument document = JsonAssetDocument.Read<IntroFinalLineTilemapDocument>(
            json, ArtworkJson.Options, "opening divider tilemap");
        if (document.Version != IntroFinalLineTilemapFormat.Version ||
            document.Cells is not { Length: IntroFinalLineTilemapFormat.CellCount })
            throw new InvalidDataException("Opening divider tilemap requires 128 ordered cells.");

        var tiles = new BackgroundTile[IntroFinalLineTilemapFormat.CellCount];
        for (int index = 0; index < tiles.Length; index++)
        {
            RoomBackgroundTilemapCell? cell = document.Cells[index];
            if (cell is null ||
                (uint)cell.TileColumn >= RoomBackgroundTilemapFormat.TileColumns ||
                (uint)cell.TileRow >= RoomBackgroundTilemapFormat.TileRows ||
                (uint)cell.Palette >= RoomBackgroundTilemapFormat.PaletteCount)
                throw new InvalidDataException($"Opening divider cell {index} has an invalid tile or palette.");
            tiles[index] = new BackgroundTile(
                cell.TileRow * RoomBackgroundTilemapFormat.TileColumns + cell.TileColumn,
                cell.Palette, cell.FlipX, cell.FlipY, cell.Priority);
        }
        return new IntroFinalLineTilemap(tiles);
    }

    /// <summary>Serializes the tile-reference document as UTF-8 JSON and validates it through <see cref="Load"/> before writing any destination bytes.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete four-row map to validate and serialize.</param>
    public static void Write(Stream json, IntroFinalLineTilemapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
