using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The presentation boundary of Mini-Kraid's programs (#641): the bank-$A6 spritemap each
/// <see cref="FakeKraidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FakeKraidFrameDefinitions
{

    /// <summary>The installed frame each <see cref="FakeKraidFrame"/> shows.</summary>
    internal static EnemyFrameTable<FakeKraidFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.FakeKraidWalkLeft0 },
        { new(Pose: 1), EnemyFrameId.FakeKraidWalkLeft1 },
        { new(Pose: 10), EnemyFrameId.FakeKraidWalkRight3 },
        { new(Pose: 11), EnemyFrameId.FakeKraidSpitRight0 },
        { new(Pose: 12), EnemyFrameId.FakeKraidSpitRight1 },
        { new(Pose: 13), EnemyFrameId.FakeKraidSpitRight2 },
        { new(Pose: 2), EnemyFrameId.FakeKraidWalkLeft2 },
        { new(Pose: 3), EnemyFrameId.FakeKraidWalkLeft3 },
        { new(Pose: 4), EnemyFrameId.FakeKraidSpitLeft0 },
        { new(Pose: 5), EnemyFrameId.FakeKraidSpitLeft1 },
        { new(Pose: 6), EnemyFrameId.FakeKraidSpitLeft2 },
        { new(Pose: 7), EnemyFrameId.FakeKraidWalkRight0 },
        { new(Pose: 8), EnemyFrameId.FakeKraidWalkRight1 },
        { new(Pose: 9), EnemyFrameId.FakeKraidWalkRight2 },
    };
}
