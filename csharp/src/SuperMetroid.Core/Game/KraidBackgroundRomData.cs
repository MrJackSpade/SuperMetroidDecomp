namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge addresses and PPU layout constants used by Kraid's private BG2 renderer.
/// The boss body is not an OBJ: bank $A7 constructs a 64x64 BG2 surface in WRAM and
/// uploads its visible pages independently of the ordinary room background streamer.
/// </summary>
public static class KraidBackgroundRomData
{
    /// <summary>Bank-$A7 base used by Kraid's private instruction-list pointers.</summary>
    public const int EnemyBankBase = 0xa70000;

    /// <summary>Compressed upper-body tilemap consumed by <c>$A7:AAC6</c>.</summary>
    public const int UpperTilemap = 0xb9fa38;

    /// <summary>Compressed lower-body tilemap consumed by <c>$A7:AAC6</c>.</summary>
    public const int LowerTilemap = 0xb9fe3e;

    /// <summary>Exact decompressed size of either source tilemap.</summary>
    public const int DecompressedTilemapBytes = 0x1000;

    /// <summary>Size of Kraid's WRAM $7E:2000-$2FFF tilemap in 16-bit words.</summary>
    public const int WorkingTilemapWords = 0x0800;

    /// <summary>First word of the lower half in Kraid's combined working tilemap.</summary>
    public const int WorkingLowerHalfWord = 0x0400;

    /// <summary>
    /// First untouched word left by <c>$A7:AB19-$AB2C</c> after the lower stream is
    /// decompressed directly into the working tilemap.
    /// </summary>
    public const int PreservedLowerTailFirstWord = 0x0700;

    /// <summary>Number of lower-stream words retained at working words $700-$7FF.</summary>
    public const int PreservedLowerTailWordCount = 0x0100;

    /// <summary>Number of lower-source words copied by <c>$A7:AAC6</c>.</summary>
    public const int LowerSourceCopyWords = 0x0300;

    /// <summary>Number of words in either visible 32x32 BG2 page.</summary>
    public const int VisiblePageWords = 0x0400;

    /// <summary>Number of animated head words copied by <c>$A7:AF3D</c>.</summary>
    public const int HeadTilemapWords = 0x0160;

    /// <summary>BG2 tilemap word base selected by native BG2SC value $43.</summary>
    public const ushort LiveBg2TilemapWord = 0x4000;

    /// <summary>Second vertical page of the native 64x64 BG2 tilemap.</summary>
    public const ushort LiveLowerBg2TilemapWord = 0x4800;

    /// <summary>Ordinary room BG2 base restored after Kraid has sunk.</summary>
    public const ushort OrdinaryRoomBg2TilemapWord = 0x4800;

    /// <summary>
    /// <c>Tiles_Standard_BG3</c> at <c>$9A:B200</c>, restored by Kraid death functions
    /// <c>$A7:C777-$C7EF</c> after the private BG2 map has overwritten VRAM word $4000.
    /// </summary>
    public const int StandardBg3TilesAddress = 0x9ab200;

    /// <summary>One of the four $0400-byte standard-BG3 transfers made during Kraid's death.</summary>
    public const ushort StandardBg3TransferBytes = 0x0400;

    /// <summary>First VRAM word restored by <c>$A7:C777</c>.</summary>
    public const ushort StandardBg3VramWord = 0x4000;

    /// <summary>Number of sequential standard-BG3 quarters restored by Kraid's death AI.</summary>
    public const int StandardBg3TransferCount = 4;

    /// <summary>
    /// <c>Tiles_KraidRoomBackground</c> at <c>$A7:A716</c>, uploaded both when Kraid
    /// finishes growing and during defeated-room initialization.
    /// </summary>
    public const int RoomBackgroundTileAddress = 0xa7a716;

    /// <summary>Byte count of the Kraid room-background character upload.</summary>
    public const ushort RoomBackgroundTileBytes = 0x0200;

    /// <summary>VRAM word destination computed by $A7:ADCC-$ADD6 for BG1 base zero.</summary>
    public const ushort RoomBackgroundTileVramWord = 0x3f00;

    /// <summary>Priority flag cleared while composing Kraid's initial BG2 surface.</summary>
    public const ushort PriorityBit = 0x2000;

    /// <summary>Blank tile installed in Kraid's final working-map row.</summary>
    public const ushort BlankTile = 0x0338;

    /// <summary>First word of the 32-word blank row at WRAM $7E:2FC0.</summary>
    public const int BlankRowWorkingWord = 0x07e0;

    /// <summary>Native visible BG2 width selected by BG2SC size bits $03.</summary>
    public const int TilemapWidthInTiles = 64;

    /// <summary>Native visible BG2 height selected by BG2SC size bits $03.</summary>
    public const int TilemapHeightInTiles = 64;
}
