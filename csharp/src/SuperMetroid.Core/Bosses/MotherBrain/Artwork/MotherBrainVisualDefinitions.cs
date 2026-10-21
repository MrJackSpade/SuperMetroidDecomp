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
        { new(Index: 0), EnemyFrameId.MotherBrainA9A586 },
        { new(Index: 1), EnemyFrameId.MotherBrainA9A5bf },
        { new(Index: 10), EnemyFrameId.MotherBrainA9A789 },
        { new(Index: 11), EnemyFrameId.MotherBrainA9Ad3e },
        { new(Index: 12), EnemyFrameId.MotherBrainA9Ad6d },
        { new(Index: 2), EnemyFrameId.MotherBrainA9A5f8 },
        { new(Index: 3), EnemyFrameId.MotherBrainA9A62c },
        { new(Index: 4), EnemyFrameId.MotherBrainA9A660 },
        { new(Index: 6), EnemyFrameId.MotherBrainA9A69b },
        { new(Index: 7), EnemyFrameId.MotherBrainA9A6d9 },
        { new(Index: 8), EnemyFrameId.MotherBrainA9A717 },
        { new(Index: 9), EnemyFrameId.MotherBrainA9A750 },
    };

    /// <summary>The neck joint each neck segment shows.</summary>
    internal const EnemyFrameId NeckJointFrame = EnemyFrameId.MotherBrainA9A694;

    /// <summary>The installed frame each <see cref="MotherBrainTubeFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainTubeFrame> TubeFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainA9Ada1 },
        { new(Index: 1), EnemyFrameId.MotherBrainA9Add5 },
        { new(Index: 2), EnemyFrameId.MotherBrainA9Ae09 },
        { new(Index: 3), EnemyFrameId.MotherBrainA9Ae33 },
        { new(Index: 4), EnemyFrameId.MotherBrainA9Ae5d },
    };
}
