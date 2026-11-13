using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room's background image as the background shows it at one moment (#1392, #1403, #1555): the image's picture placed on
/// the background by its rectangles, and the block picture each 16x16 cell shows in the image's place where the room bound
/// other artwork there after loading.
/// </summary>
public sealed class RoomBackgroundImagePlane
{
    /// <summary>Each cell's picture, null where the image shows; the array is never written after it is shared.</summary>
    private readonly LayeredPicture?[] cells;

    /// <summary>Keeps the image's picture, the cells' pictures and how the room places the image.</summary>
    /// <param name="image">The image.</param>
    /// <param name="cells">Each cell's picture, null where the image shows.</param>
    /// <param name="placement">How the room places the image on the background.</param>
    public RoomBackgroundImagePlane(RoomBackgroundImage image, LayeredPicture?[] cells, RoomBackgroundImagePlacement placement)
    {
        Ensure.NotNull(image);
        this.cells = Ensure.NotNull(cells);
        Ensure.LengthEqual(cells, image.CellCount);
        Placement = Ensure.NotNull(placement);
        Picture = image.Picture;
        Size = image.Size;
    }

    /// <summary>How the room places the image on the background: its rectangles on an area that repeats.</summary>
    public RoomBackgroundImagePlacement Placement { get; }

    /// <summary>The area the image's rectangles lie on, which repeats across the background.</summary>
    public Extent16<BackgroundPixels> Area => Placement.Area;

    /// <summary>The image's rectangles, each where it lies on the area.</summary>
    public ImmutableArray<RoomBackgroundImagePiece> Pieces => Placement.Pieces;

    /// <summary>
    /// The picture of the whole image, as its part behind and its part in front of the actors passing in front of the
    /// room's back tiles (#1634).
    /// </summary>
    public LayeredPicture Picture { get; }

    /// <summary>The image's width and height, which its cells divide row by row.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>The image's 16x16 cells, row by row across its picture.</summary>
    public int CellCount => cells.Length;

    /// <summary>The picture cell <paramref name="cell"/> shows in place of the image, or null.</summary>
    public LayeredPicture? Cell(int cell) => cells[cell];
}
