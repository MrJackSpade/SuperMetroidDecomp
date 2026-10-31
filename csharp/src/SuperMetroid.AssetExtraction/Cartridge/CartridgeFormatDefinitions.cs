using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>Cartridge layout and list-format constants the extraction decoders rely on.</summary>
[EncodingOwner("cartridge bank sizes and native list terminator words")]
internal static class CartridgeFormatDefinitions
{
    /// <summary>$FFFF: the word that ends native pointer and header lists.</summary>
    internal const ushort ListTerminator = 0xffff;

    /// <summary>$10000: the bytes one 16-bit bank offset spans.</summary>
    internal const int BankByteCount = 0x10000;

    /// <summary>$8000: the bytes of ROM one LoROM bank maps, at offsets $8000..$FFFF; also the largest compressed stream a decoder reads.</summary>
    internal const int LoRomBankByteCount = 0x8000;

    /// <summary>$300000: the retail cartridge's unheadered size (3 MiB).</summary>
    internal const int RetailRomByteCount = 0x300000;

    /// <summary>$400000: the largest LoROM image the importer maps (4 MiB).</summary>
    internal const int MaximumRomByteCount = 0x400000;

    /// <summary>$FFFFFF: the highest 24-bit CPU address.</summary>
    internal const int HighestCpuAddress = 0x00ff_ffff;

    /// <summary>$8D: the bank of enemy-projectile spritemaps.</summary>
    internal const byte EnemyProjectileSpritemapBank = 0x8d;
}
