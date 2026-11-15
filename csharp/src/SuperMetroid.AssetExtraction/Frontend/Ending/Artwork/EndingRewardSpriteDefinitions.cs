using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The compositions the suitless and suited reward lists at $8B:ED1D..EE5C show. Counts describe visuals, not actor timing.</summary>
public static class EndingRewardSpriteDefinitions
{
    /// <summary>The 37 published reward compositions in installation order.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("suitless-idle-upper", CinematicSpritePose.SuitlessIdleUpper, 28),
        new("suitless-lower", CinematicSpritePose.SuitlessLower, 14),
        new("suitless-hair-1", CinematicSpritePose.SuitlessPonytailTied, 9),
        new("suitless-hair-2", CinematicSpritePose.SuitlessHandsToPonytail, 10),
        new("suitless-hair-3", CinematicSpritePose.SuitlessUntyingPonytail, 10),
        new("suitless-hair-4", CinematicSpritePose.SuitlessPonytailUntied, 10),
        new("suitless-hair-5", CinematicSpritePose.SuitlessHairFallingLoose, 9),
        new("suitless-hair-6", CinematicSpritePose.SuitlessHairSwinging, 10),
        new("suitless-hair-7", CinematicSpritePose.SuitlessHairSweptBack, 15),
        new("suitless-hair-8", CinematicSpritePose.SuitlessHairFlowing, 13),
        new("suitless-standing", CinematicSpritePose.SuitlessStanding, 28),
        new("suitless-prepare-jump", CinematicSpritePose.SuitlessPrepareJump, 20),
        new("suitless-jumping", CinematicSpritePose.SuitlessJumping, 19),
        new("samus-falling", CinematicSpritePose.SamusFalling, 15),
        new("samus-landing", CinematicSpritePose.SamusLanding, 13),
        new("samus-landed", CinematicSpritePose.SamusLanded, 21),
        new("samus-shooting", CinematicSpritePose.SamusShooting, 15),
        new("suited-idle-body", CinematicSpritePose.SuitedIdleBody, 34),
        new("suited-helmet-head", CinematicSpritePose.SuitedHelmetHead, 4),
        new("helmetless-head-1", CinematicSpritePose.HelmetlessHeadResting, 2),
        new("suited-headless-body", CinematicSpritePose.SuitedHeadlessBody, 30),
        new("suited-arm-1", CinematicSpritePose.SuitedArmLowered, 5),
        new("suited-arm-2", CinematicSpritePose.SuitedArmSwingingUp, 6),
        new("suited-arm-3", CinematicSpritePose.SuitedArmFistForward, 5),
        new("suited-arm-4", CinematicSpritePose.SuitedArmFistRaised, 5),
        new("suited-arm-5", CinematicSpritePose.SuitedArmFistDrawingIn, 5),
        new("suited-arm-6", CinematicSpritePose.SuitedArmFistAtChest, 5),
        new("suited-arm-7", CinematicSpritePose.SuitedArmFistTurning, 5),
        new("suited-arm-8", CinematicSpritePose.SuitedArmFistTurned, 5),
        new("helmetless-head-2", CinematicSpritePose.HelmetlessHeadSmileStarting, 2),
        new("helmetless-head-3", CinematicSpritePose.HelmetlessHeadSmileWidening, 2),
        new("helmetless-head-4", CinematicSpritePose.HelmetlessHeadSmiling, 2),
        new("suited-prepare-jump", CinematicSpritePose.SuitedPrepareJump, 22),
        new("suited-jumping", CinematicSpritePose.SuitedJumping, 20),
        new("suited-helmet-jump-head-1", CinematicSpritePose.SuitedHelmetJumpLevel, 5),
        new("suited-helmet-jump-head-2", CinematicSpritePose.SuitedHelmetJumpTilted, 5),
        new("helmetless-jump-head", CinematicSpritePose.HelmetlessJumpHead, 3),
    ];
}
