using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>The geometry of a corpse's row movers: its rows' layout, its character columns and where a move stops copying.</summary>
/// <param name="Table">The layout of the corpse's character rows.</param>
/// <param name="ColumnWords">The first planes-0/1 word of each character column.</param>
/// <param name="ColumnMinimumY">The first pixel row each column moves, below its missing top characters.</param>
/// <param name="CopyLimit">The pixel row from which a move only clears.</param>
/// <param name="Wrap">The bytes a row's copy skips from a character's last two rows into the next character row.</param>
internal sealed record CorpseRowGeometry(CorpseRotationTable Table, int[] ColumnWords, int[] ColumnMinimumY, int CopyLimit, int Wrap)
{
    /// <summary>The Sidehopper corpse's movers for graphics variant <paramref name="graphicsVariant"/> (population parameter 0 or 2).</summary>
    internal static CorpseRowGeometry Sidehopper(ushort graphicsVariant, int wrap) => new(CorpseRotationTable.Sidehopper,
        [.. Enumerable.Range(0, 5).Select(column => DeadMonsterRottingDefinitions.SidehopperColumnWordOffset(graphicsVariant, column))],
        [.. Enumerable.Range(0, 5).Select(column => DeadMonsterRottingDefinitions.SidehopperColumnMinimumY(graphicsVariant, column))],
        CopyLimit: 38, wrap);

    /// <summary>A dead Zoomer's, Ripper's or Skree's movers for its variant <paramref name="layout"/>, which move every column from the top.</summary>
    internal static CorpseRowGeometry Tourian(DeadTourianBehavior.DeadTourianCorpseProfile profile, DeadTourianBehavior.DeadTourianCorpseVariant layout,
        int wrap) => new(profile.RowLayout, [.. layout.ColumnWordOffsets.Select(offset => (int)offset)],
            new int[layout.ColumnWordOffsets.Length], layout.MaximumY, wrap);

    /// <summary>The dead Torizo's movers.</summary>
    internal static CorpseRowGeometry Torizo { get; } = new(CorpseRotationTable.Torizo,
        [.. Enumerable.Range(0, DeadTorizoGeometryDefinitions.Columns).Select(DeadTorizoGeometryDefinitions.ColumnWordOffset)],
        [.. Enumerable.Range(0, DeadTorizoGeometryDefinitions.Columns).Select(DeadTorizoGeometryDefinitions.ColumnMinimumY)],
        CopyLimit: 94, DeadTorizoCorpseDefinitions.Corpse.WrapOffset);
}
