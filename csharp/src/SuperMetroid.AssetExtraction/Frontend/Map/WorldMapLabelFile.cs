using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed world-map label anchors.</summary>
public static class WorldMapLabelFile
{
    /// <summary>Loads and validates label anchors for exactly the six Zebes world-map areas.</summary>
    /// <param name="json">Caller-owned stream containing a world-map label document.</param>
    /// <returns>The compiled cosmetic label layout.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, area set, or anchors are invalid.</exception>
    public static WorldMapLabelPositions Read(Stream json)
    {
        WorldMapLabelDocument document;
        try { document = JsonAssetDocument.Read<WorldMapLabelDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("World-map label layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid world-map label layout JSON.", error); }
        if (document.Version != WorldMapLabelFormat.Version || document.Areas is null || document.Areas.Count != 6)
            throw new InvalidDataException("World-map labels require version 1 and exactly the six Zebes areas.");
        Position16<SpritePixels> RequireArea(AreaId area)
        {
            string name = area.ToString();
            if (!document.Areas.TryGetValue(name, out var point) ||
                point.X is < 1 or > 255 || point.Y is < 1 or > 223)
                throw new InvalidDataException($"World-map label {name} requires X=1..255 and Y=1..223.");
            return point;
        }
        return new(RequireArea(AreaId.Crateria), RequireArea(AreaId.Brinstar), RequireArea(AreaId.Norfair),
            RequireArea(AreaId.WreckedShip), RequireArea(AreaId.Maridia), RequireArea(AreaId.Tourian));
    }

    /// <summary>Validates and writes a world-map label document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, WorldMapLabelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
