using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Complete authored pause backdrops, including area lettering, without runtime ROM patches.</summary>
public sealed class PauseBackdropPresentation
{
    // Exact independent stock differences and supplied edits remain explicit;
    // calculated complete backdrops are never cached.
    private readonly Dictionary<int, ushort>[] areas;
    private readonly Dictionary<int, ushort> buttons;
    private PauseBackdropPresentation(Dictionary<int, ushort>[] areas, Dictionary<int, ushort> buttons)
    { this.areas = areas; this.buttons = buttons; }

    public byte[] CreateButtonTilemap()
    {
        var result = new byte[PauseBackdropDefinitions.ButtonCells * sizeof(ushort)];
        for (int cell = 0; cell < PauseBackdropDefinitions.ButtonCells; cell++)
            BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(cell * sizeof(ushort)),
                buttons.TryGetValue(cell, out ushort word) ? word : PauseBackdropDefinitions.StockButtonWord(cell));
        return result;
    }

    public void LoadTo(SnesVram vram, int destinationByteAddress, AreaId area)
    {
        var selected = areas[AreaIds.ToIndex(area)];
        Ensure.BetweenInclusive(0, SnesVram.ByteCount - PauseBackdropDefinitions.ByteCount, destinationByteAddress);
        Span<byte> wordBytes = stackalloc byte[sizeof(ushort)];
        for (int cell = 0; cell < PauseBackdropDefinitions.Cells; cell++)
        {
            ushort word = selected.TryGetValue(cell, out ushort value) ? value : PauseBackdropDefinitions.StockAreaWord(area, cell);
            BinaryPrimitives.WriteUInt16LittleEndian(wordBytes, word);
            vram.LoadBytes(destinationByteAddress + cell * sizeof(ushort), wordBytes);
        }
    }

    private static Dictionary<int, ushort> Differences(byte[] selected, Func<int, ushort> calculate)
    {
        var result = new Dictionary<int, ushort>();
        for (int cell = 0; cell < selected.Length / sizeof(ushort); cell++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(selected.AsSpan(cell * sizeof(ushort)));
            if (word != calculate(cell)) result.Add(cell, word);
        }
        return result;
    }
    public static PauseBackdropPresentation Load(Stream json)
    {
        PauseBackdropDocument document;
        try { document = JsonAssetDocument.Read<PauseBackdropDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause backdrop document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause backdrop JSON.", error); }
        if (document.Version != PauseBackdropDefinitions.Version || document.Areas is null || document.Areas.Count != AreaIds.RetailCount)
            throw new InvalidDataException("Pause backdrops require version 1 and all seven named areas.");
        var areas = new Dictionary<int, ushort>[AreaIds.RetailCount];
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            if (!document.Areas.TryGetValue(area.ToString(), out var cells) || cells is null || cells.Length != PauseBackdropDefinitions.Cells)
                throw new InvalidDataException($"Pause backdrop {area} requires 1024 cells in 32-column row order.");
            areas[AreaIds.ToIndex(area)] = Differences(PauseTileGrid.Compile(cells, area.ToString()), cell => PauseBackdropDefinitions.StockAreaWord(area, cell));
        }
        if (document.Buttons is null || document.Buttons.Length != PauseBackdropDefinitions.ButtonCells)
            throw new InvalidDataException("Pause buttons require 512 cells in 32-column row order.");
        return new(areas, Differences(PauseTileGrid.Compile(document.Buttons, "Buttons"), PauseBackdropDefinitions.StockButtonWord));
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
