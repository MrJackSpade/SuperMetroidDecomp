using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Versioned editable map cells; exploration rules are supplied by the application.</summary>
public sealed class AreaMapPresentationAsset : IAreaMapView
{
    private readonly MapTileWord[] cells;
    private readonly bool[] discoverable, station, revealsAbove;
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
            document = JsonSerializer.Deserialize<MapPresentationDocument>(json, MapPresentationFormat.JsonOptions)
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

    public MapTileWord GetTile(int x, int y) => cells[Index(x, y)];
    public bool IsDiscoverable(int x, int y) => discoverable[Index(x, y)];
    public bool IsRevealedByMapStation(int x, int y) => station[Index(x, y)];
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
    public required int Version { get; init; }
    public required string Area { get; init; }
    public required MapPresentationCell[] Cells { get; init; }
}

public sealed record MapPresentationCell
{
    public required int TileColumn { get; init; }
    public required int TileRow { get; init; }
    public required int Palette { get; init; }
    public required bool Priority { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}

/// <summary>Presentation schema geometry and its compiled SNES output encoding.</summary>
public static class MapPresentationFormat
{
    public const int Version = 1;
    public const int AtlasColumns = 32;
    public const int AtlasRows = 32;
    public const int PaletteCount = 8;
    public const int PaletteShift = 10;
    public const int PriorityBit = 0x2000;
    public const int FlipXBit = 0x4000;
    public const int FlipYBit = 0x8000;
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
}
