namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectileFrameBehaviorWords"/> that only verification reads.</summary>
internal static class EnemyProjectileFrameBehaviorWordsConstants
{
    /// <summary>The pre-instruction the bank-$86 routine <paramref name="word"/> stands for.</summary>
    internal static EnemyProjectileFrameBehavior FromWord(ushort word) => EnemyProjectileFrameBehaviorWords.Identities.TryGetValue(word, out EnemyProjectileFrameBehavior frameBehavior)
        ? frameBehavior
        : throw new InvalidDataException($"No enemy-projectile pre-instruction routine at $86:{word:X4}.");
}
