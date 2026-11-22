using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed enemy-projectile compositions and timed program frames.</summary>
/// <param name="Frames">Installed ordered OAM parts of each composition frame.</param>
/// <param name="ProgramFrames">Installed ordered OAM parts of each timed program frame.</param>
/// <param name="ElevatorArrival">Installed ordered OAM parts of each pose's picture of the Ceres elevator arrival (#2425).</param>
internal sealed partial record EnemyProjectileSpritemapSelection(Dictionary<EnemyProjectileFrameId, SpritePart[]> Frames,
    Dictionary<EnemyProjectileFrameId, SpritePart[]> ProgramFrames, Dictionary<CeresElevatorArrivalPose, SpritePart[]> ElevatorArrival);
