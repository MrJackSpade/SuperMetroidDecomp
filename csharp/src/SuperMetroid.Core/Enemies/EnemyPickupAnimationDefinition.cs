using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>One fixed pickup animation selection record.</summary>
/// <param name="Animation">The kind's drop program, or none for the table's empty first entry.</param>
internal readonly record struct EnemyPickupAnimationDefinition(
    EnemyDeathProjectileSegment? Animation);
