using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Immutable compiled map-screen pages authored as named JSON tile grids: the world map's foreground and each area's
/// background and room frame. Only the import reads them; it pictures each page once (#1467).
/// </summary>
public sealed class MapScreenPresentation
{
    /// <summary>Validated named map pages, each 1024 placed tiles in 32-column row order.</summary>
    private readonly Dictionary<MapScreenPageId, BackgroundTile[]> pages;
    /// <summary>Takes ownership of the compiled pages after load-time validation.</summary>
    private MapScreenPresentation(Dictionary<MapScreenPageId, BackgroundTile[]> pages) => this.pages = pages;

    /// <summary>The named page's tiles, row by row.</summary>
    internal ReadOnlySpan<BackgroundTile> Page(MapScreenPageId page) => pages[page];
    /// <summary>Loads and validates the complete set of named map-screen pages from JSON.</summary>
    public static MapScreenPresentation Load(Stream json)
    {
        MapScreenDocument document;
        try { document = JsonAssetDocument.Read<MapScreenDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Map-screen document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map-screen JSON.", error); }
        if (document.Version != MapScreenDefinitions.Version || document.Pages is null || document.Pages.Count != MapScreenDefinitions.PageCount)
            throw new InvalidDataException("Map screens require version 1 and all thirteen named pages.");
        var pages = new Dictionary<MapScreenPageId, BackgroundTile[]>();
        foreach (var definition in MapScreens.Pages())
        {
            if (!document.Pages.TryGetValue(definition.Id.Name, out var cells) || cells is null || cells.Length != MapScreenDefinitions.PageCells)
                throw new InvalidDataException($"Map-screen {definition.Id.Name} requires 1024 cells in 32-column row order.");
            var tiles = new BackgroundTile[MapScreenDefinitions.PageCells];
            for (int index = 0; index < cells.Length; index++)
            {
                var cell = cells[index];
                if (cell is null || (uint)cell.TileColumn >= definition.TileColumns ||
                    (uint)cell.TileRow >= definition.TileCount / definition.TileColumns || (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                    throw new InvalidDataException($"Map-screen {definition.Id.Name} cell {index} has an invalid atlas coordinate or palette.");
                tiles[index] = new BackgroundTile(cell.TileRow * definition.TileColumns + cell.TileColumn, cell.Palette,
                    cell.FlipX, cell.FlipY, cell.Priority);
            }
            pages.Add(definition.Id, tiles);
        }
        return new(pages);
    }
    /// <summary>Validates and writes a map-screen document as camel-case JSON.</summary>
    public static void Write(Stream output, MapScreenDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}
