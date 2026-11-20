namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A pre-instruction routine that stands for <see cref="Motion"/> of the family motion behaviour bound to the projectile.</summary>
/// <typeparam name="TMotion">The family's motions.</typeparam>
/// <param name="motion">The motion the routine stands for.</param>
internal sealed class EnemyProjectileFamilyMotionWord<TMotion>(TMotion motion) : EnemyProjectileMotionWord
    where TMotion : struct, Enum
{
    /// <summary>The motion the routine stands for.</summary>
    internal TMotion Motion { get; } = motion;

    /// <inheritdoc/>
    internal override EnemyProjectileMotion MotionOf(EnemyProjectileBehavior? behavior) =>
        behavior is EnemyProjectileMotionBehavior<TMotion> owner
            ? owner.Moving(Motion)
            : throw new InvalidOperationException(
                $"{behavior?.GetType().Name ?? "No behaviour"} owns no {typeof(TMotion).Name}, so it cannot move by {Motion}.");

    /// <inheritdoc/>
    internal override IEnumerable<string> FamilyMotionNames => Enum.GetNames<TMotion>().Select(name => $"{typeof(TMotion).Name}.{name}");

    /// <summary>Names the motion.</summary>
    public override string ToString() => $"{typeof(TMotion).Name}.{Motion}";
}
