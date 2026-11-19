using System.Reflection;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's view of the motion an enemy projectile moves by (#2571): the motion value its behaviour's typed motion
/// holds, for checks that compare it with a decoded native word, and the motion a snapshot import starts.
/// </summary>
internal static class EnemyProjectileMotionValueAccess
{
    /// <summary>The motion views of a projectile.</summary>
    extension(EnemyProjectile self)
    {
        /// <summary>The motion value the projectile moves by, or null while it holds still.</summary>
        internal Enum? MotionValue => self.Motion is { } motion
            ? motion.GetType().GetProperty(nameof(EnemyProjectileFamilyMotion<SingleMotion>.Motion), BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(motion) as Enum ?? throw new InvalidOperationException($"{motion.GetType().Name} holds no motion value.")
            : null;

        /// <summary>Starts <paramref name="motion"/> on the projectile through its behaviour, or holds it still for null.</summary>
        /// <param name="motion">One of the projectile's behaviour's motions, or null.</param>
        internal void StartMotionValue(Enum? motion)
        {
            if (motion is null)
            {
                self.HoldStill();
                return;
            }
            MethodInfo start = typeof(EnemyProjectile).GetMethod(nameof(EnemyProjectile.StartMotion), BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("EnemyProjectile has no StartMotion.");
            start.MakeGenericMethod(motion.GetType()).Invoke(self, [motion]);
        }
    }
}
