using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The colouring a frame family is always drawn in (#1513): the shades its draws show over the rows its frame inks,
/// among the effects of the enemy whose encounter defines them, laid in order so a later one wins.
/// </summary>
/// <param name="Owner">The enemy whose own, fixed or spawned-object effects define the shades.</param>
/// <param name="Shades">The shades, in laying order.</param>
internal sealed record FrameColouring(EnemyDefinitionId Owner, PictureShade[] Shades);
