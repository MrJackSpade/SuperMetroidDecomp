using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable map-arrow anchors and cosmetic phase durations; no controller bindings.</summary>
public sealed class MapArrowPresentation
{
    /// <summary>Cached visuals for Left, Right, Up, and Down, selected by the matching <see cref="MapScrollDirection"/>.</summary>
    private readonly MapArrowVisual left, right, up, down;

    /// <summary>Creates a presentation with one validated visual for each map-scroll direction.</summary>
    /// <param name="left">Visual returned for leftward map scrolling.</param>
    /// <param name="right">Visual returned for rightward map scrolling.</param>
    /// <param name="up">Visual returned for upward map scrolling.</param>
    /// <param name="down">Visual returned for downward map scrolling.</param>
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
    /// <summary>Validates and writes a map-arrow presentation document as JSON.</summary>
    public static void Write(Stream json, MapArrowDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
/// <summary>Provides one map arrow's screen anchor and cyclic cosmetic phase timing.</summary>
public sealed class MapArrowVisual
{
    /// <summary>Per-phase delays that differ from the native menu-selector timing defaults.</summary>
    private readonly Dictionary<int, byte>? durationOverrides;

    /// <summary>Creates an arrow visual from its screen anchor and validated phase durations.</summary>
    /// <param name="x">Horizontal screen-pixel coordinate of the arrow anchor.</param>
    /// <param name="y">Vertical screen-pixel coordinate of the arrow anchor.</param>
    /// <param name="durations">Positive update-tick duration for each phase in the visual's cycle.</param>
    internal MapArrowVisual(int x, int y, int[] durations)
    {
        X = (ushort)x;
        Y = (ushort)y;
        PhaseCount = durations.Length;
        for (int phase = 0; phase < durations.Length; phase++)
            if (durations[phase] != MenuSelectorTiming.Duration(phase))
                (durationOverrides ??= new()).Add(phase, (byte)durations[phase]);
    }
    /// <summary>Gets the arrow's horizontal screen-pixel anchor.</summary>
    public ushort X { get; }
    /// <summary>Gets the arrow's vertical screen-pixel anchor.</summary>
    public ushort Y { get; }
    /// <summary>Gets the number of authored animation phases.</summary>
    public int PhaseCount { get; }

    /// <summary>Calculates native arrow timing, with independent authored phase edits.</summary>
    /// <remarks>$82:C137 holds the initial phase for15 ticks and every following
    /// phase for2. The original14-phase cycle repeats this rule without a cache.
    /// Custom cycle lengths and individual delays remain document-owned inputs.</remarks>
    public byte Duration(int phase)
    {
        if ((uint)phase >= (uint)PhaseCount) throw new IndexOutOfRangeException();
        return durationOverrides is not null && durationOverrides.TryGetValue(phase, out byte value)
            ? value : MenuSelectorTiming.Duration(phase);
    }
}
/// <summary>Defines the four directional map-arrow visuals.</summary>
public sealed record MapArrowDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the exact Left, Right, Up, and Down arrow entries.</summary>
    public required Dictionary<string, MapArrowEntry> Arrows { get; init; }
}
/// <summary>Defines one arrow's screen anchor and per-phase durations.</summary>
public sealed record MapArrowEntry
{
    /// <summary>Gets the horizontal screen-pixel anchor from zero through 255.</summary>
    public required int X { get; init; }
    /// <summary>Gets the vertical screen-pixel anchor from zero through 223.</summary>
    public required int Y { get; init; }
    /// <summary>Gets one positive update-tick duration per cyclic animation phase.</summary>
    public required int[] DurationTicks { get; init; }
}
/// <summary>Defines the map-arrow document identity and bounded timing limits.</summary>
public static class MapArrowFormat
{
    /// <summary>Supported map-arrow document schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON filename containing directional map-arrow visuals.</summary>
    public const string FileName = "map-arrows.json";
    /// <summary>Largest supported number of cyclic animation phases.</summary>
    public const int MaximumPhases = 255;
    /// <summary>Largest supported duration, in update ticks, for one phase.</summary>
    public const int MaximumDuration = 254;
}
