using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The poses of one of a Work Robot's feet as it walks facing one way (#2451): the foot swinging from leaning back to leaning
/// forward, and settling once it is down. Each stride is written once over its feet, as a Torizo's animations are written
/// once over a side (#2411).
/// </summary>
/// <param name="LeanBack">The foot leaning back.</param>
/// <param name="EasingUp">The foot easing up.</param>
/// <param name="Upright">The foot upright.</param>
/// <param name="EasingForward">The foot easing forward.</param>
/// <param name="LeanForward">The foot leaning forward.</param>
/// <param name="Settling">The foot settling once it is down.</param>
internal sealed record WorkRobotFoot(F LeanBack, F EasingUp, F Upright, F EasingForward, F LeanForward, F Settling)
{
    /// <summary>The near foot facing left.</summary>
    internal static WorkRobotFoot LeftNear { get; } = new(
        F.WorkRobotWalkingLeftNearFootLeanBack, F.WorkRobotWalkingLeftNearFootEasingUp, F.WorkRobotWalkingLeftNearFootUpright,
        F.WorkRobotWalkingLeftNearFootEasingForward, F.WorkRobotWalkingLeftNearFootLeanForward, F.WorkRobotWalkingLeftNearFootSettling);

    /// <summary>The far foot facing left.</summary>
    internal static WorkRobotFoot LeftFar { get; } = new(
        F.WorkRobotWalkingLeftFarFootLeanBack, F.WorkRobotWalkingLeftFarFootEasingUp, F.WorkRobotWalkingLeftFarFootUpright,
        F.WorkRobotWalkingLeftFarFootEasingForward, F.WorkRobotWalkingLeftFarFootLeanForward, F.WorkRobotWalkingLeftFarFootSettling);

    /// <summary>The near foot facing right.</summary>
    internal static WorkRobotFoot RightNear { get; } = new(
        F.WorkRobotWalkingRightNearFootLeanBack, F.WorkRobotWalkingRightNearFootEasingUp, F.WorkRobotWalkingRightNearFootUpright,
        F.WorkRobotWalkingRightNearFootEasingForward, F.WorkRobotWalkingRightNearFootLeanForward, F.WorkRobotWalkingRightNearFootSettling);

    /// <summary>The far foot facing right.</summary>
    internal static WorkRobotFoot RightFar { get; } = new(
        F.WorkRobotWalkingRightFarFootLeanBack, F.WorkRobotWalkingRightFarFootEasingUp, F.WorkRobotWalkingRightFarFootUpright,
        F.WorkRobotWalkingRightFarFootEasingForward, F.WorkRobotWalkingRightFarFootLeanForward, F.WorkRobotWalkingRightFarFootSettling);
}
