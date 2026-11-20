namespace SuperMetroid.Core.Enemies;

/// <summary>Finds an enemy drop's pickup animation.</summary>
internal static class EnemyPickups
{
    /// <summary>The drop animation of <paramref name="kind"/>.</summary>
    internal static EnemyPickupAnimationDefinition Animation(EnemyPickupKind kind) => EnemyPickupDefinitions.Animations[kind];
}
