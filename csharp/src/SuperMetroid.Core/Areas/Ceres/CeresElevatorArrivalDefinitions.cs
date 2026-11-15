using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres elevator arrival's two actors, the moving pad that lowers Samus and the stationary platform that hides the
/// landing recess: where they start, how long the pad waits and where Samus lands. Frame pictures remain presentation data.
/// </summary>
internal static class CeresElevatorArrivalDefinitions
{
    /// <summary>
    /// The graphics both arrival projectiles start with: the shared sheet, as their initializer clears them, with the
    /// level-data concealer's colour cleared to black. A new game clears that colour just before the arrival spawns, so
    /// the concealer hides the landing recess until both projectiles delete themselves at touchdown.
    /// </summary>
    internal static SpriteGraphics StartingGraphics { get; } =
        SpriteGraphics.SharedIn(PictureShading.Shaded(CeresElevatorShades.ConcealerCleared));

    /// <summary>The updates the moving pad waits before it moves, which its initializer seeds.</summary>
    internal const int MovingPadWaitFrames = 60;

    /// <summary>The initializer places the moving pad twenty-eight pixels below Samus.</summary>
    internal const int MovingPadYOffset = 28;

    /// <summary>The initializer places the stationary platform at world Y=97.</summary>
    internal const int StationaryPlatformY = 97;

    /// <summary>The descent clamps Samus to world Y=72 when it ends.</summary>
    internal const int LandingSamusY = 72;

    /// <summary>The descent compares the incremented Samus Y against 73 before clamping.</summary>
    internal const int LandingThresholdY = 73;

    /// <summary>The composition each arrival pose shows.</summary>
    internal static readonly IReadOnlyDictionary<CeresElevatorArrivalPose, EnemyProjectileFrameId> Frames =
        new Dictionary<CeresElevatorArrivalPose, EnemyProjectileFrameId>
        {
            [CeresElevatorArrivalPose.PadFirst] = EnemyProjectileFrameId.CeresElevatorPadFirst,
            [CeresElevatorArrivalPose.PadSecond] = EnemyProjectileFrameId.CeresElevatorPadSecond,
            [CeresElevatorArrivalPose.Platform] = EnemyProjectileFrameId.CeresElevatorPlatform,
        };
}
