using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>The geometry of a corpse's row movers: where its character rows lie, its character columns and where a move stops copying.</summary>
/// <param name="InHeap">Whether the corpse stages its characters in the rotting heap rather than the enemy BG2 buffer.</param>
/// <param name="Rows">The rows of characters the movers reach.</param>
/// <param name="TilesPerRow">The characters in each row.</param>
/// <param name="ColumnWords">The first planes-0/1 word of each character column.</param>
/// <param name="ColumnMinimumY">The first pixel row each column moves, below its missing top characters.</param>
/// <param name="CopyLimit">The pixel row from which a move only clears.</param>
/// <param name="Wrap">The bytes a row's copy skips from a character's last two rows into the next character row.</param>
internal sealed record CorpseRowGeometry(bool InHeap, int Rows, int TilesPerRow, int[] ColumnWords, int[] ColumnMinimumY, int CopyLimit, int Wrap)
{
    /// <summary>The Sidehopper corpse's movers for graphics variant <paramref name="graphicsVariant"/> (population parameter 0 or 2).</summary>
    internal static CorpseRowGeometry Sidehopper(int graphicsVariant, int wrap) => InBg2(RottingCorpseSpecies.Sidehopper,
        [.. Enumerable.Range(0, 5).Select(column => CorpseStagingGeometryDefinitions.SidehopperColumnWordOffset(graphicsVariant, column))],
        [.. Enumerable.Range(0, 5).Select(column => CorpseStagingGeometryDefinitions.SidehopperColumnMinimumY(graphicsVariant, column))],
        copyLimit: 38, wrap);

    /// <summary>A dead Zoomer's, Ripper's or Skree's movers for its variant <paramref name="layout"/>, which move every column from the top.</summary>
    internal static CorpseRowGeometry Tourian(DeadTourianCorpseSpecies species, DeadTourianCorpseVariant layout,
        int wrap) => InBg2(DeadTourianCorpseStagingDefinitions.RowLayout(species), [.. layout.ColumnWordOffsets.Select(offset => (int)offset)],
            new int[layout.ColumnWordOffsets.Length], layout.MaximumY, wrap);

    /// <summary>The dead Torizo's movers.</summary>
    internal static CorpseRowGeometry Torizo { get; } = InBg2(RottingCorpseSpecies.Torizo,
        [.. Enumerable.Range(0, DeadTorizoGeometryDefinitions.Columns).Select(DeadTorizoGeometryDefinitions.ColumnWordOffset)],
        [.. Enumerable.Range(0, DeadTorizoGeometryDefinitions.Columns).Select(DeadTorizoGeometryDefinitions.ColumnMinimumY)],
        copyLimit: 94, DeadTorizoCorpseDefinitions.Corpse.WrapOffset);

    /// <summary>
    /// Mother Brain's corpse's movers ($A9:EA40-$EB07): its rows' last two pixel rows land in the next row of characters, and
    /// its bottom two rows clear without copying.
    /// </summary>
    internal static CorpseRowGeometry MotherBrain { get; } = new(InHeap: true, Rows: 8, MotherBrainCorpseArtworkDefinitions.ColumnCount,
        [.. Enumerable.Range(0, MotherBrainCorpseArtworkDefinitions.ColumnCount).Select(column => column * MotherBrainCorpseArtworkDefinitions.TileBytes / 2)],
        [.. Enumerable.Range(0, MotherBrainCorpseArtworkDefinitions.ColumnCount).Select(MotherBrainCorpseArtworkDefinitions.ColumnMinimumY)],
        CopyLimit: MotherBrainCorpseRottingState.EntryCount - 2, Wrap: 212);

    /// <summary>Movers of a corpse staged in the enemy BG2 buffer, laid out as <paramref name="table"/> lays its rows.</summary>
    private static CorpseRowGeometry InBg2(RottingCorpseSpecies table, int[] columnWords, int[] columnMinimumY, int copyLimit, int wrap)
    {
        CorpseRotationExtent extent = CorpseStagingGeometryDefinitions.RotationExtent(table);
        return new(InHeap: false, extent.Count, extent.Tiles, columnWords, columnMinimumY, copyLimit, wrap);
    }
}
