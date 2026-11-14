using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed map-station marker drawing layout.</summary>
public static class MapStationLayoutFile
{
    /// <summary>The station families in the order the layout lists them.</summary>
    private static readonly MapStationKind[] KindOrder = [MapStationKind.Missile, MapStationKind.Energy, MapStationKind.Map];

    /// <summary>Every compiled station marker in missile, energy, then map order: the complete marker set an editable layout must hold.</summary>
    internal static IEnumerable<MapStationDiscoveryRule> Markers =>
        KindOrder.SelectMany(kind => AreaIds.All.SelectMany(area => MapStationDiscoveryRules.Get(area, kind)));

    /// <summary>Loads and validates the editable drawing layout for every known map-station marker.</summary>
    /// <param name="json">The caller-owned stream containing the layout document.</param>
    /// <returns>The validated station-marker layout.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, marker set, or coordinates are invalid.</exception>
    public static MapStationPositions Read(Stream json)
    {
        MapStationLayoutDocument document;
        try { document = JsonAssetDocument.Read<MapStationLayoutDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Map station layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map station layout JSON.", error); }
        var rules = Markers.ToArray();
        if (document.Version != MapStationLayoutFormat.Version || document.Markers is null || document.Markers.Count != rules.Length)
            throw new InvalidDataException("Map station layout requires version 1 and every known marker exactly once.");
        foreach (var rule in rules)
            if (!document.Markers.TryGetValue(rule.Id, out var point) || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Map station {rule.Id} requires X=0..511 and Y=0..255.");
        return new(new(document.Markers, StringComparer.Ordinal));
    }

    /// <summary>Writes an editable map-station layout document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The station-marker layout to serialize.</param>
    public static void Write(Stream json, MapStationLayoutDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        json.Write(bytes);
    }
}
