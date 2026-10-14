namespace SuperMetroid.Core.Enemies;

/// <summary>One fixed bank-$86 pickup animation selection record.</summary>
internal readonly record struct EnemyPickupAnimationDefinition(
    ushort TableOffset,
    ushort InstructionList);
