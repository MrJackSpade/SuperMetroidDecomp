using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Verification's native view of <see cref="DraygonCollisionDefinitions"/>: frames by their bank-$A5 address.</summary>
internal static class DraygonCollisionDefinitionsAccess
{
    /// <summary>Bank $A5, holding Draygon's frames.</summary>
    private const byte NativeBank = 0xa5;
    /// <summary><c>ExtendedSpritemap_Draygon_4</c> at $A5:A2DF: first of six one-component frames.</summary>
    private const ushort LeftSingleFrames = 0xa2df;
    /// <summary><c>ExtendedSpritemap_Draygon_1B</c> at $A5:A3C5: first of four one-component frames.</summary>
    private const ushort LeftOtherSingleFrames = 0xa3c5;
    /// <summary><c>ExtendedSpritemap_Draygon_22</c> at $A5:A40B: first of seven two-component frames.</summary>
    private const ushort LeftPairedFrames = 0xa40b;
    /// <summary><c>ExtendedSpritemap_Draygon_29</c> at $A5:A489: six growing frames with three through eight components, then another eight.</summary>
    private const ushort LeftExpandingFrames = 0xa489;
    /// <summary><c>ExtendedSpritemap_Draygon_34</c> at $A5:A607: mirrored six one-component frames.</summary>
    private const ushort RightSingleFrames = 0xa607;
    /// <summary><c>ExtendedSpritemap_Draygon_4B</c> at $A5:A6ED: mirrored four one-component frames.</summary>
    private const ushort RightOtherSingleFrames = 0xa6ed;
    /// <summary><c>ExtendedSpritemap_Draygon_59</c> at $A5:A779: mirrored seven two-component frames.</summary>
    private const ushort RightPairedFrames = 0xa779;
    /// <summary><c>ExtendedSpritemap_Draygon_60</c> at $A5:A7F7: mirrored growing frames and terminal eight-component frame.</summary>
    private const ushort RightExpandingFrames = 0xa7f7;

    /// <summary>Native views of the collision lookup.</summary>
    extension(DraygonCollision)
    {
        /// <summary>The components of the frame at native address <paramref name="frame"/>.</summary>
        internal static ImmutableArray<DraygonCollisionComponent> ComponentsAt(ushort frame) =>
            DraygonCollision.ComponentsAt(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, frame));
    }

    /// <summary>Native views of the catalog.</summary>
    extension(DraygonCollisionDefinitions)
    {
        /// <summary>Empty OAM frames across both mirrored Draygon body sequences.</summary>
        internal static int EmptyOamFrameCount => 2 * (6 + 4 + 7 + 7);

        /// <summary>The cartridge's collision-free ordinary OAM frame roots, in fixed and expanding runs.</summary>
        internal static bool IsEmptyOamFrame(ushort pointer) =>
            InFixedFrames(pointer, LeftSingleFrames, 6, 1) ||
            InFixedFrames(pointer, RightSingleFrames, 6, 1) ||
            InFixedFrames(pointer, LeftOtherSingleFrames, 4, 1) ||
            InFixedFrames(pointer, RightOtherSingleFrames, 4, 1) ||
            InFixedFrames(pointer, LeftPairedFrames, 7, 2) ||
            InFixedFrames(pointer, RightPairedFrames, 7, 2) ||
            InExpandingFrames(pointer, LeftExpandingFrames) ||
            InExpandingFrames(pointer, RightExpandingFrames);
    }

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
        int offset = pointer - first;
        for (int frame = 0; frame < 7; frame++)
            if (offset == 2 * frame + 8 * (3 * frame + frame * (frame - 1) / 2))
                return true;
        return false;
    }
}
