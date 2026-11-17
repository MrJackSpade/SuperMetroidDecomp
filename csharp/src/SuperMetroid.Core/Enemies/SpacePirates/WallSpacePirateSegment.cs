namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The wall Space Pirates' climb, jump and laser animations (#641): one segment per animation a spawn, branch or wake
/// enters, the climbs they repeat and the choice of the next climb on each wall (#2276), in authored order.
/// Their steps are <see cref="WallSpacePirateAnimations"/>.
/// </summary>
internal enum WallSpacePirateSegment
{
    /// <summary>The fire and jump left animation.</summary>
    FireAndJumpLeft,
    /// <summary>The landed on left wall animation.</summary>
    LandedOnLeftWall,
    /// <summary>The moving up left wall animation.</summary>
    MovingUpLeftWall,
    /// <summary>The climb up the left wall, which its animation repeats.</summary>
    ClimbingUpLeftWall,
    /// <summary>The choice of the next climb on the left wall, after either climb there (#2276).</summary>
    ChooseLeftWallClimb,
    /// <summary>The moving down left wall animation.</summary>
    MovingDownLeftWall,
    /// <summary>The climb down the left wall, which its animation repeats.</summary>
    ClimbingDownLeftWall,
    /// <summary>The fire and jump right animation.</summary>
    FireAndJumpRight,
    /// <summary>The landed on right wall animation.</summary>
    LandedOnRightWall,
    /// <summary>The moving down right wall animation.</summary>
    MovingDownRightWall,
    /// <summary>The climb down the right wall, which its animation repeats.</summary>
    ClimbingDownRightWall,
    /// <summary>The choice of the next climb on the right wall, after either climb there (#2276).</summary>
    ChooseRightWallClimb,
    /// <summary>The moving up right wall animation.</summary>
    MovingUpRightWall,
    /// <summary>The climb up the right wall, which its animation repeats.</summary>
    ClimbingUpRightWall,
}
