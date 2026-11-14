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

    /// <summary>A component with no rectangle.</summary>
    internal static readonly EnemyCollisionHitbox[] NoHitboxes =
        [];

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

    /// <summary>The upper body's two rectangles: the neck, and the jaw reaching forward from it.</summary>
    internal static readonly EnemyCollisionHitbox[] UpperBodyHitboxes =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        new(28, -41, 57, -30, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
    ];

    /// <summary>The upper body's rectangles in each of the four death beam frames, as the jaw opens and closes.</summary>
    internal static readonly EnemyCollisionHitbox[][] DeathBeamUpperBodyHitboxes =
    [
        [
            new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
            new(28, -41, 54, -30, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        ],
        [
            new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
            new(29, -43, 45, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        ],
        [
            new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
            new(29, -48, 68, -40, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        ],
        [
            new(4, -59, 28, -24, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
            new(28, -41, 58, -31, EnemyReaction.Ignore, MotherBrainReactions.BodyShot),
        ],
    ];

    /// <summary>Each Mother Brain body frame's components: each places one of the hitbox lists at an offset.</summary>
    internal static readonly Dictionary<EnemyFrameId, EnemyCollisionComponent[]> FrameComponents = new()
    {
        [EnemyFrameId.MotherBrainBodyStanding] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(30, 29), NoHitboxes),
            new(new(25, 30), NoHitboxes),
            new(new(0, -4), NoHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-10, 56), LeftLegHitboxes),
            new(new(7, 28), NoHitboxes),
            new(new(2, 29), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingFirst] =
        [
            new(new(28, 47), RightLegHitboxes),
            new(new(38, 19), NoHitboxes),
            new(new(33, 19), NoHitboxes),
            new(new(0, -3), NoHitboxes),
            new(new(0, 2), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-11, 58), LeftLegHitboxes),
            new(new(6, 31), NoHitboxes),
            new(new(1, 33), NoHitboxes),
            new(new(-25, -3), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingSecond] =
        [
            new(new(40, 48), RightLegHitboxes),
            new(new(38, 19), NoHitboxes),
            new(new(35, 19), NoHitboxes),
            new(new(0, -3), NoHitboxes),
            new(new(0, 2), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-13, 58), LeftLegHitboxes),
            new(new(5, 31), NoHitboxes),
            new(new(-1, 33), NoHitboxes),
            new(new(-26, -3), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingThird] =
        [
            new(new(40, 51), RightLegHitboxes),
            new(new(38, 21), NoHitboxes),
            new(new(35, 22), NoHitboxes),
            new(new(-1, -2), NoHitboxes),
            new(new(0, 1), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-13, 57), LeftLegHitboxes),
            new(new(5, 30), NoHitboxes),
            new(new(-1, 32), NoHitboxes),
            new(new(-26, -4), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingFourth] =
        [
            new(new(36, 58), RightLegHitboxes),
            new(new(33, 29), NoHitboxes),
            new(new(31, 30), NoHitboxes),
            new(new(1, -4), NoHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-16, 56), LeftLegHitboxes),
            new(new(3, 28), NoHitboxes),
            new(new(-4, 30), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingFifth] =
        [
            new(new(21, 60), RightLegHitboxes),
            new(new(29, 31), NoHitboxes),
            new(new(27, 32), NoHitboxes),
            new(new(-2, -2), NoHitboxes),
            new(new(0, 2), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-16, 52), LeftLegHitboxes),
            new(new(0, 26), NoHitboxes),
            new(new(-5, 28), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingSixth] =
        [
            new(new(15, 64), RightLegHitboxes),
            new(new(30, 35), NoHitboxes),
            new(new(26, 37), NoHitboxes),
            new(new(-2, -1), NoHitboxes),
            new(new(0, 6), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-10, 47), LeftLegHitboxes),
            new(new(4, 22), NoHitboxes),
            new(new(-1, 24), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingSeventh] =
        [
            new(new(17, 60), RightLegHitboxes),
            new(new(30, 32), NoHitboxes),
            new(new(24, 32), NoHitboxes),
            new(new(-1, -2), NoHitboxes),
            new(new(0, 2), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-8, 47), LeftLegHitboxes),
            new(new(7, 21), NoHitboxes),
            new(new(3, 23), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyWalkingEighth] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(31, 30), NoHitboxes),
            new(new(25, 30), NoHitboxes),
            new(new(-1, -3), NoHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), UpperBodyHitboxes),
            new(new(-10, 56), LeftLegHitboxes),
            new(new(7, 28), NoHitboxes),
            new(new(2, 31), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyCrouched] =
        [
            new(new(18, 20), RightLegHitboxes),
            new(new(34, -9), NoHitboxes),
            new(new(31, -6), NoHitboxes),
            new(new(-5, 3), NoHitboxes),
            new(new(0, -38), TorsoHitboxes),
            new(new(4, 0), UpperBodyHitboxes),
            new(new(-10, 18), LeftLegHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyUncrouching] =
        [
            new(new(18, 30), RightLegHitboxes),
            new(new(36, 2), NoHitboxes),
            new(new(31, 4), NoHitboxes),
            new(new(-5, -2), NoHitboxes),
            new(new(0, -28), TorsoHitboxes),
            new(new(-2, 0), UpperBodyHitboxes),
            new(new(-10, 28), LeftLegHitboxes),
            new(new(7, 0), NoHitboxes),
            new(new(2, 1), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyLeaningDown] =
        [
            new(new(18, 46), RightLegHitboxes),
            new(new(31, 17), NoHitboxes),
            new(new(26, 18), NoHitboxes),
            new(new(-5, -4), NoHitboxes),
            new(new(0, -12), TorsoHitboxes),
            new(new(-2, 0), UpperBodyHitboxes),
            new(new(-10, 44), LeftLegHitboxes),
            new(new(7, 16), NoHitboxes),
            new(new(2, 17), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyInitialDummy] =
        [
            new(new(0, 0), BrainHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyHandBeamSecond] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(30, 29), NoHitboxes),
            new(new(25, 30), NoHitboxes),
            new(new(0, -4), NoHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), DeathBeamUpperBodyHitboxes[0]),
            new(new(-10, 56), LeftLegHitboxes),
            new(new(7, 28), NoHitboxes),
            new(new(2, 29), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyHandBeamThird] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(30, 29), NoHitboxes),
            new(new(25, 30), NoHitboxes),
            new(new(0, -4), NoHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), DeathBeamUpperBodyHitboxes[1]),
            new(new(-10, 56), LeftLegHitboxes),
            new(new(7, 28), NoHitboxes),
            new(new(2, 29), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyHandBeamFourth] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(30, 29), NoHitboxes),
            new(new(25, 30), NoHitboxes),
            new(new(0, -4), NoHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), DeathBeamUpperBodyHitboxes[2]),
            new(new(-10, 56), LeftLegHitboxes),
            new(new(7, 28), NoHitboxes),
            new(new(2, 29), NoHitboxes),
        ],
        [EnemyFrameId.MotherBrainBodyHandBeamFifth] =
        [
            new(new(18, 58), RightLegHitboxes),
            new(new(30, 29), NoHitboxes),
            new(new(25, 30), NoHitboxes),
            new(new(0, -4), NoHitboxes),
            new(new(0, 0), TorsoHitboxes),
            new(new(0, 0), DeathBeamUpperBodyHitboxes[3]),
            new(new(-10, 56), LeftLegHitboxes),
            new(new(7, 28), NoHitboxes),
            new(new(2, 29), NoHitboxes),
        ],
    };
}
