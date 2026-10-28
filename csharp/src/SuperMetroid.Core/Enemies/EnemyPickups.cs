namespace SuperMetroid.Core.Enemies;

/// <summary>Finds an enemy drop's pickup-table entry.</summary>
internal static class EnemyPickups
{
    /// <summary>The pickup-table offset and drop program of <paramref name="kind"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> is not in the pickup table.</exception>
    internal static EnemyPickupAnimationDefinition Animation(EnemyPickupKind kind) =>
        EnemyPickupDefinitions.Animations.TryGetValue(kind, out EnemyPickupAnimationDefinition animation)
            ? animation
            : throw new InvalidDataException($"Enemy pickup kind {kind} has no animation-table entry.");
}
