using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Mother Brain's body frames (#2210): her walking, posture, initial and hand-beam frames. Their hitboxes are not artwork.
/// </summary>
internal static partial class MotherBrainBodyVisualDefinitions
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

    /// <summary>
    /// The sheet frame showing her far leg as <paramref name="frame"/> shows it (#2633, #2634): every body frame shows it as a
    /// picture of its own beside the frame's, which the game draws behind the room's front tiles and her body.
    /// </summary>
    internal static string SecondPictureName(EnemyFrameId frame) => $"FarLeg.{EnemySheetDefinitions.FrameName(frame)}";

    /// <summary>The second pictures <paramref name="enemy"/>'s sheet holds beside its frames': her body's far legs, none for any other enemy.</summary>
    internal static IEnumerable<string> SecondPictureNamesOf(SuperMetroid.Core.Enemies.EnemyDefinitionId enemy) =>
        enemy == SuperMetroid.Core.Enemies.EnemyDefinitionId.MotherBrainBody ? AnimationFrames.Select(SecondPictureName) : [];
}
