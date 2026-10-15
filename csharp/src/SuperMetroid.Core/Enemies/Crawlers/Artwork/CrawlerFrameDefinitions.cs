namespace SuperMetroid.Core.Enemies.Crawlers.Artwork;

/// <summary>
/// The presentation boundary of the crawlers' programs (#641): the bank-$A3 spritemap each
/// <see cref="CrawlerFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CrawlerFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CrawlerFrame frame) => frame.Pose switch
    {
        0 => 0x9703,
        1 => 0x9719,
        2 => 0x972f,
        3 => 0x9745,
        4 => 0x975b,
        5 => 0x9771,
        6 => 0x9787,
        7 => 0x979d,
        8 => 0x97b3,
        9 => 0x97c9,
        10 => 0x97df,
        11 => 0x97f5,
        12 => 0x995b,
        13 => 0x996c,
        14 => 0x997d,
        15 => 0x998e,
        16 => 0x999f,
        17 => 0x99b0,
        18 => 0x99c1,
        19 => 0x99d2,
        20 => 0x99e3,
        21 => 0x99f4,
        22 => 0x9a05,
        23 => 0x9a16,
        24 => 0x9b37,
        25 => 0x9b48,
        26 => 0x9b59,
        27 => 0x9b6a,
        28 => 0xb689,
        29 => 0xb690,
        30 => 0xb697,
        31 => 0xb69e,
        32 => 0xb6a5,
        33 => 0xb6ac,
        34 => 0xb6b3,
        35 => 0xb6ba,
        36 => 0xe2e8,
        37 => 0xe2fe,
        38 => 0xe314,
        39 => 0xe32a,
        40 => 0xe340,
        41 => 0xe3c4,
        42 => 0xe3da,
        43 => 0xe3f0,
        44 => 0xe406,
        45 => 0xe41c,
        46 => 0xe432,
        47 => 0xe448,
        48 => 0xe45e,
        49 => 0xe474,
        50 => 0xe48a,
        51 => 0xe50e,
        52 => 0xe524,
        53 => 0xe53a,
        54 => 0xe550,
        55 => 0xe566,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined crawler pose."),
    };
}
