using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Packs a sheet's pictures onto its image (#1392, #1513): each picture's rectangle in order, in rows no wider than
/// <see cref="MaximumWidth"/>, or, for a sheet that would then be taller than an artwork image can be, the narrowest
/// doubling of it within that bound that keeps the sheet within it too. An empty picture has no rectangle and takes no
/// room.
/// </summary>
internal static class SheetPacking
{
    /// <summary>The widest a sheet that fits is packed; a picture that would cross it starts a new row of pictures.</summary>
    private const int MaximumWidth = 1024;

    /// <summary>
    /// Packs <paramref name="pictures"/> in order, each in a rectangle of the size <paramref name="sizeOf"/> gives it, and
    /// gives the sheet's size with each picture placed at its rectangle's top-left corner.
    /// </summary>
    /// <param name="pictures">The pictures in order; one of size zero is empty and placed at the sheet's corner.</param>
    /// <param name="sizeOf">Each picture's rectangle size.</param>
    /// <param name="name">Names the pictures in the failure.</param>
    /// <exception cref="InvalidDataException">The pictures do not fit in the largest square image an artwork image can be.</exception>
    internal static PackedSheet<TPicture> Pack<TPicture>(IReadOnlyList<TPicture> pictures, Func<TPicture, Extent16<ImagePixels>> sizeOf, string name)
    {
        Ensure.NotNull(pictures);
        Ensure.NotNull(sizeOf);
        for (int rowWidth = MaximumWidth; ; rowWidth *= 2)
        {
            if (rowWidth > RgbaPng.MaximumDimension)
                throw new InvalidDataException($"{name} do not fit in a {RgbaPng.MaximumDimension}-pixel square image.");
            if (Pack(pictures, sizeOf, rowWidth) is { } packed)
                return packed;
        }
    }

    /// <summary>
    /// Packs the pictures in order in rows no wider than <paramref name="rowWidth"/>, a picture that would cross it starting
    /// a new row, and gives the sheet's size with each picture placed, or none when the rows would be taller than an artwork
    /// image can be.
    /// </summary>
    private static PackedSheet<TPicture>? Pack<TPicture>(IReadOnlyList<TPicture> pictures, Func<TPicture, Extent16<ImagePixels>> sizeOf, int rowWidth)
    {
        int width = 1, x = 0, y = 0, rowHeight = 0;
        var placed = new List<PackedPicture<TPicture>>(pictures.Count);
        foreach (TPicture picture in pictures)
        {
            Extent16<ImagePixels> size = sizeOf(picture);
            if (size.X == 0)
            {
                placed.Add(new(picture, default));
                continue;
            }
            if (x > 0 && x + size.X > rowWidth)
            {
                y += rowHeight;
                x = 0;
                rowHeight = 0;
            }
            if (y + size.Y > RgbaPng.MaximumDimension)
                return null;
            placed.Add(new(picture, new(checked((ushort)x), checked((ushort)y))));
            x += size.X;
            rowHeight = Math.Max(rowHeight, size.Y);
            width = Math.Max(width, x);
        }
        return new PackedSheet<TPicture>(new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)Math.Max(1, y + rowHeight))), placed);
    }
}
