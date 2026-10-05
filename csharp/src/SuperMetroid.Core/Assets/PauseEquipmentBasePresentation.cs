using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable equipment-page base art, independent of live inventory and reserve patches.</summary>
public sealed class PauseEquipmentBasePresentation
{
    // Independent layout/connector cells remain required alongside explicit asset edits.
    private readonly Dictionary<int, ushort> remainingCells = [];
    private PauseEquipmentBasePresentation(ReadOnlySpan<byte> selected)
    {
        // Row order installs each left wireframe cell before its reflected partner.
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(selected[(cell * sizeof(ushort))..]);
            if (word != DefaultWord(cell)) remainingCells.Add(cell, word);
        }
    }

    public byte[] CreateTilemap()
    {
        var result = new byte[PauseEquipmentBaseDefinitions.Cells * sizeof(ushort)];
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
            BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(cell * sizeof(ushort)), Word(cell));
        return result;
    }

    private ushort Word(int cell) => remainingCells.TryGetValue(cell, out ushort value) ? value : DefaultWord(cell);

    private ushort DefaultWord(int cell)
    {
        int relative = cell - PauseWireframeDefinitions.DestinationByte / sizeof(ushort);
        int stride = PauseWireframeDefinitions.DestinationStride / sizeof(ushort);
        if (relative >= 0 && relative / stride < PauseWireframeDefinitions.Rows &&
            relative % stride < PauseWireframeDefinitions.Columns)
        {
            int column = relative % stride;
            int local = relative / stride * PauseWireframeDefinitions.Columns + column;
            if (PauseWireframeDefinitions.TryStockTile(PauseWireframeKind.PowerSuit, local, out int tile))
                return (ushort)(PauseWireframeDefinitions.CommonPieceAttributes | tile);
            if (column >= PauseWireframeDefinitions.Columns / 2)
            {
                ushort left = Word(cell + PauseWireframeDefinitions.Columns - 1 - column * 2);
                return left == 0 ? (ushort)0 : (ushort)(left ^ MapPresentationFormat.FlipXBit);
            }
            return 0;
        }
        return PauseEquipmentBaseDefinitions.StockWord(cell);
    }

    /// <summary>Refreshes authored base cells while preserving every footprint owned by live menu state.</summary>
    public void RebindBaseInto(Span<byte> current,
        PauseEquipmentLabelPresentation? equipmentLabels = null)
    {
        if (current.Length != PauseEquipmentBaseDefinitions.Cells * sizeof(ushort))
            throw new ArgumentException("Equipment base requires a complete 32x32 tilemap.", nameof(current));
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
        if (current.Length != PauseEquipmentBaseDefinitions.Cells * sizeof(ushort))
            throw new ArgumentException("Equipment base requires a complete 32x32 tilemap.", nameof(current));
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

    public static PauseEquipmentBasePresentation Load(Stream json)
    {
        PauseEquipmentBaseDocument document;
        try { document = JsonAssetDocument.Read<PauseEquipmentBaseDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause equipment base document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause equipment base JSON.", error); }
        if (document.Version != PauseEquipmentBaseDefinitions.Version || document.Cells is null ||
            document.Cells.Length != PauseEquipmentBaseDefinitions.Cells)
            throw new InvalidDataException("Pause equipment base requires version 1 and 1024 cells in 32-column row order.");
        return new(PauseTileGrid.Compile(document.Cells, "EquipmentBase"));
    }

    public static void Write(Stream output, PauseEquipmentBaseDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}

public sealed record PauseEquipmentBaseDocument
{
    public required int Version { get; init; }
    public required PauseBackdropCell[] Cells { get; init; }
}
