namespace SuperMetroid.Core.Enemies.Sparks.Artwork;

/// <summary>
/// The presentation boundary of the Wrecked Ship Spark's programs (#641): the bank-$A8 spritemap each
/// <see cref="SparkFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class SparkFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SparkFrame frame) => frame.Pose switch
    {
        0 => 0x804d,
        1 => 0xe71f,
        2 => 0xe72b,
        3 => 0xe737,
        4 => 0xe743,
        5 => 0xe74f,
        6 => 0xe76a,
        7 => 0xe785,
        8 => 0xe79b,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Spark pose."),
    };
}
