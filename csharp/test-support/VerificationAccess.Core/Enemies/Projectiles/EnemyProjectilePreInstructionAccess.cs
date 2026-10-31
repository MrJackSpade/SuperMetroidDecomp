namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's native view of enemy-projectile pre-instructions: the bank-$86 routine words that stand for each
/// identity. Several empty routines stand for <see cref="EnemyProjectilePreInstruction.Stationary"/>, so checks decode a
/// native word and compare identities.
/// </summary>
internal static class EnemyProjectilePreInstructionAccess
{
    /// <summary>The pre-instruction the bank-$86 routine <paramref name="word"/> stands for.</summary>
    internal static EnemyProjectilePreInstruction FromWord(ushort word) => EnemyProjectilePreInstructionWordsConstants.FromWord(word);

    /// <summary>Every bank-$86 routine word that stands for a pre-instruction, with the word zero of a freed slot.</summary>
    internal static IReadOnlyCollection<ushort> RoutineWords => EnemyProjectilePreInstructionWords.Identities.Keys;
}
