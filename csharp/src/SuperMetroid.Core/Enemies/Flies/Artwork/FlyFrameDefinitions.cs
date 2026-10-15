namespace SuperMetroid.Core.Enemies.Flies.Artwork;

/// <summary>
/// The presentation boundary of the Mellow, Mella and Memu programs (#641): the bank-$A2 spritemap each
/// <see cref="FlyFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FlyFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(FlyFrame frame) => frame.Pose switch
    {
        0 => 0xb1e8,
        1 => 0xb1ef,
        2 => 0xb1f6,
        3 => 0xb1fd,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined fly-family pose."),
    };
}
