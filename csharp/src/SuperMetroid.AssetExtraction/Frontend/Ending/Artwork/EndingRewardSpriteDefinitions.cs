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
        new("suitless-hair-1", CinematicSpritePose.SuitlessHair1, 9),
        new("suitless-hair-2", CinematicSpritePose.SuitlessHair2, 10),
        new("suitless-hair-3", CinematicSpritePose.SuitlessHair3, 10),
        new("suitless-hair-4", CinematicSpritePose.SuitlessHair4, 10),
        new("suitless-hair-5", CinematicSpritePose.SuitlessHair5, 9),
        new("suitless-hair-6", CinematicSpritePose.SuitlessHair6, 10),
        new("suitless-hair-7", CinematicSpritePose.SuitlessHair7, 15),
        new("suitless-hair-8", CinematicSpritePose.SuitlessHair8, 13),
        new("suitless-standing", CinematicSpritePose.SuitlessStanding, 28),
        new("suitless-prepare-jump", CinematicSpritePose.SuitlessPrepareJump, 20),
        new("suitless-jumping", CinematicSpritePose.SuitlessJumping, 19),
        new("samus-falling", CinematicSpritePose.SamusFalling, 15),
        new("samus-landing", CinematicSpritePose.SamusLanding, 13),
        new("samus-landed", CinematicSpritePose.SamusLanded, 21),
        new("samus-shooting", CinematicSpritePose.SamusShooting, 15),
        new("suited-idle-body", CinematicSpritePose.SuitedIdleBody, 34),
        new("suited-helmet-head", CinematicSpritePose.SuitedHelmetHead, 4),
        new("helmetless-head-1", CinematicSpritePose.HelmetlessHead1, 2),
        new("suited-headless-body", CinematicSpritePose.SuitedHeadlessBody, 30),
        new("suited-arm-1", CinematicSpritePose.SuitedArm1, 5),
        new("suited-arm-2", CinematicSpritePose.SuitedArm2, 6),
        new("suited-arm-3", CinematicSpritePose.SuitedArm3, 5),
        new("suited-arm-4", CinematicSpritePose.SuitedArm4, 5),
        new("suited-arm-5", CinematicSpritePose.SuitedArm5, 5),
        new("suited-arm-6", CinematicSpritePose.SuitedArm6, 5),
        new("suited-arm-7", CinematicSpritePose.SuitedArm7, 5),
        new("suited-arm-8", CinematicSpritePose.SuitedArm8, 5),
        new("helmetless-head-2", CinematicSpritePose.HelmetlessHead2, 2),
        new("helmetless-head-3", CinematicSpritePose.HelmetlessHead3, 2),
        new("helmetless-head-4", CinematicSpritePose.HelmetlessHead4, 2),
        new("suited-prepare-jump", CinematicSpritePose.SuitedPrepareJump, 22),
        new("suited-jumping", CinematicSpritePose.SuitedJumping, 20),
        new("suited-helmet-jump-head-1", CinematicSpritePose.SuitedHelmetJumpHead1, 5),
        new("suited-helmet-jump-head-2", CinematicSpritePose.SuitedHelmetJumpHead2, 5),
        new("helmetless-jump-head", CinematicSpritePose.HelmetlessJumpHead, 3),
    ];
}
