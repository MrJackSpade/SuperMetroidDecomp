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
