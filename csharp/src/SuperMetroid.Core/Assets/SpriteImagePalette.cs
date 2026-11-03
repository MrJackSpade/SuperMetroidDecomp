using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>The colours a pre-coloured image was coloured with: which half of the colour table, and that half's 128 colours (#1392).</summary>
/// <param name="Half">Which half of the colour table the image's palette indices name.</param>
/// <param name="Standard">The 128 colours of that half the image was coloured with.</param>
public sealed record SpriteImagePalette(SpriteImageColours Half, ImmutableArray<RgbColor> Standard)
{
    /// <summary>The shades the import pre-coloured the image in besides its standard colours, by name (#1512).</summary>
    public ImmutableDictionary<PictureShade, ShadeInks> Shades { get; init; } = ImmutableDictionary<PictureShade, ShadeInks>.Empty;
}
