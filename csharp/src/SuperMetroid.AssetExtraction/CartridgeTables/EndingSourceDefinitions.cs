using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers ending codecs resolve.</summary>
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
        /// <summary>The largest decompressed length permitted when importing the ending's compressed artwork.</summary>
        internal const int DecompressionLimit = 0x8000;

        /// <summary>$98:BCD6, Tiles_Zebes_Being_Zoomed_Out_during_Zebes_Explosion_Mode7: escape scene A's compressed high-byte Mode 7 character lane.</summary>
        internal const int EscapeACharacters = 0x98bcd6;

        /// <summary>$99:D17E, InterleavedTilesTilemap_ZebesBeingZoomedOutExplosion_Mode7: escape scene A's packed low-byte map source.</summary>
        internal const int EscapeAPackedMap = 0x99d17e;

        /// <summary>$98:ED4F, Tiles_Grey_Clouds_during_Zebes_Explosion_Mode7: escape scene B's compressed high-byte character lane.</summary>
        internal const int EscapeBCharacters = 0x98ed4f;

        /// <summary>$99:D65B, InterleavedTilesTilemap_GreyCloudsDuringZebesExplosion_Mode7: escape scene B's packed low-byte map source.</summary>
        internal const int EscapeBPackedMap = 0x99d65b;

        /// <summary>$99:9101, Tiles_Big_Zebes_during_Zebes_Explosion: the planet explosion's compressed high-byte Mode 7 character lane.</summary>
        internal const int ExplosionCharacters = 0x999101;

        /// <summary>$99:D932, InterleavedTilesTilemap_BigZebesDuringZebesExplosion_Mode7: the planet explosion's packed low-byte map source.</summary>
        internal const int ExplosionPackedMap = 0x99d932;

        /// <summary>$98:8304: the explosion sprite sheet $8B:D56C decompresses.</summary>
        internal const int ExplosionObjectCharacters = 0x988304;

        /// <summary>$99:A56F: the atmospheric cloud sprite sheet $8B:D4D1 decompresses.</summary>
        internal const int EscapeCloudCharacters = 0x99a56f;

        /// <summary>$98:B5C1, Wide_Part_of_Zebes_Explosion_Tilemap: the wide explosion fragment.</summary>
        internal const int WideExplosionTilemap = 0x98b5c1;

        /// <summary>$98:B857, Concentric_Wide_Part_of_Zebes_Explosion_Tilemap: the concentric wide explosion fragment.</summary>
        internal const int ConcentricExplosionTilemap = 0x98b857;

        /// <summary>$98:BAED, Eclipse_of_Zebes_during_Explosion_Tilemap: the eclipse fragment.</summary>
        internal const int EclipseTilemap = 0x98baed;

        /// <summary>$98:BCCD, Blank_BG2_Tilemap: the blank fragment.</summary>
        internal const int BlankTilemap = 0x98bccd;

        /// <summary>$97:E7DE, Tiles_Font3_Background: the font characters the credits and post-credits text reuse.</summary>
        internal const int FontCharacters = 0x97e7de;

        /// <summary>$97:9803, Tiles_Samus_Waiting_for_Credits_to_End: the two-frame waiting-Samus sheet, also reused for suited rewards.</summary>
        internal const int WaitingForCreditsCharacters = 0x979803;

        /// <summary>$97:B957, Tiles_PostCredits_SuitlessSamus: the under-three-hour reward's sprite sheet.</summary>
        internal const int SuitlessSamusCharacters = 0x97b957;

        /// <summary>$97:D7FC, Tiles_PostCredits_Samus_Shooting_the_Screen: the sprite sheet of Samus shooting the screen.</summary>
        internal const int ShootingScreenCharacters = 0x97d7fc;

        /// <summary>$97:96F4, Samus_Waiting_for_Credits_to_End_Tilemap: the background map accompanying the waiting-Samus sheet.</summary>
        internal const int WaitingForCreditsTilemap = 0x9796f4;

        /// <summary>$97:F987, InterleavedTilesTilemapPostCreditsSamusBeamMode7: the interleaved Mode 7 beam artwork, separate from the suited reward sheet.</summary>
        internal const int PostCreditsFloorCharacters = 0x97f987;

        /// <summary>$99:DA9F, Tiles_PostCredits_Samus_Transformation_Effect: the transformation effect's characters.</summary>
        internal const int TransformationCharacters = 0x99da9f;

        /// <summary>$99:DAB1, PostCredits_Samus_Transformation_Effect_Tilemap: the transformation effect's tilemap.</summary>
        internal const int TransformationTilemap = 0x99dab1;

        /// <summary>$99:E089: the compressed Super Metroid logo tiles the post-shot transition shows.</summary>
        internal const int PostShotLogoTiles = 0x99e089;

        /// <summary>$99:ECC4: the compressed logo map the post-shot transition shows.</summary>
        internal const int PostShotLogoMap = 0x99ecc4;
    }

    /// <summary>The scrolling credits' row program and tilemap (#1416: moved from Core's credits presentation definitions).</summary>
    internal static class Credits
    {
        /// <summary>$97:EEFF, the compressed 128-row credits source tilemap.</summary>
        internal const int Tilemap = 0x97eeff;

        /// <summary>The expanded credits source tilemap's bytes, staged at $7F:0000..1FFF.</summary>
        internal const int TilemapBytes = 0x2000;

        /// <summary>$8C:D91B, the beginning of the retail credits row instruction stream.</summary>
        internal const ushort InitialInstruction = 0xd91b;

        /// <summary>The bank holding the credits row instruction stream.</summary>
        internal const byte InstructionBank = 0x8c;

        /// <summary>The high bit distinguishing a bank-$8B callback command from a duration/source-row record.</summary>
        internal const ushort CommandBit = 0x8000;

        /// <summary>The extraction bound of 4096 interpreted stream operations, including repeated loop commands; separate from the expected 520 output rows.</summary>
        internal const int MaximumOperations = 4096;
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
