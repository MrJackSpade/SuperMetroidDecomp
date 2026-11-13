namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The wall Space Pirates' climb, jump and laser animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
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
    /// <summary>The loop.</summary>
    MovingUpLeftWallLoop,
    /// <summary>The moving down left wall animation.</summary>
    MovingDownLeftWall,
    /// <summary>The loop.</summary>
    MovingDownLeftWallLoop,
    /// <summary>The fire and jump right animation.</summary>
    FireAndJumpRight,
    /// <summary>The landed on right wall animation.</summary>
    LandedOnRightWall,
    /// <summary>The moving down right wall animation.</summary>
    MovingDownRightWall,
    /// <summary>The loop.</summary>
    MovingDownRightWallLoop,
    /// <summary>The moving up right wall animation.</summary>
    MovingUpRightWall,
    /// <summary>The loop.</summary>
    MovingUpRightWallLoop,
}
