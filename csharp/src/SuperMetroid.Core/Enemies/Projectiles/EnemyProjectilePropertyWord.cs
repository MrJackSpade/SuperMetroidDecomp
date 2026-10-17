namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The split of a bank-$86 projectile property word: its four high bits are the behavior flags of
/// <see cref="EnemyProjectilePropertyFlags"/> and its low twelve bits are the contact damage. The word stays packed
/// because a projectile program's property set and clear instructions carry it as one operand mask, applied to the
/// flags and the damage together. This type is the one place that mask is split into the typed flags and the damage.
/// </summary>
[SnesEncodingOwner]
internal static class EnemyProjectilePropertyWord
{
    /// <summary>Every behavior bit a property word carries.</summary>
    internal const EnemyProjectilePropertyFlags AllFlags = EnemyProjectilePropertyFlags.HighPriority |
        EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots;

    /// <summary>The behavior bits of <paramref name="word"/>.</summary>
    internal static EnemyProjectilePropertyFlags Flags(ushort word) => (EnemyProjectilePropertyFlags)word & AllFlags;

    /// <summary>The contact damage of <paramref name="word"/>.</summary>
    internal static ushort Damage(ushort word) => unchecked((ushort)(word & ~(ushort)AllFlags));
}
