using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// An installed picture placed on a picture layer (#1467): the point of the picture its frame names as its origin lands on
/// <see cref="At"/>, so a picture the import cut from a screen's page lands where the page shows it when placed at the
/// page's own corner.
/// </summary>
/// <param name="Picture">The picture.</param>
/// <param name="At">The layer pixel the picture's origin lands on.</param>
public readonly record struct PlacedPicture(SpriteSheetFrame Picture, Position16<TilePlanePixels> At)
{
    /// <summary>The layer pixel the picture's top-left pixel lands on; a picture reaching past the layer's top or left edge wraps.</summary>
    public Position16<TilePlanePixels> TopLeft =>
        new(unchecked((ushort)(At.X - Picture.Origin.X)), unchecked((ushort)(At.Y - Picture.Origin.Y)));
}
