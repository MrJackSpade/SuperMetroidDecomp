namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The presentation boundary of the Chozo statue's programs (#641): the bank-$AA spritemap each
/// <see cref="ChozoStatueFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ChozoStatueFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 26;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ChozoStatueFrame frame) => frame.Pose switch
    {
        0 => 0xe7dd,
        1 => 0xe839,
        2 => 0xe890,
        3 => 0xe8e7,
        4 => 0xe943,
        5 => 0xe9ae,
        6 => 0xea1e,
        7 => 0xea8e,
        8 => 0xeafe,
        9 => 0xeb69,
        10 => 0xebd9,
        11 => 0xec49,
        12 => 0xecb9,
        13 => 0xed15,
        14 => 0xed71,
        15 => 0xedcd,
        16 => 0xee29,
        17 => 0xefd8,
        18 => 0xf034,
        19 => 0xf08b,
        20 => 0xf0e2,
        21 => 0xf4b4,
        22 => 0xf510,
        23 => 0xf56c,
        24 => 0xf5c8,
        25 => 0xf624,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Chozo statue pose."),
    };
}
