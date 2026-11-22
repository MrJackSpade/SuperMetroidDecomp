using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a Chozo orb a Torizo throws keeps (#2410): the drop chances its break rolls, those of the Torizo that threw it.</summary>
/// <param name="dropChances">The drop chances of the Torizo that threw the orb.</param>
internal sealed partial class TorizoChozoOrbState(EnemyDropChances dropChances) : EnemyProjectileState
{
    /// <summary>The drop chances the orb's break rolls (#2332): Bomb Torizo's orbs leave mostly energy, Golden Torizo's often a Super Missile.</summary>
    internal EnemyDropChances DropChances { get; } = dropChances;
}
