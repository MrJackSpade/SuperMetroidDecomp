using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend background drawn from placed pictures (#1467): a plane of the given size in eight-pixel cells, which repeats
/// at its edges, holding installed pictures in drawing order, a later picture covering an earlier one where it is opaque.
/// A picture crossing the plane's edge continues from the opposite edge. Where no picture lies the plane shows nothing.
/// </summary>
public sealed class PicturedPlane : IEquatable<PicturedPlane>
{
    /// <summary>The largest side of a plane in cells: the sixteen-bit scroll's whole range.</summary>
    public const int MaximumCells = 8192;

    /// <summary>Keeps the plane's size and its pictures in drawing order.</summary>
    /// <param name="size">The plane's size in eight-pixel cells.</param>
    /// <param name="pictures">The placed pictures, earliest first.</param>
    /// <exception cref="ArgumentOutOfRangeException">The plane has no cells along an axis, or more than <see cref="MaximumCells"/>.</exception>
    /// <exception cref="ArgumentException">A picture's corner lies off the plane, or the picture is larger than the plane.</exception>
    public PicturedPlane(Extent16<TilemapCells> size, ImmutableArray<PlacedPlanePicture> pictures)
    {
        Ensure.BetweenInclusive(1, MaximumCells, size.X);
        Ensure.BetweenInclusive(1, MaximumCells, size.Y);
        if (pictures.IsDefault)
            throw new ArgumentException("A pictured plane needs its placements.", nameof(pictures));
        int width = size.X * ScreenGeometry.BackgroundTileSizePixels, height = size.Y * ScreenGeometry.BackgroundTileSizePixels;
        foreach (PlacedPlanePicture placed in pictures)
        {
            Extent16<ImagePixels> picture = Ensure.NotNull(placed.Picture).Size;
            if (placed.Corner.X >= width || placed.Corner.Y >= height || picture.X > width || picture.Y > height)
                throw new ArgumentException($"The picture placed at ({placed.Corner.X}, {placed.Corner.Y}) does not lie on the {size.X}x{size.Y}-cell plane.",
                    nameof(pictures));
        }
        Size = size;
        Pictures = pictures;
    }

    /// <summary>A <paramref name="size"/> plane showing nothing.</summary>
    public static PicturedPlane Empty(Extent16<TilemapCells> size) => new(size, []);

    /// <summary>The plane's size in eight-pixel cells.</summary>
    public Extent16<TilemapCells> Size { get; }

    /// <summary>The placed pictures, earliest first.</summary>
    public ImmutableArray<PlacedPlanePicture> Pictures { get; }

    /// <inheritdoc />
    public bool Equals(PicturedPlane? other) =>
        other is not null && Size == other.Size && Pictures.AsSpan().SequenceEqual(other.Pictures.AsSpan());

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PicturedPlane);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Size, Pictures.Length);
}
