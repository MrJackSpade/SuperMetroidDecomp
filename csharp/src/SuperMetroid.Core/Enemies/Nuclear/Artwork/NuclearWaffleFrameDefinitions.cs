namespace SuperMetroid.Core.Enemies.Nuclear.Artwork;

/// <summary>
/// The presentation boundary of Nuclear Waffle's programs (#641): the bank-$A6 spritemap each
/// <see cref="NuclearWaffleFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class NuclearWaffleFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 8;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(NuclearWaffleFrame frame) => frame.Pose switch
    {
        0 => 0x9954,
        1 => 0x995b,
        2 => 0x9962,
        3 => 0x9969,
        4 => 0x9970,
        5 => 0x9977,
        6 => 0x997e,
        7 => 0x9985,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Nuclear Waffle pose."),
    };
}
