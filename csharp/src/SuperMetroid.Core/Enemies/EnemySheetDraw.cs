using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>One frame an enemy draws from its graphics set's sheet (#1392).</summary>
/// <param name="Enemy">The enemy whose sheet is drawn.</param>
/// <param name="Frame">The sheet frame's name.</param>
/// <param name="Origin">The frame origin on screen.</param>
/// <param name="Rot">The rot the enemy selected, which a frame drawn over a rotting corpse's rows shows; none for an enemy that shows none.</param>
internal sealed record EnemySheetDraw(EnemyDefinitionId Enemy, string Frame, Position16<ScreenPixels> Origin, CorpseRotView? Rot = null)
{
    /// <summary>
    /// The shades and colour effects the draw shows its picture in (#1512), such as a hit flash or a glow's frame; null
    /// for the picture's standard colours.
    /// </summary>
    public PictureShading? Shading { get; init; }

    /// <summary>
    /// The rows a rotting corpse's frame shows as its rot has left them (#1531), which the frame is drawn through; null for
    /// an enemy whose frames show their pictures' rows as they are.
    /// </summary>
    public CorpseRotRows? RotRows { get; init; }
}
