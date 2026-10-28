namespace SuperMetroid.Core.Assets;

/// <summary>A sprite image's decoded layers, each width times height entries, row by row from the top.</summary>
/// <param name="Pixels">The pixels; a pixel with zero alpha is transparent.</param>
/// <param name="PaletteIndices">Each pixel's palette row times sixteen plus its colour, zero where transparent.</param>
/// <param name="Priorities">Each pixel's priority against the backgrounds 0..3, or a background pixel's tile priority 0..1.</param>
/// <param name="PartRows">Each pixel's part palette row 0..7, with <see cref="SpriteImage.FixedPartRow"/> where the part keeps its own row.</param>
public sealed record SpriteImageLayers(RgbaPixel[] Pixels, byte[] PaletteIndices, byte[] Priorities, byte[] PartRows);
