using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where a rotting corpse lies in a picture, as the enemy sheet manifest spells it (#1531): the picture row of the
/// corpse's first pixel row, and for each of the picture's character columns how it shows the corpse rotting, or null for
/// a column that shows no rotting rows.
/// </summary>
internal sealed record CorpsePictureDocument
{
    /// <summary>The picture row the corpse's first pixel row lies at.</summary>
    public required int Top { get; init; }

    /// <summary>How each character column shows the corpse rotting, from the picture's left, or null.</summary>
    public required CorpseColumnRotDocument?[] Columns { get; init; }

    /// <summary><paramref name="layout"/>, as the manifest spells it.</summary>
    internal static CorpsePictureDocument Of(CorpsePictureLayout layout) => new()
    {
        Top = layout.Top,
        Columns = [.. layout.Columns.Select(column => column is null ? null
            : new CorpseColumnRotDocument { FirstBand = column.FirstBand, Bands = [.. column.Bands] })],
    };

    /// <summary>The layout the manifest spells.</summary>
    /// <exception cref="InvalidDataException">The manifest spells no columns, or a column without its bands.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A column's bands are not bands of its flesh.</exception>
    internal CorpsePictureLayout Layout() => Columns is null || Columns.Length == 0
        ? throw new InvalidDataException("An enemy sheet manifest's corpse picture spells no character columns.")
        : new CorpsePictureLayout(Top, [.. Columns.Select(column => column is null ? null
            : new CorpseColumnRot(column.FirstBand, column.Bands ?? throw new InvalidDataException("An enemy sheet manifest's corpse column spells no bands.")))]);
}

/// <summary>How one character column shows a rotting corpse, as the enemy sheet manifest spells it (#1531).</summary>
internal sealed record CorpseColumnRotDocument
{
    /// <summary>The band the column's flesh begins at.</summary>
    public required int FirstBand { get; init; }

    /// <summary>The bands the column shows rotting, in order.</summary>
    public required int[] Bands { get; init; }
}
