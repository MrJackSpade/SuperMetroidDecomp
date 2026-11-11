using System.Runtime.InteropServices;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What a frame rectangle of an installed image shows (#1635), the identity of one picture: its RGBA, the RGB under alpha
/// zero included, with each pixel's recolour region and blending. Two are the same picture only when their sizes and every
/// pixel match, compared pixel by pixel until the first that differs; a cheap hash of the colours only narrows which are
/// compared. The content is read in place from the image it lies on, which must not change while it is compared.
/// </summary>
internal sealed class InstalledPictureContent : IEquatable<InstalledPictureContent>
{
    /// <summary>The image the picture lies on.</summary>
    private readonly InstalledImagePlanes image;

    /// <summary>A cheap hash of the size and colours.</summary>
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
            code.AddBytes(MemoryMarshal.AsBytes(Row(image.Planes.Pixels, row)));
        hash = code.ToHashCode();
    }

    /// <summary>Where the picture lies on its image.</summary>
    internal FrameRectangle Rectangle { get; }

    /// <summary>
    /// Whether the picture shows nothing at all: every pixel clear black, in recolour region zero and not blending, as an
    /// image assembled from the store starts. Such a picture is not stored.
    /// </summary>
    internal bool IsClear => image.IsClear(Rectangle);

    /// <summary>Whether a pixel of the picture lies in a recolour region other than zero.</summary>
    internal bool HasRecolourRegions => image.HasRecolourRegions(Rectangle);

    /// <summary>Whether a pixel of the picture blends, as a blend map says.</summary>
    internal bool HasBlending => image.HasBlending(Rectangle);

    /// <summary>Copies the picture into <paramref name="target"/> with its top-left pixel at <paramref name="corner"/>.</summary>
    /// <exception cref="ArgumentException">The picture reaches outside the target.</exception>
    /// <exception cref="InvalidOperationException">The picture holds a recolour region or blending the target has no map for.</exception>
    internal void CopyTo(InstalledImagePlanes target, Position16<ImagePixels> corner) =>
        image.CopyTo(Rectangle, target, corner);

    /// <summary>Row <paramref name="row"/> of <paramref name="plane"/> within the picture; empty for a missing plane.</summary>
    private Span<T> Row<T>(T[]? plane, int row) => plane is null ? Span<T>.Empty
        : plane.AsSpan(((Rectangle.Corner.Y + row) * image.Size.X) + Rectangle.Corner.X, Rectangle.Size.X);

    /// <summary>Whether both hold the same picture, compared pixel by pixel until the first difference.</summary>
    public bool Equals(InstalledPictureContent? other)
    {
        if (other is null || other.Rectangle.Size != Rectangle.Size)
            return false;
        if (ReferenceEquals(other, this))
            return true;
        SpriteImagePixels mine = image.Planes, theirs = other.image.Planes;
        for (int row = 0; row < Rectangle.Size.Y; row++)
        {
            if (!Row(mine.Pixels, row).SequenceEqual(other.Row(theirs.Pixels, row))
                || !SameMapRow(Row(mine.RecolourRegions, row), other.Row(theirs.RecolourRegions, row))
                || !SameMapRow(Row(mine.Blending, row), other.Row(theirs.Blending, row)))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Whether two rows of a map hold the same values, a row of a missing map (empty) holding the default value throughout.
    /// </summary>
    private static bool SameMapRow<T>(Span<T> mine, Span<T> theirs)
        where T : struct, IEquatable<T>
    {
        if (mine.IsEmpty)
            return !theirs.ContainsAnyExcept(default(T));
        if (theirs.IsEmpty)
            return !mine.ContainsAnyExcept(default(T));
        return mine.SequenceEqual(theirs);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is InstalledPictureContent other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => hash;
}
