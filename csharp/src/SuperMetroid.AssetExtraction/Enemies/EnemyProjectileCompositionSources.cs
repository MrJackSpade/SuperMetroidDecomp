using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where each enemy-projectile composition frame, and each picture of the Ceres elevator arrival, is stored
/// (<see cref="EnemyProjectileCompositionDefinitions"/>).
/// </summary>
internal static class EnemyProjectileCompositionSources
{
    /// <summary>The bank-$8D spritemap composition frame <paramref name="frame"/> is stored at.</summary>
    internal static ushort Spritemap(EnemyProjectileFrameId frame) =>
        EnemyProjectileCompositionDefinitions.Spritemaps.ContainsKey(frame)
            ? EnemyProjectileCompositionDefinitions.Spritemaps[frame]
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, "This projectile frame is not a bank-$8D composition.");

    /// <summary>The bank-$8D spritemap the Ceres elevator arrival's picture of <paramref name="pose"/> is stored at.</summary>
    internal static ushort ElevatorArrivalSpritemap(CeresElevatorArrivalPose pose) =>
        Ensure.Listed(EnemyProjectileCompositionDefinitions.ElevatorArrivalSpritemaps, pose);
}
