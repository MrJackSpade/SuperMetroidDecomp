using SuperMetroid.Core.Enemies.Artwork;


namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The presentation boundary of Kraid's foot, arm, lints and fingernails (#641): the frames
/// their typed animations show. Gameplay animates <see cref="KraidPartFrame"/> values; only this catalog knows the
/// frames.
/// </summary>
internal static class KraidPartFrameDefinitions
{

    /// <summary>The installed frame each <see cref="KraidPartFrame"/> shows.</summary>
    internal static EnemyFrameTable<KraidPartFrame> ShownFrames { get; } = new()
    {
        { new(Kind: KraidPartFrameKind.Arm, Index: 0), EnemyFrameId.KraidArmNormalFirst },
        { new(Kind: KraidPartFrameKind.Arm, Index: 1), EnemyFrameId.KraidArmNormalSecond },
        { new(Kind: KraidPartFrameKind.Arm, Index: 10), EnemyFrameId.KraidArmRisingOrSinkingFirst },
        { new(Kind: KraidPartFrameKind.Arm, Index: 11), EnemyFrameId.KraidArmRisingOrSinkingSecond },
        { new(Kind: KraidPartFrameKind.Arm, Index: 12), EnemyFrameId.KraidArmRisingOrSinkingThird },
        { new(Kind: KraidPartFrameKind.Arm, Index: 13), EnemyFrameId.KraidArmRisingOrSinkingFourth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 14), EnemyFrameId.KraidArmRisingOrSinkingFifth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 15), EnemyFrameId.KraidArmRisingOrSinkingSixth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 16), EnemyFrameId.KraidArmRisingOrSinkingSeventh },
        { new(Kind: KraidPartFrameKind.Arm, Index: 17), EnemyFrameId.KraidArmRisingOrSinkingEighth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 18), EnemyFrameId.KraidArmRisingOrSinkingNinth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 19), EnemyFrameId.KraidArmRisingOrSinkingTenth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 2), EnemyFrameId.KraidArmNormalThird },
        { new(Kind: KraidPartFrameKind.Arm, Index: 20), EnemyFrameId.KraidArmDyingOrPreparingToLungeSecond },
        { new(Kind: KraidPartFrameKind.Arm, Index: 21), EnemyFrameId.KraidArmDyingOrPreparingToLungeThird },
        { new(Kind: KraidPartFrameKind.Arm, Index: 3), EnemyFrameId.KraidArmNormalFourth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 4), EnemyFrameId.KraidArmNormalFifth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 5), EnemyFrameId.KraidArmNormalSixth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 6), EnemyFrameId.KraidArmNormalSeventh },
        { new(Kind: KraidPartFrameKind.Arm, Index: 7), EnemyFrameId.KraidArmNormalEighth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 8), EnemyFrameId.KraidArmNormalNinth },
        { new(Kind: KraidPartFrameKind.Arm, Index: 9), EnemyFrameId.KraidArmNormalTenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 0), EnemyFrameId.KraidFootNeutral },
        { new(Kind: KraidPartFrameKind.Foot, Index: 1), EnemyFrameId.KraidFootWalkingForwardSecond },
        { new(Kind: KraidPartFrameKind.Foot, Index: 10), EnemyFrameId.KraidFootWalkingForwardEleventh },
        { new(Kind: KraidPartFrameKind.Foot, Index: 11), EnemyFrameId.KraidFootWalkingForwardTwelfth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 12), EnemyFrameId.KraidFootWalkingForwardThirteenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 13), EnemyFrameId.KraidFootWalkingForwardFourteenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 14), EnemyFrameId.KraidFootWalkingForwardFifteenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 15), EnemyFrameId.KraidFootWalkingForwardSixteenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 16), EnemyFrameId.KraidFootWalkingForwardSeventeenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 17), EnemyFrameId.KraidFootWalkingForwardEighteenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 18), EnemyFrameId.KraidFootWalkingForwardNineteenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 19), EnemyFrameId.KraidFootWalkingForwardTwentieth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 2), EnemyFrameId.KraidFootWalkingForwardThird },
        { new(Kind: KraidPartFrameKind.Foot, Index: 20), EnemyFrameId.KraidFootWalkingBackwardThirteenth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 21), EnemyFrameId.KraidFootWalkingForwardTwentyFirst },
        { new(Kind: KraidPartFrameKind.Foot, Index: 22), EnemyFrameId.KraidFootWalkingForwardTwentySecond },
        { new(Kind: KraidPartFrameKind.Foot, Index: 23), EnemyFrameId.KraidFootWalkingForwardTwentyThird },
        { new(Kind: KraidPartFrameKind.Foot, Index: 24), EnemyFrameId.KraidFootWalkingForwardTwentyFourth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 25), EnemyFrameId.KraidFootWalkingForwardTwentyFifth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 26), EnemyFrameId.KraidFootWalkingForwardTwentySixth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 27), EnemyFrameId.KraidFootWalkingForwardTwentySeventh },
        { new(Kind: KraidPartFrameKind.Foot, Index: 28), EnemyFrameId.KraidFootWalkingForwardTwentyEighth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 29), EnemyFrameId.KraidFootWalkingForwardTwentyNinth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 3), EnemyFrameId.KraidFootWalkingForwardFourth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 30), EnemyFrameId.KraidFootWalkingForwardThirtieth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 31), EnemyFrameId.KraidFootWalkingForwardThirtyFirst },
        { new(Kind: KraidPartFrameKind.Foot, Index: 32), EnemyFrameId.KraidFootWalkingBackwardSecond },
        { new(Kind: KraidPartFrameKind.Foot, Index: 33), EnemyFrameId.KraidFootWalkingBackwardFirst },
        { new(Kind: KraidPartFrameKind.Foot, Index: 4), EnemyFrameId.KraidFootWalkingForwardFifth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 5), EnemyFrameId.KraidFootWalkingForwardSixth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 6), EnemyFrameId.KraidFootWalkingForwardSeventh },
        { new(Kind: KraidPartFrameKind.Foot, Index: 7), EnemyFrameId.KraidFootWalkingForwardEighth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 8), EnemyFrameId.KraidFootWalkingForwardNinth },
        { new(Kind: KraidPartFrameKind.Foot, Index: 9), EnemyFrameId.KraidFootWalkingForwardTenth },
        { new(Kind: KraidPartFrameKind.FootStanding, Index: 0), EnemyFrameId.KraidFootStanding },
        { new(Kind: KraidPartFrameKind.Lint, Index: 0), EnemyFrameId.KraidLintInitial },
        { new(Kind: KraidPartFrameKind.Lint, Index: 1), EnemyFrameId.KraidLintPostGrowth },
        { new(Kind: KraidPartFrameKind.Nail, Index: 0), EnemyFrameId.KraidNailLoopFirst },
        { new(Kind: KraidPartFrameKind.Nail, Index: 1), EnemyFrameId.KraidNailLoopSecond },
        { new(Kind: KraidPartFrameKind.Nail, Index: 2), EnemyFrameId.KraidNailLoopThird },
        { new(Kind: KraidPartFrameKind.Nail, Index: 3), EnemyFrameId.KraidNailLoopFourth },
        { new(Kind: KraidPartFrameKind.Nail, Index: 4), EnemyFrameId.KraidNailLoopFifth },
        { new(Kind: KraidPartFrameKind.Nail, Index: 5), EnemyFrameId.KraidNailLoopSixth },
        { new(Kind: KraidPartFrameKind.Nail, Index: 6), EnemyFrameId.KraidNailLoopSeventh },
        { new(Kind: KraidPartFrameKind.Nail, Index: 7), EnemyFrameId.KraidNailLoopEighth },
    };
}
