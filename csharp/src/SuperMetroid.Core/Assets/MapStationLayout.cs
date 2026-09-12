using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Drawing positions only: moving an icon never changes its compiled discovery cell.</summary>
public sealed class MapStationLayout
{
    private readonly Dictionary<string, MapLabelPoint> points;
    private MapStationLayout(Dictionary<string, MapLabelPoint> points) => this.points = points;
    public MapLabelPoint Get(string id) => points.TryGetValue(id, out var point) ? point
        : throw new InvalidDataException($"Unknown map station marker '{id}'.");

    public static MapStationLayout Load(Stream json)
    {
        MapStationLayoutDocument document;
        try { document = JsonSerializer.Deserialize<MapStationLayoutDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map station layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map station layout JSON.", error); }
        var rules = MapStationDiscoveryRules.All.ToArray();
        if (document.Version != MapStationLayoutFormat.Version || document.Markers is null || document.Markers.Count != rules.Length)
            throw new InvalidDataException("Map station layout requires version 1 and every known marker exactly once.");
        foreach (var rule in rules)
            if (!document.Markers.TryGetValue(rule.Id, out var point) || point is null || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Map station {rule.Id} requires X=0..511 and Y=0..255.");
        return new(new(document.Markers, StringComparer.Ordinal));
    }

    public static void Write(Stream json, MapStationLayoutDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record MapStationLayoutDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, MapLabelPoint> Markers { get; init; }
}
public static class MapStationLayoutFormat
{
    public const int Version = 1;
    public const string FileName = "map-station-labels.json";
}
