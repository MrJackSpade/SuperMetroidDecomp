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

    /// <summary>The installed frame each <see cref="MultiviolaFrame"/> shows.</summary>
    internal static EnemyFrameTable<MultiviolaFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.MultiviolaSpin0 },
        { new(Pose: 1), EnemyFrameId.MultiviolaSpin1 },
        { new(Pose: 2), EnemyFrameId.MultiviolaSpin2 },
        { new(Pose: 3), EnemyFrameId.MultiviolaSpin3 },
        { new(Pose: 4), EnemyFrameId.MultiviolaSpin4 },
        { new(Pose: 5), EnemyFrameId.MultiviolaSpin5 },
        { new(Pose: 6), EnemyFrameId.MultiviolaSpin6 },
        { new(Pose: 7), EnemyFrameId.MultiviolaSpin7 },
    };
}
