using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>One fixed pickup animation choice.</summary>
/// <param name="Animation">The kind's drop animation.</param>
internal readonly partial record struct EnemyPickupAnimationDefinition(
    EnemyDeathAndDropSegment Animation);
