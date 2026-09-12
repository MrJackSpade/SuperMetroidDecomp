using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Cosmetic world-map label anchors. Area availability and navigation are not content.</summary>
public sealed class WorldMapLabelLayout
{
    private readonly MapLabelPoint[] points;
    private WorldMapLabelLayout(MapLabelPoint[] points) => this.points = points;
    public MapLabelPoint Get(int area) => (uint)area < points.Length
        ? points[area] : throw new ArgumentOutOfRangeException(nameof(area));

    public static WorldMapLabelLayout Load(Stream json)
    {
        WorldMapLabelDocument document;
        try { document = JsonSerializer.Deserialize<WorldMapLabelDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("World-map label layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid world-map label layout JSON.", error); }
        if (document.Version != WorldMapLabelFormat.Version || document.Areas is null || document.Areas.Count != 6)
            throw new InvalidDataException("World-map labels require version 1 and exactly the six Zebes areas.");
        var points = new MapLabelPoint[6];
        for (int area = 0; area < points.Length; area++)
        {
            string name = ((AreaId)area).ToString();
            if (!document.Areas.TryGetValue(name, out var point) || point is null ||
                point.X is < 1 or > 255 || point.Y is < 1 or > 223)
                throw new InvalidDataException($"World-map label {name} requires X=1..255 and Y=1..223.");
            points[area] = point;
        }
        return new(points);
    }

    public static void Write(Stream json, WorldMapLabelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record MapLabelPoint(int X, int Y);
public sealed record WorldMapLabelDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, MapLabelPoint> Areas { get; init; }
}
public static class WorldMapLabelFormat
{
    public const int Version = 1;
    public const string FileName = "world-map-labels.json";
}
