namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>
/// The presentation boundary of the gunship's hull and entrance pad programs (#641): the bank-$A2 spritemap each
/// <see cref="GunshipFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class GunshipFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 13;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(GunshipFrame frame) => frame.Pose switch
    {
        0 => 0xad81,
        1 => 0xaddd,
        2 => 0xae89,
        3 => 0xae9f,
        4 => 0xaeb5,
        5 => 0xaecb,
        6 => 0xaef5,
        7 => 0xaf1f,
        8 => 0xaf49,
        9 => 0xaf73,
        10 => 0xaf9d,
        11 => 0xafc7,
        12 => 0xafdd,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined gunship pose."),
    };
}
