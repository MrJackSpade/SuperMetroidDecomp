using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Owned collision for Mother Brain's frames, read by the
/// shot and contact frame-hitbox tests for both the body and the head, whose dummy list keeps
/// its dummy root for life. Each root lists component offsets, each with the hitbox list
/// holding its rectangles; the sprite and body-layer art the same roots reference is replaceable.
/// </summary>
internal static class MotherBrainCollisionDefinitions
{
    /// <summary>The brain's rectangle, which Samus touches and shoots as the head.</summary>
    internal static readonly EnemyCollisionHitbox[] BrainHitboxes =
    [
        new(-20, -21, 16, 23, MotherBrainReactions.HeadTouch, MotherBrainReactions.HeadShot),
    ];

    /// <summary>The leg rectangle right of the body's origin, near the floor.</summary>
    internal static readonly EnemyCollisionHitbox[] RightLegHitboxes =
    [
        new(-23, -1, 23, 7, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>The leg rectangle left of the body's origin, near the floor.</summary>
    internal static readonly EnemyCollisionHitbox[] LeftLegHitboxes =
    [
        new(-23, -2, 23, 7, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>The torso's two rectangles.</summary>
    internal static readonly EnemyCollisionHitbox[] TorsoHitboxes =
    [
        new(-32, -24, 20, 52, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        new(-24, -42, 13, -25, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>The upper body's two rectangles: the neck, and the arm reaching forward from it.</summary>
    internal static readonly EnemyCollisionHitbox[] UpperBodyHitboxes =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        new(28, -41, 57, -30, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>The upper body as her hand beam's hand curls: the neck, and the arm a little shorter.</summary>
    internal static readonly EnemyCollisionHitbox[] HandCurlingUpperBodyHitboxes =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        new(28, -41, 54, -30, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>The upper body with the hand beam's arm drawn back: the neck, and the short drawn-back arm.</summary>
    internal static readonly EnemyCollisionHitbox[] ArmDrawnBackUpperBodyHitboxes =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        new(29, -43, 45, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>The upper body with the hand beam's arm flung out: the neck, and the arm reaching high and far.</summary>
    internal static readonly EnemyCollisionHitbox[] ArmFlungOutUpperBodyHitboxes =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        new(29, -48, 68, -40, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>The upper body as the hand beam's arm returns: the neck, and the arm a pixel further than standing.</summary>
    internal static readonly EnemyCollisionHitbox[] ArmReturningUpperBodyHitboxes =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        new(28, -41, 58, -31, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>
    /// Each Mother Brain body frame's components: each places one of the hitbox lists at an offset (#2211). Only the
    /// components that hold rectangles are listed: the collision tests walk the rectangles, so a component without one
    /// was never tested.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, EnemyCollisionComponent[]> FrameComponents = new()
    {
        [EnemyFrameId.MotherBrainBodyStanding] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-10, 56), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingNearLegLifting] =
        [
            new(new(28, 47), RightLegHitboxes),
            new(new(0, 2), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-11, 58), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingNearLegReaching] =
        [
            new(new(40, 48), RightLegHitboxes),
            new(new(0, 2), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-13, 58), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting] =
        [
            new(new(40, 51), RightLegHitboxes),
            new(new(0, 1), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-13, 57), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingLegsApart] =
        [
            new(new(36, 58), RightLegHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-16, 56), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingFarLegLifting] =
        [
            new(new(21, 60), RightLegHitboxes),
            new(new(0, 2), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-16, 52), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingFarLegPassing] =
        [
            new(new(15, 64), RightLegHitboxes),
            new(new(0, 6), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-10, 47), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting] =
        [
            new(new(17, 60), RightLegHitboxes),
            new(new(0, 2), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-8, 47), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingLegsTogether] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-10, 56), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyCrouched] =
        [
            new(new(18, 20), RightLegHitboxes),
            new(new(0, -38), TorsoHitboxes),
            new(new(4, 0), UpperBodyHitboxes),
            new(new(-10, 18), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyUncrouching] =
        [
            new(new(18, 30), RightLegHitboxes),
            new(new(0, -28), TorsoHitboxes),
            new(new(-2, 0), UpperBodyHitboxes),
            new(new(-10, 28), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyLeaningDown] =
        [
            new(new(18, 46), RightLegHitboxes),
            new(new(0, -12), TorsoHitboxes),
            new(new(-2, 0), UpperBodyHitboxes),
            new(new(-10, 44), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyInitialDummy] =
        [
            new(new(0, 0), BrainHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyStandingTwin] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), HandCurlingUpperBodyHitboxes),
            new(new(-10, 56), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyStandingTwinB] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), ArmDrawnBackUpperBodyHitboxes),
            new(new(-10, 56), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyStandingTwinC] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), ArmFlungOutUpperBodyHitboxes),
            new(new(-10, 56), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyStandingTwinD] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), ArmReturningUpperBodyHitboxes),
            new(new(-10, 56), LeftLegHitboxes),
        ],
    };
}
