using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The damage Mother Brain's projectiles deal, before Samus's suit divides it, as their definitions author it. Her onion
/// rings deal theirs through her own collision rather than the projectiles' common touch, to Samus and to the Baby
/// Metroid alike, and her finish-off thresholds count both her onion rings' and her bomb's.
/// </summary>
internal static class MotherBrainAttackDamage
{
    /// <summary>The damage one onion ring deals Samus or the Baby Metroid.</summary>
    internal static int OnionRing => EnemyProjectileKinds.Get(RoomEnemyProjectileKind.MotherBrainOnionRing).Damage;

    /// <summary>The damage one of her bombs deals Samus.</summary>
    internal static int Bomb => EnemyProjectileKinds.Get(RoomEnemyProjectileKind.MotherBrainBomb).Damage;
}
