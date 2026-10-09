using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Drawing positions only: moving an icon never changes its compiled discovery cell.</summary>
public sealed class MapStationLayout
{
    /// <summary>Validated screen-pixel positions indexed by stable station-marker identifier.</summary>
    private readonly Dictionary<string, MapLabelPoint> points;

    /// <summary>Stores the validated marker positions used to place map-station labels.</summary>
    /// <param name="points">Complete marker layout keyed with ordinal identifier comparison.</param>
    private MapStationLayout(Dictionary<string, MapLabelPoint> points) => this.points = points;

    /// <summary>Gets the screen-pixel drawing position for a named map-station marker.</summary>
    /// <param name="id">The stable marker identifier from the station discovery rules.</param>
    /// <returns>The marker's configured screen position.</returns>
    /// <exception cref="InvalidDataException">The marker identifier is unknown.</exception>
    public MapLabelPoint Get(string id) => points.TryGetValue(id, out var point) ? point
        : throw new InvalidDataException($"Unknown map station marker '{id}'.");

    /// <summary>Loads and validates the editable drawing layout for every known map-station marker.</summary>
    /// <param name="json">The caller-owned stream containing the layout document.</param>
    /// <returns>The validated station-marker layout.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, marker set, or coordinates are invalid.</exception>
    public static MapStationLayout Load(Stream json)
    {
        MapStationLayoutDocument document;
        try { document = JsonAssetDocument.Read<MapStationLayoutDocument>(json, MapPresentationFormat.JsonOptions)
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

    /// <summary>Validates and writes an editable map-station layout document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The station-marker layout to validate and serialize.</param>
    public static void Write(Stream json, MapStationLayoutDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable JSON schema for all map-station marker drawing positions.</summary>
public sealed record MapStationLayoutDocument
{
    /// <summary>Gets the map-station layout schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets one screen-pixel drawing position for every known station-marker identifier.</summary>
    public required Dictionary<string, MapLabelPoint> Markers { get; init; }
}

/// <summary>Names and versions the editable map-station layout asset.</summary>
public static class MapStationLayoutFormat
{
    /// <summary>The supported map-station layout schema version.</summary>
    public const int Version = 1;

    /// <summary>The embedded editable map-station layout file name.</summary>
    public const string FileName = "map-station-labels.json";
}
