using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room's background image as the background shows it at one moment (#1392, #1403): the image's picture, and the block picture of
/// each 16x16 cell whose characters show artwork the room bound after loading.
/// </summary>
public sealed class RoomBackgroundImagePlane
{
    /// <summary>Each cell's picture, null where the image shows; the array is never written after it is shared.</summary>
    private readonly SpriteSheetFrame?[] cells;

    /// <summary>Keeps the image's picture, the cells' pictures and the area its pages cover.</summary>
    /// <param name="image">The image.</param>
    /// <param name="cells">Each cell's picture, null where the image shows.</param>
    /// <param name="area">
    /// The area the image's 256-pixel pages cover, left to right and then downward, before it repeats (#1514):
    /// <see cref="RoomBackgroundImage.PagesSideBySide"/> or the image's <see cref="RoomBackgroundImage.PagesInOneColumn"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not whole pages, or covers more pages than the image pictures.</exception>
    public RoomBackgroundImagePlane(RoomBackgroundImage image, SpriteSheetFrame?[] cells, Extent16<BackgroundPixels> area)
    {
        Ensure.NotNull(image);
        this.cells = Ensure.NotNull(cells);
        Ensure.LengthEqual(cells, image.CellCount);
        if (area.X == 0 || area.Y == 0 || area.X % RoomBackgroundImage.PagePixels != 0 || area.Y % RoomBackgroundImage.PagePixels != 0
            || area.X / RoomBackgroundImage.PagePixels * area.Y > image.Picture.Size.Y)
            throw new ArgumentOutOfRangeException(nameof(area), area, $"Room background {image.Name} cannot cover that area with its pages.");
        Picture = image.Picture;
        Area = area;
    }

    /// <summary>The area the image's pages cover, left to right and then downward, before it repeats.</summary>
    public Extent16<BackgroundPixels> Area { get; }

    /// <summary>The picture of the whole image, its pages or rows one above the next.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The image's 16x16 cells.</summary>
    public int CellCount => cells.Length;

    /// <summary>The picture cell <paramref name="cell"/> shows in place of the image, or null.</summary>
    public SpriteSheetFrame? Cell(int cell) => cells[cell];
}
