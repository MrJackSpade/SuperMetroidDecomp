using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room's background image as BG2 shows it at one moment (#1392, #1403): the image's picture, and the block picture of
/// each 16x16 cell whose characters show artwork the room bound after loading.
/// </summary>
public sealed class RoomBackgroundImagePlane
{
    /// <summary>Each cell's picture, null where the image shows; the array is never written after it is shared.</summary>
    private readonly SpriteSheetFrame?[] cells;

    /// <summary>Keeps the image's picture and the cells' pictures.</summary>
    public RoomBackgroundImagePlane(RoomBackgroundImage image, SpriteSheetFrame?[] cells)
    {
        Ensure.NotNull(image);
        this.cells = Ensure.NotNull(cells);
        Ensure.LengthEqual(cells, image.CellCount);
        Picture = image.Picture;
    }

    /// <summary>The picture of the whole image, its pages or rows one above the next.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The image's 16x16 cells.</summary>
    public int CellCount => cells.Length;

    /// <summary>The picture cell <paramref name="cell"/> shows in place of the image, or null.</summary>
    public SpriteSheetFrame? Cell(int cell) => cells[cell];
}
