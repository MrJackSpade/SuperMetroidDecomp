using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What the installation holds of one sheet (#1633, #1635): its manifest and its converted pixels and maps, from which the
/// picture store takes each frame's picture.
/// </summary>
/// <param name="Manifest">The installed manifest: the image's size, the frames, the shades by colour key and the backdrop's key.</param>
/// <param name="Pixels">The installed pixels, with the recolour-region map and the blend map where the sheet needs them.</param>
internal sealed record InstalledSheet(InstalledSheetDocument Manifest, SpriteImagePixels Pixels)
{
    /// <summary>The sheet's pixels and maps with its size.</summary>
    internal InstalledImagePlanes Image => new(Pixels, Manifest.Size);

    /// <summary>
    /// Whether the game shows every pixel of each frame's rectangle, its clear margin as well, as
    /// <see cref="ComposedSpriteSheet.ShownWhole"/> says of the sheet it was composed as: the picture store then keeps each
    /// frame's whole rectangle as its picture, and otherwise the box of what the frame shows (#1646).
    /// </summary>
    internal bool ShownWhole { get; init; }
}
