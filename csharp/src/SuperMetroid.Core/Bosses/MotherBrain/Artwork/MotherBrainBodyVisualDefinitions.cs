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
        EnemyFrameId.MotherBrainBodyStandingTwin,
        EnemyFrameId.MotherBrainBodyStandingTwinB,
        EnemyFrameId.MotherBrainBodyStandingTwinC,
        EnemyFrameId.MotherBrainBodyStandingTwinD,
    };

    /// <summary>The installed frame each <see cref="MotherBrainBodyFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainBodyFrame> ShownFrames { get; } = new()
    {
        { MotherBrainBodyFrame.Standing, EnemyFrameId.MotherBrainBodyStanding },
        { MotherBrainBodyFrame.WalkingFirst, EnemyFrameId.MotherBrainBodyWalkingNearLegLifting },
        { MotherBrainBodyFrame.WalkingSecond, EnemyFrameId.MotherBrainBodyWalkingNearLegReaching },
        { MotherBrainBodyFrame.WalkingThird, EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting },
        { MotherBrainBodyFrame.WalkingFourth, EnemyFrameId.MotherBrainBodyWalkingLegsApart },
        { MotherBrainBodyFrame.WalkingFifth, EnemyFrameId.MotherBrainBodyWalkingFarLegLifting },
        { MotherBrainBodyFrame.WalkingSixth, EnemyFrameId.MotherBrainBodyWalkingFarLegPassing },
        { MotherBrainBodyFrame.WalkingSeventh, EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting },
        { MotherBrainBodyFrame.WalkingEighth, EnemyFrameId.MotherBrainBodyWalkingLegsTogether },
        { MotherBrainBodyFrame.Crouched, EnemyFrameId.MotherBrainBodyCrouched },
        { MotherBrainBodyFrame.Uncrouching, EnemyFrameId.MotherBrainBodyUncrouching },
        { MotherBrainBodyFrame.LeaningDown, EnemyFrameId.MotherBrainBodyLeaningDown },
        { MotherBrainBodyFrame.InitialDummy, EnemyFrameId.MotherBrainBodyInitialDummy },
        { MotherBrainBodyFrame.HandBeamSecond, EnemyFrameId.MotherBrainBodyStandingTwin },
        { MotherBrainBodyFrame.HandBeamThird, EnemyFrameId.MotherBrainBodyStandingTwinB },
        { MotherBrainBodyFrame.HandBeamFourth, EnemyFrameId.MotherBrainBodyStandingTwinC },
        { MotherBrainBodyFrame.HandBeamFifth, EnemyFrameId.MotherBrainBodyStandingTwinD },
    };
}
