namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>One of the motions <typeparamref name="TMotion"/> names, held through the behaviour that owns it (#2571).</summary>
/// <typeparam name="TMotion">The owning behaviour's motions.</typeparam>
internal sealed class EnemyProjectileFamilyMotion<TMotion> : EnemyProjectileMotion
    where TMotion : struct, Enum
{
    /// <summary>The behaviour that owns the motion and moves the projectile by it.</summary>
    private readonly EnemyProjectileMotionBehavior<TMotion> _owner;

    /// <summary>The motion <paramref name="motion"/> of <paramref name="owner"/>.</summary>
    /// <param name="owner">The behaviour that owns the motion.</param>
    /// <param name="motion">The motion.</param>
    internal EnemyProjectileFamilyMotion(EnemyProjectileMotionBehavior<TMotion> owner, TMotion motion)
    {
        _owner = Ensure.NotNull(owner);
        Motion = motion;
    }

    /// <summary>The motion the projectile moves by.</summary>
    internal TMotion Motion { get; }

    /// <inheritdoc/>
    internal override void Move(EnemyProjectileMotionContext context) => _owner.Move(context, Motion);

    /// <summary>Names the motion.</summary>
    public override string ToString() => $"{typeof(TMotion).Name}.{Motion}";
}
