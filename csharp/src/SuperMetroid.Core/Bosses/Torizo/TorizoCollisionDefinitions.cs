using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The body hitboxes of all 106 Torizo frames the animations show: each frame names its body's list of touch and shot
/// rectangles, placed at Torizo's origin (#1943). Sprite composition and pixels are not stored here. The blank and turning
/// frames have the empty list rather than invented collision.
/// </summary>
/// <remarks>
/// The single owner of the Torizo collision space. The original game listed each frame's sprite-part
/// components, of which only the body's held rectangles; the others named the empty list and could never be the first
/// hitbox a probe met, so a frame names its body alone. Right-facing frames name lists fitted separately to their
/// drawings, not mirrors of the left-facing ones; the jump-back frames of the two facings are drawn differently.
/// </remarks>
internal static class TorizoCollisionDefinitions
{
    /// <summary>The list of a frame without collision.</summary>
    internal static readonly TorizoHitboxList Empty = new([]);

    /// <summary>The seated right-facing body, narrower on the left than the left-facing one.</summary>
    private static readonly TorizoHitboxList SeatedBodyRight = new([new(-15, -27, 13, 27, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body at the first stand-up stage.</summary>
    private static readonly TorizoHitboxList StandingUpFirstBodyRight = new([new(-13, -27, 13, 27, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body at the second stand-up stage, growing upward to Y -32.</summary>
    private static readonly TorizoHitboxList StandingUpSecondBodyRight = new([new(-14, -32, 13, 33, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body at the third stand-up stage, reaching Y -35 and down to 39.</summary>
    private static readonly TorizoHitboxList StandingUpThirdBodyRight = new([new(-14, -35, 11, 39, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body at the fourth stand-up stage, its upper reach shifted toward positive X.</summary>
    private static readonly TorizoHitboxList StandingUpFourthBodyRight = new([new(-6, -42, 13, 47, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body at the fifth stand-up stage.</summary>
    private static readonly TorizoHitboxList StandingUpFifthBodyRight = new([new(-7, -41, 11, 47, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body at the last stand-up stage, expanding to X 22.</summary>
    private static readonly TorizoHitboxList StandingUpSixthBodyRight = new([new(-8, -41, 22, 47, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>Right-facing active body.</summary>
    private static readonly TorizoHitboxList ActiveBodyRight = new([new(-10, -38, 13, 23, TorizoReactions.Touch, TorizoReactions.Shot)]);
    /// <summary>Right-facing standing body, extending farther down and right than the right-facing jump-back body.</summary>
    private static readonly TorizoHitboxList StandingBodyRight = new([new(-9, -40, 16, 25, TorizoReactions.Touch, TorizoReactions.Shot)]);
    /// <summary>Compact right-facing jump-back body, fitted separately from its left counterpart.</summary>
    private static readonly TorizoHitboxList JumpBackBodyRight = new([new(-8, -37, 15, 14, TorizoReactions.Touch, TorizoReactions.Shot)]);

    /// <summary>The seated body, 32 by 54, with the stand-up/sit-down shot reaction.</summary>
    private static readonly TorizoHitboxList SeatedBody = new([new(-16, -27, 16, 27, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body at the first stand-up stage, narrowed on the left.</summary>
    private static readonly TorizoHitboxList StandingUpFirstBody = new([new(-14, -27, 13, 27, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body at the second stand-up stage, taller.</summary>
    private static readonly TorizoHitboxList StandingUpSecondBody = new([new(-13, -34, 9, 33, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body at the third stand-up stage, centred and extending farther vertically.</summary>
    private static readonly TorizoHitboxList StandingUpThirdBody = new([new(-11, -38, 11, 39, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body at the fourth stand-up stage, its upper reach shifted left.</summary>
    private static readonly TorizoHitboxList StandingUpFourthBody = new([new(-15, -44, 8, 47, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body at the fifth stand-up stage, shortened below the centre.</summary>
    private static readonly TorizoHitboxList StandingUpFifthBody = new([new(-18, -43, 3, 24, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body at the last stand-up stage, narrowing below under the tall upper reach.</summary>
    private static readonly TorizoHitboxList StandingUpSixthBody = new([new(-17, -42, 5, 15, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>Left-facing active body, with the ordinary body shot reaction.</summary>
    private static readonly TorizoHitboxList ActiveBody = new([new(-15, -39, 7, 21, TorizoReactions.Touch, TorizoReactions.Shot)]);
    /// <summary>Compact left-facing jump-back body with its lower edge at Y 9.</summary>
    private static readonly TorizoHitboxList JumpBackBody = new([new(-18, -38, 7, 9, TorizoReactions.Touch, TorizoReactions.Shot)]);
    /// <summary>Left-facing standing body, extending to Y 18.</summary>
    private static readonly TorizoHitboxList StandingBody = new([new(-18, -37, 7, 18, TorizoReactions.Touch, TorizoReactions.Shot)]);

    /// <summary>Each Torizo frame's body hitboxes.</summary>
    internal static readonly Dictionary<EnemyFrameId, TorizoHitboxList> FrameHitboxes = new()
    {
        [EnemyFrameId.BombTorizoDormant] = Empty,
        [EnemyFrameId.TorizoFacingLeftTurningLeft] = Empty,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFirst] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardSecond] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardThird] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFourth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFifth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardSixth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFirst] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSecond] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardThird] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFourth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFifth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSixth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSecond] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardThird] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFourth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFifth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSixth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSeventh] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEighth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardNinth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardTenth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEleventh] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSecond] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardThird] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFourth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFifth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSixth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSeventh] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEighth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardNinth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardTenth] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEleventh] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSeated] = SeatedBody,
        [EnemyFrameId.TorizoFacingLeftStandingUpFirst] = StandingUpFirstBody,
        [EnemyFrameId.TorizoFacingLeftStandingUpSecond] = StandingUpSecondBody,
        [EnemyFrameId.TorizoFacingLeftStandingUpThird] = StandingUpThirdBody,
        [EnemyFrameId.TorizoFacingLeftStandingUpFourth] = StandingUpFourthBody,
        [EnemyFrameId.TorizoFacingLeftStandingUpFifth] = StandingUpFifthBody,
        [EnemyFrameId.TorizoFacingLeftStandingUpSixth] = StandingUpSixthBody,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFirst] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardSecond] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardThird] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFourth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFifth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardSixth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFirst] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardSecond] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardThird] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFourth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFifth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardSixth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSecond] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardThird] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFourth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFifth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSixth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSeventh] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEighth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardNinth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardTenth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEleventh] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSecond] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardThird] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFourth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFifth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSixth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSeventh] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEighth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardNinth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardTenth] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEleventh] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSeated] = SeatedBodyRight,
        [EnemyFrameId.TorizoFacingRightStandingUpFirst] = StandingUpFirstBodyRight,
        [EnemyFrameId.TorizoFacingRightStandingUpSecond] = StandingUpSecondBodyRight,
        [EnemyFrameId.TorizoFacingRightStandingUpThird] = StandingUpThirdBodyRight,
        [EnemyFrameId.TorizoFacingRightStandingUpFourth] = StandingUpFourthBodyRight,
        [EnemyFrameId.TorizoFacingRightStandingUpFifth] = StandingUpFifthBodyRight,
        [EnemyFrameId.TorizoFacingRightStandingUpSixth] = StandingUpSixthBodyRight,
        [EnemyFrameId.TorizoFacingLeftJumpingForwardsFirst] = JumpBackBody,
        [EnemyFrameId.TorizoFallingLeft] = JumpBackBody,
        [EnemyFrameId.TorizoFacingLeftJumpingForwardsThird] = JumpBackBody,
        [EnemyFrameId.TorizoFacingRightJumpingForwardsFirst] = JumpBackBodyRight,
        [EnemyFrameId.TorizoFacingRightJumpingForwardsSecond] = JumpBackBodyRight,
        [EnemyFrameId.TorizoFacingRightJumpingForwardsThird] = JumpBackBodyRight,
    };
}
