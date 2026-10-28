using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The compositions the suitless and suited reward lists at $8B:ED1D..EE5C show. Counts describe visuals, not actor timing.</summary>
public static class EndingRewardSpriteDefinitions
{
    /// <summary>The 37 published reward compositions in installation order.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("suitless-idle-upper", new(165), 28),
        new("suitless-lower", new(166), 14),
        new("suitless-hair-1", new(167), 9),
        new("suitless-hair-2", new(168), 10),
        new("suitless-hair-3", new(169), 10),
        new("suitless-hair-4", new(170), 10),
        new("suitless-hair-5", new(171), 9),
        new("suitless-hair-6", new(172), 10),
        new("suitless-hair-7", new(173), 15),
        new("suitless-hair-8", new(174), 13),
        new("suitless-standing", new(175), 28),
        new("suitless-prepare-jump", new(176), 20),
        new("suitless-jumping", new(177), 19),
        new("samus-falling", new(178), 15),
        new("samus-landing", new(179), 13),
        new("samus-landed", new(180), 21),
        new("samus-shooting", new(181), 15),
        new("suited-idle-body", new(182), 34),
        new("suited-helmet-head", new(183), 4),
        new("helmetless-head-1", new(184), 2),
        new("suited-headless-body", new(185), 30),
        new("suited-arm-1", new(186), 5),
        new("suited-arm-2", new(187), 6),
        new("suited-arm-3", new(188), 5),
        new("suited-arm-4", new(189), 5),
        new("suited-arm-5", new(190), 5),
        new("suited-arm-6", new(191), 5),
        new("suited-arm-7", new(192), 5),
        new("suited-arm-8", new(193), 5),
        new("helmetless-head-2", new(194), 2),
        new("helmetless-head-3", new(195), 2),
        new("helmetless-head-4", new(196), 2),
        new("suited-prepare-jump", new(197), 22),
        new("suited-jumping", new(198), 20),
        new("suited-helmet-jump-head-1", new(199), 5),
        new("suited-helmet-jump-head-2", new(200), 5),
        new("helmetless-jump-head", new(201), 3),
    ];

    /// <summary>The cinematic pose that shows each <see cref="EndingRewardSpriteFrame"/>.</summary>
    internal static readonly Dictionary<EndingRewardSpriteFrame, CinematicSpritePose> FramePoses = new()
    {
        [EndingRewardSpriteFrame.LargeSamusFromEndingStanding] = new(182),
        [EndingRewardSpriteFrame.LargeSamusFromEndingPreparingToJump] = new(197),
        [EndingRewardSpriteFrame.LargeSamusFromEndingJumping] = new(198),
        [EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame1] = new(199),
        [EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame2] = new(200),
        [EndingRewardSpriteFrame.JumpingSamusHeadFromEnding] = new(201),
        [EndingRewardSpriteFrame.SamusArmFromEndingFrame1] = new(186),
        [EndingRewardSpriteFrame.SamusArmFromEndingFrame2] = new(187),
        [EndingRewardSpriteFrame.SamusArmFromEndingFrame3] = new(188),
        [EndingRewardSpriteFrame.SamusArmFromEndingFrame4] = new(189),
        [EndingRewardSpriteFrame.SamusArmFromEndingFrame5] = new(190),
        [EndingRewardSpriteFrame.SamusArmFromEndingFrame6] = new(191),
        [EndingRewardSpriteFrame.SamusArmFromEndingFrame7] = new(192),
        [EndingRewardSpriteFrame.SamusArmFromEndingFrame8] = new(193),
        [EndingRewardSpriteFrame.SamusHeadFromEndingFrame1] = new(184),
        [EndingRewardSpriteFrame.SamusHeadFromEndingFrame2] = new(194),
        [EndingRewardSpriteFrame.SamusHeadFromEndingFrame3] = new(195),
        [EndingRewardSpriteFrame.SamusHeadFromEndingFrame4] = new(196),
        [EndingRewardSpriteFrame.SamusHeadWithHelmetFromEnding] = new(183),
        [EndingRewardSpriteFrame.HeadlessArmlessSuitedSamus] = new(185),
        [EndingRewardSpriteFrame.SamusFalling] = new(178),
        [EndingRewardSpriteFrame.SamusLanding] = new(179),
        [EndingRewardSpriteFrame.SamusLanded] = new(180),
        [EndingRewardSpriteFrame.SamusShooting] = new(181),
        [EndingRewardSpriteFrame.SuitlessSamusStanding] = new(175),
        [EndingRewardSpriteFrame.SuitlessSamusPreparingToJump] = new(176),
        [EndingRewardSpriteFrame.SuitlessSamusJumping] = new(177),
        [EndingRewardSpriteFrame.SuitlessSamusStandingArmsStraight] = new(165),
        [EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame1] = new(167),
        [EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame2] = new(168),
        [EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame3] = new(169),
        [EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame4] = new(170),
        [EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame5] = new(171),
        [EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame6] = new(172),
        [EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame7] = new(173),
        [EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame8] = new(174),
        [EndingRewardSpriteFrame.SuitlessSamusLowerBody] = new(166),
    };
}
