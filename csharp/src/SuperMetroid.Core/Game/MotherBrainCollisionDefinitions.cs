namespace SuperMetroid.Core.Game;

/// <summary>A fixed bank-$A9 Mother Brain component offset and hitbox-list identity.</summary>
internal readonly record struct MotherBrainCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>One native Mother Brain rectangle with its touch and shot callbacks.</summary>
internal readonly record struct MotherBrainCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Cartridge-owned collision for Mother Brain's extended frames in bank $A9, read by
/// <c>$A0:9B7F</c>/<c>$A0:9A5A</c> for both the body and the head, whose dummy list keeps
/// root <c>$A320</c> for life. Each root lists component offsets whose hitbox lists hold
/// the rectangles; the BG2/OAM art the same roots reference is replaceable.
/// </summary>
internal static class MotherBrainCollisionDefinitions
{
    /// <summary>Mother Brain extended-frame and hitbox bank $A9.</summary>
    internal const byte Bank = 0xa9;

    /// <summary>$A9:9FA0, <c>ExtendedSpritemap_MotherBrainBody_Standing</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] Frame9FA0 =
    [
        new(18, 58, 0xa4ca),
        new(30, 29, 0xa4c8),
        new(25, 30, 0xa4c8),
        new(0, -4, 0xa4c8),
        new(0, 0, 0xa4e8),
        new(0, 0, 0xa504),
        new(-10, 56, 0xa4da),
        new(7, 28, 0xa4d8),
        new(2, 29, 0xa4d8),
    ];

    /// <summary>$A9:9FEA, <c>ExtendedSpritemap_MotherBrainBody_Walking_0</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] Frame9FEA =
    [
        new(28, 47, 0xa4ca),
        new(38, 19, 0xa4c8),
        new(33, 19, 0xa4c8),
        new(0, -3, 0xa4c8),
        new(0, 2, 0xa4e8),
        new(0, 0, 0xa504),
        new(-11, 58, 0xa4da),
        new(6, 31, 0xa4d8),
        new(1, 33, 0xa4d8),
        new(-25, -3, 0xa4d8),
    ];

    /// <summary>$A9:A03C, <c>ExtendedSpritemap_MotherBrainBody_Walking_1</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA03C =
    [
        new(40, 48, 0xa4ca),
        new(38, 19, 0xa4c8),
        new(35, 19, 0xa4c8),
        new(0, -3, 0xa4c8),
        new(0, 2, 0xa4e8),
        new(0, 0, 0xa504),
        new(-13, 58, 0xa4da),
        new(5, 31, 0xa4d8),
        new(-1, 33, 0xa4d8),
        new(-26, -3, 0xa4d8),
    ];

    /// <summary>$A9:A08E, <c>ExtendedSpritemap_MotherBrainBody_Walking_2</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA08E =
    [
        new(40, 51, 0xa4ca),
        new(38, 21, 0xa4c8),
        new(35, 22, 0xa4c8),
        new(-1, -2, 0xa4c8),
        new(0, 1, 0xa4e8),
        new(0, 0, 0xa504),
        new(-13, 57, 0xa4da),
        new(5, 30, 0xa4d8),
        new(-1, 32, 0xa4d8),
        new(-26, -4, 0xa4d8),
    ];

    /// <summary>$A9:A0E0, <c>ExtendedSpritemap_MotherBrainBody_Walking_3</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA0E0 =
    [
        new(36, 58, 0xa4ca),
        new(33, 29, 0xa4c8),
        new(31, 30, 0xa4c8),
        new(1, -4, 0xa4c8),
        new(0, 0, 0xa4e8),
        new(0, 0, 0xa504),
        new(-16, 56, 0xa4da),
        new(3, 28, 0xa4d8),
        new(-4, 30, 0xa4d8),
    ];

    /// <summary>$A9:A12A, <c>ExtendedSpritemap_MotherBrainBody_Walking_4</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA12A =
    [
        new(21, 60, 0xa4ca),
        new(29, 31, 0xa4c8),
        new(27, 32, 0xa4c8),
        new(-2, -2, 0xa4c8),
        new(0, 2, 0xa4e8),
        new(0, 0, 0xa504),
        new(-16, 52, 0xa4da),
        new(0, 26, 0xa4d8),
        new(-5, 28, 0xa4d8),
    ];

    /// <summary>$A9:A174, <c>ExtendedSpritemap_MotherBrainBody_Walking_5</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA174 =
    [
        new(15, 64, 0xa4ca),
        new(30, 35, 0xa4c8),
        new(26, 37, 0xa4c8),
        new(-2, -1, 0xa4c8),
        new(0, 6, 0xa4e8),
        new(0, 0, 0xa504),
        new(-10, 47, 0xa4da),
        new(4, 22, 0xa4d8),
        new(-1, 24, 0xa4d8),
    ];

    /// <summary>$A9:A1BE, <c>ExtendedSpritemap_MotherBrainBody_Walking_6</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA1BE =
    [
        new(17, 60, 0xa4ca),
        new(30, 32, 0xa4c8),
        new(24, 32, 0xa4c8),
        new(-1, -2, 0xa4c8),
        new(0, 2, 0xa4e8),
        new(0, 0, 0xa504),
        new(-8, 47, 0xa4da),
        new(7, 21, 0xa4d8),
        new(3, 23, 0xa4d8),
    ];

    /// <summary>$A9:A208, <c>ExtendedSpritemap_MotherBrainBody_Walking_7</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA208 =
    [
        new(18, 58, 0xa4ca),
        new(31, 30, 0xa4c8),
        new(25, 30, 0xa4c8),
        new(-1, -3, 0xa4c8),
        new(0, 0, 0xa4e8),
        new(0, 0, 0xa504),
        new(-10, 56, 0xa4da),
        new(7, 28, 0xa4d8),
        new(2, 31, 0xa4d8),
    ];

    /// <summary>$A9:A252, <c>ExtendedSpritemap_MotherBrainBody_Crouched</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA252 =
    [
        new(18, 20, 0xa4ca),
        new(34, -9, 0xa4c8),
        new(31, -6, 0xa4c8),
        new(-5, 3, 0xa4c8),
        new(0, -38, 0xa4e8),
        new(4, 0, 0xa504),
        new(-10, 18, 0xa4da),
    ];

    /// <summary>$A9:A28C, <c>ExtendedSpritemap_MotherBrainBody_Uncrouching</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA28C =
    [
        new(18, 30, 0xa4ca),
        new(36, 2, 0xa4c8),
        new(31, 4, 0xa4c8),
        new(-5, -2, 0xa4c8),
        new(0, -28, 0xa4e8),
        new(-2, 0, 0xa504),
        new(-10, 28, 0xa4da),
        new(7, 0, 0xa4d8),
        new(2, 1, 0xa4d8),
    ];

    /// <summary>$A9:A2D6, <c>ExtendedSpritemap_MotherBrainBody_LeaningDown</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA2D6 =
    [
        new(18, 46, 0xa4ca),
        new(31, 17, 0xa4c8),
        new(26, 18, 0xa4c8),
        new(-5, -4, 0xa4c8),
        new(0, -12, 0xa4e8),
        new(-2, 0, 0xa504),
        new(-10, 44, 0xa4da),
        new(7, 16, 0xa4d8),
        new(2, 17, 0xa4d8),
    ];

    /// <summary>$A9:A320, <c>UNUSED_ExtendedSpritemap_MotherBrainBrain_A9A320</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA320 =
    [
        new(0, 0, 0xa4ac),
    ];

    /// <summary>$A9:A384, <c>ExtendedSpritemap_MotherBrainBrain_DeathBeamMode_0</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA384 =
    [
        new(18, 58, 0xa4ca),
        new(30, 29, 0xa4c8),
        new(25, 30, 0xa4c8),
        new(0, -4, 0xa4c8),
        new(0, 0, 0xa4e8),
        new(0, 0, 0xa51e),
        new(-10, 56, 0xa4da),
        new(7, 28, 0xa4d8),
        new(2, 29, 0xa4d8),
    ];

    /// <summary>$A9:A3CE, <c>ExtendedSpritemap_MotherBrainBrain_DeathBeamMode_1</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA3CE =
    [
        new(18, 58, 0xa4ca),
        new(30, 29, 0xa4c8),
        new(25, 30, 0xa4c8),
        new(0, -4, 0xa4c8),
        new(0, 0, 0xa4e8),
        new(0, 0, 0xa538),
        new(-10, 56, 0xa4da),
        new(7, 28, 0xa4d8),
        new(2, 29, 0xa4d8),
    ];

    /// <summary>$A9:A418, <c>ExtendedSpritemap_MotherBrainBrain_DeathBeamMode_2</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA418 =
    [
        new(18, 58, 0xa4ca),
        new(30, 29, 0xa4c8),
        new(25, 30, 0xa4c8),
        new(0, -4, 0xa4c8),
        new(0, 0, 0xa4e8),
        new(0, 0, 0xa552),
        new(-10, 56, 0xa4da),
        new(7, 28, 0xa4d8),
        new(2, 29, 0xa4d8),
    ];

    /// <summary>$A9:A462, <c>ExtendedSpritemap_MotherBrainBrain_DeathBeamMode_3</c>.</summary>
    private static readonly MotherBrainCollisionComponent[] FrameA462 =
    [
        new(18, 58, 0xa4ca),
        new(30, 29, 0xa4c8),
        new(25, 30, 0xa4c8),
        new(0, -4, 0xa4c8),
        new(0, 0, 0xa4e8),
        new(0, 0, 0xa56c),
        new(-10, 56, 0xa4da),
        new(7, 28, 0xa4d8),
        new(2, 29, 0xa4d8),
    ];

    /// <summary>$A9:A4AC, <c>Hitbox_MotherBrainBody_0</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA4AC =
    [
        new(-20, -21, 16, 23, EnemyAiCodePointers.BankA9.MotherBrainHeadTouch, EnemyAiCodePointers.BankA9.MotherBrainHeadShot),
    ];

    /// <summary>$A9:A4C8, <c>Hitbox_MotherBrainBody_2</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA4C8 =
        [];

    /// <summary>$A9:A4CA, <c>Hitbox_MotherBrainBody_3</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA4CA =
    [
        new(-23, -1, 23, 7, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
    ];

    /// <summary>$A9:A4D8, <c>Hitbox_MotherBrainBody_4</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA4D8 =
        [];

    /// <summary>$A9:A4DA, <c>Hitbox_MotherBrainBody_5</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA4DA =
    [
        new(-23, -2, 23, 7, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
    ];

    /// <summary>$A9:A4E8, <c>Hitbox_MotherBrainBody_6</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA4E8 =
    [
        new(-32, -24, 20, 52, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
        new(-24, -42, 13, -25, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
    ];

    /// <summary>$A9:A504, <c>Hitbox_MotherBrainBody_8</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA504 =
    [
        new(4, -59, 28, -24, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
        new(28, -41, 57, -30, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
    ];

    /// <summary>$A9:A51E, <c>Hitbox_MotherBrainBody_9</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA51E =
    [
        new(4, -59, 28, -24, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
        new(28, -41, 54, -30, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
    ];

    /// <summary>$A9:A538, <c>Hitbox_MotherBrainBody_A</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA538 =
    [
        new(4, -59, 28, -24, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
        new(29, -43, 45, -24, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
    ];

    /// <summary>$A9:A552, <c>Hitbox_MotherBrainBody_B</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA552 =
    [
        new(4, -59, 28, -24, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
        new(29, -48, 68, -40, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
    ];

    /// <summary>$A9:A56C, <c>Hitbox_MotherBrainBody_C</c>.</summary>
    private static readonly MotherBrainCollisionHitbox[] ListA56C =
    [
        new(4, -59, 28, -24, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
        new(28, -41, 58, -31, EnemyAiCodePointers.BankA9.MotherBrainBodyTouch, EnemyAiCodePointers.BankA9.MotherBrainBodyShot),
    ];

    /// <summary>Returns the components of a native body frame root.</summary>
    internal static ReadOnlySpan<MotherBrainCollisionComponent> ComponentsAt(ushort frame) => frame switch
    {
        0x9fa0 => Frame9FA0,
        0x9fea => Frame9FEA,
        0xa03c => FrameA03C,
        0xa08e => FrameA08E,
        0xa0e0 => FrameA0E0,
        0xa12a => FrameA12A,
        0xa174 => FrameA174,
        0xa1be => FrameA1BE,
        0xa208 => FrameA208,
        0xa252 => FrameA252,
        0xa28c => FrameA28C,
        0xa2d6 => FrameA2D6,
        0xa320 => FrameA320,
        0xa384 => FrameA384,
        0xa3ce => FrameA3CE,
        0xa418 => FrameA418,
        0xa462 => FrameA462,
        _ => throw new InvalidDataException($"Mother Brain has no extended frame root at $A9:{frame:X4}."),
    };

    /// <summary>Returns the rectangles of a native body hitbox list.</summary>
    internal static ReadOnlySpan<MotherBrainCollisionHitbox> HitboxesAt(ushort list) => list switch
    {
        0xa4ac => ListA4AC,
        0xa4c8 => ListA4C8,
        0xa4ca => ListA4CA,
        0xa4d8 => ListA4D8,
        0xa4da => ListA4DA,
        0xa4e8 => ListA4E8,
        0xa504 => ListA504,
        0xa51e => ListA51E,
        0xa538 => ListA538,
        0xa552 => ListA552,
        0xa56c => ListA56C,
        _ => throw new InvalidDataException($"Mother Brain has no hitbox list at $A9:{list:X4}."),
    };
}
