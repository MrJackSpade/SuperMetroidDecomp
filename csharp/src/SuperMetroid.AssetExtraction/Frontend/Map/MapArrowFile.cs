using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed map-arrow document.</summary>
public static class MapArrowFile
{
    /// <summary>Loads and validates the four directional map-arrow visuals.</summary>
    /// <param name="json">Caller-owned stream containing the map-arrow document.</param>
    /// <returns>The validated directional visuals.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, direction set, anchors, or phase durations are invalid.</exception>
    public static MapArrowPresentation Read(Stream json)
    {
        MapArrowDocument document;
        try { document = JsonAssetDocument.Read<MapArrowDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Map arrow presentation is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map arrow JSON.", error); }
        if (document.Version != MapArrowFormat.Version || document.Arrows is null || document.Arrows.Count != MapArrowDefinitions.Count)
            throw new InvalidDataException("Map arrows require version 1 and Left/Right/Up/Down definitions.");
        return new(Require("Left"), Require("Right"), Require("Up"), Require("Down"));
        MapArrowVisual Require(string name)
        {
            if (!document.Arrows.TryGetValue(name, out var entry) || entry is null || entry.X is < 0 or > 255 || entry.Y is < 0 or > 223 ||
                entry.DurationTicks is null || entry.DurationTicks.Length is < 1 or > MapArrowFormat.MaximumPhases ||
                entry.DurationTicks.Any(ticks => ticks is < 1 or > MapArrowFormat.MaximumDuration))
                throw new InvalidDataException($"Map arrow {name} requires screen X=0..255/Y=0..223 and 1-255 phases of 1-254 ticks.");
            return new(entry.Position, entry.DurationTicks);
        }
    }

    /// <summary>Validates and writes a map-arrow presentation document as JSON.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">The document to validate and serialize.</param>
    public static void Write(Stream json, MapArrowDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
