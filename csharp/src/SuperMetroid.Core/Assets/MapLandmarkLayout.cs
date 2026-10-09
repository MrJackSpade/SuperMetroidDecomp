using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Boss, elevator and gunship drawing anchors, independent of progression and destination rules.</summary>
public sealed class MapLandmarkLayout
{
    /// <summary>Stores validated area-map anchors by their ordinal, case-sensitive landmark identity.</summary>
    private readonly Dictionary<string, MapLabelPoint> points;

    /// <summary>Creates the lookup used to resolve configured landmark drawing anchors.</summary>
    /// <param name="points">Complete validated identity-to-coordinate mapping.</param>
    private MapLandmarkLayout(Dictionary<string, MapLabelPoint> points) => this.points = points;
    /// <summary>Gets a boss, elevator-label, or gunship drawing anchor in area-map pixels before the renderer subtracts map scrolling; does not determine whether the landmark is visible.</summary>
    /// <param name="id">Case-sensitive stable identity published by <see cref="MapLandmarkDefinitions.AllIds"/>.</param>
    /// <returns>The configured anchor with X in 0..511 and Y in 0..255.</returns>
    /// <exception cref="InvalidDataException">The landmark identity is not installed.</exception>
    public MapLabelPoint Get(string id) => points.TryGetValue(id, out var point) ? point
        : throw new InvalidDataException($"Unknown map landmark '{id}'.");

    /// <summary>Loads the supported JSON schema, requiring every known landmark exactly once and non-null area-map anchors bounded by X=0..511 and Y=0..255.</summary>
    /// <param name="json">Caller-owned stream containing the editable landmark layout.</param>
    /// <returns>The validated layout with ordinal, case-sensitive landmark lookup.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, marker set, or coordinates are invalid.</exception>
    public static MapLandmarkLayout Load(Stream json)
    {
        MapLandmarkDocument document;
        try { document = JsonAssetDocument.Read<MapLandmarkDocument>(json, MapPresentationFormat.JsonOptions)
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
    /// <summary>Serializes a landmark document using the map-presentation JSON options and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream for the UTF-8 JSON.</param>
    /// <param name="document">Landmark layout to validate and serialize.</param>
    public static void Write(Stream json, MapLandmarkDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
/// <summary>Editable JSON schema for boss, elevator-label, and gunship drawing anchors; native identities, slot order, progression visibility, and destination rules remain application-owned.</summary>
public sealed record MapLandmarkDocument
{
    /// <summary>Schema revision, which must equal <see cref="MapLandmarkFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>One non-null pixel anchor for every identity from <see cref="MapLandmarkDefinitions.AllIds"/>, keyed with exact casing; coordinates lie on the area-map canvas before scroll subtraction, with X=0..511 and Y=0..255.</summary>
    public required Dictionary<string, MapLabelPoint> Markers { get; init; }
}
/// <summary>Asset filename and supported JSON schema revision for editable area-map landmark anchors.</summary>
public static class MapLandmarkFormat
{
    /// <summary>Supported revision of the complete named-landmark coordinate schema.</summary>
    public const int Version = 1;
    /// <summary>Asset filename loaded by the area-map presentation catalog for landmark drawing positions.</summary>
    public const string FileName = "map-landmarks.json";
}
