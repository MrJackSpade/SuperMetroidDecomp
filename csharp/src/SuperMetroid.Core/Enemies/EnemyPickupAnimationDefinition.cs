using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>One fixed bank-$86 pickup animation selection record.</summary>
/// <param name="TableOffset">The kind's offset in the pickup table, which the pickup keeps in variable 0.</param>
/// <param name="Program">The kind's drop program, or none for the table's empty first entry.</param>
internal readonly record struct EnemyPickupAnimationDefinition(
    ushort TableOffset,
    EnemyDeathProjectileProgram? Program);
