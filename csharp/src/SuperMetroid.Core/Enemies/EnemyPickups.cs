namespace SuperMetroid.Core.Enemies;

/// <summary>Finds an enemy drop's pickup animation.</summary>
internal static class EnemyPickups
{
    /// <summary>The drop animation of <paramref name="kind"/>, none for <see cref="EnemyPickupKind.None"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> has no pickup animation.</exception>
    internal static EnemyPickupAnimationDefinition Animation(EnemyPickupKind kind) =>
        EnemyPickupDefinitions.Animations.TryGetValue(kind, out EnemyPickupAnimationDefinition animation)
            ? animation
            : throw new InvalidDataException($"Enemy pickup kind {kind} has no pickup animation.");
}
