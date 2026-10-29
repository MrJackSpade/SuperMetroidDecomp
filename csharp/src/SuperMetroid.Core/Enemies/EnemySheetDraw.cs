using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>One frame an enemy draws from its graphics set's sheet (#1392).</summary>
/// <param name="Enemy">The enemy whose sheet is drawn.</param>
/// <param name="Frame">The sheet frame's name.</param>
/// <param name="Origin">The frame origin on screen.</param>
/// <param name="Colouring">The colours the draw selects (#1464).</param>
/// <param name="Rot">The rot the enemy selected, which a frame drawn over a rotting corpse's rows shows; none for an enemy that shows none.</param>
internal sealed record EnemySheetDraw(EnemyDefinitionId Enemy, string Frame, Position16<ScreenPixels> Origin, SpriteColouring Colouring,
    CorpseRotView? Rot = null);
