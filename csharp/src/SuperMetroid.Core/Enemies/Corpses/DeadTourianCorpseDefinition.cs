namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>How one dead Zoomer, Ripper or Skree variant starts and rots.</summary>
/// <param name="InitialProgram">The pose program the corpse starts with.</param>
/// <param name="Kind">The kind of corpse whose staged characters it rots.</param>
/// <param name="RowCount">The corpse's height in pixel rows, each of which its rot schedule drops.</param>
internal readonly record struct DeadTourianCorpseDefinition(DeadTourianProgram InitialProgram, RottingCorpseKind Kind, int RowCount);
