using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed boss, elevator and gunship landmark anchors.</summary>
public static class MapLandmarkFile
{
    /// <summary>Enumerates every published boss, elevator, and gunship landmark identity the document must hold.</summary>
    /// <returns>All non-reserved landmark IDs in area and slot order.</returns>
    internal static IEnumerable<string> Ids()
    {
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            foreach (string? id in MapLandmarks.Bosses(area)) if (id is not null) yield return id;
            if (area != AreaId.Ceres)
                foreach (var label in MapLandmarks.Elevators(area)) yield return label.Id;
        }
        yield return MapLandmarkDefinitions.Gunship;
    }

    /// <summary>Loads the supported JSON schema, requiring every known landmark exactly once and non-null area-map anchors bounded by X=0..511 and Y=0..255.</summary>
    /// <param name="json">Caller-owned stream containing the editable landmark layout.</param>
    /// <returns>The validated layout with ordinal, case-sensitive landmark lookup.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, marker set, or coordinates are invalid.</exception>
    public static MapLandmarkPositions Read(Stream json)
    {
        MapLandmarkDocument document;
        try { document = JsonAssetDocument.Read<MapLandmarkDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Map landmark layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map landmark layout JSON.", error); }
        var ids = Ids().ToArray();
        if (document.Version != MapLandmarkFormat.Version || document.Markers is null || document.Markers.Count != ids.Length)
            throw new InvalidDataException("Map landmarks require version 1 and every known marker.");
        foreach (string id in ids)
            if (!document.Markers.TryGetValue(id, out var point) || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Map landmark {id} requires X=0..511 and Y=0..255.");
        return new(new(document.Markers, StringComparer.Ordinal));
    }

    /// <summary>Serializes a landmark document using the map-presentation JSON options and validates it through <see cref="Read"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream for the UTF-8 JSON.</param>
    /// <param name="document">Landmark layout to validate and serialize.</param>
    public static void Write(Stream json, MapLandmarkDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
