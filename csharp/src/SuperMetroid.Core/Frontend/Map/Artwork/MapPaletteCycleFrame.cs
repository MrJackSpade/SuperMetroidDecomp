using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>One highlight phase's palette row and update-counted hold.</summary>
public sealed record MapPaletteCycleFrame
{
    /// <summary>Number of map-palette animation ticks to hold this frame, one through 254; not elapsed milliseconds or a native callback opcode.</summary>
    public required int DurationTicks { get; init; }
    /// <summary>Exactly sixteen native-precision RGB colors in destination palette-row order, including its first color.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
