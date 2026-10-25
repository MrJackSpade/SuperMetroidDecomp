using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Native-precision RGB components, not a packed hardware word.</summary>
public sealed record PaletteRgb5
{
    /// <summary>Red intensity, zero through 31, encoded in BGR555 bits zero through four by consuming presentation loaders.</summary>
    public required int Red { get; init; }
    /// <summary>Green intensity, zero through 31, encoded in BGR555 bits five through nine by consuming presentation loaders.</summary>
    public required int Green { get; init; }
    /// <summary>Blue intensity, zero through 31, encoded in BGR555 bits ten through fourteen by consuming presentation loaders; no alpha channel is represented.</summary>
    public required int Blue { get; init; }

    /// <summary>Packs the channels; each must be 0-31.</summary>
    public RgbColor ToRgbColor() => new(Red, Green, Blue);

    /// <summary>The JSON channels of <paramref name="color"/>.</summary>
    public static PaletteRgb5 From(RgbColor color) => new() { Red = color.Red, Green = color.Green, Blue = color.Blue };
}
