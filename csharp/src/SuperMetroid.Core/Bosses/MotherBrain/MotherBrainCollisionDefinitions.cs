using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Owned collision for Mother Brain's extended frames, read by the
/// shot and contact collision walkers for both the body and the head, whose dummy list keeps
/// its dummy root for life. Each root lists component offsets, each with the hitbox list
/// holding its rectangles; the sprite and body-layer art the same roots reference is replaceable.
/// </summary>
internal static class MotherBrainCollisionDefinitions
{

    /// <summary>Mother Brain body hitbox 0.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA4AC =
    [
        new(-20, -21, 16, 23, EnemyReaction.MotherBrainHeadTouch, EnemyReaction.MotherBrainHeadShot),
    ];

    /// <summary>Mother Brain body hitbox 2.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA4C8 =
        [];

    /// <summary>Mother Brain body hitbox 3.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA4CA =
    [
        new(-23, -1, 23, 7, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
    ];

    /// <summary>Mother Brain body hitbox 4.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA4D8 =
        [];

    /// <summary>Mother Brain body hitbox 5.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA4DA =
    [
        new(-23, -2, 23, 7, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
    ];

    /// <summary>Mother Brain body hitbox 6.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA4E8 =
    [
        new(-32, -24, 20, 52, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
        new(-24, -42, 13, -25, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
    ];

    /// <summary>Mother Brain body hitbox 8.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA504 =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
        new(28, -41, 57, -30, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
    ];

    /// <summary>Mother Brain body hitbox 9.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA51E =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
        new(28, -41, 54, -30, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
    ];

    /// <summary>Mother Brain body hitbox: a.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA538 =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
        new(29, -43, 45, -24, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
    ];

    /// <summary>Mother Brain body hitbox: b.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA552 =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
        new(29, -48, 68, -40, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
    ];

    /// <summary>Mother Brain body hitbox: c.</summary>
    internal static readonly EnemyCollisionHitbox[] ListA56C =
    [
        new(4, -59, 28, -24, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
        new(28, -41, 58, -31, EnemyReaction.Ignore, EnemyReaction.MotherBrainBodyShot),
    ];

    /// <summary>Mother Brain body standing frame.</summary>
    internal static readonly EnemyCollisionComponent[] Frame9FA0 =
    [
        new(new(18, 58), ListA4CA),
        new(new(30, 29), ListA4C8),
        new(new(25, 30), ListA4C8),
        new(new(0, -4), ListA4C8),
        new(new(0, 0), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-10, 56), ListA4DA),
        new(new(7, 28), ListA4D8),
        new(new(2, 29), ListA4D8),
    ];

    /// <summary>Mother Brain body walking, frame 0.</summary>
    internal static readonly EnemyCollisionComponent[] Frame9FEA =
    [
        new(new(28, 47), ListA4CA),
        new(new(38, 19), ListA4C8),
        new(new(33, 19), ListA4C8),
        new(new(0, -3), ListA4C8),
        new(new(0, 2), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-11, 58), ListA4DA),
        new(new(6, 31), ListA4D8),
        new(new(1, 33), ListA4D8),
        new(new(-25, -3), ListA4D8),
    ];

    /// <summary>Mother Brain body walking, frame 1.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA03C =
    [
        new(new(40, 48), ListA4CA),
        new(new(38, 19), ListA4C8),
        new(new(35, 19), ListA4C8),
        new(new(0, -3), ListA4C8),
        new(new(0, 2), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-13, 58), ListA4DA),
        new(new(5, 31), ListA4D8),
        new(new(-1, 33), ListA4D8),
        new(new(-26, -3), ListA4D8),
    ];

    /// <summary>Mother Brain body walking, frame 2.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA08E =
    [
        new(new(40, 51), ListA4CA),
        new(new(38, 21), ListA4C8),
        new(new(35, 22), ListA4C8),
        new(new(-1, -2), ListA4C8),
        new(new(0, 1), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-13, 57), ListA4DA),
        new(new(5, 30), ListA4D8),
        new(new(-1, 32), ListA4D8),
        new(new(-26, -4), ListA4D8),
    ];

    /// <summary>Mother Brain body walking, frame 3.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA0E0 =
    [
        new(new(36, 58), ListA4CA),
        new(new(33, 29), ListA4C8),
        new(new(31, 30), ListA4C8),
        new(new(1, -4), ListA4C8),
        new(new(0, 0), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-16, 56), ListA4DA),
        new(new(3, 28), ListA4D8),
        new(new(-4, 30), ListA4D8),
    ];

    /// <summary>Mother Brain body walking, frame 4.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA12A =
    [
        new(new(21, 60), ListA4CA),
        new(new(29, 31), ListA4C8),
        new(new(27, 32), ListA4C8),
        new(new(-2, -2), ListA4C8),
        new(new(0, 2), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-16, 52), ListA4DA),
        new(new(0, 26), ListA4D8),
        new(new(-5, 28), ListA4D8),
    ];

    /// <summary>Mother Brain body walking, frame 5.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA174 =
    [
        new(new(15, 64), ListA4CA),
        new(new(30, 35), ListA4C8),
        new(new(26, 37), ListA4C8),
        new(new(-2, -1), ListA4C8),
        new(new(0, 6), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-10, 47), ListA4DA),
        new(new(4, 22), ListA4D8),
        new(new(-1, 24), ListA4D8),
    ];

    /// <summary>Mother Brain body walking, frame 6.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA1BE =
    [
        new(new(17, 60), ListA4CA),
        new(new(30, 32), ListA4C8),
        new(new(24, 32), ListA4C8),
        new(new(-1, -2), ListA4C8),
        new(new(0, 2), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-8, 47), ListA4DA),
        new(new(7, 21), ListA4D8),
        new(new(3, 23), ListA4D8),
    ];

    /// <summary>Mother Brain body walking, frame 7.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA208 =
    [
        new(new(18, 58), ListA4CA),
        new(new(31, 30), ListA4C8),
        new(new(25, 30), ListA4C8),
        new(new(-1, -3), ListA4C8),
        new(new(0, 0), ListA4E8),
        new(new(0, 0), ListA504),
        new(new(-10, 56), ListA4DA),
        new(new(7, 28), ListA4D8),
        new(new(2, 31), ListA4D8),
    ];

    /// <summary>Mother Brain body crouched frame.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA252 =
    [
        new(new(18, 20), ListA4CA),
        new(new(34, -9), ListA4C8),
        new(new(31, -6), ListA4C8),
        new(new(-5, 3), ListA4C8),
        new(new(0, -38), ListA4E8),
        new(new(4, 0), ListA504),
        new(new(-10, 18), ListA4DA),
    ];

    /// <summary>Mother Brain body uncrouching frame.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA28C =
    [
        new(new(18, 30), ListA4CA),
        new(new(36, 2), ListA4C8),
        new(new(31, 4), ListA4C8),
        new(new(-5, -2), ListA4C8),
        new(new(0, -28), ListA4E8),
        new(new(-2, 0), ListA504),
        new(new(-10, 28), ListA4DA),
        new(new(7, 0), ListA4D8),
        new(new(2, 1), ListA4D8),
    ];

    /// <summary>Mother Brain body leaning down frame.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA2D6 =
    [
        new(new(18, 46), ListA4CA),
        new(new(31, 17), ListA4C8),
        new(new(26, 18), ListA4C8),
        new(new(-5, -4), ListA4C8),
        new(new(0, -12), ListA4E8),
        new(new(-2, 0), ListA504),
        new(new(-10, 44), ListA4DA),
        new(new(7, 16), ListA4D8),
        new(new(2, 17), ListA4D8),
    ];

    /// <summary>Unused Mother Brain Brain frame.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA320 =
    [
        new(new(0, 0), ListA4AC),
    ];

    /// <summary>Mother Brain Brain death beam mode, frame 0.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA384 =
    [
        new(new(18, 58), ListA4CA),
        new(new(30, 29), ListA4C8),
        new(new(25, 30), ListA4C8),
        new(new(0, -4), ListA4C8),
        new(new(0, 0), ListA4E8),
        new(new(0, 0), ListA51E),
        new(new(-10, 56), ListA4DA),
        new(new(7, 28), ListA4D8),
        new(new(2, 29), ListA4D8),
    ];

    /// <summary>Mother Brain Brain death beam mode, frame 1.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA3CE =
    [
        new(new(18, 58), ListA4CA),
        new(new(30, 29), ListA4C8),
        new(new(25, 30), ListA4C8),
        new(new(0, -4), ListA4C8),
        new(new(0, 0), ListA4E8),
        new(new(0, 0), ListA538),
        new(new(-10, 56), ListA4DA),
        new(new(7, 28), ListA4D8),
        new(new(2, 29), ListA4D8),
    ];

    /// <summary>Mother Brain Brain death beam mode, frame 2.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA418 =
    [
        new(new(18, 58), ListA4CA),
        new(new(30, 29), ListA4C8),
        new(new(25, 30), ListA4C8),
        new(new(0, -4), ListA4C8),
        new(new(0, 0), ListA4E8),
        new(new(0, 0), ListA552),
        new(new(-10, 56), ListA4DA),
        new(new(7, 28), ListA4D8),
        new(new(2, 29), ListA4D8),
    ];

    /// <summary>Mother Brain Brain death beam mode, frame 3.</summary>
    internal static readonly EnemyCollisionComponent[] FrameA462 =
    [
        new(new(18, 58), ListA4CA),
        new(new(30, 29), ListA4C8),
        new(new(25, 30), ListA4C8),
        new(new(0, -4), ListA4C8),
        new(new(0, 0), ListA4E8),
        new(new(0, 0), ListA56C),
        new(new(-10, 56), ListA4DA),
        new(new(7, 28), ListA4D8),
        new(new(2, 29), ListA4D8),
    ];

    /// <summary>Each Mother Brain body frame's components.</summary>
    internal static readonly Dictionary<EnemyFrameId, EnemyCollisionComponent[]> FrameComponents = new()
    {
        [EnemyFrameId.MotherBrainBodyStanding] = Frame9FA0,
        [EnemyFrameId.MotherBrainBodyWalking0] = Frame9FEA,
        [EnemyFrameId.MotherBrainBodyWalking1] = FrameA03C,
        [EnemyFrameId.MotherBrainBodyWalking2] = FrameA08E,
        [EnemyFrameId.MotherBrainBodyWalking3] = FrameA0E0,
        [EnemyFrameId.MotherBrainBodyWalking4] = FrameA12A,
        [EnemyFrameId.MotherBrainBodyWalking5] = FrameA174,
        [EnemyFrameId.MotherBrainBodyWalking6] = FrameA1BE,
        [EnemyFrameId.MotherBrainBodyWalking7] = FrameA208,
        [EnemyFrameId.MotherBrainBodyCrouched] = FrameA252,
        [EnemyFrameId.MotherBrainBodyUncrouching] = FrameA28C,
        [EnemyFrameId.MotherBrainBodyLeaningDown] = FrameA2D6,
        [EnemyFrameId.MotherBrainBodyInitialDummy] = FrameA320,
        [EnemyFrameId.MotherBrainBodyDeathBeam0] = FrameA384,
        [EnemyFrameId.MotherBrainBodyDeathBeam1] = FrameA3CE,
        [EnemyFrameId.MotherBrainBodyDeathBeam2] = FrameA418,
        [EnemyFrameId.MotherBrainBodyDeathBeam3] = FrameA462,
    };
}
