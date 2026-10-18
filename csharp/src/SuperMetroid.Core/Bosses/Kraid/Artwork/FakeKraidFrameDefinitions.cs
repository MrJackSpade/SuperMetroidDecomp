using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The presentation boundary of Mini-Kraid's programs (#641): the bank-$A6 spritemap each
/// <see cref="FakeKraidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FakeKraidFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(FakeKraidFrame frame) => frame.Pose switch
    {
        0 => 0x9c64,
        1 => 0x9cb6,
        2 => 0x9d08,
        3 => 0x9d5a,
        4 => 0x9dac,
        5 => 0x9dfe,
        6 => 0x9e50,
        7 => 0x9ea2,
        8 => 0x9ef4,
        9 => 0x9f46,
        10 => 0x9f98,
        11 => 0x9fea,
        12 => 0xa03c,
        13 => 0xa08e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Mini-Kraid pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(FakeKraidFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.FakeKraidWalkLeft0,
        { Pose: 1 } => EnemyFrameId.FakeKraidWalkLeft1,
        { Pose: 10 } => EnemyFrameId.FakeKraidWalkRight3,
        { Pose: 11 } => EnemyFrameId.FakeKraidSpitRight0,
        { Pose: 12 } => EnemyFrameId.FakeKraidSpitRight1,
        { Pose: 13 } => EnemyFrameId.FakeKraidSpitRight2,
        { Pose: 2 } => EnemyFrameId.FakeKraidWalkLeft2,
        { Pose: 3 } => EnemyFrameId.FakeKraidWalkLeft3,
        { Pose: 4 } => EnemyFrameId.FakeKraidSpitLeft0,
        { Pose: 5 } => EnemyFrameId.FakeKraidSpitLeft1,
        { Pose: 6 } => EnemyFrameId.FakeKraidSpitLeft2,
        { Pose: 7 } => EnemyFrameId.FakeKraidWalkRight0,
        { Pose: 8 } => EnemyFrameId.FakeKraidWalkRight1,
        { Pose: 9 } => EnemyFrameId.FakeKraidWalkRight2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the FakeKraidFrame programs show."),
    };
}
