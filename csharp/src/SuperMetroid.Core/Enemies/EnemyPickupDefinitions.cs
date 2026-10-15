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
        int index = (int)kind;
        EnemyDeathProjectileProgram? program = kind switch
        {
            EnemyPickupKind.None => null,
            EnemyPickupKind.SmallEnergy => EnemyDeathProjectileProgram.SmallEnergy,
            EnemyPickupKind.BigEnergy => EnemyDeathProjectileProgram.BigEnergy,
            EnemyPickupKind.PowerBomb => EnemyDeathProjectileProgram.PowerBombs,
            EnemyPickupKind.Missile => EnemyDeathProjectileProgram.Missiles,
            EnemyPickupKind.SuperMissile => EnemyDeathProjectileProgram.SuperMissiles,
            _ => throw new InvalidDataException($"Enemy pickup kind ${index:X4} has no animation-table entry."),
        };
        return new EnemyPickupAnimationDefinition(checked((ushort)(index * 2)), program);
    }
}
