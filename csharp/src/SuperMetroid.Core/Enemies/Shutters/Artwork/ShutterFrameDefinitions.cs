namespace SuperMetroid.Core.Enemies.Shutters.Artwork;

/// <summary>
/// The presentation boundary of the shutters' and Kamer's programs (#641): the bank-$A2 spritemap each
/// <see cref="ShutterFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ShutterFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ShutterFrame frame) => frame.Pose switch
    {
        0 => 0xed44,
        1 => 0xed57,
        2 => 0xed74,
        3 => 0xed9b,
        4 => 0xedb1,
        5 => 0xf468,
        6 => 0xf474,
        7 => 0xf480,
        8 => 0xf48c,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined shutter pose."),
    };
}
