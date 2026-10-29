namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>Cartridge values and helpers of <see cref="AssetExtraction.Bosses.DraygonBg2FrameDefinitions"/> that only verification reads.</summary>
internal static class DraygonBg2FrameDefinitionsConstants
{
    /// <summary>$A5:A31B, first left-facing one-component BG2 body frame.</summary>
    internal const ushort FacingLeftStart = 0xa31b;
    /// <summary>$A5:A643, first right-facing one-component BG2 body frame.</summary>
    internal const ushort FacingRightStart = 0xa643;
    /// <summary>Recognizes only ten-byte-spaced roots in either seventeen-frame facing run.</summary>
    internal static bool IsFrame(ushort pointer) => InFacing(pointer, FacingLeftStart) || InFacing(pointer, FacingRightStart);
    /// <summary>Checks a pointer's alignment and extent within one native facing run.</summary>
    internal static bool InFacing(ushort pointer, ushort start)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < 170 && offset % 10 == 0;
    }
}
