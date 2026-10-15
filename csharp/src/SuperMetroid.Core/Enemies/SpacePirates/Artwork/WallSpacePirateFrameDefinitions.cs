namespace SuperMetroid.Core.Enemies.SpacePirates.Artwork;

/// <summary>
/// The presentation boundary of the wall Space Pirates' programs (#641): the bank-$B2 spritemap each
/// <see cref="WallSpacePirateFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WallSpacePirateFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xb2;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 18;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WallSpacePirateFrame frame) => frame.Pose switch
    {
        0 => 0x88a0,
        1 => 0x88b2,
        2 => 0x88c4,
        3 => 0x88d6,
        4 => 0x88e8,
        5 => 0x88fa,
        6 => 0x890c,
        7 => 0x891e,
        8 => 0x8928,
        9 => 0x8932,
        10 => 0x8944,
        11 => 0x8956,
        12 => 0x8968,
        13 => 0x897a,
        14 => 0x898c,
        15 => 0x899e,
        16 => 0x89b0,
        17 => 0x89ba,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Wall Space Pirate pose."),
    };
}
