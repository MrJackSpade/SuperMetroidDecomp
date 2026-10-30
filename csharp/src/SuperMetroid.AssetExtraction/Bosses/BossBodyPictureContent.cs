using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Compares boss body pictures by what they paint (#1466): their size and each cell's place, tile and character bytes,
/// whatever they are named. Pictures equal by it share one place on the body sheet.
/// </summary>
internal sealed class BossBodyPictureContent : IEqualityComparer<BossBodyPicture>
{
    /// <summary>The comparer.</summary>
    internal static readonly BossBodyPictureContent Instance = new();

    /// <summary>Creates the one comparer.</summary>
    private BossBodyPictureContent()
    {
    }

    /// <inheritdoc/>
    public bool Equals(BossBodyPicture? x, BossBodyPicture? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null || x.Columns != y.Columns || x.Rows != y.Rows || x.Cells.Count != y.Cells.Count)
            return false;
        for (int index = 0; index < x.Cells.Count; index++)
        {
            BossBodyPaintedCell a = x.Cells[index], b = y.Cells[index];
            if (a.Column != b.Column || a.Row != b.Row || a.Tile != b.Tile || !a.Character.Span.SequenceEqual(b.Character.Span))
                return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public int GetHashCode(BossBodyPicture obj)
    {
        BossBodyPicture picture = Ensure.NotNull(obj);
        var hash = new HashCode();
        hash.Add(picture.Columns);
        hash.Add(picture.Rows);
        foreach (BossBodyPaintedCell cell in picture.Cells)
        {
            hash.Add(cell.Column);
            hash.Add(cell.Row);
            hash.Add(cell.Tile);
            hash.AddBytes(cell.Character.Span);
        }
        return hash.ToHashCode();
    }
}
