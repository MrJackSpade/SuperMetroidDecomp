using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable equipment-page base art, independent of live inventory and reserve patches.</summary>
public sealed class PauseEquipmentBasePresentation
{
    private readonly byte[] tilemap;
    private PauseEquipmentBasePresentation(byte[] tilemap) => this.tilemap = tilemap;
    public byte[] CreateTilemap() => tilemap.ToArray();

    /// <summary>Refreshes authored base cells while preserving every footprint owned by live menu state.</summary>
    public void RebindBaseInto(Span<byte> current)
    {
        if (current.Length != PauseEquipmentBaseDefinitions.Cells * sizeof(ushort))
            throw new ArgumentException("Equipment base requires a complete 32x32 tilemap.", nameof(current));
        for (int cell = 0; cell < PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            if (PauseEquipmentBaseDefinitions.IsLiveOwnedCell(cell)) continue;
            int offset = cell * sizeof(ushort);
            ushort replacement = BinaryPrimitives.ReadUInt16LittleEndian(tilemap.AsSpan(offset));
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
        try { document = JsonSerializer.Deserialize<PauseEquipmentBaseDocument>(json, MapPresentationFormat.JsonOptions)
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

