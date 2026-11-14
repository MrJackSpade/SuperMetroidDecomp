namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The behaviour a walking Space Pirate's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum WalkingSpacePirateAction
{
    /// <summary>
    /// Stands while its animation moves and poses it: initialization, flinching, firing and looking around.
    /// </summary>
    FollowingAnimation,

    /// <summary>Walk left, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingLeft,

    /// <summary>Walk right, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingRight,
}
