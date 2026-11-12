using System.Runtime.InteropServices;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One purpose-named map a stored picture is shown with (#1644): a value for each pixel of the picture, in reading order,
/// such as the recolour region each pixel lies in (<see cref="RecolourRegionMapFile"/>) or whether each pixel blends
/// (<see cref="BlendMapFile"/>). A map is not part of the picture: the reference that shows the picture names the map it
/// shows it with, so references to one picture may name different maps, and references with the same map share it. Two
/// maps are the same map when their sizes and every value match.
/// </summary>
/// <typeparam name="T">The value each pixel holds.</typeparam>
internal sealed class PictureMap<T> : IEquatable<PictureMap<T>>
    where T : unmanaged, IEquatable<T>
{
    /// <summary>Each pixel's value, in reading order.</summary>
    private readonly T[] values;

    /// <summary>A cheap hash of the size and values.</summary>
    private readonly int hash;

    /// <summary>The map of a picture of <paramref name="size"/> whose pixels hold <paramref name="values"/>, in reading order.</summary>
    /// <exception cref="ArgumentException">The values are not one per pixel.</exception>
    internal PictureMap(Extent16<ImagePixels> size, T[] values)
    {
        Ensure.LengthEqual(Ensure.NotNull(values), size.X * size.Y);
        Size = size;
        this.values = values;
        var code = new HashCode();
        code.Add(size);
        code.AddBytes(MemoryMarshal.AsBytes(values.AsSpan()));
        hash = code.ToHashCode();
    }

    /// <summary>The width and height of the picture the map is for.</summary>
    internal Extent16<ImagePixels> Size { get; }

    /// <summary>Each pixel's value, in reading order.</summary>
    internal ReadOnlySpan<T> Values => values;

    /// <summary>
    /// The map <paramref name="rectangle"/> of an image of <paramref name="imageSize"/> pixels shows with, read from
    /// <paramref name="plane"/>, the image's whole map; null where the image has no such map or every pixel of the rectangle
    /// holds the default value, which a picture shown with no map holds throughout.
    /// </summary>
    /// <exception cref="ArgumentException">The plane is not one value per pixel of the image, or the rectangle reaches outside it.</exception>
    internal static PictureMap<T>? Within(T[]? plane, Extent16<ImagePixels> imageSize, FrameRectangle rectangle)
    {
        if (plane is null)
            return null;
        Ensure.LengthEqual(plane, imageSize.X * imageSize.Y);
        if (!rectangle.LiesInside(imageSize))
            throw new ArgumentException($"Map rectangle {rectangle} reaches outside its {imageSize.X}x{imageSize.Y} image.", nameof(rectangle));
        int width = rectangle.Size.X;
        var values = new T[width * rectangle.Size.Y];
        bool shown = false;
        for (int row = 0; row < rectangle.Size.Y; row++)
        {
            ReadOnlySpan<T> line = plane.AsSpan(((rectangle.Corner.Y + row) * imageSize.X) + rectangle.Corner.X, width);
            shown |= line.ContainsAnyExcept(default(T));
            line.CopyTo(values.AsSpan(row * width, width));
        }
        return shown ? new PictureMap<T>(rectangle.Size, values) : null;
    }

    /// <summary>
    /// Writes the map into <paramref name="rectangle"/> of <paramref name="plane"/>, the whole map of an image of
    /// <paramref name="imageSize"/> pixels.
    /// </summary>
    /// <exception cref="ArgumentException">The plane is not one value per pixel of the image, or the rectangle is not the map's size or reaches outside the image.</exception>
    internal void CopyTo(T[] plane, Extent16<ImagePixels> imageSize, FrameRectangle rectangle)
    {
        Ensure.LengthEqual(Ensure.NotNull(plane), imageSize.X * imageSize.Y);
        if (rectangle.Size != Size)
            throw new ArgumentException($"A {Size.X}x{Size.Y} map is written to rectangle {rectangle} of another size.", nameof(rectangle));
        if (!rectangle.LiesInside(imageSize))
            throw new ArgumentException($"Map rectangle {rectangle} reaches outside its {imageSize.X}x{imageSize.Y} image.", nameof(rectangle));
        int width = Size.X;
        for (int row = 0; row < Size.Y; row++)
            values.AsSpan(row * width, width).CopyTo(plane.AsSpan(((rectangle.Corner.Y + row) * imageSize.X) + rectangle.Corner.X, width));
    }

    /// <summary>Whether both are the same map, value for value.</summary>
    public bool Equals(PictureMap<T>? other) =>
        other is not null && (ReferenceEquals(other, this) || (other.Size == Size && other.values.AsSpan().SequenceEqual(values)));

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PictureMap<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => hash;
}
