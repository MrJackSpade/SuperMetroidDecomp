namespace SuperMetroid.Core.Rooms;

/// <summary>Masks over a 16-bit room pixel coordinate and its fraction, as the cartridge's terrain code applies them.</summary>
internal static class RoomPixelMasks
{
    /// <summary>A pixel's offset within its 16-pixel block, 0-15; OR-ing it selects the block's last pixel.</summary>
    internal const int WithinBlock = 0x000f;

    /// <summary>Clears the within-block offset, leaving the coordinate of the block's first pixel.</summary>
    internal const int BlockStart = 0xfff0;

    /// <summary>The largest fraction: the position sits just short of the next whole pixel.</summary>
    internal const ushort LastFraction = 0xffff;

    /// <summary>Clears the coordinate's offset within an eight-pixel half block.</summary>
    internal const int HalfBlockStart = 0xfff8;

    /// <summary>Bits 4-7: a coordinate's block row within its 256-pixel screen.</summary>
    internal const int BlockRowWithinScreenBits = 0x00f0;
}
