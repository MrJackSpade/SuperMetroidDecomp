using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Versioned editable map cells; exploration rules are supplied by the application.</summary>
public sealed class AreaMapPresentationAsset : IAreaMapView
{
    private readonly MapTileWord[] cells;
    private readonly bool[] discoverable, station, revealsAbove;
    /// <summary>Area shared by the validated presentation document and its separately supplied exploration rules.</summary>
    public AreaId Area { get; }

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
            int i = Index(x, y);
            discoverable[i] = rules.IsDiscoverable(x, y);
            station[i] = rules.IsRevealedByMapStation(x, y);
            revealsAbove[i] = rules.RevealsCellAbove(x, y);
        }
    }

    /// <summary>Loads a full row-major layout, without ROM reads or writable file operations.</summary>
    public static AreaMapPresentationAsset Load(Stream json, IAreaMapView rules)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(rules);
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
        ArgumentNullException.ThrowIfNull(rules);
        if (rules.Area != Area) throw new ArgumentException($"Rules for {rules.Area} cannot apply to the {Area} map.", nameof(rules));
        return new(cells, rules);
    }

    /// <summary>Gets the edited packed SNES BG word at logical map-tile coordinates X 0–63, Y 0–31; native two-page ordering is applied later when building a screen tilemap.</summary>
    public MapTileWord GetTile(int x, int y) => cells[Index(x, y)];
    /// <summary>Gets the supplied rule snapshot for whether logical cell (X 0–63, Y 0–31) can be discovered, independent of the edited character or blank artwork.</summary>
    public bool IsDiscoverable(int x, int y) => discoverable[Index(x, y)];
    /// <summary>Gets the supplied rule snapshot for whether downloading this area's map reveals logical cell (X 0–63, Y 0–31); no exploration or save bits are changed.</summary>
    public bool IsRevealedByMapStation(int x, int y) => station[Index(x, y)];
    /// <summary>Gets the supplied slope-rule snapshot for whether visiting logical cell (X 0–63, Y 0–31) also explores the cell above; replacing its artwork does not change this rule.</summary>
    public bool RevealsCellAbove(int x, int y) => revealsAbove[Index(x, y)];

    /// <summary>Importer entrypoint: writes presentation only, never source addresses or gameplay rules.</summary>
    public static void Write(Stream json, IAreaMapView map)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(map);
        var cells = new MapPresentationCell[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            MapTileWord tile = map.GetTile(x, y);
            cells[Index(x, y)] = new()
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

    private static int Index(int x, int y)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(x, y); // Shared coordinate validation; storage here is row-major.
        return y * AreaMapLayout.WidthInTiles + x;
    }
}

/// <summary>Presentation-only schema: no reveal masks, callbacks, save indexes or exploration commands.</summary>
public sealed record MapPresentationDocument
{
    /// <summary>Schema revision for the area's lowercase-name <c>.json</c> file; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Case-sensitive <see cref="AreaId"/> name that must match the area of the supplied exploration rules.</summary>
    public required string Area { get; init; }
    /// <summary>Exactly 2048 presentation cells in 64-column, 32-row order, rather than the cartridge's two adjacent 32-by-32 pages.</summary>
    public required MapPresentationCell[] Cells { get; init; }
}

/// <summary>Editable artwork and display attributes for one map cell; these fields compile to a SNES BG word but do not define exploration or map-station rules.</summary>
public sealed record MapPresentationCell
{
    /// <summary>Zero-based 8-pixel character column from 0 through 31 in <c>map-tiles.png</c>; combined with the atlas row to form the character index.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based 8-pixel character row from 0 through 7 in the 256-by-64-pixel <c>map-tiles.png</c> atlas.</summary>
    public required int TileRow { get; init; }
    /// <summary>SNES BG palette index from 0 through 7, encoded in bits 10–12; screen projection may adjust exploration or HUD palette attributes.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether to set the BG tile priority bit $2000 in the compiled word; this is a display attribute, not a reveal flag.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether to mirror the selected character horizontally, setting packed-word bit $4000.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether to mirror the selected character vertically, setting packed-word bit $8000.</summary>
    public required bool FlipY { get; init; }
}

/// <summary>Presentation schema geometry and its compiled SNES output encoding.</summary>
public static class MapPresentationFormat
{
    /// <summary>Supported per-area map JSON schema revision, checked before compiling the cells.</summary>
    public const int Version = 1;
    /// <summary>Number of 8-pixel character columns in the shared map atlas; atlas row times 32 plus column yields the native character index.</summary>
    public const int AtlasColumns = 32;
    /// <summary>Number of 8-pixel character rows in the shared 64-pixel-high map atlas, bounding editable tile-row values.</summary>
    public const int AtlasRows = MapTileAtlasFormat.Height / 8;
    /// <summary>Eight selectable SNES BG palettes representable by the tilemap word's three palette bits.</summary>
    public const int PaletteCount = 8;
    /// <summary>Bit position 10 of the three-bit palette field, immediately above the ten-bit SNES BG character index.</summary>
    public const int PaletteShift = 10;
    /// <summary>$2000, SNES BG tile priority mask at bit 13, used when compiling an edited cell's priority attribute.</summary>
    public const int PriorityBit = 0x2000;
    /// <summary>$4000, SNES BG character horizontal-mirror mask at bit 14, preserved when exporting or compiling map cells.</summary>
    public const int FlipXBit = 0x4000;
    /// <summary>$8000, SNES BG character vertical-mirror mask at bit 15, preserved when exporting or compiling map cells.</summary>
    public const int FlipYBit = 0x8000;
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
}
