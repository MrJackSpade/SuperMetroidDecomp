using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed enemy-projectile compositions and timed program frames.</summary>
/// <param name="Frames">Installed ordered OAM parts of each composition frame.</param>
/// <param name="ProgramFrames">Installed ordered OAM parts of each timed program frame.</param>
internal sealed record EnemyProjectileSpritemapSelection(Dictionary<EnemyProjectileFrameId, SpritePart[]> Frames,
    Dictionary<EnemyProjectileFrameId, SpritePart[]> ProgramFrames);
