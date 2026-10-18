using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Puyo.Artwork;

/// <summary>
/// The presentation boundary of Puyo's programs (#641): the bank-$A2 spritemap each
/// <see cref="PuyoFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class PuyoFrameDefinitions
{

    /// <summary>The installed frame each <see cref="PuyoFrame"/> shows.</summary>
    internal static EnemyFrameTable<PuyoFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.PuyoGround0 },
        { new(Pose: 1), EnemyFrameId.PuyoGround1 },
        { new(Pose: 2), EnemyFrameId.PuyoGround2 },
        { new(Pose: 3), EnemyFrameId.PuyoAir0 },
        { new(Pose: 4), EnemyFrameId.PuyoAir1 },
        { new(Pose: 5), EnemyFrameId.PuyoAir2 },
        { new(Pose: 6), EnemyFrameId.PuyoAir3 },
        { new(Pose: 7), EnemyFrameId.PuyoAir4 },
    };
}
