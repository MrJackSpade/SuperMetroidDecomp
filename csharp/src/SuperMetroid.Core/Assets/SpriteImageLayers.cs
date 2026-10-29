namespace SuperMetroid.Core.Assets;

/// <summary>A sprite image's decoded layers, each width times height entries, row by row from the top.</summary>
/// <param name="Pixels">The pixels; a pixel with zero alpha is transparent.</param>
/// <param name="PaletteIndices">Each pixel's colour group's place times sixteen plus its colour, zero where transparent.</param>
/// <param name="Priorities">Each pixel's priority against the backgrounds 0..3, or a background pixel's tile priority 0..1.</param>
/// <param name="PartGroups">Each pixel's <see cref="PartColourGroup.Code"/>, as the installed image stores it.</param>
public sealed record SpriteImageLayers(RgbaPixel[] Pixels, byte[] PaletteIndices, byte[] Priorities, byte[] PartGroups);
