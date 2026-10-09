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
    /// <summary>Per-area sparse tilemap overrides, indexed by retail area; omitted cells use the stock word.</summary>
    private readonly Dictionary<int, ushort>[] areas;
    /// <summary>Sparse button-page tilemap overrides; omitted cells use the stock button word.</summary>
    private readonly Dictionary<int, ushort> buttons;

    /// <summary>Stores the sparse area and button overrides used to materialize complete pause backdrops.</summary>
    /// <param name="areas">One cell-indexed set of non-stock words for each retail area.</param>
    /// <param name="buttons">Cell-indexed non-stock words for the separate button page.</param>
    private PauseBackdropPresentation(Dictionary<int, ushort>[] areas, Dictionary<int, ushort> buttons)
    { this.areas = areas; this.buttons = buttons; }

    /// <summary>Materializes the selected 32-by-16 button/foreground image without caching or modifying the presentation.</summary>
    /// <returns>A new caller-owned 1024-byte array containing 512 little-endian BG tilemap words in row-major order; the live pause menu may change its button palette fields.</returns>
    public byte[] CreateButtonTilemap()
    {
        var result = new byte[PauseBackdropDefinitions.ButtonCells * sizeof(ushort)];
        for (int cell = 0; cell < PauseBackdropDefinitions.ButtonCells; cell++)
            BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(cell * sizeof(ushort)),
                buttons.TryGetValue(cell, out ushort word) ? word : PauseBackdropDefinitions.StockButtonWord(cell));
        return result;
    }

    /// <summary>Writes the selected area's complete 32-by-32 backdrop, including its authored area lettering, directly into VRAM.</summary>
    /// <param name="vram">Destination video memory; bytes outside the 2048-byte tilemap image remain unchanged.</param>
    /// <param name="destinationByteAddress">Physical VRAM byte offset, from 0 through <see cref="SnesVram.ByteCount"/> minus 2048; this is not a native word address.</param>
    /// <param name="area">Retail area whose installed backdrop is selected; the native Ceres label is COLONY.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not a retail area.</exception>
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

    /// <summary>Extracts only tilemap words that differ from the supplied stock-word calculation.</summary>
    /// <param name="selected">Compiled row-major tilemap bytes containing little-endian words.</param>
    /// <param name="calculate">Provides the stock word for each cell index.</param>
    /// <returns>A cell-indexed dictionary containing the selected words that override stock.</returns>
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
    /// <summary>Validates all seven authored area pages and the separate button page, compiling independent tile words and retaining only their differences from stock.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <returns>An immutable selection of backdrop and button artwork that does not retain the document's mutable dictionaries or arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, the version or area/page counts are wrong, or a cell has an invalid atlas, coordinate or palette.</exception>
    public static PauseBackdropPresentation Load(Stream json)
    {
        PauseBackdropDocument document;
        try { document = JsonAssetDocument.Read<PauseBackdropDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause backdrop document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause backdrop JSON.", error); }
        if (document.Version != PauseBackdropDefinitions.Version || document.Areas is null || document.Areas.Count != AreaIds.RetailCount)
            throw new InvalidDataException("Pause backdrops require version 1 and all seven named areas.");
        var areas = new byte[AreaIds.RetailCount][];
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            if (!document.Areas.TryGetValue(area.ToString(), out var cells) || cells is null || cells.Length != PauseBackdropDefinitions.Cells)
                throw new InvalidDataException($"Pause backdrop {area} requires 1024 cells in 32-column row order.");
            areas[AreaIds.ToIndex(area)] = PauseTileGrid.Compile(cells, area.ToString());
        }
        if (document.Buttons is null || document.Buttons.Length != PauseBackdropDefinitions.ButtonCells)
            throw new InvalidDataException("Pause buttons require 512 cells in 32-column row order.");
        return FromTilemaps(areas, PauseTileGrid.Compile(document.Buttons, "Buttons"));
    }

    /// <summary>
    /// Builds the presentation from compiled tilemaps (one 1024-cell page per retail area in
    /// area order, plus the 512-cell button page), keeping only words that differ from stock.
    /// </summary>
    internal static PauseBackdropPresentation FromTilemaps(byte[][] areaTilemaps, byte[] buttonTilemap)
    {
        ArgumentNullException.ThrowIfNull(areaTilemaps);
        ArgumentNullException.ThrowIfNull(buttonTilemap);
        if (areaTilemaps.Length != AreaIds.RetailCount)
            throw new InvalidDataException("Pause backdrops require one tilemap per retail area.");
        if (buttonTilemap.Length != PauseBackdropDefinitions.ButtonCells * sizeof(ushort))
            throw new InvalidDataException("Pause buttons require 512 cells.");
        var areas = new Dictionary<int, ushort>[AreaIds.RetailCount];
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            byte[] tilemap = areaTilemaps[AreaIds.ToIndex(area)] ?? throw new InvalidDataException($"Pause backdrop {area} is missing.");
            if (tilemap.Length != PauseBackdropDefinitions.Cells * sizeof(ushort))
                throw new InvalidDataException($"Pause backdrop {area} requires 1024 cells.");
            areas[AreaIds.ToIndex(area)] = Differences(tilemap, cell => PauseBackdropDefinitions.StockAreaWord(area, cell));
        }
        return new(areas, Differences(buttonTilemap, PauseBackdropDefinitions.StockButtonWord));
    }

    /// <summary>Serializes and validates every backdrop and button cell before writing the UTF-8 JSON document.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Editable page collections read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema or artwork-reference validation.</exception>
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
    /// <summary>Schema revision; loading requires <see cref="PauseBackdropDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly Crateria, Brinstar, Norfair, WreckedShip, Maridia, Tourian and Ceres, each containing 1024 nonnull cells in 32-column row-major order; collections remain caller-mutable.</summary>
    public required Dictionary<string, PauseBackdropCell[]> Areas { get; init; }
    /// <summary>Separate foreground/button page with 512 nonnull cells in 32-column, 16-row order, initially corresponding to the lower half of native $B6:E000's backdrop.</summary>
    public required PauseBackdropCell[] Buttons { get; init; }
}

/// <summary>Native-size artwork references, not tile numbers, addresses or gameplay commands.</summary>
public sealed record PauseBackdropCell
{
    /// <summary>Case-sensitive artwork sheet key, Map or Interface, selecting the first or second 256-character page of the loaded pause graphics.</summary>
    public required string Atlas { get; init; }
    /// <summary>Zero-based column 0..31 of an eight-pixel tile within the selected atlas, not its destination tilemap column.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based row 0..7 of an eight-pixel tile within the selected atlas, not its destination tilemap row.</summary>
    public required int TileRow { get; init; }
    /// <summary>BG palette selector 0..7, encoded in tilemap attribute bits 10 through 12.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether the tilemap's BG priority bit 13 is set; this is not an OBJ priority tier.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether the selected tile's pixels are reflected horizontally via attribute bit 14.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether the selected tile's pixels are reflected vertically via attribute bit 15.</summary>
    public required bool FlipY { get; init; }
}
