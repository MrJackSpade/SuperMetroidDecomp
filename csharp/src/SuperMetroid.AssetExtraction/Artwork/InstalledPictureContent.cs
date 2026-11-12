using System.Runtime.InteropServices;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What a frame rectangle of an installed image shows (#1635, #1644), the identity of one stored picture: its RGBA pixels
/// only, the RGB under alpha zero included. The recolour regions and blending the rectangle is shown with are not part of
/// it: they are maps of the reference that shows the picture (<see cref="PictureMap{T}"/>), so rectangles whose pixels match
/// are one picture whatever maps each is shown with. Two are the same picture only when their sizes and every pixel match,
/// compared pixel by pixel until the first that differs; a cheap hash narrows which are compared. The content is read in
/// place from the image it lies on, which must not change while it is compared.
/// </summary>
internal sealed class InstalledPictureContent : IEquatable<InstalledPictureContent>
{
    /// <summary>The image the picture lies on.</summary>
    private readonly InstalledImagePlanes image;

    /// <summary>A cheap hash of the size and pixels.</summary>
    private readonly int hash;

    /// <summary>The picture lying in <paramref name="rectangle"/> of <paramref name="image"/>.</summary>
    /// <exception cref="InvalidDataException">The rectangle reaches outside the image.</exception>
    internal InstalledPictureContent(InstalledImagePlanes image, FrameRectangle rectangle)
    {
        Ensure.NotNull(image.Planes);
        if (!rectangle.LiesInside(image.Size))
            throw new InvalidDataException($"Frame rectangle {rectangle} reaches outside its {image.Size.X}x{image.Size.Y} sheet.");
        this.image = image;
        Rectangle = rectangle;
        var code = new HashCode();
        code.Add(rectangle.Size);
        for (int row = 0; row < rectangle.Size.Y; row++)
            code.AddBytes(MemoryMarshal.AsBytes(Row(row)));
        hash = code.ToHashCode();
    }

    /// <summary>Where the picture lies on its image.</summary>
    internal FrameRectangle Rectangle { get; }

    /// <summary>
    /// Whether the picture shows no pixel: every pixel clear black, as an image assembled from the store starts. Such a
    /// picture is not stored.
    /// </summary>
    internal bool IsClear => image.ShowsNoPixel(Rectangle);

    /// <summary>The recolour regions the rectangle is shown with, or null where every pixel lies in region zero.</summary>
    internal PictureMap<int>? RecolourRegions => PictureMap<int>.Within(image.Planes.RecolourRegions, image.Size, Rectangle);

    /// <summary>The blending the rectangle is shown with, or null where no pixel blends by a blend map.</summary>
    internal PictureMap<bool>? Blending => PictureMap<bool>.Within(image.Planes.Blending, image.Size, Rectangle);

    /// <summary>Copies the picture's pixels into <paramref name="target"/> with its top-left pixel at <paramref name="corner"/>.</summary>
    /// <exception cref="ArgumentException">The picture reaches outside the target.</exception>
    internal void CopyPixelsTo(InstalledImagePlanes target, Position16<ImagePixels> corner) =>
        image.CopyPixelsTo(Rectangle, target, corner);

    /// <summary>Row <paramref name="row"/> of the picture's pixels.</summary>
    private Span<RgbaPixel> Row(int row) =>
        image.Planes.Pixels.AsSpan(((Rectangle.Corner.Y + row) * image.Size.X) + Rectangle.Corner.X, Rectangle.Size.X);

    /// <summary>Whether both hold the same pixels, compared pixel by pixel until the first difference.</summary>
    public bool Equals(InstalledPictureContent? other)
    {
        if (other is null || other.Rectangle.Size != Rectangle.Size)
            return false;
        if (ReferenceEquals(other, this))
            return true;
        for (int row = 0; row < Rectangle.Size.Y; row++)
        {
            if (!Row(row).SequenceEqual(other.Row(row)))
                return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is InstalledPictureContent other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => hash;
}
