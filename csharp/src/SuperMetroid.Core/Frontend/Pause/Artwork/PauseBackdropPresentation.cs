using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Complete authored pause backdrops, including area lettering, without runtime ROM patches.</summary>
public sealed class PauseBackdropPresentation
{
    /// <summary>Per-area authored tiles that differ from stock; complete backdrops are calculated on demand.</summary>
    internal readonly Dictionary<int, BackgroundTile>[] areas;
    /// <summary>Authored foreground tiles that differ from the stock button page.</summary>
    internal readonly Dictionary<int, BackgroundTile> buttons;
    /// <summary>Retains validated sparse edits for every area and the separate button overlay.</summary>
    private PauseBackdropPresentation(Dictionary<int, BackgroundTile>[] areas, Dictionary<int, BackgroundTile> buttons)
    { this.areas = areas; this.buttons = buttons; }

    /// <summary>Materializes the selected 32-by-16 button/foreground image without caching or modifying the presentation.</summary>
    /// <returns>A new caller-owned array of 512 tiles in row-major order; the live pause menu may change its buttons' colour variants.</returns>
    public BackgroundTile[] CreateButtonTilemap()
    {
        var result = new BackgroundTile[PauseBackdropDefinitions.ButtonCells];
        for (int cell = 0; cell < PauseBackdropDefinitions.ButtonCells; cell++)
            result[cell] = buttons.TryGetValue(cell, out BackgroundTile edited) ? edited : PauseBackdropTiles.StockButtonTile(cell);
        return result;
    }

    /// <summary>Writes the selected area's complete 32-by-32 backdrop, including its authored area lettering, into the pause BG2 tilemap.</summary>
    /// <param name="destination">The pause BG2 tilemap; tiles outside the 32-by-32 image remain unchanged.</param>
    /// <param name="area">Retail area whose installed backdrop is selected; the native Ceres label is COLONY.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not a retail area.</exception>
    public void LoadTo(Span<BackgroundTile> destination, AreaId area)
    {
        var selected = areas[AreaIds.ToIndex(area)];
        Ensure.BetweenInclusive(PauseBackdropDefinitions.Cells, int.MaxValue, destination.Length);
        for (int cell = 0; cell < PauseBackdropDefinitions.Cells; cell++)
            destination[cell] = selected.TryGetValue(cell, out BackgroundTile edited) ? edited : PauseBackdropTiles.StockAreaTile(area, cell);
    }

    /// <summary>Extracts only tiles changed from the stock calculation, preserving independent authored edits.</summary>
    private static Dictionary<int, BackgroundTile> Differences(BackgroundTile[] selected, Func<int, BackgroundTile> calculate)
    {
        var result = new Dictionary<int, BackgroundTile>();
        for (int cell = 0; cell < selected.Length; cell++)
            if (selected[cell] != calculate(cell)) result.Add(cell, selected[cell]);
        return result;
    }
    /// <summary>Validates all seven authored area pages and the separate button page, compiling independent tiles and retaining only their differences from stock.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <returns>An immutable selection of backdrop and button artwork that does not retain the document's mutable dictionaries or arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, the version or area/page counts are wrong, or a cell has an invalid atlas, coordinate or palette.</exception>
    public static PauseBackdropPresentation Load(Stream json)
    {
        PauseBackdropDocument document;
        try { document = JsonAssetDocument.Read<PauseBackdropDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Pause backdrop document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause backdrop JSON.", error); }
        if (document.Version != PauseBackdropDefinitions.Version || document.Areas is null || document.Areas.Count != AreaIds.RetailCount)
            throw new InvalidDataException("Pause backdrops require version 1 and all seven named areas.");
        var areas = new BackgroundTile[AreaIds.RetailCount][];
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
    /// area order, plus the 512-cell button page), keeping only tiles that differ from stock.
    /// </summary>
    internal static PauseBackdropPresentation FromTilemaps(BackgroundTile[][] areaTilemaps, BackgroundTile[] buttonTilemap)
    {
        Ensure.NotNull(areaTilemaps);
        Ensure.NotNull(buttonTilemap);
        if (areaTilemaps.Length != AreaIds.RetailCount)
            throw new InvalidDataException("Pause backdrops require one tilemap per retail area.");
        if (buttonTilemap.Length != PauseBackdropDefinitions.ButtonCells)
            throw new InvalidDataException("Pause buttons require 512 cells.");
        var areas = new Dictionary<int, BackgroundTile>[AreaIds.RetailCount];
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            BackgroundTile[] tilemap = areaTilemaps[AreaIds.ToIndex(area)] ?? throw new InvalidDataException($"Pause backdrop {area} is missing.");
            if (tilemap.Length != PauseBackdropDefinitions.Cells)
                throw new InvalidDataException($"Pause backdrop {area} requires 1024 cells.");
            areas[AreaIds.ToIndex(area)] = Differences(tilemap, cell => PauseBackdropTiles.StockAreaTile(area, cell));
        }
        return new(areas, Differences(buttonTilemap, PauseBackdropTiles.StockButtonTile));
    }

    /// <summary>Serializes and validates every backdrop and button cell before writing the UTF-8 JSON document.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Editable page collections read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema or artwork-reference validation.</exception>
    public static void Write(Stream output, PauseBackdropDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(json, writable: false));
        output.Write(json);
    }
}
