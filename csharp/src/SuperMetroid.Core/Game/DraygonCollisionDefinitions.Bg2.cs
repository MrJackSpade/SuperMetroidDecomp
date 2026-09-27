using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>A fixed bank-$A5 component offset and hitbox-list identity, not editable art.</summary>
internal readonly record struct DraygonCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>One native Draygon rectangle with its touch and shot callbacks.</summary>
internal readonly record struct DraygonCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Cartridge-owned collision for Draygon's BG2 and ordinary OAM frames. The
/// visual streams referenced by these frames are independently replaceable;
/// editing their artwork cannot move a hitbox or change a callback.
/// </summary>
internal static partial class DraygonCollisionDefinitions
{
    /// <summary>$A5:AA95, Draygon's first four-rectangle body hitbox list.</summary>
    internal const ushort FirstBodyList = 0xaa95;
    /// <summary>$A5:AAC7, the empty hitbox list used while the body cannot be hit.</summary>
    internal const ushort EmptyList = 0xaac7;
    /// <summary>$A5:ABAB, Draygon's second four-rectangle body hitbox list.</summary>
    internal const ushort SecondBodyList = 0xabab;

    private static readonly DraygonCollisionComponent[] FirstBody =
        [new(0, 0, FirstBodyList)];
    private static readonly DraygonCollisionComponent[] Empty =
        [new(0, 0, EmptyList)];
    private static readonly DraygonCollisionComponent[] SecondBody =
        [new(0, 0, SecondBodyList)];

    private static readonly DraygonCollisionHitbox[] FirstBodyHitboxes =
    [
        new(-17, -16, 18, 30, EnemyAiCodePointers.BankA0.NoOp,
            EnemyAiCodePointers.BankA5.DraygonShot),
        new(-59, -66, 3, -30, EnemyAiCodePointers.BankA5.DraygonTouch,
            EnemyAiCodePointers.BankA0.DudShot),
        new(16, 1, 60, 53, EnemyAiCodePointers.BankA5.DraygonTouch,
            EnemyAiCodePointers.BankA0.DudShot),
        new(-28, -37, 61, -10, EnemyAiCodePointers.BankA5.DraygonTouch,
            EnemyAiCodePointers.BankA0.DudShot),
    ];

    private static readonly DraygonCollisionHitbox[] SecondBodyHitboxes =
    [
        new(-18, -7, 18, 30, EnemyAiCodePointers.BankA0.NoOp,
            EnemyAiCodePointers.BankA5.DraygonShot),
        new(-10, -63, 62, -26, EnemyAiCodePointers.BankA5.DraygonTouch,
            EnemyAiCodePointers.BankA0.DudShot),
        new(-55, 0, -21, 57, EnemyAiCodePointers.BankA5.DraygonTouch,
            EnemyAiCodePointers.BankA0.DudShot),
        new(-62, -33, 15, -5, EnemyAiCodePointers.BankA5.DraygonTouch,
            EnemyAiCodePointers.BankA0.DudShot),
    ];

    internal static ReadOnlySpan<DraygonCollisionComponent> ComponentsAt(
        ushort pointer)
    {
        if (DraygonBg2FrameDefinitions.IsFrame(pointer))
            return pointer switch
        {
            >= 0xa31b and <= 0xa361 or 0xa3bb => FirstBody,
            >= 0xa643 and <= 0xa689 or 0xa6e3 => SecondBody,
            _ => Empty,
        };
        return OamComponentsAt(pointer);
    }

    internal static ReadOnlySpan<DraygonCollisionHitbox> HitboxesAt(
        ushort pointer) => pointer switch
    {
        FirstBodyList => FirstBodyHitboxes,
        EmptyList => [],
        SecondBodyList => SecondBodyHitboxes,
        _ => OamHitboxesAt(pointer),
    };
}
