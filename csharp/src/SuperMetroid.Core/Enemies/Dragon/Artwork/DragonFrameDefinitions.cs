namespace SuperMetroid.Core.Enemies.Dragon.Artwork;

/// <summary>
/// The presentation boundary of Dragon's programs (#641): the bank-$A2 spritemap each
/// <see cref="DragonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class DragonFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 12;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(DragonFrame frame) => frame.Pose switch
    {
        0 => 0xe80c,
        1 => 0xe836,
        2 => 0xe860,
        3 => 0xe88a,
        4 => 0xe8b4,
        5 => 0xe8bb,
        6 => 0xe8c2,
        7 => 0xe8ec,
        8 => 0xe916,
        9 => 0xe940,
        10 => 0xe96a,
        11 => 0xe971,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Dragon pose."),
    };
}
