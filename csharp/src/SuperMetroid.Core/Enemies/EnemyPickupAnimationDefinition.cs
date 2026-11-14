using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>One fixed pickup animation choice.</summary>
/// <param name="Animation">The kind's drop animation.</param>
internal readonly record struct EnemyPickupAnimationDefinition(
    EnemyDeathProjectileSegment Animation);
