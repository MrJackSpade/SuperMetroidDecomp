namespace SuperMetroid.Core.Enemies.Powamp.Artwork;

/// <summary>
/// The presentation boundary of the Powamp spike's programs (#641): the installed artwork key of each
/// <see cref="PowampSpikeFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class PowampSpikeFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 3;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(PowampSpikeFrame frame) => frame.Pose switch
    {
        0 => 0xd20a,
        1 => 0xd20e,
        2 => 0xd212,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Powamp spike pose."),
    };
}
