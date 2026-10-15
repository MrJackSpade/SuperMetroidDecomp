namespace SuperMetroid.Core.Enemies.Owtch.Artwork;

/// <summary>
/// The presentation boundary of Owtch's programs (#641): the bank-$A2 spritemap each
/// <see cref="OwtchFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class OwtchFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(OwtchFrame frame) => frame.Pose switch
    {
        0 => 0xa589,
        1 => 0xa590,
        2 => 0xa597,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Owtch pose."),
    };
}
