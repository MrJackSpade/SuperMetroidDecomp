using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers.Artwork;

/// <summary>
/// The presentation boundary of Multiviola's programs (#641): the bank-$A2 spritemap each
/// <see cref="MultiviolaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MultiviolaFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MultiviolaFrame frame) => frame.Pose switch
    {
        0 => 0xb4aa,
        1 => 0xb4b1,
        2 => 0xb4b8,
        3 => 0xb4bf,
        4 => 0xb4c6,
        5 => 0xb4cd,
        6 => 0xb4d4,
        7 => 0xb4db,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Multiviola pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(MultiviolaFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.MultiviolaSpin0,
        { Pose: 1 } => EnemyFrameId.MultiviolaSpin1,
        { Pose: 2 } => EnemyFrameId.MultiviolaSpin2,
        { Pose: 3 } => EnemyFrameId.MultiviolaSpin3,
        { Pose: 4 } => EnemyFrameId.MultiviolaSpin4,
        { Pose: 5 } => EnemyFrameId.MultiviolaSpin5,
        { Pose: 6 } => EnemyFrameId.MultiviolaSpin6,
        { Pose: 7 } => EnemyFrameId.MultiviolaSpin7,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the MultiviolaFrame programs show."),
    };
}
