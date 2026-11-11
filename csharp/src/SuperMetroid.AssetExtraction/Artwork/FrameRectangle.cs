using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A rectangle of an installed image a frame's picture fills (#1635): on a sheet, or in the picture store.</summary>
/// <param name="Corner">The rectangle's top-left pixel.</param>
/// <param name="Size">The rectangle's width and height.</param>
internal readonly record struct FrameRectangle(Position16<ImagePixels> Corner, Extent16<ImagePixels> Size)
{
    /// <summary>Whether the rectangle holds no pixel.</summary>
    internal bool IsEmpty => Size.X == 0 || Size.Y == 0;

    /// <summary>Whether the rectangle lies wholly inside an image of <paramref name="image"/>.</summary>
    internal bool LiesInside(Extent16<ImagePixels> image) => Corner.X + Size.X <= image.X && Corner.Y + Size.Y <= image.Y;

    /// <inheritdoc />
    public override string ToString() => $"({Corner.X}, {Corner.Y}) {Size.X}x{Size.Y}";
}
