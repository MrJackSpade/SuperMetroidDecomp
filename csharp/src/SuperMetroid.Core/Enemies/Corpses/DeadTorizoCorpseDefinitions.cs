namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>How the dead Torizo's corpse rots.</summary>
internal static class DeadTorizoCorpseDefinitions
{
    /// <summary>The corpse's height in pixel rows, each of which its rot schedule drops.</summary>
    internal const int RowCount = 96;

    /// <summary>The dead Torizo's kind of rotting corpse, which its sheet's rot pictures are named for (#1406).</summary>
    internal static readonly RottingCorpseKind Kind = new(RottingCorpseSpecies.Torizo, 0);
}
