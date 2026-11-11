namespace SuperMetroid.Core.Assets;

/// <summary>
/// A sheet image's decoded pixels (#1633): its colours, each pixel's alpha telling whether it is clear, and the
/// purpose-named maps the sheet carries where it needs them.
/// </summary>
/// <param name="Pixels">Each pixel's colour, in reading order. A clear pixel's alpha is zero; its RGB is the colour it keeps
/// of its own, which a layer drawn opaque shows, and black where it keeps none.</param>
/// <param name="RecolourRegions">Each pixel's recolour region (<see cref="ColourKey.Region"/>), in reading order, for a sheet
/// whose pixels of one colour recolour differently under some shade; null where every pixel of a colour recolours alike.</param>
/// <param name="Blending">Whether each pixel blends with what lies behind it when drawn as a sprite, in reading order, for a
/// sheet with a picture that blends in some pixels and not others; null where every picture blends wholly or not at all
/// (<see cref="SpriteSheetFrame.Blends"/>).</param>
public sealed record SpriteImagePixels(RgbaPixel[] Pixels, int[]? RecolourRegions, bool[]? Blending);
