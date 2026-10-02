namespace SuperMetroid.Core.Assets;

/// <summary>
/// The 34 bank-$A5 Draygon extended frames whose sole zero-offset component
/// points to a $FFFE BG2 tilemap stream. The remaining 48 selected frames are
/// ordinary OAM components in <see cref="EnemyExtendedFrameDefinitions"/>.
/// Hitbox pointers and instruction timing are not part of this visual catalog.
/// </summary>
internal static class DraygonBg2FrameDefinitions
{
    internal const int Version = 1;
    internal const string FileName = "draygon-bg2-frames.json";
    internal const byte Bank = 0xa5;
    /// <summary>Each selected Draygon BG2 frame has one zero-offset stream.</summary>
    internal const int MaximumComponents = 1;

    /// <summary>$A5:A31B, first left-facing one-component BG2 body frame.</summary>
    private const ushort FacingLeftStart = 0xa31b;
    /// <summary>$A5:A643, first right-facing one-component BG2 body frame.</summary>
    private const ushort FacingRightStart = 0xa643;
    internal const int FrameCount = 34;

    internal static EnemyBg2FrameDefinitionSequence Frames => new(FrameCount, Frame);

    /// <summary>Each facing has seventeen ten-byte extended frames: a two-byte
    /// count and one eight-byte component. Preserve the published pointer-based key.</summary>
    internal static EnemyBg2FrameDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
        ushort pointer = (ushort)(index < 17 ? FacingLeftStart + 10 * index
            : FacingRightStart + 10 * (index - 17));
        return new(pointer, $"draygon_bg2_{pointer:X4}");
    }

    internal static bool IsFrame(ushort pointer) => InFacing(pointer, FacingLeftStart) || InFacing(pointer, FacingRightStart);

    private static bool InFacing(ushort pointer, ushort start)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < 170 && offset % 10 == 0;
    }
}