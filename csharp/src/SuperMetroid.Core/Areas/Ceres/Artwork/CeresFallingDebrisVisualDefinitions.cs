using SuperMetroid.Core.Enemies.Projectiles;

using P = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisProgram;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The Ceres falling tiles' installed frames and their projectile definitions' native list identities.</summary>
internal static class CeresFallingDebrisVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<CeresFallingDebrisProgram, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<CeresFallingDebrisProgram, EnemyProjectileFrameId>
        {
            [CeresFallingDebrisProgram.Light] = EnemyProjectileFrameId.CeresFallingDebrisFrame00,
            [CeresFallingDebrisProgram.Dark] = EnemyProjectileFrameId.CeresFallingDebrisFrame01,
        };
}
