using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Mother Brain's body frames (#2210): her walking, posture, initial and hand-beam frames, and which of them show a picture
/// of her body beside her limbs. Their hitboxes are not artwork.
/// </summary>
internal static class MotherBrainBodyVisualDefinitions
{
    /// <summary>
    /// The body frames that show a picture of her body (#1466, #1949): every pose but the initial dummy, which shows only
    /// limbs. Each picture lies where the import places its frame against her (#2031).
    /// </summary>
    internal static IReadOnlySet<EnemyFrameId> BodyPictureFrames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.MotherBrainBodyStanding,
        EnemyFrameId.MotherBrainBodyWalkingNearLegLifting,
        EnemyFrameId.MotherBrainBodyWalkingNearLegReaching,
        EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting,
        EnemyFrameId.MotherBrainBodyWalkingLegsApart,
        EnemyFrameId.MotherBrainBodyWalkingFarLegLifting,
        EnemyFrameId.MotherBrainBodyWalkingFarLegPassing,
        EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting,
        EnemyFrameId.MotherBrainBodyWalkingLegsTogether,
        EnemyFrameId.MotherBrainBodyCrouched,
        EnemyFrameId.MotherBrainBodyUncrouching,
        EnemyFrameId.MotherBrainBodyLeaningDown,
        EnemyFrameId.MotherBrainBodyHandCurling,
        EnemyFrameId.MotherBrainBodyArmDrawnBack,
        EnemyFrameId.MotherBrainBodyArmFlungOut,
        EnemyFrameId.MotherBrainBodyArmReturning,
    };

    /// <summary>Every body frame her animations show (#2210): her body pictures' and the initial dummy.</summary>
    internal static IReadOnlyList<EnemyFrameId> AnimationFrames { get; } =
    [
        EnemyFrameId.MotherBrainBodyStanding,
        EnemyFrameId.MotherBrainBodyWalkingNearLegLifting,
        EnemyFrameId.MotherBrainBodyWalkingNearLegReaching,
        EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting,
        EnemyFrameId.MotherBrainBodyWalkingLegsApart,
        EnemyFrameId.MotherBrainBodyWalkingFarLegLifting,
        EnemyFrameId.MotherBrainBodyWalkingFarLegPassing,
        EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting,
        EnemyFrameId.MotherBrainBodyWalkingLegsTogether,
        EnemyFrameId.MotherBrainBodyCrouched,
        EnemyFrameId.MotherBrainBodyUncrouching,
        EnemyFrameId.MotherBrainBodyLeaningDown,
        EnemyFrameId.MotherBrainBodyInitialDummy,
        EnemyFrameId.MotherBrainBodyHandCurling,
        EnemyFrameId.MotherBrainBodyArmDrawnBack,
        EnemyFrameId.MotherBrainBodyArmFlungOut,
        EnemyFrameId.MotherBrainBodyArmReturning,
    ];
}
