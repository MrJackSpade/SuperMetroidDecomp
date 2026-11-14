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
        { MotherBrainHeadFrame.Initial, EnemyFrameId.MotherBrainHeadInitial },
        { MotherBrainHeadFrame.NeutralPhaseTwoSecond, EnemyFrameId.MotherBrainHeadNeutralPhaseTwoSecond },
        { MotherBrainHeadFrame.StretchingPhaseTwoFirst, EnemyFrameId.MotherBrainHeadStretchingPhaseTwoFirst },
        { MotherBrainHeadFrame.StretchingPhaseTwoSecond, EnemyFrameId.MotherBrainHeadStretchingPhaseTwoSecond },
        { MotherBrainHeadFrame.StretchingPhaseTwoThird, EnemyFrameId.MotherBrainHeadStretchingPhaseTwoThird },
        { MotherBrainHeadFrame.DecapitatedLoop, EnemyFrameId.MotherBrainHeadDecapitatedLoop },
        { MotherBrainHeadFrame.DecapitatedSecond, EnemyFrameId.MotherBrainHeadDecapitatedSecond },
        { MotherBrainHeadFrame.StretchingPhaseThreeFirst, EnemyFrameId.MotherBrainHeadStretchingPhaseThreeFirst },
        { MotherBrainHeadFrame.StretchingPhaseThreeSecond, EnemyFrameId.MotherBrainHeadStretchingPhaseThreeSecond },
        { MotherBrainHeadFrame.StretchingPhaseThreeThird, EnemyFrameId.MotherBrainHeadStretchingPhaseThreeThird },
        { MotherBrainHeadFrame.CorpseFourth, EnemyFrameId.MotherBrainHeadCorpseFourth },
        { MotherBrainHeadFrame.CorpseLoop, EnemyFrameId.MotherBrainHeadCorpseLoop },
    };

    /// <summary>The neck joint each neck segment shows.</summary>
    internal const EnemyFrameId NeckJointFrame = EnemyFrameId.MotherBrainNeckJoint;

    /// <summary>The installed frame each <see cref="MotherBrainTubeFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainTubeFrame> TubeFrames { get; } = new()
    {
        { MotherBrainTubeFrame.BottomLeft, EnemyFrameId.MotherBrainTubeFirst },
        { MotherBrainTubeFrame.BottomRight, EnemyFrameId.MotherBrainTubeSecond },
        { MotherBrainTubeFrame.BottomMiddleLeft, EnemyFrameId.MotherBrainTubeThird },
        { MotherBrainTubeFrame.BottomMiddleRight, EnemyFrameId.MotherBrainTubeFourth },
        { MotherBrainTubeFrame.Main, EnemyFrameId.MotherBrainTubeFifth },
    };
}
