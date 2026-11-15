namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's native view of enemy-projectile pre-instructions: the bank-$86 routine words that stand for each motion
/// (#1843). Several empty routines stand for holding still, and the routines of every kind that moves one way stand for
/// <see cref="SingleMotion.Moving"/>, so checks decode a native word and compare motions.
/// </summary>
internal static class EnemyProjectileMotionAccess
{
    /// <summary>The motion the bank-$86 routine <paramref name="word"/> stands for, or null for holding still.</summary>
    internal static Enum? FromWord(ushort word) => EnemyProjectileMotionWordsConstants.FromWord(word);

    /// <summary>Every bank-$86 routine word that stands for a motion or for holding still, with the word zero of a freed slot.</summary>
    internal static IReadOnlyCollection<ushort> RoutineWords => EnemyProjectileMotionWords.Motions.Keys;
}
