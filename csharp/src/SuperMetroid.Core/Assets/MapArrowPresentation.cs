using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable map-arrow anchors and cosmetic phase durations; no controller bindings.</summary>
public sealed class MapArrowPresentation
{
    private readonly MapArrowVisual left, right, up, down;
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
    public static MapArrowPresentation Load(Stream json)
    {
        MapArrowDocument document;
        try { document = JsonAssetDocument.Read<MapArrowDocument>(json, MapPresentationFormat.JsonOptions)
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
            return new(entry.X, entry.Y, entry.DurationTicks);
        }
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
    private readonly Dictionary<int, byte>? durationOverrides;
    internal MapArrowVisual(int x, int y, int[] durations)
    {
        X = (ushort)x;
        Y = (ushort)y;
        PhaseCount = durations.Length;
        for (int phase = 0; phase < durations.Length; phase++)
            if (durations[phase] != BaseDuration(phase))
                (durationOverrides ??= new()).Add(phase, (byte)durations[phase]);
    }
    public ushort X { get; }
    public ushort Y { get; }
    public int PhaseCount { get; }
    internal int StoredDurationCount => durationOverrides?.Count ?? 0;

    /// <summary>Calculates native arrow timing, with independent authored phase edits.</summary>
    /// <remarks>$82:C137 holds the initial phase for15 ticks and every following
    /// phase for2. The original14-phase cycle repeats this rule without a cache.
    /// Custom cycle lengths and individual delays remain document-owned inputs.</remarks>
    public byte Duration(int phase)
    {
        if ((uint)phase >= (uint)PhaseCount) throw new IndexOutOfRangeException();
        return durationOverrides is not null && durationOverrides.TryGetValue(phase, out byte value)
            ? value : BaseDuration(phase);
    }
    private static byte BaseDuration(int phase) => phase == 0 ? (byte)15 : (byte)2;
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
