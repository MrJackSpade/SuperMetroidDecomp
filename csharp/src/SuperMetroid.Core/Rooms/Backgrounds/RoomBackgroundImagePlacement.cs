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
    /// The whole image on an area of its own size: a scrolling sky's rows, or a library background, which the import pictures
    /// as one period of what it shows, laid out as its room shows it (#1651, #1617).
    /// </summary>
    public static RoomBackgroundImagePlacement Whole(RoomBackgroundImage image)
    {
        Ensure.NotNull(image);
        var size = new Extent16<BackgroundPixels>(image.Size.X, image.Size.Y);
        return new(image, [new RoomBackgroundImagePiece(default, default, size)], size);
    }
}
