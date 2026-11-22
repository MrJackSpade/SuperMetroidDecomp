using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The final-line map: the four rows of the opening's Japanese subtitle line, as ordered editable tile references.</summary>
public sealed partial class IntroFinalLineTilemap
{
    /// <summary>The 128 installed map tiles in native cell order.</summary>
    private readonly BackgroundTile[] tiles;

    /// <summary>Retains the installed map tiles.</summary>
    private IntroFinalLineTilemap(BackgroundTile[] tiles) => this.tiles = tiles;

    /// <summary>Native cell order: 32 columns in each of four consecutive rows.</summary>
    public ReadOnlyMemory<BackgroundTile> Tiles => tiles;

    /// <summary>Validates and compiles 128 ordered character references into the four-row BG3 subtitle map corresponding to $8B:A72B-A82A, kept as installed.</summary>
    /// <param name="json">Caller-owned stream containing the supported editable tile-reference document.</param>
    /// <returns>The final-line map, including editable margins, palette, priority, and flip attributes.</returns>
    /// <exception cref="InvalidDataException">The document schema, cell count, or character/palette selectors are invalid.</exception>
    public static IntroFinalLineTilemap Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroFinalLineTilemapDocument document = JsonAssetDocument.Read<IntroFinalLineTilemapDocument>(
            json, ArtworkJson.Options, "final-line map");
        if (document.Version != IntroFinalLineTilemapFormat.Version ||
            document.Cells is not { Length: IntroFinalLineTilemapFormat.CellCount })
            throw new InvalidDataException("Final-line map requires 128 ordered cells.");

        var tiles = new BackgroundTile[IntroFinalLineTilemapFormat.CellCount];
        for (int index = 0; index < tiles.Length; index++)
        {
            tiles[index] = document.Cells[index]?.Placement()
                ?? throw new InvalidDataException($"Final-line map cell {index} has an invalid tile or palette.");
        }
        return new IntroFinalLineTilemap(tiles);
    }

    /// <summary>Serializes the tile-reference document as UTF-8 JSON.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete four-row map to serialize.</param>
    public static void Write(Stream json, IntroFinalLineTilemapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Frontend.Intro.Artwork.IntroFinalLineTilemapDocument>(ArtworkJson.Options));
        json.Write(bytes);
    }
}
