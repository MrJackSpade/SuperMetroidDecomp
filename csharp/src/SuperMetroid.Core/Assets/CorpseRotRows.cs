namespace SuperMetroid.Core.Assets;

/// <summary>
/// The pixel rows a rotting corpse's picture shows as its rot has left them (#1531): for each row of the picture, from its
/// top, the row of the picture the rot has moved there, or none once the rot has cleared it. The corpse's sheet holds each
/// of its frames once, and the frame is drawn through these rows, so the rot moves the picture's rows when it is drawn.
/// </summary>
public sealed class CorpseRotRows
{
    /// <summary>The picture row each row shows, or none where the rot has cleared it.</summary>
    private readonly int?[] shown;

    /// <summary>Keeps a copy of the row each of the picture's rows shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A row shows a row outside the picture.</exception>
    internal CorpseRotRows(ReadOnlySpan<int?> shown)
    {
        foreach (int? row in shown)
        {
            if (row is int source && (uint)source >= (uint)shown.Length)
                throw new ArgumentOutOfRangeException(nameof(shown), source, $"A rotting corpse's row shows row {source} of a picture {shown.Length} rows tall.");
        }
        this.shown = shown.ToArray();
    }

    /// <summary>The picture's rows, which the rot moves among themselves.</summary>
    public int Count => shown.Length;

    /// <summary>Whether the rot has cleared every row, so the picture draws nothing.</summary>
    public bool ShowsNothing => Array.TrueForAll(shown, row => row is null);

    /// <summary>The picture row that row <paramref name="row"/> shows, or none where the rot has cleared it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The picture has no such row.</exception>
    public int? Shown(int row) => (uint)row < (uint)shown.Length
        ? shown[row]
        : throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse's picture has {shown.Length} rows.");
}
