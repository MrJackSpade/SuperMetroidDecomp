using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The state of an unpowered Work Robot (#2179): the pose its placement names, which it holds, and the fall that a
/// powered robot's definition placed while the ship has no power runs.
/// </summary>
/// <param name="playback">The pose the robot holds.</param>
public sealed class UnpoweredWorkRobotEnemyState(AnimationPlayback<WorkRobotSegment> playback)
{
    /// <summary>The pose the robot holds and where its animation stands (#1600).</summary>
    internal AnimationPlayback<WorkRobotSegment> Playback { get; set; } = playback;

    /// <summary>
    /// The robot's signed 1/65536-pixel falling speed in pixels per update, starting at the unpowered fall's speed: a
    /// powered robot's definition placed while the ship has no power falls by it, and gravity accumulates it with the
    /// fraction's overflow into the whole part and wraparound. The unpowered robot's definition takes no action and never
    /// reads it.
    /// </summary>
    public SignedSixteenSixteen FallVelocity { get; internal set; }
}
