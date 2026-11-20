using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>Reads which typed death explosion or drop program a projectile runs (#1291, #2554).</summary>
internal static class EnemyDeathProjectilePrograms
{
    /// <summary>The drop program <paramref name="projectile"/> runs.</summary>
    internal static EnemyDeathAndDropSegment ProgramOf(EnemyProjectile projectile) =>
        projectile.Animation is EnemyDeathAndDropAnimation animation ? animation.PlayingSegment
            : throw new InvalidOperationException($"Projectile {projectile.Place} runs no drop program.");

    /// <summary>The death explosion program <paramref name="projectile"/> runs.</summary>
    internal static EnemyDeathExplosionSegment ExplosionProgramOf(EnemyProjectile projectile) =>
        projectile.Animation is EnemyDeathExplosionAnimation animation ? animation.PlayingSegment
            : throw new InvalidOperationException($"Projectile {projectile.Place} runs no death explosion program.");

    /// <summary>Whether <paramref name="projectile"/> still plays its death explosion, which it hands over to its drop as it ends.</summary>
    internal static bool IsExploding(EnemyProjectile projectile) => projectile.Animation is EnemyDeathExplosionAnimation;

    /// <summary>The death explosion animation <paramref name="projectile"/> plays (#2061).</summary>
    internal static Animation<EnemyDeathExplosionSegment> AnimationOf(EnemyProjectile projectile) =>
        projectile.Animation is EnemyDeathExplosionAnimation animation ? animation.PlaybackOf().Animation
            : throw new InvalidOperationException($"Projectile {projectile.Place} runs no death explosion program.");
}
