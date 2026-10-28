using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The bit each projectile behaviour occupies in a bank-$86 projectile's property word, above its twelve damage bits;
/// an instruction list's property OR and AND operands are words of these bits. The lowest of the four is the projectile's
/// depth relative to Samus: set when it is drawn in front of her.
/// </summary>
internal static class EnemyProjectilePropertyBitDefinitions
{
    /// <summary>The property-word bit of the depth relative to Samus: bit 12, set in front of Samus.</summary>
    internal static readonly FrozenDictionary<EnemyProjectileDepth, ushort> DepthBits = new Dictionary<EnemyProjectileDepth, ushort>
    {
        [EnemyProjectileDepth.BehindSamus] = 0,
        [EnemyProjectileDepth.InFrontOfSamus] = 0x1000,
    }.ToFrozenDictionary();

    /// <summary>The property-word bit of each behaviour: bits 13 to 15.</summary>
    internal static readonly FrozenDictionary<EnemyProjectilePropertyFlags, ushort> Bits = new Dictionary<EnemyProjectilePropertyFlags, ushort>
    {
        [EnemyProjectilePropertyFlags.HarmlessToSamus] = 0x2000,
        [EnemyProjectilePropertyFlags.PersistsOnContact] = 0x4000,
        [EnemyProjectilePropertyFlags.BlocksShots] = 0x8000,
    }.ToFrozenDictionary();
}
