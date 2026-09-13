using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Complete authored pause backdrops, including area lettering, without runtime ROM patches.</summary>
public sealed class PauseBackdropPresentation
{
    private readonly byte[][] areas;
    private readonly byte[] buttons;
    private PauseBackdropPresentation(byte[][] areas, byte[] buttons) { this.areas = areas; this.buttons = buttons; }
    public byte[] CreateButtonTilemap() => buttons.ToArray();
    public void LoadTo(SnesVram vram, int destinationByteAddress, AreaId area) =>
        vram.LoadBytes(destinationByteAddress, areas[AreaIds.ToIndex(area)]);

    public static PauseBackdropPresentation Load(Stream json)
    {
        PauseBackdropDocument document;
        try { document = JsonSerializer.Deserialize<PauseBackdropDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause backdrop document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause backdrop JSON.", error); }
        if (document.Version != PauseBackdropDefinitions.Version || document.Areas is null || document.Areas.Count != AreaIds.RetailCount)
            throw new InvalidDataException("Pause backdrops require version 1 and all seven named areas.");
        var areas = new byte[AreaIds.RetailCount][];
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            if (!document.Areas.TryGetValue(area.ToString(), out var cells) || cells is null || cells.Length != PauseBackdropDefinitions.Cells)
                throw new InvalidDataException($"Pause backdrop {area} requires 1024 cells in 32-column row order.");
            areas[AreaIds.ToIndex(area)] = Compile(cells, area.ToString());
        }
        if (document.Buttons is null || document.Buttons.Length != PauseBackdropDefinitions.ButtonCells)
            throw new InvalidDataException("Pause buttons require 512 cells in 32-column row order.");
        return new(areas, Compile(document.Buttons, "Buttons"));

        static byte[] Compile(PauseBackdropCell[] cells, string name)
        {
            var bytes = new byte[cells.Length * sizeof(ushort)];
            for (int index = 0; index < cells.Length; index++)
            {
                var cell = cells[index];
                if (cell is null || (uint)cell.TileColumn >= PauseBackdropDefinitions.AtlasColumns ||
                    (uint)cell.TileRow >= PauseBackdropDefinitions.AtlasRows || (uint)cell.Palette >= MapPresentationFormat.PaletteCount ||
                    cell.Atlas is not (PauseBackdropDefinitions.MapAtlas or PauseBackdropDefinitions.InterfaceAtlas))
                    throw new InvalidDataException($"Pause backdrop {name} cell {index} has an invalid atlas, coordinate or palette.");
                int character = cell.TileRow * PauseBackdropDefinitions.AtlasColumns + cell.TileColumn;
                if (cell.Atlas == PauseBackdropDefinitions.InterfaceAtlas) character += PauseBackdropDefinitions.AtlasTileCount;
                ushort word = (ushort)(character | cell.Palette << MapPresentationFormat.PaletteShift |
                    (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
                    (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) | (cell.FlipY ? MapPresentationFormat.FlipYBit : 0));
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), word);
            }
            return bytes;
        }
    }

    public static void Write(Stream output, PauseBackdropDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(json, writable: false));
        output.Write(json);
    }
}

/// <summary>One complete 32x32 backdrop per area; live buttons are a separate foreground overlay.</summary>
public sealed record PauseBackdropDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, PauseBackdropCell[]> Areas { get; init; }
    public required PauseBackdropCell[] Buttons { get; init; }
}

/// <summary>Native-size artwork references, not tile numbers, addresses or gameplay commands.</summary>
public sealed record PauseBackdropCell
{
    public required string Atlas { get; init; }
    public required int TileColumn { get; init; }
    public required int TileRow { get; init; }
    public required int Palette { get; init; }
    public required bool Priority { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}
