namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Where a rotting corpse lies in one of its frame's pictures (#1531): the picture row the corpse's first pixel row lies at,
/// and for each of the picture's eight-pixel character columns how it shows the corpse rotting, or none for
/// a column that shows no rotting rows. A column shows one column of the corpse, and its rotting rows begin at the band its
/// flesh begins at, which no move of a row above it reaches. Every other pixel of the picture shows itself.
/// </summary>
public sealed class CorpsePictureLayout
{
    /// <summary>The picture row of the corpse's first pixel row and how each character column shows the corpse rotting.</summary>
    /// <exception cref="ArgumentException">There are no columns.</exception>
    public CorpsePictureLayout(int top, IReadOnlyList<CorpseColumnRot?> columns)
    {
        Ensure.NotNull(columns);
        if (columns.Count == 0)
            throw new ArgumentException("A corpse's picture has at least one character column.", nameof(columns));
        Top = top;
        Columns = [.. columns];
    }

    /// <summary>The picture row the corpse's first pixel row lies at, which may lie above the picture.</summary>
    public int Top { get; }

    /// <summary>How each character column shows the corpse rotting, from the picture's left; none for a column that shows no rotting rows.</summary>
    public IReadOnlyList<CorpseColumnRot?> Columns { get; }
}
