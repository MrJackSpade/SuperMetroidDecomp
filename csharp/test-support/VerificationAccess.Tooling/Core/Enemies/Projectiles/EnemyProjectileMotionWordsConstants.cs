namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectileMotionWords"/> that only verification reads.</summary>
internal static class EnemyProjectileMotionWordsConstants
{
    /// <summary>What the bank-$86 routine <paramref name="word"/> stands for: a motion of the projectile's behaviour, or holding still.</summary>
    internal static EnemyProjectileMotionWord FromWord(ushort word) => EnemyProjectileMotionWords.Motions.TryGetValue(word, out EnemyProjectileMotionWord? motion)
        ? motion
        : throw new InvalidDataException($"No enemy-projectile pre-instruction routine at $86:{word:X4}.");
}
