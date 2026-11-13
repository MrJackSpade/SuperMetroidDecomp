namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The behaviour a walking Space Pirate's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum WalkingSpacePirateFunction
{
    /// <summary>The shared empty behaviour copied by initialization and flinch animations.</summary>
    NoOperation = 1,

    /// <summary>Walk left, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingLeft,

    /// <summary>Walk right, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingRight,

    /// <summary>The second empty behaviour, used while a firing/look-around animation owns presentation.</summary>
    AnimationOwnedNoOperation,
}
