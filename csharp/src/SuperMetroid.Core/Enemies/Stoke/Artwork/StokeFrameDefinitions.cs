using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Stoke.Artwork;

/// <summary>
/// The presentation boundary of Stoke's programs (#641): the bank-$A2 spritemap each
/// <see cref="StokeFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class StokeFrameDefinitions
{

    /// <summary>The installed frame each <see cref="StokeFrame"/> shows.</summary>
    internal static EnemyFrameTable<StokeFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.StokeWalkLeft0 },
        { new(Pose: 1), EnemyFrameId.StokeWalkLeft1 },
        { new(Pose: 2), EnemyFrameId.StokeWalkLeft2 },
        { new(Pose: 3), EnemyFrameId.StokeWalkLeft3 },
        { new(Pose: 4), EnemyFrameId.StokeAttackLeft },
        { new(Pose: 5), EnemyFrameId.StokeWalkRight0 },
        { new(Pose: 6), EnemyFrameId.StokeWalkRight1 },
        { new(Pose: 7), EnemyFrameId.StokeWalkRight2 },
        { new(Pose: 8), EnemyFrameId.StokeWalkRight3 },
        { new(Pose: 9), EnemyFrameId.StokeAttackRight },
    };
}
