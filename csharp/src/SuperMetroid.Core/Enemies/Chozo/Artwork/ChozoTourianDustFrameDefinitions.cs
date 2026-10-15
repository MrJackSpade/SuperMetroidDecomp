namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The presentation boundary of the Wrecked Ship Chozo's and Tourian statue's dust programs (#641): the installed artwork key of each
/// <see cref="ChozoTourianDustFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class ChozoTourianDustFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 14;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(ChozoTourianDustFrame frame) => frame.Pose switch
    {
        0 => 0xaecc,
        1 => 0xaed0,
        2 => 0xaed4,
        3 => 0xaed8,
        4 => 0xaee4,
        5 => 0xaee8,
        6 => 0xaeec,
        7 => 0xaef0,
        8 => 0xaef4,
        9 => 0xaef8,
        10 => 0xaf22,
        11 => 0xaf26,
        12 => 0xaf2a,
        13 => 0xaf2e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Chozo/Tourian dust pose."),
    };
}
