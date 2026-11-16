namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The behaviour a walking Space Pirate's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum WalkingSpacePirateAction
{
    /// <summary>
    /// Stands still, its main AI idle, while its animation poses it: initialization, flinching, firing and looking around.
    /// </summary>
    StandingStill,

    /// <summary>
    /// Walk its heading's way (<see cref="WalkingSpacePirateEnemyState.Heading"/>), fall when unsupported, and turn at a
    /// wall, ledge, or patrol post.
    /// </summary>
    Walking,
}
