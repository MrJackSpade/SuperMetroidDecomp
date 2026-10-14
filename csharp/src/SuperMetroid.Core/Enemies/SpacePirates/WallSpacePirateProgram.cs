namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The wall Space Pirates' climb, jump and laser animations as typed programs (#641), one per bank-$B2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="WallSpacePirateAnimations"/>.
/// </summary>
internal enum WallSpacePirateProgram
{
    /// <summary><c>InstList_PirateWall_FireLaser_WallJumpLeft</c> at $B2:ECC0.</summary>
    FireAndJumpLeft,
    /// <summary><c>InstList_PirateWall_LandedOnLeftWall</c> at $B2:ECE4.</summary>
    LandedOnLeftWall,
    /// <summary><c>InstList_PirateWall_MovingUpLeftWall_0</c> at $B2:ECEC.</summary>
    MovingUpLeftWall,
    /// <summary>The loop at $B2:ECF4.</summary>
    MovingUpLeftWallLoop,
    /// <summary><c>InstList_PirateWall_MovingDownLeftWall_0</c> at $B2:ED36.</summary>
    MovingDownLeftWall,
    /// <summary>The loop at $B2:ED3E.</summary>
    MovingDownLeftWallLoop,
    /// <summary><c>InstList_PirateWall_FireLaser_WallJumpRight</c> at $B2:ED80.</summary>
    FireAndJumpRight,
    /// <summary><c>InstList_PirateWall_LandingOnRightWall</c> at $B2:EDA4.</summary>
    LandedOnRightWall,
    /// <summary><c>InstList_PirateWall_MovingDownRightWall_0</c> at $B2:EDAC.</summary>
    MovingDownRightWall,
    /// <summary>The loop at $B2:EDB4.</summary>
    MovingDownRightWallLoop,
    /// <summary><c>InstList_PirateWall_MovingUpRightWall_0</c> at $B2:EDF6.</summary>
    MovingUpRightWall,
    /// <summary>The loop at $B2:EDFE.</summary>
    MovingUpRightWallLoop,
}
