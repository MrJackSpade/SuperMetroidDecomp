using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The Chozo orbs the Torizos throw (#2332, #2472): how many Bomb Torizo spews in each burst, and what an orb leaves when it
/// breaks, each Torizo's orb rolling its own drop chances.
/// </summary>
internal static partial class TorizoChozoOrbDefinitions
{
    /// <summary>The orbs each burst of Bomb Torizo's spew throws at once; Golden Torizo's bursts throw one each.</summary>
    internal const int BombTorizoOrbsPerBurst = 3;

    /// <summary>The chances the drop a broken Bomb Torizo orb leaves rolls: mostly energy, often a missile, never a Super Missile or Power Bomb.</summary>
    internal static readonly EnemyDropChances BombTorizoOrbDropChances = new(SmallEnergy: 116, BigEnergy: 20, Missile: 65, Nothing: 54, SuperMissile: 0, PowerBomb: 0);

    /// <summary>The chances the drop a broken Golden Torizo orb leaves rolls: often a Super Missile, never a Power Bomb.</summary>
    internal static readonly EnemyDropChances GoldenTorizoOrbDropChances = new(SmallEnergy: 30, BigEnergy: 30, Missile: 50, Nothing: 60, SuperMissile: 85, PowerBomb: 0);
}
