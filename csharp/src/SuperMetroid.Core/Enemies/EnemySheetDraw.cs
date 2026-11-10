using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>One frame an enemy draws from the sheet of the variant its room's population places it in (#1392, #1599).</summary>
/// <param name="Enemy">The enemy whose sheet is drawn.</param>
/// <param name="Frame">The sheet frame's name.</param>
/// <param name="Origin">The frame origin on screen.</param>
internal sealed record EnemySheetDraw(EnemyDefinitionId Enemy, string Frame, Position16<ScreenPixels> Origin)
{
    /// <summary>
    /// The shades and colour effects the draw shows its picture in (#1512), such as a hit flash or a glow's frame; null
    /// for the picture's standard colours.
    /// </summary>
    public PictureShading? Shading { get; init; }

    /// <summary>
    /// The rows the enemy's own rotting corpse shows as its rot has left them (#1539), which each of the enemy's frames that
    /// shows the corpse is drawn whole through; null for an enemy that shows no rotting corpse.
    /// </summary>
    public CorpseRotRows? RotRows { get; init; }
}
