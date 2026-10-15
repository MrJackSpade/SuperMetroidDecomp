namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The presentation boundary of the Work Robot's programs (#641): the bank-$A8 spritemap each
/// <see cref="WorkRobotFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WorkRobotFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 27;

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
}
