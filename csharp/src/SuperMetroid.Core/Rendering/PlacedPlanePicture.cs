using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>An installed picture placed on a pictured plane (#1467).</summary>
/// <param name="Picture">The picture, pre-coloured under the palette its screen shows it in, each pixel in its own priority tier.</param>
/// <param name="Corner">The plane pixel the picture's upper-left corner sits on.</param>
public readonly record struct PlacedPlanePicture(SpriteSheetFrame Picture, Position16<TilePlanePixels> Corner);
