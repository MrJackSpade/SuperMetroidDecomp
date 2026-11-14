using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The codec of an area's editable map presentation document (#1466, #1513): each cell's kind, sloped-hallway piece and
/// edges, by name, read into the <see cref="MapCell"/>s the game shows. Exploration rules are supplied separately and never
/// come from the document.
/// </summary>
public static class AreaMapPresentationFile
{
    /// <summary>Reads a full row-major layout, without ROM reads or writable file operations.</summary>
    /// <param name="json">The area's presentation document.</param>
    /// <param name="rules">The exploration rules the cells are shown under; its area must be the document's.</param>
    /// <exception cref="InvalidDataException">The document is malformed, for another area, or names an undefined kind, slope or edge.</exception>
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
        int count = AreaMapSize.WidthInMapTiles * AreaMapSize.HeightInMapTiles;
        if (document.Cells is null || document.Cells.Length != count)
            throw new InvalidDataException($"Map presentation requires exactly {count} cells in 64-column row order.");
        var cells = new MapCell[count];
        for (int i = 0; i < count; i++)
        {
            AreaMapDocumentCell? cell = document.Cells[i];
            if (cell is null || !Enum.IsDefined(cell.Kind) || !Enum.IsDefined(cell.Slope) || !Enum.IsDefined(cell.Top) ||
                !Enum.IsDefined(cell.Bottom) || !Enum.IsDefined(cell.Left) || !Enum.IsDefined(cell.Right))
                throw new InvalidDataException($"Map presentation cell ({i % AreaMapSize.WidthInMapTiles},{i / AreaMapSize.WidthInMapTiles}) names an undefined kind, slope or edge.");
            cells[i] = new MapCell(cell.Kind, new MapCellEdges(cell.Top, cell.Bottom, cell.Left, cell.Right), cell.Slope);
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
        var cells = new AreaMapDocumentCell[AreaMapSize.WidthInMapTiles * AreaMapSize.HeightInMapTiles];
        for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
        for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
        {
            var cell = new Position16<MapTiles>(checked((ushort)x), checked((ushort)y));
            MapCell shown = map.Cell(cell);
            cells[AreaMapSize.Extent.RowMajorIndex(cell)] = new()
            {
                Kind = shown.Kind, Slope = shown.Slope,
                Top = shown.Edges.Top, Bottom = shown.Edges.Bottom, Left = shown.Edges.Left, Right = shown.Edges.Right,
            };
        }
        JsonSerializer.Serialize(json, new MapPresentationDocument
        {
            Version = MapPresentationFormat.Version, Area = map.Area.ToString(), Cells = cells
        }, ArtworkJson.Options);
    }
}
