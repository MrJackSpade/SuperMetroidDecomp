namespace SuperMetroid.Core.Frontend;

/// <summary>PPU register definitions shared by file-select and options composition.</summary>
internal static class MenuRenderDefinitions
{
    /// <summary>
    /// OBSEL ($2101) = $03: menu OBJ characters begin at VRAM word $6000;
    /// size selector zero chooses 8x8/16x16 records.
    /// </summary>
    internal const byte ObjectSelection = 0x03;
}
