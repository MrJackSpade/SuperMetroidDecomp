namespace SuperMetroid.Core.Enemies.FaceBlocks.Artwork;

/// <summary>
/// The presentation boundary of the Blue Brinstar face block's programs (#641): the bank-$A8 spritemap each
/// <see cref="BlueBrinstarFaceBlockFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BlueBrinstarFaceBlockFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 5;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BlueBrinstarFaceBlockFrame frame) => frame.Pose switch
    {
        0 => 0xe92c,
        1 => 0xe942,
        2 => 0xe958,
        3 => 0xe96e,
        4 => 0xe984,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Blue Brinstar face block pose."),
    };
}
