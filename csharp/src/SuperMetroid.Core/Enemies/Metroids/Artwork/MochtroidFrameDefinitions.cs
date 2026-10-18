using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids.Artwork;

/// <summary>
/// The presentation boundary of Mochtroid's programs (#641): the bank-$A3 spritemap each
/// <see cref="MochtroidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MochtroidFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MochtroidFrame frame) => frame.Pose switch
    {
        0 => 0xa9b0,
        1 => 0xa9d0,
        2 => 0xa9f0,
        3 => 0xaa06,
        4 => 0xaa1c,
        5 => 0xaa32,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Mochtroid pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(MochtroidFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.MochtroidFlight0,
        { Pose: 1 } => EnemyFrameId.MochtroidFlight1,
        { Pose: 2 } => EnemyFrameId.MochtroidFlight2,
        { Pose: 3 } => EnemyFrameId.MochtroidAttached0,
        { Pose: 4 } => EnemyFrameId.MochtroidAttached1,
        { Pose: 5 } => EnemyFrameId.MochtroidAttached2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the MochtroidFrame programs show."),
    };
}
