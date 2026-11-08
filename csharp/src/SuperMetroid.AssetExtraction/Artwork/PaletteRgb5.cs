using SuperMetroid.Core.Assets;

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

    /// <summary>The plain colour of these levels; each must be 0-31.</summary>
    public RgbColor ToRgbColor() => new FadeLevelColour(Red, Green, Blue).ToColour();

    /// <summary>The JSON levels of <paramref name="color"/>: its own, or its nearest for a colour a plain blend made (<see cref="FadeLevelColour.Nearest"/>).</summary>
    public static PaletteRgb5 From(RgbColor color)
    {
        FadeLevelColour levels = FadeLevelColour.Nearest(color);
        return new() { Red = levels.Red, Green = levels.Green, Blue = levels.Blue };
    }
}
