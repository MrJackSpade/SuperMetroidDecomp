namespace SuperMetroid.Core.Game;

/// <summary>One fixed bank-$86 pickup animation selection record.</summary>
/// <param name="TableOffset">Byte offset into the native pickup-animation selector table, retained in the projectile's variable field.</param>
/// <param name="InstructionList">Bank-$86 instruction-list pointer installed on the pickup projectile.</param>
internal readonly record struct EnemyPickupAnimationDefinition(
    ushort TableOffset,
    ushort InstructionList);

/// <summary>Compiled fixed instruction selectors for enemy drops.</summary>
internal static class EnemyPickupDefinitions
{
    /// <summary>
    /// None, small energy, big energy, Power Bomb, missile, and Super Missile list
    /// pointers at <c>$86:EF04-$86:EF0F</c>. <see cref="EnemyPickupKind.NoDrop"/> is not
    /// part of this table and is handled by the dormant pickup path.
    /// </summary>
    internal static EnemyPickupAnimationDefinition Animation(EnemyPickupKind kind)
    {
        int index = (int)kind;
        ushort instruction = kind switch
        {
            EnemyPickupKind.None => 0,
            EnemyPickupKind.SmallEnergy => EnemyPickupInstructionProgramDefinitions.SmallEnergy,
            EnemyPickupKind.BigEnergy => EnemyPickupInstructionProgramDefinitions.BigEnergy,
            EnemyPickupKind.PowerBomb => EnemyPickupInstructionProgramDefinitions.PowerBombs,
            EnemyPickupKind.Missile => EnemyPickupInstructionProgramDefinitions.Missiles,
            EnemyPickupKind.SuperMissile => EnemyPickupInstructionProgramDefinitions.SuperMissiles,
            _ => throw new InvalidDataException(
                $"Enemy pickup kind ${index:X4} has no animation-table entry."),
        };

        return new EnemyPickupAnimationDefinition(
            checked((ushort)(index * 2)),
            instruction);
    }
}
