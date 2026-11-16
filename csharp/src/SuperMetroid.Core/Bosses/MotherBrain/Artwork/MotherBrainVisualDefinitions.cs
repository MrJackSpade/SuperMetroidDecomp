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
        { MotherBrainHeadFrame.Initial, EnemyFrameId.MotherBrainHeadEyeShut },
        { MotherBrainHeadFrame.NeutralPhaseTwoSecond, EnemyFrameId.MotherBrainHeadEyeOpen },
        { MotherBrainHeadFrame.StretchingPhaseTwoFirst, EnemyFrameId.MotherBrainHeadEyeGlaring },
        { MotherBrainHeadFrame.StretchingPhaseTwoSecond, EnemyFrameId.MotherBrainHeadJawDropping },
        { MotherBrainHeadFrame.StretchingPhaseTwoThird, EnemyFrameId.MotherBrainHeadJawDropped },
        { MotherBrainHeadFrame.DecapitatedLoop, EnemyFrameId.MotherBrainDarkHeadEyeShut },
        { MotherBrainHeadFrame.DecapitatedSecond, EnemyFrameId.MotherBrainDarkHeadEyeOpen },
        { MotherBrainHeadFrame.StretchingPhaseThreeFirst, EnemyFrameId.MotherBrainDarkHeadEyeGlaring },
        { MotherBrainHeadFrame.StretchingPhaseThreeSecond, EnemyFrameId.MotherBrainDarkHeadJawDropping },
        { MotherBrainHeadFrame.StretchingPhaseThreeThird, EnemyFrameId.MotherBrainDarkHeadJawDropped },
        { MotherBrainHeadFrame.CorpseFourth, EnemyFrameId.MotherBrainHeadCorpseSlumping },
        { MotherBrainHeadFrame.CorpseLoop, EnemyFrameId.MotherBrainHeadCorpseSlumped },
    };

    /// <summary>The neck joint each neck segment shows.</summary>
    internal const EnemyFrameId NeckJointFrame = EnemyFrameId.MotherBrainNeckJoint;

    /// <summary>The installed frame each <see cref="MotherBrainTubeFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainTubeFrame> TubeFrames { get; } = new()
    {
        { MotherBrainTubeFrame.BottomLeft, EnemyFrameId.MotherBrainTubeBendRight },
        { MotherBrainTubeFrame.BottomRight, EnemyFrameId.MotherBrainTubeBendLeft },
        { MotherBrainTubeFrame.BottomMiddleLeft, EnemyFrameId.MotherBrainTubeStraightLeft },
        { MotherBrainTubeFrame.BottomMiddleRight, EnemyFrameId.MotherBrainTubeStraightRight },
        { MotherBrainTubeFrame.Main, EnemyFrameId.MotherBrainTubeColumn },
    };
}
