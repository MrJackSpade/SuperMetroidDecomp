using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable map-arrow anchors and cosmetic phase durations; no controller bindings.</summary>
public sealed class MapArrowPresentation
{
    private readonly MapArrowVisual[] arrows;
    private MapArrowPresentation(MapArrowVisual[] arrows) => this.arrows = arrows;
    public MapArrowVisual Get(MapScrollDirection direction)
    {
        int index = (int)direction - 1;
        return (uint)index < arrows.Length ? arrows[index] : throw new ArgumentOutOfRangeException(nameof(direction));
    }
    public static MapArrowPresentation Load(Stream json)
    {
        MapArrowDocument document;
        try { document = JsonSerializer.Deserialize<MapArrowDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map arrow presentation is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map arrow JSON.", error); }
        if (document.Version != MapArrowFormat.Version || document.Arrows is null || document.Arrows.Count != MapArrowDefinitions.Count)
            throw new InvalidDataException("Map arrows require version 1 and Left/Right/Up/Down definitions.");
        var arrows = new MapArrowVisual[MapArrowDefinitions.Count];
        for (int index = 0; index < arrows.Length; index++)
        {
            string name = ((MapScrollDirection)(index + 1)).ToString();
            if (!document.Arrows.TryGetValue(name, out var entry) || entry is null || entry.X is < 0 or > 255 || entry.Y is < 0 or > 223 ||
                entry.DurationTicks is null || entry.DurationTicks.Length is < 1 or > MapArrowFormat.MaximumPhases ||
                entry.DurationTicks.Any(ticks => ticks is < 1 or > MapArrowFormat.MaximumDuration))
                throw new InvalidDataException($"Map arrow {name} requires screen X=0..255/Y=0..223 and 1-255 phases of 1-254 ticks.");
            arrows[index] = new(entry.X, entry.Y, entry.DurationTicks.Select(ticks => (byte)ticks).ToArray());
        }
        return new(arrows);
    }
    public static void Write(Stream json, MapArrowDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
public sealed class MapArrowVisual
{
    private readonly byte[] durations;
    internal MapArrowVisual(int x, int y, byte[] durations) { X = (ushort)x; Y = (ushort)y; this.durations = durations; }
    public ushort X { get; }
    public ushort Y { get; }
    public int PhaseCount => durations.Length;
    public byte Duration(int phase) => durations[phase];
}
public sealed record MapArrowDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, MapArrowEntry> Arrows { get; init; }
}
public sealed record MapArrowEntry
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int[] DurationTicks { get; init; }
}
public static class MapArrowFormat
{
    public const int Version = 1;
    public const string FileName = "map-arrows.json";
    public const int MaximumPhases = 255;
    public const int MaximumDuration = 254;
}
