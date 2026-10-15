namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The presentation boundary of Mini-Kraid's programs (#641): the bank-$A6 spritemap each
/// <see cref="FakeKraidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FakeKraidFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(FakeKraidFrame frame) => frame.Pose switch
    {
        0 => 0x9c64,
        1 => 0x9cb6,
        2 => 0x9d08,
        3 => 0x9d5a,
        4 => 0x9dac,
        5 => 0x9dfe,
        6 => 0x9e50,
        7 => 0x9ea2,
        8 => 0x9ef4,
        9 => 0x9f46,
        10 => 0x9f98,
        11 => 0x9fea,
        12 => 0xa03c,
        13 => 0xa08e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Mini-Kraid pose."),
    };
}
