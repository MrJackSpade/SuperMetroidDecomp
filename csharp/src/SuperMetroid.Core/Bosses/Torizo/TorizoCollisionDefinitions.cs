using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The body hitboxes of all 106 Torizo frames the animations show: each frame names its body's list of touch and shot
/// rectangles, placed at Torizo's origin (#1943). Sprite composition and pixels are not stored here. The blank and turning
/// frames have the empty list rather than invented collision.
/// </summary>
/// <remarks>
/// The single owner of the Torizo collision space. Only a Torizo's body has hit areas, so a frame names its body's
/// list alone. Right-facing frames name lists fitted separately to their
/// drawings, not mirrors of the left-facing ones; the jump-back frames of the two facings are drawn differently.
/// </remarks>
internal static partial class TorizoCollisionDefinitions
{
    /// <summary>The list of a frame without collision.</summary>
    internal static readonly TorizoHitboxList Empty = new([]);

    /// <summary>The seated right-facing body, narrower on the left than the left-facing one.</summary>
    private static readonly TorizoHitboxList SeatedBodyRight = new([new(-15, -27, 13, 27, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body rising from its seat (#2241).</summary>
    private static readonly TorizoHitboxList RisingFromSeatBodyRight = new([new(-13, -27, 13, 27, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body crouched, growing upward.</summary>
    private static readonly TorizoHitboxList CrouchedBodyRight = new([new(-14, -32, 13, 33, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body half risen.</summary>
    private static readonly TorizoHitboxList HalfRisenBodyRight = new([new(-14, -35, 11, 39, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body nearly upright, its upper reach shifted toward positive X.</summary>
    private static readonly TorizoHitboxList NearlyUprightBodyRight = new([new(-6, -42, 13, 47, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body upright and stepping.</summary>
    private static readonly TorizoHitboxList UprightSteppingBodyRight = new([new(-7, -41, 11, 47, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The right-facing body upright in its wide stance, expanding forward.</summary>
    private static readonly TorizoHitboxList UprightWideStanceBodyRight = new([new(-8, -41, 22, 47, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>Right-facing active body.</summary>
    private static readonly TorizoHitboxList ActiveBodyRight = new([new(-10, -38, 13, 23, TorizoReactions.Touch, TorizoReactions.Shot)]);
    /// <summary>Right-facing standing body, extending farther down and right than the right-facing jump-back body.</summary>
    private static readonly TorizoHitboxList StandingBodyRight = new([new(-9, -40, 16, 25, TorizoReactions.Touch, TorizoReactions.Shot)]);
    /// <summary>Compact right-facing jump-back body, fitted separately from its left counterpart.</summary>
    private static readonly TorizoHitboxList JumpBackBodyRight = new([new(-8, -37, 15, 14, TorizoReactions.Touch, TorizoReactions.Shot)]);

    /// <summary>The seated body, with the stand-up/sit-down shot reaction.</summary>
    private static readonly TorizoHitboxList SeatedBody = new([new(-16, -27, 16, 27, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body rising from its seat (#2241), narrowed on the left.</summary>
    private static readonly TorizoHitboxList RisingFromSeatBody = new([new(-14, -27, 13, 27, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body crouched, taller.</summary>
    private static readonly TorizoHitboxList CrouchedBody = new([new(-13, -34, 9, 33, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body half risen, centred and extending farther vertically.</summary>
    private static readonly TorizoHitboxList HalfRisenBody = new([new(-11, -38, 11, 39, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body nearly upright, its upper reach shifted left.</summary>
    private static readonly TorizoHitboxList NearlyUprightBody = new([new(-15, -44, 8, 47, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body upright and stepping, shortened below the centre.</summary>
    private static readonly TorizoHitboxList UprightSteppingBody = new([new(-18, -43, 3, 24, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>The body upright in its wide stance, narrowing below under the tall upper reach.</summary>
    private static readonly TorizoHitboxList UprightWideStanceBody = new([new(-17, -42, 5, 15, TorizoReactions.Touch, TorizoReactions.StandUpSitDownShot)]);
    /// <summary>Left-facing active body, with the ordinary body shot reaction.</summary>
    private static readonly TorizoHitboxList ActiveBody = new([new(-15, -39, 7, 21, TorizoReactions.Touch, TorizoReactions.Shot)]);
    /// <summary>Compact left-facing jump-back body, its lower edge drawn up.</summary>
    private static readonly TorizoHitboxList JumpBackBody = new([new(-18, -38, 7, 9, TorizoReactions.Touch, TorizoReactions.Shot)]);
    /// <summary>Left-facing standing body.</summary>
    private static readonly TorizoHitboxList StandingBody = new([new(-18, -37, 7, 18, TorizoReactions.Touch, TorizoReactions.Shot)]);

    /// <summary>Each Torizo frame's body hitboxes.</summary>
    internal static readonly Dictionary<EnemyFrameId, TorizoHitboxList> FrameHitboxes = new()
    {
        [EnemyFrameId.BombTorizoDormant] = Empty,
        [EnemyFrameId.TorizoFacingViewer] = Empty,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegLifting] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegSwinging] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegReaching] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanting] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanted] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegLifting] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegSwinging] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegReaching] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanting] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanted] = ActiveBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadLowered] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadRaised] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingMouthOpening] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmThrust] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmRaised] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmDrawnBack] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadLowered] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadRaised] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingMouthOpening] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmThrust] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmRaised] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmDrawnBack] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmDown] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLifting] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLevel] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmRaised] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmOverhead] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmDown] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLifting] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLevel] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmRaised] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmOverhead] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmDown] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLifting] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLevel] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmRaised] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmOverhead] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmDown] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLifting] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLevel] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmRaised] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmOverhead] = StandingBody,
        [EnemyFrameId.TorizoFacingLeftSeated] = SeatedBody,
        [EnemyFrameId.TorizoFacingLeftRisingFromSeat] = RisingFromSeatBody,
        [EnemyFrameId.TorizoFacingLeftCrouched] = CrouchedBody,
        [EnemyFrameId.TorizoFacingLeftHalfRisen] = HalfRisenBody,
        [EnemyFrameId.TorizoFacingLeftNearlyUpright] = NearlyUprightBody,
        [EnemyFrameId.TorizoFacingLeftUprightStepping] = UprightSteppingBody,
        [EnemyFrameId.TorizoFacingLeftUprightWideStance] = UprightWideStanceBody,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegLifting] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegSwinging] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegReaching] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanting] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanted] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegLifting] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegSwinging] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegReaching] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegPlanting] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightWalkingRightLegPlanted] = ActiveBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadLowered] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadRaised] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingMouthOpening] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmThrust] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmRaised] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmDrawnBack] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadLowered] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadRaised] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardSpewingMouthOpening] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmThrust] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmRaised] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmDrawnBack] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmDown] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLifting] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLevel] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmRaised] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmOverhead] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmDown] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLifting] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLevel] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmRaised] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmOverhead] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardNearArmDown] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLifting] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLevel] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardNearArmRaised] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardNearArmOverhead] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardFarArmDown] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLifting] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLevel] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardFarArmRaised] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightRightFootForwardFarArmOverhead] = StandingBodyRight,
        [EnemyFrameId.TorizoFacingRightSeated] = SeatedBodyRight,
        [EnemyFrameId.TorizoFacingRightRisingFromSeat] = RisingFromSeatBodyRight,
        [EnemyFrameId.TorizoFacingRightCrouched] = CrouchedBodyRight,
        [EnemyFrameId.TorizoFacingRightHalfRisen] = HalfRisenBodyRight,
        [EnemyFrameId.TorizoFacingRightNearlyUpright] = NearlyUprightBodyRight,
        [EnemyFrameId.TorizoFacingRightUprightStepping] = UprightSteppingBodyRight,
        [EnemyFrameId.TorizoFacingRightUprightWideStance] = UprightWideStanceBodyRight,
        [EnemyFrameId.TorizoFacingLeftJumpCrouch] = JumpBackBody,
        [EnemyFrameId.TorizoFacingLeftSpringing] = JumpBackBody,
        [EnemyFrameId.TorizoFacingLeftLeaping] = JumpBackBody,
        [EnemyFrameId.TorizoFacingRightJumpCrouch] = JumpBackBodyRight,
        [EnemyFrameId.TorizoFacingRightSpringing] = JumpBackBodyRight,
        [EnemyFrameId.TorizoFacingRightLeaping] = JumpBackBodyRight,
    };
}
