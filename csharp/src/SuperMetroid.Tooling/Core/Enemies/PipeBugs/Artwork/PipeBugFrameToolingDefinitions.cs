namespace SuperMetroid.Core.Enemies.PipeBugs.Artwork;

/// <summary>Development-tool members of <see cref="PipeBugFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class PipeBugFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xb3;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PipeBugFrame frame) => frame.Pose switch
    {
        0 => 0x89b7,
        1 => 0x89be,
        2 => 0x89c5,
        3 => 0x89cc,
        4 => 0x89d3,
        5 => 0x89da,
        6 => 0x89e1,
        7 => 0x89e8,
        8 => 0x89ef,
        9 => 0x89f6,
        10 => 0x8a6d,
        11 => 0x8a74,
        12 => 0x8a7b,
        13 => 0x8a82,
        14 => 0x8a89,
        15 => 0x8a90,
        16 => 0x8a97,
        17 => 0x8a9e,
        18 => 0x8aa5,
        19 => 0x8aac,
        20 => 0x8ab3,
        21 => 0x8aba,
        22 => 0x8e96,
        23 => 0x8e9d,
        24 => 0x8ea4,
        25 => 0x8eab,
        26 => 0x8eb2,
        27 => 0x8eb9,
        28 => 0x8ec0,
        29 => 0x8ec7,
        30 => 0x8ece,
        31 => 0x8ed5,
        32 => 0x92ad,
        33 => 0x92b4,
        34 => 0x92bb,
        35 => 0x92c2,
        36 => 0x92c9,
        37 => 0x92d0,
        38 => 0x92d7,
        39 => 0x92de,
        40 => 0x92e5,
        41 => 0x92ec,
        42 => 0x92f3,
        43 => 0x92fa,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Pipe Bug pose."),
    };
}
