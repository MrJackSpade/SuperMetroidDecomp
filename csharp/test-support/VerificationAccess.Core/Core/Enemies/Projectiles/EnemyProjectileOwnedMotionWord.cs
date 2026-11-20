namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A pre-instruction routine that stands for one of the motion objects a <typeparamref name="TBehavior"/> owns, the one
/// the codec's picker takes from the behaviour bound to the projectile.
/// </summary>
/// <typeparam name="TBehavior">The behaviour that owns the motion.</typeparam>
/// <param name="motion">Picks the behaviour's motion.</param>
internal sealed class EnemyProjectileOwnedMotionWord<TBehavior>(Func<TBehavior, EnemyProjectileOwnedMotion> motion) : EnemyProjectileMotionWord
    where TBehavior : EnemyProjectileBehavior
{
    /// <inheritdoc/>
    internal override EnemyProjectileMotion MotionOf(EnemyProjectileBehavior? behavior) =>
        behavior is TBehavior owner
            ? motion(owner)
            : throw new InvalidOperationException(
                $"{behavior?.GetType().Name ?? "No behaviour"} is not a {typeof(TBehavior).Name}, which owns the motion this routine stands for.");

    /// <summary>Names the behaviour that owns the motion.</summary>
    public override string ToString() => $"a motion of {typeof(TBehavior).Name}";
}
