namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The routine a walking Space Pirate's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum WalkingSpacePirateFunction
{
    /// <summary>The shared RTS copied by initialization and flinch lists.</summary>
    NoOperation = 1,

    /// <summary>Walk left, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingLeft,

    /// <summary>Walk right, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingRight,

    /// <summary>The second RTS used while a firing/look-around list owns presentation.</summary>
    AnimationOwnedNoOperation,
}
