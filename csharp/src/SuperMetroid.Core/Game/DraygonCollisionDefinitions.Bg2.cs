using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>A fixed bank-$A5 component offset and hitbox-list identity, not editable art.</summary>
/// <param name="X">Horizontal component offset relative to the frame origin.</param>
/// <param name="Y">Vertical component offset relative to the frame origin.</param>
/// <param name="HitboxPointer">Bank-$A5 pointer selecting the native hitbox list for this component.</param>
internal readonly record struct DraygonCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>One native Draygon rectangle with its touch and shot callbacks.</summary>
/// <param name="Left">Left edge of the rectangle relative to the enemy origin.</param>
/// <param name="Top">Top edge of the rectangle relative to the enemy origin.</param>
/// <param name="Right">Right edge of the rectangle relative to the enemy origin.</param>
/// <param name="Bottom">Bottom edge of the rectangle relative to the enemy origin.</param>
/// <param name="TouchAi">Native routine selected when Samus touches this rectangle.</param>
/// <param name="ShotAi">Native routine selected when a projectile hits this rectangle.</param>
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

    /// <summary>$A5:A31B, ExtendedSpritemap_Draygon_A: first left-facing BG2 body frame.</summary>
    private const ushort FirstLeftBodyFrame = 0xa31b;
    /// <summary>$A5:A643, ExtendedSpritemap_Draygon_3A: first right-facing BG2 body frame.</summary>
    private const ushort FirstRightBodyFrame = 0xa643;
    /// <summary>Native hitboxes for Draygon's left-facing body poses, retaining each rectangle's original callbacks.</summary>
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

    /// <summary>Native hitboxes for Draygon's right-facing body poses, retaining each rectangle's original callbacks.</summary>
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

    /// <summary>Represents the zero-or-one BG2 collision component associated with a frame.</summary>
    /// <param name="HasComponent"><see langword="true"/> when the frame contributes a collision component.</param>
    /// <param name="HitboxPointer">Hitbox-list pointer carried by the component when present.</param>
    internal readonly record struct ComponentSequence(bool HasComponent, ushort HitboxPointer)
        : IEnumerable<DraygonCollisionComponent>
    {
        /// <summary>Enumerates the component only when this frame has one.</summary>
        /// <returns>An enumerator containing the zero-offset component or no elements.</returns>
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
            return new(false, 0);
        }


        bool right = pointer >= FirstRightBodyFrame;
        int phase = (pointer - (right ? FirstRightBodyFrame : FirstLeftBodyFrame)) / 10;
        ushort hitboxes = phase < 8 || phase == 16
            ? right ? SecondBodyList : FirstBodyList : EmptyList;
        return new(true, hitboxes);
    }
    /// <summary>Returns the collision rectangles associated with a native hitbox-list pointer.</summary>
    /// <param name="pointer">Bank-$A5 hitbox-list pointer selected by the component data.</param>
    /// <returns>The corresponding rectangle sequence, empty for the native empty list.</returns>
    internal static ReadOnlySpan<DraygonCollisionHitbox> HitboxesAt(
        ushort pointer) => pointer switch
    {
        FirstBodyList => FirstBodyHitboxes,
        EmptyList => [],
        SecondBodyList => SecondBodyHitboxes,
        _ => OamHitboxesAt(pointer),
    };
}
