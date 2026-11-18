using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The state of an unpowered Work Robot (#2179): the pose its placement names, which it holds, and the fall that a
/// powered robot's definition placed while the ship has no power runs.
/// </summary>
public sealed class UnpoweredWorkRobotEnemyState
{
    /// <summary>Starts the state holding <paramref name="playback"/>, the pose the robot holds.</summary>
    internal UnpoweredWorkRobotEnemyState(AnimationPlayback<WorkRobotSegment> playback) => Playback = playback;

    /// <summary>The pose the robot holds and where its animation stands (#1600).</summary>
    internal AnimationPlayback<WorkRobotSegment> Playback { get; set; }

    /// <summary>
    /// The robot's falling speed in pixels per update, starting at the unpowered fall's speed: a
    /// powered robot's definition placed while the ship has no power falls by it, and gravity adds to it, its
    /// fraction carried into the whole pixels. The unpowered robot's definition takes no action and never
    /// reads it.
    /// </summary>
    public SignedSixteenSixteen FallVelocity { get; internal set; }
}
