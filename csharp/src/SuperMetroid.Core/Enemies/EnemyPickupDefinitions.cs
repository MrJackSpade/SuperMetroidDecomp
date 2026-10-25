using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Compiled fixed program selectors for enemy drops.</summary>
internal static class EnemyPickupDefinitions
{
    /// <summary>
    /// The none, small energy, big energy, Power Bomb, missile and Super Missile entries of the pickup table at
    /// <c>$86:EF04-$86:EF0F</c>: each kind's table offset and drop program. <see cref="EnemyPickupKind.None"/> has no
    /// program, and <see cref="EnemyPickupKind.NoDrop"/> is not part of the table; the dormant pickup path handles both.
    /// </summary>
    internal static EnemyPickupAnimationDefinition Animation(EnemyPickupKind kind)
    {
        return kind switch
        {
            EnemyPickupKind.None => new(0, null),
            EnemyPickupKind.SmallEnergy => new(2, EnemyDeathProjectileProgram.SmallEnergy),
            EnemyPickupKind.BigEnergy => new(4, EnemyDeathProjectileProgram.BigEnergy),
            EnemyPickupKind.PowerBomb => new(6, EnemyDeathProjectileProgram.PowerBombs),
            EnemyPickupKind.Missile => new(8, EnemyDeathProjectileProgram.Missiles),
            EnemyPickupKind.SuperMissile => new(10, EnemyDeathProjectileProgram.SuperMissiles),
            _ => throw new InvalidDataException($"Enemy pickup kind {kind} has no animation-table entry."),
        };
    }
}
