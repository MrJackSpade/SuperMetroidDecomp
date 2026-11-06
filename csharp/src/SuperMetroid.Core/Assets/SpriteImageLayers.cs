namespace SuperMetroid.Core.Assets;

/// <summary>A sprite image's decoded layers, each width times height entries, row by row from the top.</summary>
/// <param name="Pixels">The pixels; a pixel with zero alpha is transparent.</param>
/// <param name="PaletteIndices">Each pixel's colour group's place times sixteen plus its colour, zero where transparent.</param>
/// <param name="FrontTiles">
/// One where the pixel lies in one of its layer's front tiles (#1514), zero elsewhere and for every pixel of an actor's
/// frame.
/// </param>
public sealed record SpriteImageLayers(RgbaPixel[] Pixels, byte[] PaletteIndices, byte[] FrontTiles);
