namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectileMotionWords"/> that only verification reads.</summary>
internal static class EnemyProjectileMotionWordsConstants
{
    /// <summary>The motion the bank-$86 routine <paramref name="word"/> stands for, or null for holding still.</summary>
    internal static Enum? FromWord(ushort word) => EnemyProjectileMotionWords.Motions.TryGetValue(word, out Enum? motion)
        ? motion
        : throw new InvalidDataException($"No enemy-projectile pre-instruction routine at $86:{word:X4}.");
}
