using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// How a room places its background image on the background (#1555): the image's rectangles, each where it lies, on an area
/// that repeats across the background as the room's backdrop tiles it. Nothing shows where no rectangle lies.
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

    /// <summary>The area the rectangles lie on, which repeats across the background.</summary>
    public Extent16<BackgroundPixels> Area { get; }

    /// <summary>The whole image on an area of its own size: a scrolling sky's rows, or the Tourian entrance's background (#1514).</summary>
    public static RoomBackgroundImagePlacement Whole(RoomBackgroundImage image)
    {
        Ensure.NotNull(image);
        var size = new Extent16<BackgroundPixels>(image.Size.X, image.Size.Y);
        return new(image, [new RoomBackgroundImagePiece(default, default, size)], size);
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
