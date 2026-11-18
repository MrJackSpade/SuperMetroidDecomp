using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The state of an unpowered Work Robot (#2179): the pose its placement names, which it holds.</summary>
public sealed class UnpoweredWorkRobotEnemyState
{
    /// <summary>Starts the state holding <paramref name="playback"/>, the pose the robot holds.</summary>
    internal UnpoweredWorkRobotEnemyState(AnimationPlayback<WorkRobotSegment> playback) => Playback = playback;

    /// <summary>The pose the robot holds and where its animation stands (#1600).</summary>
    internal AnimationPlayback<WorkRobotSegment> Playback { get; set; }
}
