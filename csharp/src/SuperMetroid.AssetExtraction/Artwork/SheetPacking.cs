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
    /// Packs rectangles of <paramref name="sizes"/> in order, writing each one's top-left corner into
    /// <paramref name="corners"/>, and gives the sheet's size.
    /// </summary>
    /// <param name="sizes">Each picture's rectangle size; zero for an empty picture.</param>
    /// <param name="corners">Receives each picture's top-left corner on the sheet.</param>
    /// <param name="pictures">Names the pictures in the failure.</param>
    /// <exception cref="InvalidDataException">The pictures do not fit in the largest square image an artwork image can be.</exception>
    internal static Extent16<ImagePixels> Pack(Extent16<ImagePixels>[] sizes, Position16<ImagePixels>[] corners, string pictures)
    {
        for (int rowWidth = MaximumWidth; ; rowWidth *= 2)
        {
            if (rowWidth > RgbaPng.MaximumDimension)
                throw new InvalidDataException($"{pictures} do not fit in a {RgbaPng.MaximumDimension}-pixel square image.");
            if (Pack(sizes, corners, rowWidth) is { } packed)
                return packed;
        }
    }

    /// <summary>
    /// Packs the rectangles in order in rows no wider than <paramref name="rowWidth"/>, a rectangle that would cross it
    /// starting a new row, writing each one's corner, and gives the sheet's size, or none when the rows would be taller
    /// than an artwork image can be.
    /// </summary>
    private static Extent16<ImagePixels>? Pack(Extent16<ImagePixels>[] sizes, Position16<ImagePixels>[] corners, int rowWidth)
    {
        int width = 1, x = 0, y = 0, rowHeight = 0;
        for (int index = 0; index < sizes.Length; index++)
        {
            Extent16<ImagePixels> size = sizes[index];
            if (size.X == 0)
                continue;
            if (x > 0 && x + size.X > rowWidth)
            {
                y += rowHeight;
                x = 0;
                rowHeight = 0;
            }
            if (y + size.Y > RgbaPng.MaximumDimension)
                return null;
            corners[index] = new(checked((ushort)x), checked((ushort)y));
            x += size.X;
            rowHeight = Math.Max(rowHeight, size.Y);
            width = Math.Max(width, x);
        }
        return new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)Math.Max(1, y + rowHeight)));
    }
}
