using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Ordinary bank-$A9 OAM layouts selected by Mother Brain's head hook and
/// falling-tube programs. These identities name artwork, not encounter timing.
/// </summary>
internal static class MotherBrainVisualDefinitions
{

    /// <summary>The installed frame each <see cref="MotherBrainHeadFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainHeadFrame> HeadFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainHead0 },
        { new(Index: 1), EnemyFrameId.MotherBrainHead1 },
        { new(Index: 10), EnemyFrameId.MotherBrainHead10 },
        { new(Index: 11), EnemyFrameId.MotherBrainHead11 },
        { new(Index: 12), EnemyFrameId.MotherBrainHead12 },
        { new(Index: 2), EnemyFrameId.MotherBrainHead2 },
        { new(Index: 3), EnemyFrameId.MotherBrainHead3 },
        { new(Index: 4), EnemyFrameId.MotherBrainHead4 },
        { new(Index: 6), EnemyFrameId.MotherBrainHead6 },
        { new(Index: 7), EnemyFrameId.MotherBrainHead7 },
        { new(Index: 8), EnemyFrameId.MotherBrainHead8 },
        { new(Index: 9), EnemyFrameId.MotherBrainHead9 },
    };

    /// <summary>The neck joint each neck segment shows.</summary>
    internal const EnemyFrameId NeckJointFrame = EnemyFrameId.MotherBrainNeckJoint;

    /// <summary>The installed frame each <see cref="MotherBrainTubeFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainTubeFrame> TubeFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainTube0 },
        { new(Index: 1), EnemyFrameId.MotherBrainTube1 },
        { new(Index: 2), EnemyFrameId.MotherBrainTube2 },
        { new(Index: 3), EnemyFrameId.MotherBrainTube3 },
        { new(Index: 4), EnemyFrameId.MotherBrainTube4 },
    };
}
