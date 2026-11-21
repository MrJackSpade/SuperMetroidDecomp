using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed map-arrow document.</summary>
public static class MapArrowFile
{
    /// <summary>Loads and validates the four directional map-arrow visuals.</summary>
    /// <param name="json">Caller-owned stream containing the map-arrow document.</param>
    /// <returns>The validated directional visuals.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, direction set or anchors are invalid.</exception>
    public static MapArrowPresentation Read(Stream json)
    {
        MapArrowDocument document;
        try { document = JsonAssetDocument.Read<MapArrowDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Map arrow presentation is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map arrow JSON.", error); }
        if (document.Version != MapArrowFormat.Version || document.Arrows is null || document.Arrows.Count != MapScrollControls.Directions.Length)
            throw new InvalidDataException($"Map arrows require version {MapArrowFormat.Version} and Left/Right/Up/Down definitions.");
        return new(Require(MapScrollDirection.Left), Require(MapScrollDirection.Right), Require(MapScrollDirection.Up),
            Require(MapScrollDirection.Down));
        MapArrowVisual Require(MapScrollDirection direction)
        {
            string name = MapScrollDirectionKeys.Installed.Of(direction);
            if (!document.Arrows.TryGetValue(name, out var entry) || entry is null || entry.X is < 0 or > 255 || entry.Y is < 0 or > 223)
                throw new InvalidDataException($"Map arrow {name} requires screen X=0..255/Y=0..223.");
            return new(entry.Position);
        }
    }

    /// <summary>Writes a map-arrow presentation document as JSON.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">The document to serialize.</param>
    public static void Write(Stream json, MapArrowDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Frontend.Map.Artwork.MapArrowDocument>(ArtworkJson.Options));
        json.Write(bytes);
    }
}
