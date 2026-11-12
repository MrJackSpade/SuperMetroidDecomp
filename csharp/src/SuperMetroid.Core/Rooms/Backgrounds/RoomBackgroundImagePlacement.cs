using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// How a room places its background image on the background (#1555): the image's rectangles, each where it lies, on an area
/// that repeats across the background as the room's backdrop tiles it. Nothing shows where no rectangle lies. The area is
/// the background's repeat (#1651): what the background shows repeats across with the area's width and down with its
/// height, so an image that is one period of the background lies whole on an area of its own size.
/// </summary>
public sealed class RoomBackgroundImagePlacement
{
    /// <summary>Checks every rectangle lies inside the area and shows part of the image.</summary>
    /// <exception cref="ArgumentException">A rectangle is empty, lies past the area or shows past the image.</exception>
    private RoomBackgroundImagePlacement(RoomBackgroundImage image, ImmutableArray<RoomBackgroundImagePiece> pieces, Extent16<BackgroundPixels> area)
    {
        if (area.X == 0 || area.Y == 0)
            throw new ArgumentException($"Room background {image.Name} is placed on an empty area.", nameof(area));
        foreach (RoomBackgroundImagePiece piece in pieces)
        {
            if (piece.Size.X == 0 || piece.Size.Y == 0
                || piece.Corner.X + piece.Size.X > area.X || piece.Corner.Y + piece.Size.Y > area.Y
                || piece.Source.X + piece.Size.X > image.Size.X || piece.Source.Y + piece.Size.Y > image.Size.Y)
                throw new ArgumentException($"Room background {image.Name} places a rectangle outside its area or its picture.", nameof(pieces));
        }
        Pieces = pieces;
        Area = area;
    }

    /// <summary>The image's rectangles, each where it lies on the area.</summary>
    public ImmutableArray<RoomBackgroundImagePiece> Pieces { get; }

    /// <summary>
    /// The area the rectangles lie on, which repeats across the background: the period of what the background shows, across
    /// and down (#1651).
    /// </summary>
    public Extent16<BackgroundPixels> Area { get; }

    /// <summary>
    /// The whole image on an area of its own size: a scrolling sky's rows, the Tourian entrance's background (#1514), or a
    /// library background that is one period of what it shows (#1651).
    /// </summary>
    public static RoomBackgroundImagePlacement Whole(RoomBackgroundImage image)
    {
        Ensure.NotNull(image);
        var size = new Extent16<BackgroundPixels>(image.Size.X, image.Size.Y);
        return new(image, [new RoomBackgroundImagePiece(default, default, size)], size);
    }

    /// <summary>
    /// How a room places its library background's image (#1514, #1651). An image no taller than it is wide is one period of
    /// what the background shows, one page or the rows of one page down to their period: it lies whole on an area of its
    /// own size, which repeats across and down, so the period is the image's own width and height. An image twice as tall
    /// as it is wide holds the background's two different halves: one above the other where
    /// <paramref name="halvesStacked"/>, as the Tourian entrance's statues descend through them, and side by side in every
    /// other room.
    /// </summary>
    /// <param name="image">The library background's image.</param>
    /// <param name="halvesStacked">Whether the room shows the background's two halves one above the other.</param>
    /// <exception cref="ArgumentException">The image is neither one period nor two halves.</exception>
    public static RoomBackgroundImagePlacement OfLibrary(RoomBackgroundImage image, bool halvesStacked)
    {
        Ensure.NotNull(image);
        if (image.Size.Y <= image.Size.X)
            return Whole(image);
        if (image.Size.Y != 2 * image.Size.X)
            throw new ArgumentException(
                $"Room background {image.Name} is {image.Size.X}x{image.Size.Y}: neither one period, no taller than it is wide, nor two halves, twice as tall.", nameof(image));
        return halvesStacked ? Whole(image) : HalvesSideBySide(image);
    }

    /// <summary>
    /// A library background's two halves, the image's upper half on the left and its lower half on the right, as every
    /// room but the Tourian entrance shows them (#1514).
    /// </summary>
    /// <exception cref="ArgumentException">The image's height does not split into two halves of whole cells.</exception>
    public static RoomBackgroundImagePlacement HalvesSideBySide(RoomBackgroundImage image)
    {
        Ensure.NotNull(image);
        int width = image.Size.X, half = image.Size.Y / 2;
        if (half == 0 || half % RoomBackgroundImage.CellPixels != 0)
            throw new ArgumentException($"Room background {image.Name} has no two halves of whole cells.", nameof(image));
        var size = new Extent16<BackgroundPixels>(width, half);
        return new(image,
        [
            new RoomBackgroundImagePiece(default, default, size),
            new RoomBackgroundImagePiece(new Position16<BackgroundPixels>(width, 0), new Position16<ImagePixels>(0, half), size),
        ], new Extent16<BackgroundPixels>(2 * width, half));
    }
}
