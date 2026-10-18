using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids.Artwork;

/// <summary>
/// The presentation boundary of the Metroid's programs (#641): the bank-$A3 spritemap each
/// <see cref="MetroidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MetroidFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MetroidFrame frame) => frame.Pose switch
    {
        0 => 0xf10d,
        1 => 0xf137,
        2 => 0xf157,
        3 => 0xf181,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Metroid pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(MetroidFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.MetroidBody0,
        { Pose: 1 } => EnemyFrameId.MetroidBody1,
        { Pose: 2 } => EnemyFrameId.MetroidBody2,
        { Pose: 3 } => EnemyFrameId.MetroidBody3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the MetroidFrame programs show."),
    };
}
