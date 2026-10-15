namespace SuperMetroid.Core.Enemies.Skultera.Artwork;

/// <summary>
/// The presentation boundary of Skultera's programs (#641): the bank-$A3 spritemap each
/// <see cref="SkulteraFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class SkulteraFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SkulteraFrame frame) => frame.Pose switch
    {
        0 => 0x928a,
        1 => 0x92a5,
        2 => 0x92c0,
        3 => 0x92db,
        4 => 0x92f6,
        5 => 0x9311,
        6 => 0x9327,
        7 => 0x933d,
        8 => 0x934e,
        9 => 0x9364,
        10 => 0x937f,
        11 => 0x939a,
        12 => 0x93b5,
        13 => 0x93d0,
        14 => 0x93eb,
        15 => 0x9406,
        16 => 0x9421,
        17 => 0x9437,
        18 => 0x944d,
        19 => 0x945e,
        20 => 0x9474,
        21 => 0x948f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Skultera pose."),
    };
}
