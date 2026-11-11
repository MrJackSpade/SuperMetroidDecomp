using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The four-row Japanese subtitle staging map, as ordered editable BG tile references.</summary>
public sealed class IntroFinalLineTilemap
{
    /// <summary>The 128 installed map tiles in native cell order.</summary>
    private readonly BackgroundTile[] tiles;

    /// <summary>Retains the installed map tiles.</summary>
    private IntroFinalLineTilemap(BackgroundTile[] tiles) => this.tiles = tiles;

    /// <summary>Native cell order: 32 columns in each of four consecutive rows.</summary>
    public ReadOnlyMemory<BackgroundTile> Tiles => tiles;

    /// <summary>Validates and compiles 128 ordered character references into the four-row BG3 subtitle map corresponding to $8B:A72B-A82A, kept as installed.</summary>
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

    /// <summary>Serializes the tile-reference document as UTF-8 JSON.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete four-row map to serialize.</param>
    public static void Write(Stream json, IntroFinalLineTilemapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
