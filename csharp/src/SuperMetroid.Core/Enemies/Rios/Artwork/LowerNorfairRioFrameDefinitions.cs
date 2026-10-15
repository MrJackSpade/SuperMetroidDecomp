namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Lower Norfair Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="LowerNorfairRioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class LowerNorfairRioFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 18;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(LowerNorfairRioFrame frame) => frame.Pose switch
    {
        0 => 0xc8bd,
        1 => 0xc8d3,
        2 => 0xc8e9,
        3 => 0xc8ff,
        4 => 0xc915,
        5 => 0xc92b,
        6 => 0xc941,
        7 => 0xc957,
        8 => 0xc96d,
        9 => 0xc983,
        10 => 0xc999,
        11 => 0xc9af,
        12 => 0xc9c5,
        13 => 0xc9db,
        14 => 0xc9f1,
        15 => 0xca07,
        16 => 0xca13,
        17 => 0xca1f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Lower Norfair Rio pose."),
    };
}
