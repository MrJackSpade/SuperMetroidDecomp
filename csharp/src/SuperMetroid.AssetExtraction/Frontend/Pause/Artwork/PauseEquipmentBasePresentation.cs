using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Editable equipment-page base art, independent of live inventory and reserve patches, kept as installed (#1615): all
/// 1024 cells.
/// </summary>
public sealed class PauseEquipmentBasePresentation
{
    /// <summary>The installed page's every cell, in row-major order.</summary>
    private readonly BackgroundTile[] tilemap;
    /// <summary>Keeps every installed cell.</summary>
    private PauseEquipmentBasePresentation(BackgroundTile[] tilemap) => this.tilemap = tilemap;

    /// <summary>The complete installed 32-by-32 tilemap.</summary>
    /// <returns>A new caller-owned 1024-tile tilemap in row-major order.</returns>
    public BackgroundTile[] CreateTilemap() => (BackgroundTile[])tilemap.Clone();

    /// <summary>Loads and validates all 1024 editable equipment-page base cells.</summary>
    /// <param name="json">The caller-owned stream containing the tile-reference document.</param>
    /// <returns>The presentation holding every installed cell.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, cell count, or tile references are invalid.</exception>
    public static PauseEquipmentBasePresentation Load(Stream json)
    {
        PauseEquipmentBaseDocument document;
        try { document = JsonAssetDocument.Read<PauseEquipmentBaseDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Pause equipment base document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause equipment base JSON.", error); }
        if (document.Version != PauseEquipmentBaseDefinitions.Version || document.Cells is null ||
            document.Cells.Length != PauseEquipmentBaseDefinitions.Cells)
            throw new InvalidDataException("Pause equipment base requires version 1 and 1024 cells in 32-column row order.");
        return new(PauseTileGrid.Compile(document.Cells, "EquipmentBase"));
    }

    /// <summary>Writes an editable equipment-page base document as JSON.</summary>
    /// <param name="output">The caller-owned destination stream.</param>
    /// <param name="document">The complete 32-by-32 tile-reference document to serialize.</param>
    public static void Write(Stream output, PauseEquipmentBaseDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }
}
