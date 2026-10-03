using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Selected-save marker artwork anchors; neither load targets nor scroll origins are content.</summary>
/// <remarks>
/// Independently reviewed for #1165: all34 stock marker pairs in the six $82:C80B
/// views equal the native load-station map projection. Calculate stock coordinates
/// through FileSelectMapLoadAnchors and retain only independently edited X/Y fields.
/// Imported artwork changes never feed back into compiled load targets or scrolling.
/// Original JSON names, coordinate bounds and serialization remain unchanged.
/// </remarks>
public sealed class MapSaveMarkerLayout
{
    private readonly Dictionary<string, (int? X, int? Y)> coordinateOverrides;
    private MapSaveMarkerLayout(Dictionary<string, (int? X, int? Y)> coordinateOverrides) =>
        this.coordinateOverrides = coordinateOverrides;
    internal int StoredCoordinateComponentCount => coordinateOverrides.Values.Sum(point =>
        (point.X.HasValue ? 1 : 0) + (point.Y.HasValue ? 1 : 0));

    public MapLabelPoint Get(AreaId area, int index)
    {
        string id = MapSaveMarkerDefinitions.Id(area, index);
        FileSelectMapAnchor basis = FileSelectMapLoadAnchors.Get(area, index);
        var edited = coordinateOverrides.GetValueOrDefault(id);
        return new(edited.X ?? basis.X, edited.Y ?? basis.Y);
    }
    public static MapSaveMarkerLayout Load(Stream json)
    {
        MapSaveMarkerDocument document;
        try { document = JsonAssetDocument.Read<MapSaveMarkerDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Save marker layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid save marker layout JSON.", error); }
        if (document.Version != MapSaveMarkerFormat.Version || document.Markers is null ||
            document.Markers.Count != MapSaveMarkerDefinitions.AllIds().Count())
            throw new InvalidDataException("Save marker layout requires version 1 and every known marker.");
        var overrides = new Dictionary<string, (int? X, int? Y)>(StringComparer.Ordinal);
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        foreach (int index in MapSaveMarkerDefinitions.Indices((AreaId)area))
        {
            string id = MapSaveMarkerDefinitions.Id((AreaId)area, index);
            if (!document.Markers.TryGetValue(id, out var point) || point is null || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Save marker {id} requires X=0..511 and Y=0..255.");
            FileSelectMapAnchor basis = FileSelectMapLoadAnchors.Get((AreaId)area, index);
            int? x = point.X == basis.X ? null : point.X;
            int? y = point.Y == basis.Y ? null : point.Y;
            if (x.HasValue || y.HasValue) overrides.Add(id, (x, y));
        }
        return new(overrides);
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
