using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Draws a rotting corpse's frame as its rot leaves it (#1531): from where the corpse lies in the frame's picture and the
/// corpse's rows, the picture row each pixel row of each character column of the picture shows.
/// </summary>
internal static class CorpseRotDrawing
{
    /// <summary>
    /// How picture <paramref name="picture"/>, <paramref name="size"/> pixels, in which the corpse lies as
    /// <paramref name="layout"/> says, shows the corpse's <paramref name="rows"/>: a column's rotting rows show the rows the
    /// corpse's rot has moved there, by the band its flesh begins at, and every other pixel row shows itself.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The picture is not as wide as its columns, a column rots rows the corpse has not, or the rot moves a row there from a
    /// row the picture does not show rotting, where it does not hold the corpse's own pixels.
    /// </exception>
    internal static CorpseRotPicture Picture(CorpsePictureLayout layout, CorpseRotRows rows, Extent16<ImagePixels> size, string picture)
    {
        Ensure.NotNull(layout);
        Ensure.NotNull(rows);
        if (size.X != layout.Columns.Count * CorpseRotPicture.ColumnPixels)
            throw new InvalidDataException($"Picture {picture} is {size.X} pixels wide, not its {layout.Columns.Count} corpse columns.");
        var columns = new int?[layout.Columns.Count][];
        for (int column = 0; column < columns.Length; column++)
        {
            CorpseColumnRot? rot = layout.Columns[column];
            if (rot is not null && (rot.Bands[^1] + 1) * CorpseRowMoveDefinitions.BandRows > rows.Count)
                throw new InvalidDataException($"Picture {picture}'s column {column} rots band {rot.Bands[^1]} of a corpse {rows.Count} rows tall.");
            var shown = new int?[size.Y];
            for (int row = 0; row < shown.Length; row++)
                shown[row] = Shown(rot, rows, row, layout.Top, picture, column);
            columns[column] = shown;
        }
        return new CorpseRotPicture(columns);
    }

    /// <summary>
    /// The picture row picture row <paramref name="row"/> of a column rotting as <paramref name="rot"/> says shows: itself
    /// outside its rotting bands, else the corpse row the rot has moved there, or none where the rot has cleared it.
    /// </summary>
    /// <exception cref="InvalidDataException">The rot moves a row there from a row the picture does not show rotting.</exception>
    private static int? Shown(CorpseColumnRot? rot, CorpseRotRows rows, int row, int top, string picture, int column)
    {
        int corpseRow = row - top;
        if (rot is null || corpseRow < 0 || !rot.Rots(corpseRow / CorpseRowMoveDefinitions.BandRows))
            return row;
        if (rows.Shown(rot.FirstBand, corpseRow) is not int source)
            return null;
        return rot.Rots(source / CorpseRowMoveDefinitions.BandRows) ? source + top
            : throw new InvalidDataException($"Picture {picture}'s column {column} shows corpse row {source} rotted to row {corpseRow}, where the picture does not hold the corpse's own pixels.");
    }
}
