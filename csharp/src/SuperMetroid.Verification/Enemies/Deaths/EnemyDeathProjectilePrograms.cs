using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>Reads which typed death or drop program a projectile runs (#1291).</summary>
internal static class EnemyDeathProjectilePrograms
{
    /// <summary>The death or drop program <paramref name="projectile"/> runs.</summary>
    internal static EnemyDeathProjectileSegment ProgramOf(EnemyProjectile projectile) =>
        projectile.Animation is EnemyDeathProjectileAnimation animation ? animation.PlayingSegment
            : throw new InvalidOperationException($"Projectile {projectile.Place} runs no death or drop program.");

    /// <summary>The death or drop animation <paramref name="projectile"/> plays (#2061).</summary>
    internal static Animation<EnemyDeathProjectileSegment> AnimationOf(EnemyProjectile projectile) =>
        projectile.Animation is EnemyDeathProjectileAnimation animation ? animation.Playback.Animation
            : throw new InvalidOperationException($"Projectile {projectile.Place} runs no death or drop program.");
}
