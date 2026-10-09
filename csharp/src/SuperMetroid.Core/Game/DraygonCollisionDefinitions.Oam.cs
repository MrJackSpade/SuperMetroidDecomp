namespace SuperMetroid.Core.Game;

/// <summary>Fixed collision identity for Draygon's ordinary bank-$A5 OAM frames.</summary>
internal static partial class DraygonCollisionDefinitions
{
    /// <summary>$A5:ABDD, the second empty hitbox list used by mirrored OAM frames.</summary>
    internal const ushort OtherEmptyList = 0xabdd;

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

    /// <summary>Identifies the fixed and expanding Draygon OAM frames whose collision component list is empty.</summary>
    /// <param name="pointer">Bank-relative pointer to the OAM frame.</param>
    /// <returns><see langword="true"/> when the pointer is one of the recognized empty frames.</returns>
    internal static bool IsEmptyOamFrame(ushort pointer) =>
        InFixedFrames(pointer, LeftSingleFrames, 6, 1) ||
        InFixedFrames(pointer, RightSingleFrames, 6, 1) ||
        InFixedFrames(pointer, LeftOtherSingleFrames, 4, 1) ||
        InFixedFrames(pointer, RightOtherSingleFrames, 4, 1) ||
        InFixedFrames(pointer, LeftPairedFrames, 7, 2) ||
        InFixedFrames(pointer, RightPairedFrames, 7, 2) ||
        InExpandingFrames(pointer, LeftExpandingFrames) ||
        InExpandingFrames(pointer, RightExpandingFrames);

    /// <summary>Tests whether a pointer lands on a frame boundary in a fixed-size OAM sequence.</summary>
    /// <param name="pointer">Candidate bank-relative frame pointer.</param>
    /// <param name="first">Pointer to the first frame in the sequence.</param>
    /// <param name="frames">Number of frames in the sequence.</param>
    /// <param name="components">Components stored in each frame, which determines its byte stride.</param>
    /// <returns><see langword="true"/> only for an aligned frame pointer within the sequence.</returns>
    private static bool InFixedFrames(ushort pointer, ushort first, int frames, int components)
    {
        int stride = 2 + components * 8;
        int offset = pointer - first;
        return offset >= 0 && offset < frames * stride && offset % stride == 0;
    }

    /// <summary>Tests frame starts in the sequence whose component count grows from three to eight.</summary>
    /// <param name="pointer">Candidate bank-relative frame pointer.</param>
    /// <param name="first">Pointer to the first expanding frame.</param>
    /// <returns><see langword="true"/> when the candidate is one of the seven frame starts.</returns>
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
    /// <summary>Returns no compiled OAM components for a recognized empty frame and rejects other uncompiled frames.</summary>
    /// <param name="pointer">Bank-relative pointer to the Draygon OAM frame.</param>
    /// <returns>An empty span for a recognized frame without components.</returns>
    private static ReadOnlySpan<DraygonCollisionComponent> OamComponentsAt(
        ushort pointer)
    {
        if (IsEmptyOamFrame(pointer))
            return [];
        throw new InvalidDataException(
            $"Draygon frame $A5:{pointer:X4} has no compiled collision identity.");
    }

    /// <summary>Returns no hitboxes for the recognized alternate empty list and rejects uncompiled lists.</summary>
    /// <param name="pointer">Bank-relative pointer to the Draygon hitbox list.</param>
    /// <returns>An empty span for <see cref="OtherEmptyList"/>.</returns>
    private static ReadOnlySpan<DraygonCollisionHitbox> OamHitboxesAt(
        ushort pointer)
    {
        if (pointer == OtherEmptyList)
            return [];
        throw new InvalidDataException(
            $"Draygon hitbox list $A5:{pointer:X4} is not compiled.");
    }
}
