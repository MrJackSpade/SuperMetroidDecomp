namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>How one dead Zoomer, Ripper or Skree corpse lies and rots.</summary>
/// <param name="Species">The species whose corpse it is.</param>
/// <param name="InitialAnimation">The pose animation the corpse starts with.</param>
/// <param name="RowCount">The corpse's height in pixel rows, each of which its rot schedule drops.</param>
internal readonly record struct DeadTourianCorpseDefinition(DeadTourianCorpseSpecies Species, DeadTourianSegment InitialAnimation, int RowCount);
