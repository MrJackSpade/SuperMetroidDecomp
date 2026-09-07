namespace SuperMetroid.Core.Rooms;

/// <summary>Native two-screen reveal buffer layout used by $91:CB8E and its VRAM transfers.</summary>
public static class XrayTilemapLayout
{
    /// <summary>One SNES tilemap screen contains 32 by 32 tile words.</summary>
    public const int ScreenWords = 1024;
    /// <summary>Both BG2 screens are transferred, even though only the first column of the second is populated.</summary>
    public const int BufferWords = ScreenWords * 2;
    /// <summary>Sixteen rows of metatiles are constructed, including offscreen rows.</summary>
    public const int MetatileRows = 16;
    /// <summary>Sixteen complete columns plus the right-hand fine-scroll column.</summary>
    public const int MetatileColumns = 17;
    /// <summary>Two tile rows per metatile row, 32 words per tile row.</summary>
    public const int MetatileRowStride = 64;
}
