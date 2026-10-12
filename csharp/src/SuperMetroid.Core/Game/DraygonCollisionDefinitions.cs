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
    Offset16<RoomPixels> Offset, DraygonHitboxList HitboxPointer);

/// <summary>One native Draygon rectangle with its touch and shot callbacks.</summary>
internal readonly record struct DraygonCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Cartridge-owned collision for Draygon's BG2 and ordinary OAM frames. The
/// visual streams referenced by these frames are independently replaceable;
/// editing their artwork cannot move a hitbox or change a callback.
/// </summary>
internal static class DraygonCollisionDefinitions
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
            if (HasComponent) yield return new(Offset16<RoomPixels>.Zero, HitboxPointer);
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

    internal static ReadOnlySpan<DraygonCollisionHitbox> HitboxesAt(
        DraygonHitboxList pointer) => pointer switch
    {
        DraygonHitboxList.FirstBody => FirstBodyHitboxes,
        DraygonHitboxList.Empty or DraygonHitboxList.OtherEmpty => [],
        DraygonHitboxList.SecondBody => SecondBodyHitboxes,
        _ => throw new InvalidOperationException($"Undefined DraygonHitboxList {pointer}."),
    };

    /// <summary><c>ExtendedSpritemap_Draygon_4</c> at $A5:A2DF: first of six one-component frames.</summary>
    private const ushort LeftSingleFrames = 0xa2df;
    /// <summary><c>ExtendedSpritemap_Draygon_1B</c> at $A5:A3C5: first of four one-component frames.</summary>
    private const ushort LeftOtherSingleFrames = 0xa3c5;
    /// <summary><c>ExtendedSpritemap_Draygon_22</c> at $A5:A40B: first of seven two-component frames.</summary>
    private const ushort LeftPairedFrames = 0xa40b;
    /// <summary><c>ExtendedSpritemap_Draygon_29</c> at $A5:A489: first of six growing frames with three through eight components, then another eight-component frame.</summary>
    private const ushort LeftExpandingFrames = 0xa489;
    /// <summary><c>ExtendedSpritemap_Draygon_34</c> at $A5:A607: mirrored six one-component frames.</summary>
    private const ushort RightSingleFrames = 0xa607;
    /// <summary><c>ExtendedSpritemap_Draygon_4B</c> at $A5:A6ED: mirrored four one-component frames.</summary>
    private const ushort RightOtherSingleFrames = 0xa6ed;
    /// <summary><c>ExtendedSpritemap_Draygon_59</c> at $A5:A779: mirrored seven two-component frames.</summary>
    private const ushort RightPairedFrames = 0xa779;
    /// <summary><c>ExtendedSpritemap_Draygon_60</c> at $A5:A7F7: mirrored growing component frames and terminal eight-component frame.</summary>
    private const ushort RightExpandingFrames = 0xa7f7;

    internal static bool IsEmptyOamFrame(ushort pointer) =>
        InFixedFrames(pointer, LeftSingleFrames, 6, 1) ||
        InFixedFrames(pointer, RightSingleFrames, 6, 1) ||
        InFixedFrames(pointer, LeftOtherSingleFrames, 4, 1) ||
        InFixedFrames(pointer, RightOtherSingleFrames, 4, 1) ||
        InFixedFrames(pointer, LeftPairedFrames, 7, 2) ||
        InFixedFrames(pointer, RightPairedFrames, 7, 2) ||
        InExpandingFrames(pointer, LeftExpandingFrames) ||
        InExpandingFrames(pointer, RightExpandingFrames);

    private static bool InFixedFrames(ushort pointer, ushort first, int frames, int components)
    {
        int stride = 2 + components * 8;
        int offset = pointer - first;
        return offset >= 0 && offset < frames * stride && offset % stride == 0;
    }

    private static bool InExpandingFrames(ushort pointer, ushort first)
    {
        // Two-byte counts and eight-byte components; the component count starts
        // at three and grows through eight in the six preceding frames.
        int offset = pointer - first;
        for (int frame = 0; frame < 7; frame++)
            if (offset == 2 * frame + 8 * (3 * frame + frame * (frame - 1) / 2))
                return true;
        return false;
    }
    private static ReadOnlySpan<DraygonCollisionComponent> OamComponentsAt(
        ushort pointer)
    {
        if (IsEmptyOamFrame(pointer))
            return [];
        throw new InvalidDataException(
            $"Draygon frame $A5:{pointer:X4} has no compiled collision identity.");
    }
}
