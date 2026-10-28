using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The work RAM and VRAM a library-background command list reads and writes as bank $82's interpreter runs it, which
/// extraction's replay of the list reproduces (#1406).
/// </summary>
internal static class LibraryBackgroundStagingDefinitions
{
    /// <summary><c>$7E:4000</c>: the tilemap staging decompression and fills write, and transfers read.</summary>
    internal static readonly SnesAddress TilemapStagingAddress = new(0x7e, 0x4000);

    /// <summary>The tilemap staging's offset within bank $7E, which decompression and fill destinations count from.</summary>
    internal const int TilemapStagingBankOffset = 0x4000;

    /// <summary>The tilemap staging spans <c>$7E:4000-$7E:4FFF</c>.</summary>
    internal const int TilemapStagingByteCount = 0x1000;

    /// <summary><c>$7E:2000</c>: the enemy BG2 staging boss-room lists copy to BG2.</summary>
    internal static readonly SnesAddress EnemyBg2StagingAddress = new(0x7e, 0x2000);

    /// <summary>The enemy BG2 staging spans <c>$7E:2000-$7E:2FFF</c>.</summary>
    internal const int EnemyBg2StagingByteCount = 0x1000;

    /// <summary>The bytes of the Tourian statue ghost's 48 uncompressed 4-bpp characters, which its list transfers whole.</summary>
    internal const int TourianStatueGhostByteCount = 0x0600;

    /// <summary>Command six's fill of the staging with the blank FX tile and its transfer to BG3's tilemap; no retail list runs it.</summary>
    internal static readonly LibraryBackgroundClear ClearFx = new((int)TilemapStagingAddress, 0x4000, 0x0f00,
        new SnesBgTilemapWord(0x184e), VramDmaDestination.FromNative(0x5880));

    /// <summary>Commands A and C's fill of the staging with the blank BG2 tile and its transfer to BG2's two pages.</summary>
    internal static readonly LibraryBackgroundClear ClearBg2 = new((int)TilemapStagingAddress, 0x4000, 0x1000,
        new SnesBgTilemapWord(0x0338), VramDmaDestination.FromNative(0x4800));

    /// <summary>Command C's additional destination for the filled staging: Kraid's BG2 page.</summary>
    internal const ushort KraidBg2VramDestinationWord = 0x4000;
}
