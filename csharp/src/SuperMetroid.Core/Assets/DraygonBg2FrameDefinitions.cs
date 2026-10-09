namespace SuperMetroid.Core.Assets;

/// <summary>
/// The 34 bank-$A5 Draygon extended frames whose sole zero-offset component
/// points to a $FFFE BG2 tilemap stream. The remaining 48 selected frames are
/// ordinary OAM components in <see cref="EnemyExtendedFrameDefinitions"/>.
/// Hitbox pointers and instruction timing are not part of this visual catalog.
/// </summary>
internal static class DraygonBg2FrameDefinitions
{
    /// <summary>Schema version used when importing or validating the extracted frame list.</summary>
    internal const int Version = 1;
    /// <summary>Installed JSON filename containing the selected Draygon BG2 frame pointers.</summary>
    internal const string FileName = "draygon-bg2-frames.json";
    /// <summary>Cartridge bank containing the extended frame records and BG2 streams.</summary>
    internal const byte Bank = 0xa5;
    /// <summary>Each selected Draygon BG2 frame has one zero-offset stream.</summary>
    internal const int MaximumComponents = 1;

    /// <summary>$A5:A31B, first left-facing one-component BG2 body frame.</summary>
    private const ushort FacingLeftStart = 0xa31b;
    /// <summary>$A5:A643, first right-facing one-component BG2 body frame.</summary>
    private const ushort FacingRightStart = 0xa643;
    /// <summary>Number of selected BG2 frames across the two facing-specific runs.</summary>
    internal const int FrameCount = 34;

    /// <summary>Provides the ordered pointer-keyed definitions for all selected Draygon BG2 frames.</summary>
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

    /// <summary>Determines whether a pointer is one of the selected left- or right-facing BG2 frames.</summary>
    /// <param name="pointer">Bank-local extended-frame pointer to test.</param>
    /// <returns><see langword="true"/> for a pointer in either recognized facing run.</returns>
    internal static bool IsFrame(ushort pointer) => InFacing(pointer, FacingLeftStart) || InFacing(pointer, FacingRightStart);

    /// <summary>Checks alignment and extent within one facing's consecutive ten-byte frame records.</summary>
    /// <param name="pointer">Bank-local frame pointer being checked.</param>
    /// <param name="start">First pointer in the facing-specific run.</param>
    /// <returns><see langword="true"/> when the pointer selects one of the seventeen records in that run.</returns>
    private static bool InFacing(ushort pointer, ushort start)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < 170 && offset % 10 == 0;
    }
}
