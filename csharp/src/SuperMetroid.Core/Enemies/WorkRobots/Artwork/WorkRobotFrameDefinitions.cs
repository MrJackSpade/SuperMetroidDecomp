using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The presentation boundary of the Work Robot's programs (#641): the bank-$A8 spritemap each
/// <see cref="WorkRobotFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WorkRobotFrameDefinitions
{

    /// <summary>The installed frame each <see cref="WorkRobotFrame"/> shows.</summary>
    internal static EnemyFrameTable<WorkRobotFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.WorkRobotLeft00 },
        { new(Pose: 1), EnemyFrameId.WorkRobotLeft01 },
        { new(Pose: 10), EnemyFrameId.WorkRobotLeft10 },
        { new(Pose: 11), EnemyFrameId.WorkRobotLeft11 },
        { new(Pose: 12), EnemyFrameId.WorkRobotRight00 },
        { new(Pose: 13), EnemyFrameId.WorkRobotRight01 },
        { new(Pose: 14), EnemyFrameId.WorkRobotRight02 },
        { new(Pose: 15), EnemyFrameId.WorkRobotRight03 },
        { new(Pose: 16), EnemyFrameId.WorkRobotRight04 },
        { new(Pose: 17), EnemyFrameId.WorkRobotRight05 },
        { new(Pose: 18), EnemyFrameId.WorkRobotRight06 },
        { new(Pose: 19), EnemyFrameId.WorkRobotRight07 },
        { new(Pose: 2), EnemyFrameId.WorkRobotLeft02 },
        { new(Pose: 20), EnemyFrameId.WorkRobotRight08 },
        { new(Pose: 21), EnemyFrameId.WorkRobotRight09 },
        { new(Pose: 22), EnemyFrameId.WorkRobotRight10 },
        { new(Pose: 23), EnemyFrameId.WorkRobotRight11 },
        { new(Pose: 24), EnemyFrameId.WorkRobotUnpoweredLeft },
        { new(Pose: 25), EnemyFrameId.WorkRobotUnpoweredNeutral },
        { new(Pose: 26), EnemyFrameId.WorkRobotUnpoweredRight },
        { new(Pose: 3), EnemyFrameId.WorkRobotLeft03 },
        { new(Pose: 4), EnemyFrameId.WorkRobotLeft04 },
        { new(Pose: 5), EnemyFrameId.WorkRobotLeft05 },
        { new(Pose: 6), EnemyFrameId.WorkRobotLeft06 },
        { new(Pose: 7), EnemyFrameId.WorkRobotLeft07 },
        { new(Pose: 8), EnemyFrameId.WorkRobotLeft08 },
        { new(Pose: 9), EnemyFrameId.WorkRobotLeft09 },
    };
}
