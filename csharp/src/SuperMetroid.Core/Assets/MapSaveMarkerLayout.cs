using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Selected-save marker artwork anchors; neither load targets nor scroll origins are content.</summary>
public sealed class MapSaveMarkerLayout
{
    private readonly Dictionary<string, MapLabelPoint> points;
    private MapSaveMarkerLayout(Dictionary<string, MapLabelPoint> points) => this.points = points;
    public MapLabelPoint Get(AreaId area, int index) => points[MapSaveMarkerDefinitions.Id(area, index)];
    public static MapSaveMarkerLayout Load(Stream json)
    {
        MapSaveMarkerDocument document;
        try { document = JsonSerializer.Deserialize<MapSaveMarkerDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Save marker layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid save marker layout JSON.", error); }
        var ids = MapSaveMarkerDefinitions.AllIds().ToArray();
        if (document.Version != MapSaveMarkerFormat.Version || document.Markers is null || document.Markers.Count != ids.Length)
            throw new InvalidDataException("Save marker layout requires version 1 and every known marker.");
        foreach (string id in ids)
            if (!document.Markers.TryGetValue(id, out var point) || point is null || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Save marker {id} requires X=0..511 and Y=0..255.");
        return new(new(document.Markers, StringComparer.Ordinal));
    }
    public static void Write(Stream json, MapSaveMarkerDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
public sealed record MapSaveMarkerDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, MapLabelPoint> Markers { get; init; }
}
public static class MapSaveMarkerFormat
{
    public const int Version = 1;
    public const string FileName = "map-save-markers.json";
}
