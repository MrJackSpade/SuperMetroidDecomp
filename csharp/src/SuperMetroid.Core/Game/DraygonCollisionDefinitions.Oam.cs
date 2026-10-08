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

    private static ReadOnlySpan<DraygonCollisionHitbox> OamHitboxesAt(
        ushort pointer)
    {
        if (pointer == OtherEmptyList)
            return [];
        throw new InvalidDataException(
            $"Draygon hitbox list $A5:{pointer:X4} is not compiled.");
    }
}
