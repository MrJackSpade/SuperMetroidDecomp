using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Ordinary sprite layouts selected by Mother Brain's brain draw and
/// falling-tube animations. These identities name artwork, not encounter timing.
/// </summary>
internal static class MotherBrainVisualDefinitions
{

    /// <summary>The installed frame each <see cref="MotherBrainHeadFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainHeadFrame> HeadFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainHeadInitial },
        { new(Index: 1), EnemyFrameId.MotherBrainHeadNeutralPhaseTwoSecond },
        { new(Index: 10), EnemyFrameId.MotherBrainHeadStretchingPhaseThreeThird },
        { new(Index: 11), EnemyFrameId.MotherBrainHeadCorpseFourth },
        { new(Index: 12), EnemyFrameId.MotherBrainHeadCorpseLoop },
        { new(Index: 2), EnemyFrameId.MotherBrainHeadStretchingPhaseTwoFirst },
        { new(Index: 3), EnemyFrameId.MotherBrainHeadStretchingPhaseTwoSecond },
        { new(Index: 4), EnemyFrameId.MotherBrainHeadStretchingPhaseTwoThird },
        { new(Index: 6), EnemyFrameId.MotherBrainHeadDecapitatedLoop },
        { new(Index: 7), EnemyFrameId.MotherBrainHeadDecapitatedSecond },
        { new(Index: 8), EnemyFrameId.MotherBrainHeadStretchingPhaseThreeFirst },
        { new(Index: 9), EnemyFrameId.MotherBrainHeadStretchingPhaseThreeSecond },
    };

    /// <summary>The neck joint each neck segment shows.</summary>
    internal const EnemyFrameId NeckJointFrame = EnemyFrameId.MotherBrainNeckJoint;

    /// <summary>The installed frame each <see cref="MotherBrainTubeFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainTubeFrame> TubeFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainTubeFirst },
        { new(Index: 1), EnemyFrameId.MotherBrainTubeSecond },
        { new(Index: 2), EnemyFrameId.MotherBrainTubeThird },
        { new(Index: 3), EnemyFrameId.MotherBrainTubeFourth },
        { new(Index: 4), EnemyFrameId.MotherBrainTubeFifth },
    };
}
