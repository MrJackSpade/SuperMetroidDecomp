using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The frames Mother Brain's head and neck show. These identities name artwork, not encounter timing.
/// </summary>
internal static partial class MotherBrainVisualDefinitions
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
}
