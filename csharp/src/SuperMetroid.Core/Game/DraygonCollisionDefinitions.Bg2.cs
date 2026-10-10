using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>The four bank-$A5 hitbox lists Draygon's BG2 and OAM frames refer to.</summary>
internal enum DraygonHitboxList : ushort
{
    /// <summary>$A5:AA95, Draygon's first four-rectangle body hitbox list.</summary>
    FirstBody = 0xaa95,
    /// <summary>$A5:AAC7, the empty hitbox list used while the body cannot be hit.</summary>
    Empty = 0xaac7,
    /// <summary>$A5:ABAB, Draygon's second four-rectangle body hitbox list.</summary>
    SecondBody = 0xabab,
    /// <summary>$A5:ABDD, the second empty hitbox list used by mirrored OAM frames.</summary>
    OtherEmpty = 0xabdd,
}

/// <summary>A fixed bank-$A5 component offset and hitbox-list identity, not editable art.</summary>
internal readonly record struct DraygonCollisionComponent(
    short X, short Y, DraygonHitboxList HitboxPointer);

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
    /// <summary>$A5:A31B, ExtendedSpritemap_Draygon_A: first left-facing BG2 body frame.</summary>
    private const ushort FirstLeftBodyFrame = 0xa31b;
    /// <summary>$A5:A643, ExtendedSpritemap_Draygon_3A: first right-facing BG2 body frame.</summary>
    private const ushort FirstRightBodyFrame = 0xa643;
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

    internal readonly record struct ComponentSequence(bool HasComponent, DraygonHitboxList HitboxPointer)
        : IEnumerable<DraygonCollisionComponent>
    {
        public IEnumerator<DraygonCollisionComponent> GetEnumerator()
        {
            if (HasComponent) yield return new(0, 0, HitboxPointer);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// $A5:A31B..A3BB / A643..A6E3: each BG2 frame has one zero-offset component.
    /// The first eight and final frame of each facing use its body hitboxes;
    /// intervening frames use the empty list. Ordinary OAM frames have no collision components.
    /// </summary>
    internal static ComponentSequence ComponentsAt(ushort pointer)
    {
        if (!DraygonBg2FrameDefinitions.IsFrame(pointer))
        {
            _ = OamComponentsAt(pointer);
            return new(false, default);
        }


        bool right = pointer >= FirstRightBodyFrame;
        int phase = (pointer - (right ? FirstRightBodyFrame : FirstLeftBodyFrame)) / 10;
        DraygonHitboxList hitboxes = phase is < 8 or 16
            ? right ? DraygonHitboxList.SecondBody : DraygonHitboxList.FirstBody : DraygonHitboxList.Empty;
        return new(true, hitboxes);
    }
    internal static ReadOnlySpan<DraygonCollisionHitbox> HitboxesAt(ushort pointer) =>
        HitboxesAt(ClosedNativeWords.Decode<DraygonHitboxList>(pointer, "compiled Draygon hitbox list"));

    internal static ReadOnlySpan<DraygonCollisionHitbox> HitboxesAt(
        DraygonHitboxList pointer) => pointer switch
    {
        DraygonHitboxList.FirstBody => FirstBodyHitboxes,
        DraygonHitboxList.Empty or DraygonHitboxList.OtherEmpty => [],
        DraygonHitboxList.SecondBody => SecondBodyHitboxes,
        _ => throw new InvalidOperationException($"Undefined DraygonHitboxList {pointer}."),
    };
}
