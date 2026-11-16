using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Ordinary sprite layouts selected by Mother Brain's brain draw and
/// falling-tube animations. These identities name artwork, not encounter timing.
/// </summary>
internal static class MotherBrainVisualDefinitions
{

    /// <summary>
    /// Every brain frame her head animations show (#2210): the whole and the dark head at each mouth stage, and the corpse.
    /// </summary>
    internal static IReadOnlyList<EnemyFrameId> HeadFrames { get; } =
    [
        EnemyFrameId.MotherBrainHeadEyeShut, EnemyFrameId.MotherBrainHeadEyeOpen, EnemyFrameId.MotherBrainHeadEyeGlaring,
        EnemyFrameId.MotherBrainHeadJawDropping, EnemyFrameId.MotherBrainHeadJawDropped, EnemyFrameId.MotherBrainDarkHeadEyeShut,
        EnemyFrameId.MotherBrainDarkHeadEyeOpen, EnemyFrameId.MotherBrainDarkHeadEyeGlaring, EnemyFrameId.MotherBrainDarkHeadJawDropping,
        EnemyFrameId.MotherBrainDarkHeadJawDropped, EnemyFrameId.MotherBrainHeadCorpseSlumping, EnemyFrameId.MotherBrainHeadCorpseSlumped,
    ];

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
