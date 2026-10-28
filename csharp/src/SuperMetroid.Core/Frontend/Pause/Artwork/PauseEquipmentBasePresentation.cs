using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable equipment-page base art, independent of live inventory and reserve patches.</summary>
public sealed class PauseEquipmentBasePresentation
{
    /// <summary>Only independently edited cells; unchanged tiles come from the composed stock page.</summary>
    internal readonly Dictionary<int, BackgroundTile> remainingCells = [];
    /// <summary>Compares every selected cell with the immutable stock composition.</summary>
    private PauseEquipmentBasePresentation(ReadOnlySpan<BackgroundTile> selected)
    {
        // Compare each supplied tile independently with immutable stock composition.
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
            if (selected[cell] != DefaultTile(cell)) remainingCells.Add(cell, selected[cell]);
    }

    /// <summary>Builds a complete 32-by-32 tilemap from selected edits and calculated stock composition.</summary>
    /// <returns>A new 1024-tile tilemap in row-major order.</returns>
    public BackgroundTile[] CreateTilemap()
    {
        var result = new BackgroundTile[PauseEquipmentBaseDefinitions.Cells];
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
            result[cell] = Tile(cell);
        return result;
    }

    /// <summary>Uses a supplied visual edit when present and calculates the stock cell otherwise.</summary>
    private BackgroundTile Tile(int cell) => remainingCells.TryGetValue(cell, out BackgroundTile edited) ? edited : DefaultTile(cell);

    /// <summary>Combines the power-suit wireframe with the equipment base outside its footprint.</summary>
    private static BackgroundTile DefaultTile(int cell)
    {
        int relative = cell - PauseWireframeDefinitions.FirstPageByte / sizeof(ushort);
        int stride = PauseWireframeDefinitions.PageRowBytes / sizeof(ushort);
        if (relative >= 0 && relative / stride < PauseWireframeDefinitions.Rows &&
            relative % stride < PauseWireframeDefinitions.Columns)
        {
            int local = relative / stride * PauseWireframeDefinitions.Columns + relative % stride;
            return PauseWireframeDefinitions.StockTile(PauseWireframeKind.PowerSuit, local);
        }
        return PauseEquipmentBaseDefinitions.StockTile(cell);
    }

    /// <summary>Refreshes authored base cells while preserving every footprint owned by live menu state.</summary>
    public void RebindBaseInto(Span<BackgroundTile> current,
        PauseEquipmentLabelPresentation? equipmentLabels = null)
    {
        Ensure.LengthEqual(current, PauseEquipmentBaseDefinitions.Cells);
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            bool liveOwned = equipmentLabels is null
                ? PauseEquipmentBaseDefinitions.IsLiveOwnedCell(cell)
                : PauseEquipmentBaseDefinitions.IsNonInventoryLiveOwnedCell(cell) ||
                    equipmentLabels.OwnsLiveCell(cell);
            if (liveOwned) continue;
            current[cell] = Replacement(current[cell], cell);
        }
    }

    /// <summary>
    /// Refreshes the complete authored base before semantic inventory labels are rebuilt,
    /// while retaining the independent wireframe/reserve owners and live arrow palette.
    /// </summary>
    public void RebindBeforeInventoryRefreshInto(Span<BackgroundTile> current)
    {
        Ensure.LengthEqual(current, PauseEquipmentBaseDefinitions.Cells);
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            if (PauseEquipmentBaseDefinitions.IsNonInventoryLiveOwnedCell(cell)) continue;
            current[cell] = Replacement(current[cell], cell);
        }
    }

    /// <summary>The selected tile of <paramref name="cell"/>, keeping a reserve arrow cell's live colour variant from <paramref name="shown"/>.</summary>
    private BackgroundTile Replacement(BackgroundTile shown, int cell)
    {
        BackgroundTile replacement = Tile(cell);
        return PauseEquipmentBaseDefinitions.IsArrowCell(cell) ? replacement with { ColourVariant = shown.ColourVariant } : replacement;
    }

    /// <summary>Loads and validates all 1024 editable equipment-page base cells.</summary>
    /// <param name="json">The caller-owned stream containing the tile-reference document.</param>
    /// <returns>The presentation retaining only cells that differ from calculated stock composition.</returns>
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

    /// <summary>Validates and writes an editable equipment-page base document as JSON.</summary>
    /// <param name="output">The caller-owned destination stream.</param>
    /// <param name="document">The complete 32-by-32 tile-reference document to serialize.</param>
    public static void Write(Stream output, PauseEquipmentBaseDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}
