using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Cartridge-owned collision for Mother Brain's extended frames in bank $A9, read by
/// <c>$A0:9B7F</c>/<c>$A0:9A5A</c> for both the body and the head, whose dummy list keeps
/// root <c>$A320</c> for life. Each root lists component offsets, each with the hitbox list
/// holding its rectangles; the BG2/OAM art the same roots reference is replaceable.
/// </summary>
internal static class MotherBrainCollisionDefinitions
{

    /// <summary>$A9:A4AC, <c>Hitbox_MotherBrainBody_0</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA4AC =
    [
        new(-20, -21, 16, 23, MotherBrainHitboxTouch.Head, MotherBrainHitboxShot.Head),
    ];

    /// <summary>$A9:A4C8, <c>Hitbox_MotherBrainBody_2</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA4C8 =
        [];

    /// <summary>$A9:A4CA, <c>Hitbox_MotherBrainBody_3</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA4CA =
    [
        new(-23, -1, 23, 7, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
    ];

    /// <summary>$A9:A4D8, <c>Hitbox_MotherBrainBody_4</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA4D8 =
        [];

    /// <summary>$A9:A4DA, <c>Hitbox_MotherBrainBody_5</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA4DA =
    [
        new(-23, -2, 23, 7, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
    ];

    /// <summary>$A9:A4E8, <c>Hitbox_MotherBrainBody_6</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA4E8 =
    [
        new(-32, -24, 20, 52, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
        new(-24, -42, 13, -25, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
    ];

    /// <summary>$A9:A504, <c>Hitbox_MotherBrainBody_8</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA504 =
    [
        new(4, -59, 28, -24, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
        new(28, -41, 57, -30, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
    ];

    /// <summary>$A9:A51E, <c>Hitbox_MotherBrainBody_9</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA51E =
    [
        new(4, -59, 28, -24, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
        new(28, -41, 54, -30, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
    ];

    /// <summary>$A9:A538, <c>Hitbox_MotherBrainBody_A</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA538 =
    [
        new(4, -59, 28, -24, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
        new(29, -43, 45, -24, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
    ];

    /// <summary>$A9:A552, <c>Hitbox_MotherBrainBody_B</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA552 =
    [
        new(4, -59, 28, -24, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
        new(29, -48, 68, -40, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
    ];

    /// <summary>$A9:A56C, <c>Hitbox_MotherBrainBody_C</c>.</summary>
    internal static readonly MotherBrainCollisionHitbox[] ListA56C =
    [
        new(4, -59, 28, -24, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
        new(28, -41, 58, -31, MotherBrainHitboxTouch.Body, MotherBrainHitboxShot.Body),
    ];

    /// <summary>$A9:9FA0, <c>ExtendedSpritemap_MotherBrainBody_Standing</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] Frame9FA0 =
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

    /// <summary>$A9:9FEA, <c>ExtendedSpritemap_MotherBrainBody_Walking_0</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] Frame9FEA =
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

    /// <summary>$A9:A03C, <c>ExtendedSpritemap_MotherBrainBody_Walking_1</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA03C =
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

    /// <summary>$A9:A08E, <c>ExtendedSpritemap_MotherBrainBody_Walking_2</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA08E =
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

    /// <summary>$A9:A0E0, <c>ExtendedSpritemap_MotherBrainBody_Walking_3</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA0E0 =
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

    /// <summary>$A9:A12A, <c>ExtendedSpritemap_MotherBrainBody_Walking_4</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA12A =
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

    /// <summary>$A9:A174, <c>ExtendedSpritemap_MotherBrainBody_Walking_5</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA174 =
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

    /// <summary>$A9:A1BE, <c>ExtendedSpritemap_MotherBrainBody_Walking_6</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA1BE =
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

    /// <summary>$A9:A208, <c>ExtendedSpritemap_MotherBrainBody_Walking_7</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA208 =
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

    /// <summary>$A9:A252, <c>ExtendedSpritemap_MotherBrainBody_Crouched</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA252 =
    [
        new(new(18, 20), ListA4CA),
        new(new(34, -9), ListA4C8),
        new(new(31, -6), ListA4C8),
        new(new(-5, 3), ListA4C8),
        new(new(0, -38), ListA4E8),
        new(new(4, 0), ListA504),
        new(new(-10, 18), ListA4DA),
    ];

    /// <summary>$A9:A28C, <c>ExtendedSpritemap_MotherBrainBody_Uncrouching</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA28C =
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

    /// <summary>$A9:A2D6, <c>ExtendedSpritemap_MotherBrainBody_LeaningDown</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA2D6 =
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

    /// <summary>$A9:A320, <c>UNUSED_ExtendedSpritemap_MotherBrainBrain_A9A320</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA320 =
    [
        new(new(0, 0), ListA4AC),
    ];

    /// <summary>$A9:A384, <c>ExtendedSpritemap_MotherBrainBrain_DeathBeamMode_0</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA384 =
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

    /// <summary>$A9:A3CE, <c>ExtendedSpritemap_MotherBrainBrain_DeathBeamMode_1</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA3CE =
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

    /// <summary>$A9:A418, <c>ExtendedSpritemap_MotherBrainBrain_DeathBeamMode_2</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA418 =
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

    /// <summary>$A9:A462, <c>ExtendedSpritemap_MotherBrainBrain_DeathBeamMode_3</c>.</summary>
    internal static readonly MotherBrainCollisionComponent[] FrameA462 =
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
    internal static readonly Dictionary<EnemyFrameId, MotherBrainCollisionComponent[]> FrameComponents = new()
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
