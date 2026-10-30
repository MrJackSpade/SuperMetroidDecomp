using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>An installed picture placed on a pictured plane (#1467).</summary>
/// <param name="Picture">The picture, pre-coloured under the palette its screen shows it in, each pixel in its own priority tier.</param>
/// <param name="Corner">The plane pixel the picture's upper-left corner sits on.</param>
public readonly record struct PlacedPlanePicture(SpriteSheetFrame Picture, Position16<TilePlanePixels> Corner)
{
    /// <summary>
    /// <paramref name="picture"/> placed at cell <paramref name="cell"/> of a plane <paramref name="plane"/> cells in size: the
    /// point its frame names as its origin lands on the cell's corner. A picture reaching past the plane's top or left edge
    /// continues from the opposite edge.
    /// </summary>
    public static PlacedPlanePicture AtCell(SpriteSheetFrame picture, Position16<TilemapCells> cell, Extent16<TilemapCells> plane)
    {
        Ensure.NotNull(picture);
        int width = plane.X * ScreenGeometry.BackgroundTileSizePixels;
        int height = plane.Y * ScreenGeometry.BackgroundTileSizePixels;
        int left = cell.X * ScreenGeometry.BackgroundTileSizePixels - picture.Origin.X;
        int top = cell.Y * ScreenGeometry.BackgroundTileSizePixels - picture.Origin.Y;
        return new(picture, new((ushort)(((left % width) + width) % width), (ushort)(((top % height) + height) % height)));
    }
}
