using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>The colours a pre-coloured image was coloured with: which half of CGRAM, and that half's 128 colours (#1392).</summary>
/// <param name="Half">Which half of CGRAM the image's palette indices name.</param>
/// <param name="Standard">The 128 colours of that half the image was coloured with.</param>
public sealed record SpriteImagePalette(SpriteImageColours Half, ImmutableArray<Bgr555> Standard);
