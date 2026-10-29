using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The codec of an area's editable map presentation document (#1466): each cell's picture coordinates, palette, priority
/// and mirroring decoded once into the <see cref="MapCell"/>s the game shows, with the explored colour variant the
/// cartridge's explored-cell rule gives each. Exploration rules are supplied separately and never come from the document.
/// </summary>
public static class AreaMapPresentationFile
{
    /// <summary>Reads a full row-major layout, without ROM reads or writable file operations.</summary>
    /// <param name="json">The area's presentation document.</param>
    /// <param name="rules">The exploration rules the cells are shown under; its area must be the document's.</param>
    /// <exception cref="InvalidDataException">The document is malformed, for another area, or names a cell outside the map pictures.</exception>
    public static AreaMapPresentationAsset Read(Stream json, IAreaMapView rules)
    {
        Ensure.NotNull(json);
        Ensure.NotNull(rules);
        MapPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<MapPresentationDocument>(json, ArtworkJson.Options)
                ?? throw new InvalidDataException("Map presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid map presentation JSON: {error.Message}", error);
        }
        if (document.Version != MapPresentationFormat.Version || document.Area != rules.Area.ToString())
            throw new InvalidDataException($"Map presentation requires version {MapPresentationFormat.Version} and area {rules.Area}.");
        int count = AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles;
        if (document.Cells is null || document.Cells.Length != count)
            throw new InvalidDataException($"Map presentation requires exactly {count} cells in 64-column row order.");
        var cells = new MapCell[count];
        for (int i = 0; i < count; i++)
        {
            MapPresentationCell? cell = document.Cells[i];
            if (cell is null || (uint)cell.TileColumn >= MapPresentationFormat.AtlasColumns ||
                (uint)cell.TileRow >= MapPresentationFormat.AtlasRows || (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                throw new InvalidDataException($"Map presentation cell ({i % AreaMapLayout.WidthInTiles},{i / AreaMapLayout.WidthInTiles}) has an invalid atlas coordinate or palette.");
            cells[i] = WordOf(cell).ToCell();
        }
        return new(cells, rules);
    }

    /// <summary>Importer entrypoint: writes presentation only, never source addresses or gameplay rules.</summary>
    /// <param name="json">The destination document stream.</param>
    /// <param name="map">The area map whose cells are written.</param>
    public static void Write(Stream json, IAreaMapView map)
    {
        Ensure.NotNull(json);
        Ensure.NotNull(map);
        var cells = new MapPresentationCell[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var cell = new Position16<MapTiles>(checked((ushort)x), checked((ushort)y));
            MapCell shown = map.Cell(cell);
            cells[AreaMapLayout.Size.RowMajorIndex(cell)] = new()
            {
                TileColumn = shown.Shape.Picture % MapPresentationFormat.AtlasColumns,
                TileRow = shown.Shape.Picture / MapPresentationFormat.AtlasColumns,
                Palette = shown.ColourVariant, Priority = shown.InFront,
                FlipX = (shown.Mirroring & TileFlip.Horizontal) != TileFlip.None,
                FlipY = (shown.Mirroring & TileFlip.Vertical) != TileFlip.None,
            };
        }
        JsonSerializer.Serialize(json, new MapPresentationDocument
        {
            Version = MapPresentationFormat.Version, Area = map.Area.ToString(), Cells = cells
        }, ArtworkJson.Options);
    }

    /// <summary>The cartridge word a document cell describes.</summary>
    private static MapTileWord WordOf(MapPresentationCell cell) => MapTileWord.Of(new MapCell(
        new MapCellShape(cell.TileRow * MapPresentationFormat.AtlasColumns + cell.TileColumn), cell.Palette, cell.Palette,
        (cell.FlipX ? TileFlip.Horizontal : TileFlip.None) | (cell.FlipY ? TileFlip.Vertical : TileFlip.None), cell.Priority));
}
