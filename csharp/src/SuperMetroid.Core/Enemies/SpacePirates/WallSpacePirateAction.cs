namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The behaviour a wall Space Pirate's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum WallSpacePirateAction
{
    /// <summary>Watch Samus while continuing the left-wall climbing animation.</summary>
    ClimbingLeftWall = 1,

    /// <summary>Holds on its wall while its attack animation fires and readies the jump to the other wall.</summary>
    Attacking,

    /// <summary>Follow the game's sine/cosine arc from the left wall to the right.</summary>
    WallJumpingRight,

    /// <summary>Watch Samus while continuing the right-wall climbing animation.</summary>
    ClimbingRightWall,

    /// <summary>Follow the game's sine/cosine arc from the right wall to the left.</summary>
    WallJumpingLeft,
}
