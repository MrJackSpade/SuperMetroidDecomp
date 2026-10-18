using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The presentation boundary of the Work Robot's programs (#641): the bank-$A8 spritemap each
/// <see cref="WorkRobotFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WorkRobotFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WorkRobotFrame frame) => frame.Pose switch
    {
        0 => 0xd1f1,
        1 => 0xd22f,
        2 => 0xd26d,
        3 => 0xd2ab,
        4 => 0xd2e9,
        5 => 0xd327,
        6 => 0xd365,
        7 => 0xd3a3,
        8 => 0xd3e1,
        9 => 0xd41f,
        10 => 0xd45d,
        11 => 0xd49b,
        12 => 0xd4d9,
        13 => 0xd517,
        14 => 0xd555,
        15 => 0xd593,
        16 => 0xd5d1,
        17 => 0xd60f,
        18 => 0xd64d,
        19 => 0xd68b,
        20 => 0xd6c9,
        21 => 0xd707,
        22 => 0xd745,
        23 => 0xd783,
        24 => 0xd7c1,
        25 => 0xd7e1,
        26 => 0xd801,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Work Robot pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(WorkRobotFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.WorkRobotLeft00,
        { Pose: 1 } => EnemyFrameId.WorkRobotLeft01,
        { Pose: 10 } => EnemyFrameId.WorkRobotLeft10,
        { Pose: 11 } => EnemyFrameId.WorkRobotLeft11,
        { Pose: 12 } => EnemyFrameId.WorkRobotRight00,
        { Pose: 13 } => EnemyFrameId.WorkRobotRight01,
        { Pose: 14 } => EnemyFrameId.WorkRobotRight02,
        { Pose: 15 } => EnemyFrameId.WorkRobotRight03,
        { Pose: 16 } => EnemyFrameId.WorkRobotRight04,
        { Pose: 17 } => EnemyFrameId.WorkRobotRight05,
        { Pose: 18 } => EnemyFrameId.WorkRobotRight06,
        { Pose: 19 } => EnemyFrameId.WorkRobotRight07,
        { Pose: 2 } => EnemyFrameId.WorkRobotLeft02,
        { Pose: 20 } => EnemyFrameId.WorkRobotRight08,
        { Pose: 21 } => EnemyFrameId.WorkRobotRight09,
        { Pose: 22 } => EnemyFrameId.WorkRobotRight10,
        { Pose: 23 } => EnemyFrameId.WorkRobotRight11,
        { Pose: 24 } => EnemyFrameId.WorkRobotUnpoweredLeft,
        { Pose: 25 } => EnemyFrameId.WorkRobotUnpoweredNeutral,
        { Pose: 26 } => EnemyFrameId.WorkRobotUnpoweredRight,
        { Pose: 3 } => EnemyFrameId.WorkRobotLeft03,
        { Pose: 4 } => EnemyFrameId.WorkRobotLeft04,
        { Pose: 5 } => EnemyFrameId.WorkRobotLeft05,
        { Pose: 6 } => EnemyFrameId.WorkRobotLeft06,
        { Pose: 7 } => EnemyFrameId.WorkRobotLeft07,
        { Pose: 8 } => EnemyFrameId.WorkRobotLeft08,
        { Pose: 9 } => EnemyFrameId.WorkRobotLeft09,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the WorkRobotFrame programs show."),
    };
}
