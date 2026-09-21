namespace SuperMetroid.Core.Game;

/// <summary>One fixed bank-$86 pickup animation selection record.</summary>
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
    private static readonly ushort[] InstructionLists =
    [
        0x0000,
        EnemyPickupInstructionProgramDefinitions.SmallEnergy,
        EnemyPickupInstructionProgramDefinitions.BigEnergy,
        EnemyPickupInstructionProgramDefinitions.PowerBombs,
        EnemyPickupInstructionProgramDefinitions.Missiles,
        EnemyPickupInstructionProgramDefinitions.SuperMissiles,
    ];

    /// <summary>Returns the native table offset and instruction list for a table-backed kind.</summary>
    internal static EnemyPickupAnimationDefinition Animation(EnemyPickupKind kind)
    {
        int index = (int)kind;
        if ((uint)index >= InstructionLists.Length)
        {
            throw new InvalidDataException(
                $"Enemy pickup kind ${index:X4} has no animation-table entry.");
        }

        return new EnemyPickupAnimationDefinition(
            checked((ushort)(index * 2)),
            InstructionLists[index]);
    }
}
