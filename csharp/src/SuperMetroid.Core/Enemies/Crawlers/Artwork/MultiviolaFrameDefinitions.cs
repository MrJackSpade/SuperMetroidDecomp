namespace SuperMetroid.Core.Enemies.Crawlers.Artwork;

/// <summary>
/// The presentation boundary of Multiviola's programs (#641): the bank-$A2 spritemap each
/// <see cref="MultiviolaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MultiviolaFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MultiviolaFrame frame) => frame.Pose switch
    {
        0 => 0xb4aa,
        1 => 0xb4b1,
        2 => 0xb4b8,
        3 => 0xb4bf,
        4 => 0xb4c6,
        5 => 0xb4cd,
        6 => 0xb4d4,
        7 => 0xb4db,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Multiviola pose."),
    };
}
