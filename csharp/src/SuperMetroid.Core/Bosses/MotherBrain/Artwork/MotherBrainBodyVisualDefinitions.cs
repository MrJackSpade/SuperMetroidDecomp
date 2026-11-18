using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Mother Brain's body frames (#2210): her walking, posture, initial and hand-beam frames. Their hitboxes are not artwork.
/// </summary>
internal static class MotherBrainBodyVisualDefinitions
{
    /// <summary>
    /// Every body frame her animations show (#2210), each showing a picture of her body beside her limbs (#1466, #1949)
    /// where the import places its frame against her (#2031).
    /// </summary>
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

        EnemyFrameId.MotherBrainBodyHandCurling,
        EnemyFrameId.MotherBrainBodyArmDrawnBack,
        EnemyFrameId.MotherBrainBodyArmFlungOut,
        EnemyFrameId.MotherBrainBodyArmReturning,
    ];
}
