using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Immutable map-arrow anchors and cosmetic phase durations; no controller bindings.</summary>
public sealed class MapArrowPresentation
{
    /// <summary>Four validated direction entries, with no neutral arrow selection.</summary>
    private readonly MapArrowVisual left, right, up, down;
    /// <summary>Retains the directional visuals after JSON validation.</summary>
    private MapArrowPresentation(MapArrowVisual left, MapArrowVisual right, MapArrowVisual up, MapArrowVisual down)
    { this.left = left; this.right = right; this.up = up; this.down = down; }

    /// <summary>Selects the authored visual for a named map-scroll direction.</summary>
    public MapArrowVisual Get(MapScrollDirection direction) => direction switch
    {
        MapScrollDirection.Left => left,
        MapScrollDirection.Right => right,
        MapScrollDirection.Up => up,
        MapScrollDirection.Down => down,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
    /// <summary>Loads and validates the four directional map-arrow visuals.</summary>
    public static MapArrowPresentation Load(Stream json)
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
    public static void Write(Stream json, MapArrowDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
