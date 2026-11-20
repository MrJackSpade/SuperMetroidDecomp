namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The wall Space Pirates' climb, jump and laser animations (#641), the climbs they repeat and the choice of the next
/// climb on each wall (#2276).
/// Their steps are <see cref="WallSpacePirateAnimations"/>.
/// </summary>
internal enum WallSpacePirateSegment
{
    /// <summary>
    /// Clinging to the right wall, spreading its claws to fire a laser left, then leaping curled for the left wall with
    /// the attack sound.
    /// </summary>
    FireAndJumpLeft,
    /// <summary>Reaching for the left wall as it lands, then climbing up it.</summary>
    LandedOnLeftWall,
    /// <summary>Climbing up the left wall, then choosing its next climb.</summary>
    MovingUpLeftWall,
    /// <summary>The climb up the left wall, which its animation repeats.</summary>
    ClimbingUpLeftWall,
    /// <summary>The choice of the next climb on the left wall, after either climb there (#2276).</summary>
    ChooseLeftWallClimb,
    /// <summary>Climbing down the left wall, then choosing its next climb.</summary>
    MovingDownLeftWall,
    /// <summary>The climb down the left wall, which its animation repeats.</summary>
    ClimbingDownLeftWall,
    /// <summary>
    /// Clinging to the left wall, spreading its claws to fire a laser right, then leaping curled for the right wall
    /// with the attack sound.
    /// </summary>
    FireAndJumpRight,
    /// <summary>Reaching for the right wall as it lands, then climbing down it.</summary>
    LandedOnRightWall,
    /// <summary>Climbing down the right wall, then choosing its next climb.</summary>
    MovingDownRightWall,
    /// <summary>The climb down the right wall, which its animation repeats.</summary>
    ClimbingDownRightWall,
    /// <summary>The choice of the next climb on the right wall, after either climb there (#2276).</summary>
    ChooseRightWallClimb,
    /// <summary>Climbing up the right wall, then choosing its next climb.</summary>
    MovingUpRightWall,
    /// <summary>The climb up the right wall, which its animation repeats.</summary>
    ClimbingUpRightWall,
}
