using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>One frame an enemy draws from its graphics set's sheet (#1392).</summary>
/// <param name="Enemy">The enemy whose sheet is drawn.</param>
/// <param name="Frame">The sheet frame's name.</param>
/// <param name="Origin">The frame origin on screen.</param>
/// <param name="PaletteBits">The draw's palette selection.</param>
/// <param name="TileBase">The draw's packed tile base, which must be the one its sheet was composed at.</param>
/// <param name="Rot">The rot the enemy selected, which a frame drawn over a rotting corpse's rows shows; none for an enemy that shows none.</param>
internal sealed record EnemySheetDraw(EnemyDefinitionId Enemy, string Frame, Position16<ScreenPixels> Origin, int PaletteBits, int TileBase,
    CorpseRotView? Rot = null);
