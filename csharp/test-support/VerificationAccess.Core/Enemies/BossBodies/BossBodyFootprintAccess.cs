namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>Cell lookups on <see cref="BossBodyFootprint"/> that only verification reads (#1466).</summary>
internal static class BossBodyFootprintAccess
{
    /// <summary>Whether a footprint covers one cell.</summary>
    extension(BossBodyFootprint footprint)
    {
        /// <summary>Whether the cell at <paramref name="column"/> of <paramref name="row"/> is covered.</summary>
        internal bool Covers(int row, int column) =>
            (uint)column < BossBodyDefinitions.ColumnCount && (footprint.Row(row) & (1u << column)) != 0;
    }
}
