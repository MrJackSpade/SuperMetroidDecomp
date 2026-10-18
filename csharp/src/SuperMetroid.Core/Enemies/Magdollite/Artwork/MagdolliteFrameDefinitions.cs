using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite.Artwork;

/// <summary>
/// The presentation boundary of the Magdollite's programs (#641): the bank-$A8 spritemap each
/// <see cref="MagdolliteFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MagdolliteFrameDefinitions
{

    /// <summary>The installed frame each <see cref="MagdolliteFrame"/> shows.</summary>
    internal static EnemyFrameTable<MagdolliteFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.MagdolliteLeftIdle0 },
        { new(Pose: 1), EnemyFrameId.MagdolliteLeftIdle1 },
        { new(Pose: 10), EnemyFrameId.MagdolliteLeftSubmerge2 },
        { new(Pose: 11), EnemyFrameId.MagdolliteRightIdle0 },
        { new(Pose: 12), EnemyFrameId.MagdolliteRightIdle1 },
        { new(Pose: 13), EnemyFrameId.MagdolliteRightIdle2 },
        { new(Pose: 14), EnemyFrameId.MagdolliteRightThrow0 },
        { new(Pose: 15), EnemyFrameId.MagdolliteRightThrow1 },
        { new(Pose: 16), EnemyFrameId.MagdolliteRightThrow2 },
        { new(Pose: 17), EnemyFrameId.MagdolliteRightThrow3 },
        { new(Pose: 18), EnemyFrameId.MagdolliteRightSubmerge0 },
        { new(Pose: 19), EnemyFrameId.MagdolliteRightSubmerge1 },
        { new(Pose: 2), EnemyFrameId.MagdolliteLeftIdle2 },
        { new(Pose: 20), EnemyFrameId.MagdolliteRightSubmerge2 },
        { new(Pose: 21), EnemyFrameId.MagdollitePillarPhase0 },
        { new(Pose: 22), EnemyFrameId.MagdollitePillarPhase1 },
        { new(Pose: 23), EnemyFrameId.MagdollitePillarPhase2 },
        { new(Pose: 24), EnemyFrameId.MagdollitePillarPhase3 },
        { new(Pose: 25), EnemyFrameId.MagdollitePillarPhase4 },
        { new(Pose: 26), EnemyFrameId.MagdollitePillarPhase5 },
        { new(Pose: 27), EnemyFrameId.MagdollitePillarPhase6 },
        { new(Pose: 28), EnemyFrameId.MagdollitePillarPhase7 },
        { new(Pose: 3), EnemyFrameId.MagdolliteLeftThrow0 },
        { new(Pose: 4), EnemyFrameId.MagdolliteLeftThrow1 },
        { new(Pose: 5), EnemyFrameId.MagdolliteLeftThrow2 },
        { new(Pose: 6), EnemyFrameId.MagdolliteLeftThrow3 },
        { new(Pose: 7), EnemyFrameId.MagdollitePillarCap },
        { new(Pose: 8), EnemyFrameId.MagdolliteLeftSubmerge0 },
        { new(Pose: 9), EnemyFrameId.MagdolliteLeftSubmerge1 },
    };
}
