namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The presentation boundary of the Work Robot's laser programs (#641): the installed artwork key of each
/// <see cref="WorkRobotLaserFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class WorkRobotLaserFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 7;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(WorkRobotLaserFrame frame) => frame.Pose switch
    {
        0 => 0xd2ee,
        1 => 0xd2f2,
        2 => 0xd2f6,
        3 => 0xd2fa,
        4 => 0xd2fe,
        5 => 0xd302,
        6 => 0xd306,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Work Robot laser pose."),
    };
}
