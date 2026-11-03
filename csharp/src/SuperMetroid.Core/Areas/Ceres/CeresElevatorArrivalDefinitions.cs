using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Compiled mechanics for the two enemy projectiles used by the initial
/// Ceres elevator arrival. Spritemap payloads remain presentation data.
/// </summary>
internal static class CeresElevatorArrivalDefinitions
{

    /// <summary>Moving pad spawned first, into the last enemy-projectile slot.</summary>
    internal static readonly CeresElevatorProjectileDefinition MovingPad = new(
        Program: CeresElevatorArrivalProgram.Pad);

    /// <summary>Stationary level-data concealer spawned second, into the slot before it.</summary>
    internal static readonly CeresElevatorProjectileDefinition StationaryPlatform = new(
        Program: CeresElevatorArrivalProgram.Platform);

    /// <summary>The graphics both arrival projectiles start with: character zero in the common colours, as their initializer clears them.</summary>
    internal static SpriteGraphics StartingGraphics { get; } = SpriteGraphics.CharacterZeroIn(PictureShading.Standard);

    /// <summary>The initializer seeds the moving pad's frame-behaviour delay to sixty frames.</summary>
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
            [CeresElevatorArrivalPose.PadFirst] = EnemyProjectileFrameId.CeresElevatorPad0,
            [CeresElevatorArrivalPose.PadSecond] = EnemyProjectileFrameId.CeresElevatorPad1,
            [CeresElevatorArrivalPose.Platform] = EnemyProjectileFrameId.CeresElevatorPlatform,
        };
}
