namespace SuperMetroid.Core.Enemies.Fireflea.Artwork;

/// <summary>
/// The presentation boundary of Fireflea's programs (#641): the bank-$A3 spritemap each
/// <see cref="FirefleaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FirefleaFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 21;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(FirefleaFrame frame) => frame.Pose switch
    {
        0 => 0x8ea5,
        1 => 0x8eb6,
        2 => 0x8ec7,
        3 => 0x8ed8,
        4 => 0x8ee9,
        5 => 0x8efa,
        6 => 0x8f0b,
        7 => 0x8f1c,
        8 => 0x8f2d,
        9 => 0x8f3e,
        10 => 0x8f4f,
        11 => 0x8f60,
        12 => 0x8f71,
        13 => 0x8f82,
        14 => 0x8f93,
        15 => 0x8fa4,
        16 => 0x8fb5,
        17 => 0x8fc6,
        18 => 0x8fd7,
        19 => 0x8fe8,
        20 => 0x8ff9,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Fireflea pose."),
    };
}
