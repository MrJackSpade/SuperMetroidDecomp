namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The routine a wall Space Pirate's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum WallSpacePirateFunction
{
    /// <summary>Watch Samus while continuing the left-wall climbing animation.</summary>
    ClimbingLeftWall = 1,

    /// <summary>The attack list owns presentation until it prepares a rightward jump.</summary>
    AttackAnimationOnLeftWall,

    /// <summary>Follow the cartridge's sine/cosine arc from the left wall to the right.</summary>
    WallJumpingRight,

    /// <summary>Watch Samus while continuing the right-wall climbing animation.</summary>
    ClimbingRightWall,

    /// <summary>The attack list owns presentation until it prepares a leftward jump.</summary>
    AttackAnimationOnRightWall,

    /// <summary>Follow the cartridge's sine/cosine arc from the right wall to the left.</summary>
    WallJumpingLeft,
}
