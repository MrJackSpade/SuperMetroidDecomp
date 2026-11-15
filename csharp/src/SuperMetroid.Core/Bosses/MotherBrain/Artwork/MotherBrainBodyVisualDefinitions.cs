using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Mother Brain's body frames: the installed frame each of her walking, posture, initial and hand-beam poses shows, and
/// which of them show a picture of her body beside her limbs. Their hitboxes are not artwork.
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
        EnemyFrameId.MotherBrainBodyWalkingFirst,
        EnemyFrameId.MotherBrainBodyWalkingSecond,
        EnemyFrameId.MotherBrainBodyWalkingThird,
        EnemyFrameId.MotherBrainBodyWalkingFourth,
        EnemyFrameId.MotherBrainBodyWalkingFifth,
        EnemyFrameId.MotherBrainBodyWalkingSixth,
        EnemyFrameId.MotherBrainBodyWalkingSeventh,
        EnemyFrameId.MotherBrainBodyWalkingEighth,
        EnemyFrameId.MotherBrainBodyCrouched,
        EnemyFrameId.MotherBrainBodyUncrouching,
        EnemyFrameId.MotherBrainBodyLeaningDown,
        EnemyFrameId.MotherBrainBodyHandBeamSecond,
        EnemyFrameId.MotherBrainBodyHandBeamThird,
        EnemyFrameId.MotherBrainBodyHandBeamFourth,
        EnemyFrameId.MotherBrainBodyHandBeamFifth,
    };

    /// <summary>The installed frame each <see cref="MotherBrainBodyFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainBodyFrame> ShownFrames { get; } = new()
    {
        { MotherBrainBodyFrame.Standing, EnemyFrameId.MotherBrainBodyStanding },
        { MotherBrainBodyFrame.WalkingFirst, EnemyFrameId.MotherBrainBodyWalkingFirst },
        { MotherBrainBodyFrame.WalkingSecond, EnemyFrameId.MotherBrainBodyWalkingSecond },
        { MotherBrainBodyFrame.WalkingThird, EnemyFrameId.MotherBrainBodyWalkingThird },
        { MotherBrainBodyFrame.WalkingFourth, EnemyFrameId.MotherBrainBodyWalkingFourth },
        { MotherBrainBodyFrame.WalkingFifth, EnemyFrameId.MotherBrainBodyWalkingFifth },
        { MotherBrainBodyFrame.WalkingSixth, EnemyFrameId.MotherBrainBodyWalkingSixth },
        { MotherBrainBodyFrame.WalkingSeventh, EnemyFrameId.MotherBrainBodyWalkingSeventh },
        { MotherBrainBodyFrame.WalkingEighth, EnemyFrameId.MotherBrainBodyWalkingEighth },
        { MotherBrainBodyFrame.Crouched, EnemyFrameId.MotherBrainBodyCrouched },
        { MotherBrainBodyFrame.Uncrouching, EnemyFrameId.MotherBrainBodyUncrouching },
        { MotherBrainBodyFrame.LeaningDown, EnemyFrameId.MotherBrainBodyLeaningDown },
        { MotherBrainBodyFrame.InitialDummy, EnemyFrameId.MotherBrainBodyInitialDummy },
        { MotherBrainBodyFrame.HandBeamSecond, EnemyFrameId.MotherBrainBodyHandBeamSecond },
        { MotherBrainBodyFrame.HandBeamThird, EnemyFrameId.MotherBrainBodyHandBeamThird },
        { MotherBrainBodyFrame.HandBeamFourth, EnemyFrameId.MotherBrainBodyHandBeamFourth },
        { MotherBrainBodyFrame.HandBeamFifth, EnemyFrameId.MotherBrainBodyHandBeamFifth },
    };
}
