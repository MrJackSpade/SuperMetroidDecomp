namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// How a Shaktool segment moves itself before its animation runs.
/// Its values are arbitrary.
/// </summary>
public enum ShaktoolSegmentMotion
{
    /// <summary>
    /// The segment does not move itself: the anchoring saw always, and every segment while its collision response plays,
    /// until the response's reset restores each segment's own motion.
    /// </summary>
    Idle,
    /// <summary>
    /// Places an arm on its orbit around the preceding segment, then turns the orbit the chain's way round by the segment's
    /// own turn speed, or by the straightening turn while the chain straightens.
    /// </summary>
    OrbitPreviousSegment,
    /// <summary>
    /// Orbits the middle segment as an arm and aims its head halfway between the segments either side of it, a half turn
    /// round while the chain runs reversed, showing one of eight head pictures for that facing.
    /// </summary>
    OrbitAndOrientCenter,
    /// <summary>Collision-tests the final saw's orbit, updates the chain's curl, and reverses the chain's ends when it is obstructed.</summary>
    DriveTailAndReverseAtWalls,
}
