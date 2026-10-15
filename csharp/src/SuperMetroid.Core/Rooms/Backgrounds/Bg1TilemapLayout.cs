namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The BG1 tilemap ring the room streamer writes (#1299): two 32x32 tilemap screens side by side in VRAM, holding a
/// ring of 32 block columns and 16 block rows.
/// </summary>
internal static class Bg1TilemapLayout
{
    /// <summary>VRAM word address of BG1's first tilemap screen.</summary>
    internal const ushort FirstScreen = 0x5000;
    /// <summary>VRAM word address of BG1's second tilemap screen.</summary>
    internal const ushort SecondScreen = 0x5400;
    /// <summary>The words of one 32x32 tilemap screen.</summary>
    internal const ushort ScreenWords = 0x0400;
    /// <summary>The blocks across one screen.</summary>
    internal const int ScreenBlocks = 16;
    /// <summary>The ring's block columns across both screens, as a mask.</summary>
    internal const int RingColumnMask = 2 * ScreenBlocks - 1;
    /// <summary>The ring's block rows down one screen, as a mask.</summary>
    internal const int RingRowMask = ScreenBlocks - 1;
    /// <summary>The words of one tilemap row.</summary>
    internal const ushort TileRowWords = 32;
    /// <summary>The words of one block row: two tilemap rows.</summary>
    internal const int BlockRowWords = 2 * TileRowWords;
    /// <summary>The base a ring column of the second screen (16..31) offsets from: its columns start 16 blocks in.</summary>
    internal const ushort SecondScreenColumnBase = SecondScreen - 2 * ScreenBlocks;
    /// <summary>The BG1 horizontal offset bit at which the two screens swap places.</summary>
    internal const ushort ScreenSwapScrollBit = 0x0100;
    /// <summary>The bytes of one block column down the ring (sixteen blocks of four bytes): where a column transfer wraps.</summary>
    internal const int ColumnBytes = 4 * ScreenBlocks;
}
