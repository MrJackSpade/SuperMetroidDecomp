namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>How one character column shows a rotting corpse, as the enemy sheet manifest spells it (#1531).</summary>
internal sealed record CorpseColumnRotDocument
{
    /// <summary>The band the column's flesh begins at.</summary>
    public required int FirstBand { get; init; }

    /// <summary>The bands the column shows rotting, in order.</summary>
    public required int[] Bands { get; init; }
}
