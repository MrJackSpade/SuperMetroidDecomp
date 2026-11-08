namespace SuperMetroid.Core.Assets;

/// <summary>A sprite image's decoded layers, each width times height entries, row by row from the top.</summary>
/// <param name="Pixels">The pre-coloured pixels; a pixel with zero alpha is transparent.</param>
/// <param name="Inks">
/// What each pixel is beside its colour (#1544): the place of the colour it shows among the image's own colours, whether it
/// is clear, whether it blends, and whether it lies in one of its layer's front tiles (#1514).
/// </param>
public sealed record SpriteImageLayers(RgbaPixel[] Pixels, SpriteImageInk[] Inks);
