namespace SuperMetroid.Core.Rooms;

/// <summary>Masks over a 16-bit room pixel coordinate and its fraction, as the cartridge's terrain code applies them.</summary>
internal static class RoomPixelMasks
{
    /// <summary>Pixels along a block's side.</summary>
    private const int BlockPixels = 16;

    /// <summary>Pixels along a half block's side.</summary>
    private const int HalfBlockPixels = 8;

    /// <summary>A pixel's offset within its 16-pixel block, 0-15; OR-ing it selects the block's last pixel.</summary>
    internal const int WithinBlock = BlockPixels - 1;

    /// <summary>Clears a 16-bit coordinate's within-block offset, leaving the coordinate of the block's first pixel.</summary>
    internal const int BlockStart = ushort.MaxValue ^ WithinBlock;

    /// <summary>The largest fraction: the position sits just short of the next whole pixel.</summary>
    internal const ushort LastFraction = ushort.MaxValue;

    /// <summary>Clears a 16-bit coordinate's offset within an eight-pixel half block.</summary>
    internal const int HalfBlockStart = ushort.MaxValue ^ (HalfBlockPixels - 1);

    /// <summary>Bits 4-7: a coordinate's block row within its 256-pixel screen, sixteen rows of sixteen pixels.</summary>
    internal const int BlockRowWithinScreenBits = WithinBlock * BlockPixels;
}
