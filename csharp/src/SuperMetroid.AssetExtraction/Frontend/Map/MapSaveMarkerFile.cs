using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed selected-save marker drawing anchors.</summary>
public static class MapSaveMarkerFile
{
    /// <summary>Enumerates the complete stable save/elevator marker identity set the document must hold, independent of player discovery.</summary>
    /// <returns>Lazy marker keys in area-ID order and ascending usable station-slot order.</returns>
    internal static IEnumerable<string> Ids()
    {
        foreach (AreaId area in AreaIds.Zebes)
            foreach (int index in MapSaveMarkers.Indices(area)) yield return MapSaveMarkers.Id(area, index);
    }

    /// <summary>Loads all 34 supported save/elevator marker anchors, validates the complete identity set and coordinate bounds, and retains only coordinates differing from compiled station projections.</summary>
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
            throw new InvalidDataException("Save marker layout requires version 1 and every known marker.");
        var overrides = new Dictionary<string, AnchorOverride>(StringComparer.Ordinal);
        foreach (AreaId area in AreaIds.Zebes)
        foreach (int index in MapSaveMarkers.Indices(area))
        {
            string id = MapSaveMarkers.Id(area, index);
            if (!document.Markers.TryGetValue(id, out var point) || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Save marker {id} requires X=0..511 and Y=0..255.");
            Position16<MapPixels> basis = FileSelectMapLoadAnchors.Get(area, index);
            int? x = point.X == basis.X ? null : point.X;
            int? y = point.Y == basis.Y ? null : point.Y;
            if (x.HasValue || y.HasValue) overrides.Add(id, new AnchorOverride(x, y));
        }
        return new(overrides);
    }

    /// <summary>Serializes the complete marker document as UTF-8 JSON and validates it through <see cref="Read"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete editable marker layout to validate and serialize.</param>
    public static void Write(Stream json, MapSaveMarkerDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
