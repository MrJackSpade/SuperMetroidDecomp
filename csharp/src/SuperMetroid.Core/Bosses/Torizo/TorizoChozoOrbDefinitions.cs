using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What the Chozo orbs the Torizos throw leave when they break (#2332): each Torizo's orb rolls its own drop chances.</summary>
internal static class TorizoChozoOrbDefinitions
{
    /// <summary>The chances the drop a broken Bomb Torizo orb leaves rolls: mostly energy, often a missile, never a Super Missile or Power Bomb.</summary>
    internal static readonly EnemyDropChances BombTorizoOrbDropChances = new(SmallEnergy: 116, BigEnergy: 20, Missile: 65, Nothing: 54, SuperMissile: 0, PowerBomb: 0);

    /// <summary>The chances the drop a broken Golden Torizo orb leaves rolls: often a Super Missile, never a Power Bomb.</summary>
    internal static readonly EnemyDropChances GoldenTorizoOrbDropChances = new(SmallEnergy: 30, BigEnergy: 30, Missile: 50, Nothing: 60, SuperMissile: 85, PowerBomb: 0);
}
