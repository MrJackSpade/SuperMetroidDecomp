using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable equipment-page base art, independent of live inventory and reserve patches.</summary>
public sealed class PauseEquipmentBasePresentation
{
    /// <summary>Only independently edited cells; unchanged words come from the composed stock page.</summary>
    internal readonly Dictionary<int, ushort> remainingCells = [];
    /// <summary>Compares every selected cell with the immutable stock composition.</summary>
    private PauseEquipmentBasePresentation(ReadOnlySpan<byte> selected)
    {
        // Compare each supplied word independently with immutable stock composition.
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(selected[(cell * sizeof(ushort))..]);
            if (word != DefaultWord(cell)) remainingCells.Add(cell, word);
        }
    }

    /// <summary>Builds a complete 32-by-32 native BG tilemap from selected edits and calculated stock composition.</summary>
    /// <returns>A new 2048-byte little-endian tilemap in row-major order.</returns>
    public byte[] CreateTilemap()
    {
        var result = new byte[PauseEquipmentBaseDefinitions.Cells * sizeof(ushort)];
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
            BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(cell * sizeof(ushort)), Word(cell));
        return result;
    }

    /// <summary>Uses a supplied visual edit when present and calculates the stock cell otherwise.</summary>
    private ushort Word(int cell) => remainingCells.TryGetValue(cell, out ushort value) ? value : DefaultWord(cell);

    /// <summary>Combines the power-suit wireframe with the equipment base outside its footprint.</summary>
    private static ushort DefaultWord(int cell)
    {
        int relative = cell - PauseWireframeDefinitions.FirstPageByte / sizeof(ushort);
        int stride = PauseWireframeDefinitions.PageRowBytes / sizeof(ushort);
        if (relative >= 0 && relative / stride < PauseWireframeDefinitions.Rows &&
            relative % stride < PauseWireframeDefinitions.Columns)
        {
            int local = relative / stride * PauseWireframeDefinitions.Columns + relative % stride;
            return PauseWireframeDefinitions.StockWord(PauseWireframeKind.PowerSuit, local);
        }
        return PauseEquipmentBaseDefinitions.StockWord(cell);
    }

    /// <summary>Refreshes authored base cells while preserving every footprint owned by live menu state.</summary>
    public void RebindBaseInto(Span<byte> current,
        PauseEquipmentLabelPresentation? equipmentLabels = null)
    {
        Ensure.LengthEqual(current, PauseEquipmentBaseDefinitions.Cells * sizeof(ushort));
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            bool liveOwned = equipmentLabels is null
                ? PauseEquipmentBaseDefinitions.IsLiveOwnedCell(cell)
                : PauseEquipmentBaseDefinitions.IsNonInventoryLiveOwnedCell(cell) ||
                    equipmentLabels.OwnsLiveCell(cell);
            if (liveOwned) continue;
            int offset = cell * sizeof(ushort);
            ushort replacement = Word(cell);
            if (PauseEquipmentBaseDefinitions.IsArrowCell(cell))
            {
                int palette = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(current.Slice(offset))).PaletteIndex;
                replacement = new SnesBgTilemapWord(replacement).WithPaletteIndex(palette).Raw;
            }
            BinaryPrimitives.WriteUInt16LittleEndian(current.Slice(offset), replacement);
        }
    }

    /// <summary>
    /// Refreshes the complete authored base before semantic inventory labels are rebuilt,
    /// while retaining the independent wireframe/reserve owners and live arrow palette.
    /// </summary>
    public void RebindBeforeInventoryRefreshInto(Span<byte> current)
    {
        Ensure.LengthEqual(current, PauseEquipmentBaseDefinitions.Cells * sizeof(ushort));
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            if (PauseEquipmentBaseDefinitions.IsNonInventoryLiveOwnedCell(cell)) continue;
            int offset = cell * sizeof(ushort);
            ushort replacement = Word(cell);
            if (PauseEquipmentBaseDefinitions.IsArrowCell(cell))
            {
                int palette = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(current.Slice(offset))).PaletteIndex;
                replacement = new SnesBgTilemapWord(replacement).WithPaletteIndex(palette).Raw;
            }
            BinaryPrimitives.WriteUInt16LittleEndian(current.Slice(offset), replacement);
        }
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
