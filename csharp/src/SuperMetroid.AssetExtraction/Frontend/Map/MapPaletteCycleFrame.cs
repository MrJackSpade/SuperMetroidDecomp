using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>One frame of the map highlight cycle (#1637): its update-counted hold and its look, one record of the effect.</summary>
public sealed record MapPaletteCycleFrame
{
    /// <summary>Number of map-palette animation ticks to hold this frame, one through 254; not elapsed milliseconds or a native callback opcode.</summary>
    public required int DurationTicks { get; init; }
    /// <summary>The colour the highlighted map sprites' highlight shows during this frame; their outline keeps its own colour.</summary>
    public required PaletteRgb5 Highlight { get; init; }
}
