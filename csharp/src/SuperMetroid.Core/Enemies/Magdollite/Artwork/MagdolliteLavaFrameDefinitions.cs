namespace SuperMetroid.Core.Enemies.Magdollite.Artwork;

/// <summary>
/// The presentation boundary of the Magdollite's lava programs (#641): the installed artwork key of each
/// <see cref="MagdolliteLavaFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class MagdolliteLavaFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 2;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(MagdolliteLavaFrame frame) => frame.Pose switch
    {
        0 => 0xdfda,
        1 => 0xdfe0,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Magdollite lava pose."),
    };
}
