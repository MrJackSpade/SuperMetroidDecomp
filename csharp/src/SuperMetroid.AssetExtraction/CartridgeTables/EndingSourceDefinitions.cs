using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers ending codecs resolve.</summary>
[CpuBusBoundary("catalog: ending cartridge tables")]
internal static class EndingSourceDefinitions
{
    /// <summary>
    /// $8C:8000-$8C:FFFF: cinematic data including the ending logo crossfade palettes. Logo palette
    /// pointers address the last color of a sixteen-color BGR555 row in this bank.
    /// </summary>
    internal static readonly CpuBusCell LogoPaletteBank = CpuBusCell.CartridgeBytes(nameof(LogoPaletteBank), 0x8c8000, 0x8000,
        "BGR555 palette rows (u16 little-endian)");

    /// <summary>The ending's compressed artwork sources (#1416: moved from Core's ending catalogs).</summary>
    internal static class Artwork
    {
        /// <summary>$99:E089: the compressed Super Metroid logo tiles the post-shot transition shows.</summary>
        internal const int PostShotLogoTiles = 0x99e089;

        /// <summary>$99:ECC4: the compressed logo map the post-shot transition shows.</summary>
        internal const int PostShotLogoMap = 0x99ecc4;
    }

    /// <summary>The ending's palette sources (#1416: moved from Core's ending catalogs).</summary>
    internal static class Palettes
    {
        /// <summary>$8C:EDE9, Palettes_CloudSpritesInZebesExplosionScene: the escape scenes' full palette image.</summary>
        internal const int Escape = 0x8cede9;

        /// <summary>$8B:DE43, the final gunship palette restored before the operation text.</summary>
        internal const int FinalGunship = 0x8bde43;

        /// <summary>$8C:E7E9, kPalettes_Intro4, restored by the end-credits instruction $8B:F6FE.</summary>
        internal const int PostCredits = 0x8ce7e9;

        /// <summary>$8C:E9E9, Palettes_Credits: the credits palette image, whose background colours supply the scrolling text.</summary>
        internal const int Credits = 0x8ce9e9;

        /// <summary>$8C:EBE9, Palettes_ZebesExplosionScene: the planet-explosion palette image.</summary>
        internal const int Explosion = 0x8cebe9;

        /// <summary>$8C:EFE9: the sixteen colours $8B:E504 loads into sprite palette seven for the assembling logo.</summary>
        internal const int LogoInitial = 0x8cefe9;

        /// <summary>$8C:F3E7 ($8B:E5E7's first background pointer): the last colour of background palette $10.</summary>
        internal static readonly SnesAddress LogoCrossfadeFirstBackgroundEnd = SnesAddress.FromBusAddress(0x8cf3e7);

        /// <summary>$8C:F007 ($8B:E5E9's first sprite pointer): the last colour of sprite palette $1.</summary>
        internal static readonly SnesAddress LogoCrossfadeFirstSpriteEnd = SnesAddress.FromBusAddress(0x8cf007);
    }

    /// <summary>Bank containing every post-credit text resource in this catalog.</summary>
    internal const byte EndingBank = 0x8c;

    /// <summary>$8C:DC9B, nine-row producer/result panel.</summary>
    internal const ushort EndingResultPanel = 0xdc9b;

    /// <summary>$8C:DEDB, two-row 1994 Nintendo panel.</summary>
    internal const ushort EndingCopyrightPanel = 0xdedb;

    /// <summary>$8C:DF5B, two-row alternate-language percentage subtitle.</summary>
    internal const ushort EndingJapaneseSubtitle = 0xdf5b;

    /// <summary>$8C:DFDB, item-percentage typewriter stream.</summary>
    internal const ushort EndingItemPercentage = 0xdfdb;

    /// <summary>$8C:E0AF, final-message typewriter stream.</summary>
    internal const ushort EndingFinalMessage = 0xe0af;

    /// <summary>$8B:E627, draws the dynamic percentage and optional subtitle.</summary>
    internal const ushort EndingDrawPercentageOpcode = 0xe627;

    /// <summary>$8B:E769, installs the alternate-language percentage subtitle.</summary>
    internal const ushort EndingDrawSubtitleOpcode = 0xe769;

    /// <summary>$8B:E780, clears the subtitle and requests the ending scroll.</summary>
    internal const ushort EndingClearSubtitleOpcode = 0xe780;

    /// <summary>$8B:9698, deletes the cinematic background object.</summary>
    internal const ushort EndingDeleteOpcode = 0x9698;

    /// <summary>$8B:88B7, draws a bounded ending-text rectangle.</summary>
    internal const ushort EndingDrawFunction = 0x88b7;

    /// <summary>$8B:8849, no-op marker payload.</summary>
    internal const ushort EndingDoNothingFunction = 0x8849;

    /// <summary>$8C:E12F, shared no-op marker record.</summary>
    internal const ushort EndingMarkerData = 0xe12f;

    /// <summary>Packed origin (0,0) used by both no-op marker records.</summary>
    internal const ushort EndingMarkerPackedPosition = 0x0000;

    /// <summary>Low-byte X component of packed ending-text positions.</summary>
    internal const ushort EndingPackedPositionXMask = 0x00ff;

    /// <summary>Width in tile cells of every native ending-text glyph.</summary>
    internal const byte EndingGlyphWidth = 1;

    /// <summary>High bit distinguishing an instruction word from a frame delay.</summary>
    internal const ushort EndingCommandBit = 0x8000;

    /// <summary>Safety limit on records decoded from one native text stream.</summary>
    internal const int EndingMaximumRecords = 128;
}
