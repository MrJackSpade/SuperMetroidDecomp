namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>How one dead sidehopper variant's corpse rots.</summary>
/// <param name="Kind">The kind of corpse whose staged characters it rots.</param>
/// <param name="RowCount">The corpse's height in pixel rows, each of which its rot schedule drops.</param>
internal readonly record struct DeadSidehopperCorpseDefinition(RottingCorpseKind Kind, int RowCount);
