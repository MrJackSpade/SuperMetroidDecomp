using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Versioned editable map cells; exploration rules are supplied by the application.</summary>
public sealed class AreaMapPresentationAsset : IAreaMapView
{
    /// <summary>Validated visual BG words in logical 64-column row order, independent of native page layout.</summary>
    private readonly MapTileWord[] cells;
    /// <summary>Snapshot of stock discoverability rules, unaffected by visual tile edits.</summary>
    private readonly bool[] discoverable;
    /// <summary>Snapshot of map-station reveal rules, unaffected by visual tile edits.</summary>
    private readonly bool[] station;
    /// <summary>Snapshot of sloped-cell reveal-above rules, unaffected by visual tile edits.</summary>
    private readonly bool[] revealsAbove;
    /// <summary>Area shared by the validated presentation document and its separately supplied exploration rules.</summary>
    public AreaId Area { get; }

    /// <summary>Pairs copied visual cells with a complete independent snapshot of gameplay map rules.</summary>
    private AreaMapPresentationAsset(MapTileWord[] cells, IAreaMapView rules)
    {
        this.cells = cells;
        Area = rules.Area;
        discoverable = new bool[cells.Length];
        station = new bool[cells.Length];
        revealsAbove = new bool[cells.Length];
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var cell = new Position16<MapTiles>(checked((ushort)x), checked((ushort)y));
            int i = Index(cell);
            discoverable[i] = rules.IsDiscoverable(cell);
            station[i] = rules.IsRevealedByMapStation(cell);
            revealsAbove[i] = rules.RevealsCellAbove(cell);
        }
    }

    /// <summary>Loads a full row-major layout, without ROM reads or writable file operations.</summary>
    public static AreaMapPresentationAsset Load(Stream json, IAreaMapView rules)
    {
        Ensure.NotNull(json);
        Ensure.NotNull(rules);
        MapPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<MapPresentationDocument>(json, MapPresentationFormat.JsonOptions)
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
        var cells = new MapTileWord[count];
        for (int i = 0; i < count; i++)
        {
            var cell = document.Cells[i];
            if (cell is null || (uint)cell.TileColumn >= MapPresentationFormat.AtlasColumns ||
                (uint)cell.TileRow >= MapPresentationFormat.AtlasRows || (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                throw new InvalidDataException($"Map presentation cell ({i % 64},{i / 64}) has an invalid atlas coordinate or palette.");
            cells[i] = new MapTileWord((ushort)(cell.TileRow * MapPresentationFormat.AtlasColumns + cell.TileColumn |
                cell.Palette << MapPresentationFormat.PaletteShift |
                (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
                (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) |
                (cell.FlipY ? MapPresentationFormat.FlipYBit : 0)));
        }
        return new(cells, rules);
    }

    /// <summary>Applies other exploration rules to these already-validated, never-mutated cells.</summary>
    internal AreaMapPresentationAsset WithRules(IAreaMapView rules)
    {
        Ensure.NotNull(rules);
        if (rules.Area != Area) throw new ArgumentException($"Rules for {rules.Area} cannot apply to the {Area} map.", nameof(rules));
        return new(cells, rules);
    }

    /// <summary>Gets the edited packed SNES BG word at logical map-tile coordinates X 0–63, Y 0–31; native two-page ordering is applied later when building a screen tilemap.</summary>
    public MapTileWord GetTile(Position16<MapTiles> cell) => cells[Index(cell)];
    /// <summary>Gets the supplied rule snapshot for whether logical cell (X 0–63, Y 0–31) can be discovered, independent of the edited character or blank artwork.</summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => discoverable[Index(cell)];
    /// <summary>Gets the supplied rule snapshot for whether downloading this area's map reveals logical cell (X 0–63, Y 0–31); no exploration or save bits are changed.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell) => station[Index(cell)];
    /// <summary>Gets the supplied slope-rule snapshot for whether visiting logical cell (X 0–63, Y 0–31) also explores the cell above; replacing its artwork does not change this rule.</summary>
    public bool RevealsCellAbove(Position16<MapTiles> cell) => revealsAbove[Index(cell)];

    /// <summary>Importer entrypoint: writes presentation only, never source addresses or gameplay rules.</summary>
    public static void Write(Stream json, IAreaMapView map)
    {
        Ensure.NotNull(json);
        Ensure.NotNull(map);
        var cells = new MapPresentationCell[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var cell = new Position16<MapTiles>(checked((ushort)x), checked((ushort)y));
            MapTileWord tile = map.GetTile(cell);
            cells[Index(cell)] = new()
            {
                TileColumn = tile.CharacterIndex % MapPresentationFormat.AtlasColumns,
                TileRow = tile.CharacterIndex / MapPresentationFormat.AtlasColumns,
                Palette = tile.PaletteIndex, Priority = tile.HasPriority,
                FlipX = (tile.Raw & MapPresentationFormat.FlipXBit) != 0,
                FlipY = (tile.Raw & MapPresentationFormat.FlipYBit) != 0
            };
        }
        JsonSerializer.Serialize(json, new MapPresentationDocument
        {
            Version = MapPresentationFormat.Version, Area = map.Area.ToString(), Cells = cells
        }, MapPresentationFormat.JsonOptions);
    }

    /// <summary>Validates the native two-page coordinate, then indexes the editable logical row-major array.</summary>
    private static int Index(Position16<MapTiles> cell)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(cell); // Shared coordinate validation; storage here is row-major.
        return AreaMapLayout.Size.RowMajorIndex(cell);
    }
}
