namespace SuperMetroid.Core.Enemies.Metroids.Artwork;

/// <summary>
/// The presentation boundary of the Metroid's programs (#641): the bank-$A3 spritemap each
/// <see cref="MetroidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MetroidFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 4;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MetroidFrame frame) => frame.Pose switch
    {
        0 => 0xf10d,
        1 => 0xf137,
        2 => 0xf157,
        3 => 0xf181,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Metroid pose."),
    };
}
