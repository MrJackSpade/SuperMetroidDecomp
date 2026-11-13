using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed selected-save marker drawing anchors.</summary>
public static class MapSaveMarkerFile
{
    /// <summary>Enumerates the complete stable save/elevator marker identity set the document must hold, independent of player discovery.</summary>
    /// <returns>Lazy marker keys, each a marked station's name, area by area.</returns>
    internal static IEnumerable<string> Ids() => AreaIds.Zebes.SelectMany(MapSaveMarkers.Stations).Select(MapSaveMarkers.Id);

    /// <summary>Loads all 34 supported save/elevator marker anchors, validating the complete identity set and coordinate bounds; the installed anchors are the only source (#1596).</summary>
    /// <param name="json">Caller-owned stream containing the editable save-marker JSON document.</param>
    /// <returns>A drawing-only layout with independently editable X/Y coordinates.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, marker identities, or coordinates are invalid.</exception>
    public static MapSaveMarkerPositions Read(Stream json)
    {
        MapSaveMarkerDocument document;
        try { document = JsonAssetDocument.Read<MapSaveMarkerDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Save marker layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid save marker layout JSON.", error); }
        if (document.Version != MapSaveMarkerFormat.Version || document.Markers is null ||
            document.Markers.Count != Ids().Count())
            throw new InvalidDataException($"Save marker layout requires version {MapSaveMarkerFormat.Version} and every known marker.");
        var anchors = new Dictionary<string, Position16<MapPixels>>(StringComparer.Ordinal);
        foreach (string id in Ids())
        {
            if (!document.Markers.TryGetValue(id, out var point) || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Save marker {id} requires X=0..511 and Y=0..255.");
            anchors.Add(id, point);
        }
        return new(anchors);
    }

    /// <summary>Serializes the complete marker document as UTF-8 JSON to the destination.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete editable marker layout to serialize.</param>
    public static void Write(Stream json, MapSaveMarkerDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        json.Write(bytes);
    }
}
