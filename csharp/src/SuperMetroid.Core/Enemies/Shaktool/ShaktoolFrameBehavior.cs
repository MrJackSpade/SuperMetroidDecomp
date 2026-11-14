namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// How a Shaktool segment moves itself before its animation runs.
/// Its values are arbitrary; verification's view relates each member to its source (#1412).
/// </summary>
public enum ShaktoolFrameBehavior
{
    /// <summary>
    /// The segment does not move itself: the anchoring saw always, and every segment while its collision response plays,
    /// until the response's reset restores each segment's own motion.
    /// </summary>
    Idle,
    /// <summary>Positions an arm relative to the preceding segment, then advances its neighbour angle by the curling flags.</summary>
    OrbitPreviousSegment,
    /// <summary>Moves the middle segment as an arm and picks one of eight head poses from the adjacent segment angles and the flip flag.</summary>
    OrbitAndOrientCenter,
    /// <summary>Collision-tests the final saw's orbit, updates the chain's curl, and reverses the chain's ends when it is obstructed.</summary>
    DriveTailAndReverseAtWalls,
}
