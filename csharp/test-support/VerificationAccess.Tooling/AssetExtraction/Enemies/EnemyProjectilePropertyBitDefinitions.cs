using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The bit each projectile behaviour occupies in a bank-$86 projectile's property word, above its twelve damage bits;
/// an instruction list's property OR and AND operands are words of these bits.
/// </summary>
internal static class EnemyProjectilePropertyBitDefinitions
{
    /// <summary>The property-word bit of each behaviour: bits 12 to 15.</summary>
    internal static readonly FrozenDictionary<EnemyProjectilePropertyFlags, ushort> Bits = new Dictionary<EnemyProjectilePropertyFlags, ushort>
    {
        [EnemyProjectilePropertyFlags.HighPriority] = 0x1000,
        [EnemyProjectilePropertyFlags.HarmlessToSamus] = 0x2000,
        [EnemyProjectilePropertyFlags.PersistsOnContact] = 0x4000,
        [EnemyProjectilePropertyFlags.BlocksShots] = 0x8000,
    }.ToFrozenDictionary();
}
