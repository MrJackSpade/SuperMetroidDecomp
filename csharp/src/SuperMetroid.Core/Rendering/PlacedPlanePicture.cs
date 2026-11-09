using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>An installed picture placed on a pictured plane (#1467, #1555).</summary>
/// <param name="Picture">The picture, pre-coloured under the palette its screen shows it in, drawn whole at its plane's place.</param>
/// <param name="Corner">The plane pixel the picture's upper-left corner sits on, which may lie left of or above the plane's origin.</param>
/// <param name="Shading">
/// The shades and colour effects its screen shows it in (#1512); <see cref="PictureShading.Standard"/> where the screen
/// shows no effect on it.
/// </param>
public readonly record struct PlacedPlanePicture(SpriteSheetFrame Picture, Offset16<PlanePixels> Corner, PictureShading Shading)
{
    /// <summary>
    /// <paramref name="picture"/> placed at cell <paramref name="cell"/> of its plane, shown in <paramref name="shading"/>:
    /// the point its frame names as its origin lands on the cell's corner, so a picture whose origin lies inside it reaches
    /// left of or above the cell.
    /// </summary>
    public static PlacedPlanePicture AtCell(SpriteSheetFrame picture, Position16<TilemapCells> cell, PictureShading shading)
    {
        Ensure.NotNull(picture);
        Ensure.NotNull(shading);
        return new(picture, Offset16<PlanePixels>.Of(cell.X * ScreenGeometry.BackgroundTileSizePixels - picture.Origin.X,
            cell.Y * ScreenGeometry.BackgroundTileSizePixels - picture.Origin.Y), shading);
    }
}
