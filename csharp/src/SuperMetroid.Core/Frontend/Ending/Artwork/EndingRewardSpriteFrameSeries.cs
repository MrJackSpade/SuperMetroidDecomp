namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Ordered reward-frame series that the native lists and stock compositions step through.</summary>
internal static class EndingRewardSpriteFrameSeries
{
    /// <summary>The eight suitless hair-opening frames, in display order.</summary>
    internal static IReadOnlyList<EndingRewardSpriteFrame> HairOpening { get; } =
    [
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame1,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame2,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame3,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame4,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame5,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame6,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame7,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame8,
    ];

    /// <summary>The eight suited arm-gesture frames, in display order.</summary>
    internal static IReadOnlyList<EndingRewardSpriteFrame> Arm { get; } =
    [
        EndingRewardSpriteFrame.SamusArmFromEndingFrame1,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame2,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame3,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame4,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame5,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame6,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame7,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame8,
    ];

    /// <summary>The four helmetless head-turn frames, in display order.</summary>
    internal static IReadOnlyList<EndingRewardSpriteFrame> HelmetlessHead { get; } =
    [
        EndingRewardSpriteFrame.SamusHeadFromEndingFrame1,
        EndingRewardSpriteFrame.SamusHeadFromEndingFrame2,
        EndingRewardSpriteFrame.SamusHeadFromEndingFrame3,
        EndingRewardSpriteFrame.SamusHeadFromEndingFrame4,
    ];

    /// <summary>The four small falling, landing, landed and shooting figures.</summary>
    internal static IReadOnlyList<EndingRewardSpriteFrame> ShootingScene { get; } =
    [
        EndingRewardSpriteFrame.SamusFalling,
        EndingRewardSpriteFrame.SamusLanding,
        EndingRewardSpriteFrame.SamusLanded,
        EndingRewardSpriteFrame.SamusShooting,
    ];
}
