namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's native view of enemy-projectile pre-instructions: the bank-$86 routine words that stand for each
/// identity. Several empty routines stand for <see cref="EnemyProjectileFrameBehavior.Stationary"/>, so checks decode a
/// native word and compare identities.
/// </summary>
internal static class EnemyProjectileFrameBehaviorAccess
{
    /// <summary>The pre-instruction the bank-$86 routine <paramref name="word"/> stands for.</summary>
    internal static EnemyProjectileFrameBehavior FromWord(ushort word) => EnemyProjectileFrameBehaviorWordsConstants.FromWord(word);

    /// <summary>Every bank-$86 routine word that stands for a pre-instruction, with the word zero of a freed slot.</summary>
    internal static IReadOnlyCollection<ushort> RoutineWords => EnemyProjectileFrameBehaviorWords.Identities.Keys;
}
