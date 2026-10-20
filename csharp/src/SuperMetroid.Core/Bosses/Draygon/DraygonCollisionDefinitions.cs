using System.Collections.Immutable;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

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
    /// <summary>Left-facing body rectangles: one shootable eye and three touch-only regions.</summary>
    private static readonly DraygonCollisionHitbox[] FirstBodyHitboxes =
    [
        new(-17, -16, 18, 30, DraygonHitboxTouch.Harmless, DraygonHitboxShot.Eye),
        new(-59, -66, 3, -30, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(16, 1, 60, 53, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-28, -37, 61, -10, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
    ];

    /// <summary>Mirrored right-facing body rectangles with the same shootable-eye rule.</summary>
    private static readonly DraygonCollisionHitbox[] SecondBodyHitboxes =
    [
        new(-18, -7, 18, 30, DraygonHitboxTouch.Harmless, DraygonHitboxShot.Eye),
        new(-10, -63, 62, -26, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-55, 0, -21, 57, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-62, -33, 15, -5, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
    ];

    /// <summary>The empty list of the BG2 frames in which the body cannot be hit or touched.</summary>
    private static readonly DraygonCollisionHitbox[] EmptyHitboxes = [];

    /// <summary>Ordinary OAM art has no collision components.</summary>
    private static readonly ImmutableArray<DraygonCollisionComponent> NoComponents = [];
    /// <summary>The left-facing body's one zero-offset component.</summary>
    private static readonly ImmutableArray<DraygonCollisionComponent> FirstBodyComponents = [new(Offset16<RoomPixels>.Zero, FirstBodyHitboxes)];
    /// <summary>The right-facing body's one zero-offset component.</summary>
    private static readonly ImmutableArray<DraygonCollisionComponent> SecondBodyComponents = [new(Offset16<RoomPixels>.Zero, SecondBodyHitboxes)];
    /// <summary>The one zero-offset component of a BG2 frame in which the body cannot be hit or touched.</summary>
    private static readonly ImmutableArray<DraygonCollisionComponent> EmptyBodyComponents = [new(Offset16<RoomPixels>.Zero, EmptyHitboxes)];

    /// <summary>
    /// $A5:A31B..A3BB / A643..A6E3: each BG2 frame has one zero-offset component.
    /// The first eight and final frame of each facing use its body hitboxes;
    /// intervening frames use the empty list. Ordinary OAM frames have no collision components.
    /// </summary>
    internal static ImmutableArray<DraygonCollisionComponent> ComponentsAt(ushort pointer)
    {
        if (!DraygonBg2FrameDefinitions.IsFrame(pointer))
        {
            _ = OamComponentsAt(pointer);
            return NoComponents;
        }

        bool right = pointer >= FirstRightBodyFrame;
        int phase = (pointer - (right ? FirstRightBodyFrame : FirstLeftBodyFrame)) / 10;
        return phase is < 8 or 16
            ? right ? SecondBodyComponents : FirstBodyComponents
            : EmptyBodyComponents;
    }

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

    /// <summary>Recognizes collision-free ordinary OAM frame roots in fixed and expanding runs.</summary>
    internal static bool IsEmptyOamFrame(ushort pointer) =>
        InFixedFrames(pointer, LeftSingleFrames, 6, 1) ||
        InFixedFrames(pointer, RightSingleFrames, 6, 1) ||
        InFixedFrames(pointer, LeftOtherSingleFrames, 4, 1) ||
        InFixedFrames(pointer, RightOtherSingleFrames, 4, 1) ||
        InFixedFrames(pointer, LeftPairedFrames, 7, 2) ||
        InFixedFrames(pointer, RightPairedFrames, 7, 2) ||
        InExpandingFrames(pointer, LeftExpandingFrames) ||
        InExpandingFrames(pointer, RightExpandingFrames);

    /// <summary>Checks exact roots in a run whose frame stride is a count word plus fixed-size components.</summary>
    private static bool InFixedFrames(ushort pointer, ushort first, int frames, int components)
    {
        int stride = 2 + components * 8;
        int offset = pointer - first;
        return offset >= 0 && offset < frames * stride && offset % stride == 0;
    }

    /// <summary>Checks the seven roots whose component counts grow from three through eight, then repeat eight.</summary>
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
    /// <summary>Accepts a known OAM-only frame as physically empty and rejects unknown frame identities.</summary>
    private static ReadOnlySpan<DraygonCollisionComponent> OamComponentsAt(
        ushort pointer)
    {
        if (IsEmptyOamFrame(pointer))
            return [];
        throw new InvalidDataException(
            $"Draygon frame $A5:{pointer:X4} has no compiled collision identity.");
    }
}
