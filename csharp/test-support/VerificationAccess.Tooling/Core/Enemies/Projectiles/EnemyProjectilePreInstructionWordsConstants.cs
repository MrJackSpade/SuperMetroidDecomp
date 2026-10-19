namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectilePreInstructionWords"/> that only verification reads.</summary>
internal static class EnemyProjectilePreInstructionWordsConstants
{
    /// <summary>Each routine address's pre-instruction.</summary>
    internal static readonly Dictionary<ushort, EnemyProjectilePreInstruction> Identities =
        EnemyProjectilePreInstructionWords.Words.ToDictionary(pair => pair.Value, pair => pair.Key);
    /// <summary>The pre-instruction whose bank-$86 routine is <paramref name="word"/>.</summary>
    internal static EnemyProjectilePreInstruction FromWord(ushort word) => Identities.TryGetValue(word, out EnemyProjectilePreInstruction preInstruction)
        ? preInstruction
        : throw new InvalidDataException($"No enemy-projectile pre-instruction routine at $86:{word:X4}.");
}
