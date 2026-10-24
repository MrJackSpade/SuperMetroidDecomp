namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>Describes tile-column offsets and sheet copies for one dead Zoomer, Ripper or Skree art variant.</summary>
/// <param name="MaximumY">First pixel row that no longer copies downward: the block height less two pixels (14 or 30).</param>
/// <param name="ColumnWordOffsets">Work-buffer word offset of each tile column of the corpse block.</param>
/// <param name="InitialGraphicsCopies">Sheet-to-work-buffer copies that stage the corpse's initial art.</param>
internal sealed record DeadTourianCorpseVariant(
    int MaximumY,
    ushort[] ColumnWordOffsets,
    CorpseGraphicsCopy[] InitialGraphicsCopies);
