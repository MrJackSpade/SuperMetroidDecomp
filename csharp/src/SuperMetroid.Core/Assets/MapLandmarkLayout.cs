using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Boss, elevator and gunship drawing anchors, independent of progression and destination rules.</summary>
public sealed class MapLandmarkLayout
{
    private readonly Dictionary<string, MapLabelPoint> points;
    private MapLandmarkLayout(Dictionary<string, MapLabelPoint> points) => this.points = points;
    public MapLabelPoint Get(string id) => points.TryGetValue(id, out var point) ? point
        : throw new InvalidDataException($"Unknown map landmark '{id}'.");

    public static MapLandmarkLayout Load(Stream json)
    {
        MapLandmarkDocument document;
        try { document = JsonSerializer.Deserialize<MapLandmarkDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map landmark layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map landmark layout JSON.", error); }
        var ids = MapLandmarkDefinitions.AllIds().ToArray();
        if (document.Version != MapLandmarkFormat.Version || document.Markers is null || document.Markers.Count != ids.Length)
            throw new InvalidDataException("Map landmarks require version 1 and every known marker.");
        foreach (string id in ids)
            if (!document.Markers.TryGetValue(id, out var point) || point is null || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Map landmark {id} requires X=0..511 and Y=0..255.");
        return new(new(document.Markers, StringComparer.Ordinal));
    }
    public static void Write(Stream json, MapLandmarkDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
public sealed record MapLandmarkDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, MapLabelPoint> Markers { get; init; }
}
public static class MapLandmarkFormat
{
    public const int Version = 1;
    public const string FileName = "map-landmarks.json";
}
