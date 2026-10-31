namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectilePreInstructionWords"/> that only verification reads.</summary>
internal static class EnemyProjectilePreInstructionWordsConstants
{
    /// <summary>The pre-instruction the bank-$86 routine <paramref name="word"/> stands for.</summary>
    internal static EnemyProjectilePreInstruction FromWord(ushort word) => EnemyProjectilePreInstructionWords.Identities.TryGetValue(word, out EnemyProjectilePreInstruction preInstruction)
        ? preInstruction
        : throw new InvalidDataException($"No enemy-projectile pre-instruction routine at $86:{word:X4}.");
}
