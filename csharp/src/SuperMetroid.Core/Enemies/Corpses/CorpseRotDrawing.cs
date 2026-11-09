using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Draws a rotting corpse's frame whole as its own rot leaves it (#1539): from where the corpse lies in the frame's picture
/// and the corpse's rows, the picture row each pixel row of the picture shows.
/// </summary>
internal static class CorpseRotDrawing
{
    /// <summary>
    /// How picture <paramref name="picture"/>, <paramref name="size"/> pixels, in which the corpse lies as
    /// <paramref name="placement"/> says, shows the corpse's <paramref name="rows"/>: each of the corpse's rows shows the row
    /// its rot has moved there, across the whole picture, or nothing where the rot has cleared it or moved there a row the
    /// picture does not hold; every other pixel row shows itself.
    /// </summary>
    internal static CorpseRotPicture Picture(CorpsePicturePlacement placement, CorpseRotRows rows, Extent16<ImagePixels> size)
    {
        Ensure.NotNull(placement);
        Ensure.NotNull(rows);
        var shown = new int?[size.Y];
        for (int row = 0; row < shown.Length; row++)
            shown[row] = Shown(placement.Top, rows, row, size.Y);
        return new CorpseRotPicture(shown);
    }

    /// <summary>
    /// The picture row that picture row <paramref name="row"/> of a picture <paramref name="height"/> rows tall shows: itself
    /// outside the corpse, else the picture row of the corpse row the rot has moved there, or none where the rot has cleared
    /// it or that corpse row lies outside the picture.
    /// </summary>
    private static int? Shown(int top, CorpseRotRows rows, int row, int height)
    {
        int corpseRow = row - top;
        if (corpseRow < 0 || corpseRow >= rows.Count)
            return row;
        if (rows.Shown(corpseRow) is not int source)
            return null;
        int pictureRow = source + top;
        return pictureRow >= 0 && pictureRow < height ? pictureRow : null;
    }
}
