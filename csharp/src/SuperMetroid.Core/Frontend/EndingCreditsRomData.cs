using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verified bank-$8B/$8C data used by the ending, credits, and result screen.</summary>
public static class EndingCreditsRomData
{
    /// <summary>
    /// $8B:D484-$D48C: <c>CinematicFunction_Ending_Setup</c> loads X=8 and calls WaitForNMI
    /// until DEX/BPL falls through, nine NMI waits before its PPU and scene setup.
    /// </summary>
    public const int SetupNmiWaits = 9;

    /// <summary>Native palette images and compressed artwork sources used to import ending and credits presentation assets.</summary>
    public static class Assets
    {
        /// <summary>$8C:EDE9, Palettes_CloudSpritesInZebesExplosionScene: full atmospheric-cloud palette image loaded for the escape scenes.</summary>
        public const int EscapePalette = 0x8cede9;
        /// <summary>$8B:DE43, final gunship palette restored by Func124 before operation text.</summary>
        public const int FinalGunshipPalette = 0x8bde43;
        /// <summary>$8C:E7E9, kPalettes_Intro4, restored by the end-credits instruction $8B:F6FE.</summary>
        public const int PostCreditsPalette = 0x8ce7e9;
        /// <summary>$8C:E9E9, Palettes_Credits: full credits palette image, whose BG colors supply scrolling text.</summary>
        public const int CreditsPalette = 0x8ce9e9;
        /// <summary>$8C:EBE9, Palettes_ZebesExplosionScene: explosion palette image used when transitioning from cloud scenes to Zebes destruction.</summary>
        public const int ExplosionPalette = 0x8cebe9;
        /// <summary>$98:BCD6, Tiles_Zebes_Being_Zoomed_Out_during_Zebes_Explosion_Mode7: compressed high-byte Mode 7 character lane for escape scene A; the legacy Map name does not denote its lane.</summary>
        public const int EscapeMapA = 0x98bcd6;
        /// <summary>$98:ED4F, Tiles_Grey_Clouds_during_Zebes_Explosion_Mode7: compressed high-byte character lane for escape scene B.</summary>
        public const int EscapeMapB = 0x98ed4f;
        /// <summary>$99:9101, Tiles_Big_Zebes_during_Zebes_Explosion: compressed high-byte Mode 7 character lane for the planet-explosion scene.</summary>
        public const int ExplosionMap = 0x999101;
        /// <summary>$99:D17E, InterleavedTilesTilemap_ZebesBeingZoomedOutExplosion_Mode7: packed low-byte map source for escape A; its interleaved character bytes are not the selected character lane.</summary>
        public const int EscapeCharactersA = 0x99d17e;
        /// <summary>$99:D65B, InterleavedTilesTilemap_GreyCloudsDuringZebesExplosion_Mode7: packed low-byte map source for escape B.</summary>
        public const int EscapeCharactersB = 0x99d65b;
        /// <summary>$99:D932, InterleavedTilesTilemap_BigZebesDuringZebesExplosion_Mode7: packed low-byte map source for the planet explosion.</summary>
        public const int ExplosionCharacters = 0x99d932;
        /// <summary>Original ending scene decompression sources. EscapeA: $8B:D4B4/D4D6;
        /// EscapeB: $8B:D775/D604; explosion: $8B:D87B/D615. Each pair is the
        /// high-byte character lane followed by the packed low-byte map source.
        /// The legacy Map/Characters constant names are inverted relative to these lanes.</summary>
        public static (int Characters, int PackedMap) Mode7Sources(EndingMode7SceneId id) => id switch
        {
            EndingMode7SceneId.EscapeA => (EscapeMapA, EscapeCharactersA),
            EndingMode7SceneId.EscapeB => (EscapeMapB, EscapeCharactersB),
            EndingMode7SceneId.PlanetExplosion => (ExplosionMap, ExplosionCharacters),
            _ => throw new ArgumentOutOfRangeException(nameof(id)),
        };
        /// <summary>$8B:D56C decompresses the explosion OBJ sheet into $7F:8000.</summary>
        public const int EndingObjectCharacters = 0x988304;
        /// <summary>$8B:D4D1 decompresses the atmospheric cloud OBJ sheet for VRAM word $6000.</summary>
        public const int EscapeCloudCharacters = 0x99a56f;
        /// <summary>$98:B5C1, Wide_Part_of_Zebes_Explosion_Tilemap: compressed fragment uploaded to VRAM word $7000, despite the legacy Characters name.</summary>
        public const int EndingObjectCharacters70 = 0x98b5c1;
        /// <summary>$98:B857, Concentric_Wide_Part_of_Zebes_Explosion_Tilemap: compressed fragment uploaded to VRAM word $7400.</summary>
        public const int EndingObjectCharacters74 = 0x98b857;
        /// <summary>$98:BAED, Eclipse_of_Zebes_during_Explosion_Tilemap: compressed fragment uploaded to VRAM word $7800.</summary>
        public const int EndingObjectCharacters78 = 0x98baed;
        /// <summary>$98:BCCD, Blank_BG2_Tilemap: compressed blank fragment uploaded to VRAM word $7C00.</summary>
        public const int EndingObjectCharacters7C = 0x98bccd;
        /// <summary>$8B:D5AF..D5F2 decompression sources for the four ending fragments.</summary>
        public static int ObjectFragmentSource(EndingObjectFragmentId id) => id switch
        {
            EndingObjectFragmentId.Segment70 => EndingObjectCharacters70,
            EndingObjectFragmentId.Segment74 => EndingObjectCharacters74,
            EndingObjectFragmentId.Segment78 => EndingObjectCharacters78,
            EndingObjectFragmentId.Segment7C => EndingObjectCharacters7C,
            _ => throw new ArgumentOutOfRangeException(nameof(id)),
        };
        /// <summary>$97:E7DE, Tiles_Font3_Background: compressed Font 3 character artwork reused by credits and post-credits text.</summary>
        public const int EndingFontCharacters = 0x97e7de;
        /// <summary>$97:9803, Tiles_Samus_Waiting_for_Credits_to_End: compressed two-frame waiting-Samus sheet, also reused for suited rewards.</summary>
        public const int WaitingForCreditsCharacters = 0x979803;
        /// <summary>$97:B957, Tiles_PostCredits_SuitlessSamus: compressed OBJ sheet selected for the under-three-hour reward.</summary>
        public const int SuitlessSamusCharacters = 0x97b957;
        /// <summary>$97:D7FC, Tiles_PostCredits_Samus_Shooting_the_Screen: compressed OBJ sheet for Samus shooting the screen.</summary>
        public const int ShootingScreenCharacters = 0x97d7fc;
        /// <summary>$97:96F4, Samus_Waiting_for_Credits_to_End_Tilemap: compressed BG2 map accompanying the waiting-Samus sheet.</summary>
        public const int WaitingForCreditsTilemap = 0x9796f4;
        /// <summary>$97:F987, InterleavedTilesTilemapPostCreditsSamusBeamMode7: compressed interleaved Mode 7 beam artwork, separate from the suited reward sheet.</summary>
        public const int PostCreditsMode7Characters = 0x97f987;
        /// <summary>$99:DA9F, Tiles_PostCredits_Samus_Transformation_Effect: compressed BG3 transformation characters uploaded at VRAM byte $4000.</summary>
        public const int PostCreditsTileFragmentA = 0x99da9f;
        /// <summary>$99:DAB1, PostCredits_Samus_Transformation_Effect_Tilemap: compressed BG3 transformation tilemap uploaded at VRAM byte $4800.</summary>
        public const int PostCreditsTileFragmentB = 0x99dab1;
    }

    /// <summary>Bank-$8C cinematic background-object instruction-list identities for the final text sequences.</summary>
    public static class Instructions
    {
        /// <summary>$8C:DFDB, CinematicBGObjectInstLists_Ending_ItemPercentage: bank-relative instruction-list identity for the item-collection percentage text.</summary>
        public const ushort ItemPercentageText = 0xdfdb;
        /// <summary>$8C:E0AF, CinematicBGObjectInstLists_Ending_SeeYouNextMission: bank-relative instruction-list identity for the final message.</summary>
        public const ushort SeeYouNextMissionText = 0xe0af;
    }

    /// <summary>Ending VRAM transfer extents, byte and word destinations, tilemap geometry, and palette color-index ranges.</summary>
    public static class Rendering
    {
        /// <summary>$8B:E110 clears the 16-color BG2 waiting palette at byte index $0040.</summary>
        public const int WaitingPaletteStart = 32;
        /// <summary>Sixteen color words in the waiting-Samus BG2 palette and each reward palette fade range.</summary>
        public const int WaitingPaletteCount = 16;
        /// <summary>$8B:E1D2 clears byte-index $01A0 for the suitless reward palette.</summary>
        public const int SuitlessRewardPaletteStart = 208;
        /// <summary>$8B:E1D2 additionally clears byte-index $01C0 for the helmetless suited body.</summary>
        public const int SuitedRewardPaletteStart = 224;
        /// <summary>Ending bank-$8B Mode-7 projection center X, in physical map pixels.</summary>
        public const short Mode7CenterX = 56;
        /// <summary>Ending bank-$8B Mode-7 projection center Y, in physical map pixels.</summary>
        public const short Mode7CenterY = 24;
        /// <summary>$8B:D4xx/$D8xx atmospheric/explosion setup writes M7X and M7Y to $0080.</summary>
        public const short AtmosphericMode7Center = 128;
        /// <summary>Ending OBSEL=$02 selects the escape/explosion OBJ character base.</summary>
        public const byte EscapeObjectSelection = 2;
        /// <summary>$8B:8293 initializes OBSEL=$A3 for the atmospheric cloud scenes.</summary>
        public const byte CloudObjectSelection = 0xa3;
        /// <summary>$8B:83D3 sets OBSEL=$00; reward sprites use the contiguous sheet at VRAM word zero.</summary>
        public const byte RewardObjectSelection = 0;
        /// <summary>Font 3 blank tilemap word used to clear credits and post-credits pages before drawing text.</summary>
        public const ushort BlankTile = 0x007f;
        /// <summary>Width of credits and result tilemaps in tile cells.</summary>
        public const int TilemapWidth = 32;
        /// <summary>Height of credits and result tilemaps in tile cells.</summary>
        public const int TilemapHeight = 32;
        /// <summary>Number of sixteen-bit tilemap words in one 32-by-32 credits or result page.</summary>
        public const int TilemapWords = TilemapWidth * TilemapHeight;
        /// <summary>VRAM word address $4800 for the scrolling-credit BG1 tilemap.</summary>
        public const ushort CreditsTilemapWord = 0x4800;
        /// <summary>VRAM word address $4000 for the Font 3 character base used by scrolling credits.</summary>
        public const ushort CreditsCharacterWord = 0x4000;
        /// <summary>$8B:E190 selects BG1 for the result text, using the credits font/map bases.</summary>
        public const ushort PostCreditsTilemapWord = 0x4800;
        /// <summary>VRAM word address $4000 for the post-credits BG1 Font 3 character base, shared with the credits font.</summary>
        public const ushort PostCreditsCharacterWord = 0x4000;
        /// <summary>$8B:DFD9/$DFB9 load the waiting-Samus BG2 map and characters.</summary>
        public const ushort WaitingTilemapWord = 0x4c00;
        /// <summary>VRAM word address $5000 for BG2 waiting-Samus characters loaded by the post-credits setup.</summary>
        public const ushort WaitingCharacterWord = 0x5000;
        /// <summary>Size in bytes of one complete ending Mode 7 lane, also used for the full reward character sheet.</summary>
        public const int Mode7Bytes = 0x4000;
        /// <summary>$8B:DAD3 table stride and Func118 queue transfer size.</summary>
        public const int FlyawayUploadBytes = 0x0800;
        /// <summary>$8B:D56C preserves the first $0300 map bytes and fills the remainder with tile $8C.</summary>
        public const int FlyawayMapDataBytes = 0x0300;
        /// <summary>$8B:D5A1 blank map tile $8C fills the flyaway map after its retained $0300-byte prefix.</summary>
        public const byte FlyawayBlankTile = 0x8c;
        /// <summary>$8B:D8C1 explosion OBJ DMA byte count from $7F:8000.</summary>
        public const int ExplosionObjectBytes = 0x6000;
        /// <summary>Maximum decompressed byte length permitted when importing ending compressed assets.</summary>
        public const int DecompressionLimit = 0x8000;
        /// <summary>VRAM byte address $8000 for explosion artwork and the credits character upload.</summary>
        public const int ObjectCharactersDestination = 0x8000;
        /// <summary>VRAM byte address $A000 for ending font or waiting-Samus BG2 character uploads, depending on the scene.</summary>
        public const int FontCharactersDestination = 0xa000;
        /// <summary>VRAM byte address $C000 for atmospheric cloud or post-credits shooting OBJ characters, depending on the scene.</summary>
        public const int PostCreditsObjectDestination = 0xc000;
        /// <summary>VRAM byte address $9800, corresponding to word $4C00, for the waiting-Samus BG2 tilemap.</summary>
        public const int WaitingTilemapDestination = 0x9800;
        /// <summary>VRAM byte address $4000 for the small BG3 transformation-character upload.</summary>
        public const int PostCreditsFragmentADestination = 0x4000;
        /// <summary>VRAM byte address $4800 for the BG3 transformation tilemap upload.</summary>
        public const int PostCreditsFragmentBDestination = 0x4800;
        /// <summary>Byte length of each ending explosion tilemap fragment and the post-credits BG3 transformation tilemap upload.</summary>
        public const int ObjectFragmentBytes = 0x0800;
        /// <summary>Byte extent required of the ending font transfer before its $1000-byte upload.</summary>
        public const int ObjectFragmentLimit = 0x1000;
        /// <summary>Byte length of the Font 3 character upload for credits.</summary>
        public const int FontCharacterBytes = 0x1000;
        /// <summary>Byte length of the first waiting-Samus frame uploaded to its BG2 character base.</summary>
        public const int WaitingCharacterBytes = 0x2000;
        /// <summary>Byte length of the post-credits shooting OBJ character upload.</summary>
        public const int ShootingCharacterBytes = 0x4000;
        /// <summary>Byte length of the complete 32-by-32 waiting-Samus BG2 tilemap.</summary>
        public const int WaitingTilemapBytes = 0x0800;
        /// <summary>Byte length of the small BG3 transformation-character upload.</summary>
        public const int PostCreditsFragmentABytes = 0x0100;
        /// <summary>VRAM byte address $E000, corresponding to word $7000, for the wide explosion fragment.</summary>
        public const int Fragment70Destination = 0xe000;
        /// <summary>VRAM byte address $E800, corresponding to word $7400, for the concentric-wide explosion fragment.</summary>
        public const int Fragment74Destination = 0xe800;
        /// <summary>VRAM byte address $F000, corresponding to word $7800, for the eclipse explosion fragment.</summary>
        public const int Fragment78Destination = 0xf000;
        /// <summary>VRAM byte address $F800, corresponding to word $7C00, for the blank BG2 fragment.</summary>
        public const int Fragment7CDestination = 0xf800;
        /// <summary>Half-palette extent of 128 color words, used as source, length, and CGRAM destination for explosion OBJ colors; the legacy Bytes suffix does not denote bytes.</summary>
        public const int PaletteHalfBytes = 128;
        /// <summary>Number of color words restored after credits, covering color indexes 4-255; the legacy Bytes suffix does not denote bytes.</summary>
        public const int PostCreditsPaletteBytes = 252;
        /// <summary>Source and destination color index four for the post-credits palette restoration, preserving colors zero through three.</summary>
        public const int PostCreditsPaletteDestination = 4;
        /// <summary>Packed SNES BGR555 white, with all three five-bit channels at full intensity.</summary>
        public const ushort WhiteColor = 0x7fff;
    }

    /// <summary>Native music data and track selectors queued at escape and ending sequence boundaries.</summary>
    public static class Music
    {
        /// <summary>Music data selector $33 queued for the Zebes escape cinematic before its first scene becomes visible.</summary>
        public const byte EscapeDataIndex = 0x33;
        /// <summary>Music data selector $3C queued for the completion and credits sequence.</summary>
        public const byte EndingDataIndex = 0x3c;
        /// <summary>Track command five used with both the escape and ending music data sets.</summary>
        public const byte Track = 5;
        /// <summary>Native delayed-music Y argument $000E, interpreted by the music queue's delay conversion rather than as a direct frame count.</summary>
        public const ushort DelayArgument = 0x000e;
    }

    /// <summary>Whole-hour thresholds selecting suitless, helmetless, or fully armored Samus reward presentations.</summary>
    public static class Rewards
    {
        /// <summary>Exclusive whole-hour cutoff for the suitless reward: recorded game time below three hours.</summary>
        public const ushort SuitlessMaximumHoursExclusive = 3;
        /// <summary>Exclusive whole-hour cutoff for the helmetless suited reward: at least three but fewer than ten hours; ten or more keeps the helmet.</summary>
        public const ushort HelmetlessMaximumHoursExclusive = 10;
    }

    /// <summary>Authored reveal, palette-fade, and panel-hold durations measured in cinematic gameplay updates.</summary>
    public static class Timing
    {
        /// <summary>$8B:E293 interrupts reveal after var4 counts from 127 to 63; E342 later consumes the remaining 64 calls.</summary>
        public const int RewardRevealHalfFrames = 64;
        /// <summary>$8B:E293 sets var13=$00B4 for the copyright panel held by Func137.</summary>
        public const int CopyrightHoldFrames = 180;
        /// <summary>$8B:E110/E158 use 32 additions of target-component / 32 in 8.8 precision.</summary>
        public const int WaitingPaletteFadeFrames = 32;
        /// <summary>$8B:E158 sets cinematic_var4 to $00B4 for Func132's waiting backdrop.</summary>
        public const int WaitingBackdropFrames = 180;
    }

    /// <summary>Native Mode 7 zoom and angle thresholds, fixed-point ship translation, and credits scrolling increments.</summary>
    public static class Motion
    {
        /// <summary>Mode 7 zoom/matrix identity $0100, representing unity in the native 8.8 transform scale.</summary>
        public const ushort IdentityScale = 0x0100;
        /// <summary>$8B:D480/D731 seed cinematic_var5 (angle), not horizontal scrolling.</summary>
        public const byte EscapeInitialAngle = 0x20;
        /// <summary>$8B:D480/D731/D837 seed cinematic_var6 (zoom), not vertical scrolling.</summary>
        public const ushort EscapeInitialScale = 0x0040;
        /// <summary>Atmospheric zoom threshold $0180 at which escape scenes A and B begin fading out.</summary>
        public const ushort EscapeEndScale = 0x0180;
        /// <summary>Scale threshold $0060 enabling side-cloud motion in escape scene A.</summary>
        public const ushort CloudMotionStartScale = 0x0060;
        /// <summary>Scale limit $00B0 below which scene-B top and bottom clouds continue moving.</summary>
        public const ushort CloudSceneBScaleLimit = 0x00b0;
        /// <summary>$8B:DC2A initial space-view zoom $0C00 for the gunship emergence sequence.</summary>
        public const ushort PlanetEscapeInitialScale = 0x0c00;
        /// <summary>$8B:DCED zoom threshold $05B0; crossing below it switches fast spinning to the slower emergence phase.</summary>
        public const ushort PlanetFastEndScale = 0x05b0;
        /// <summary>$8B:DD84 zoom threshold $04A0; crossing below it starts the accelerating fly-to-camera phase.</summary>
        public const ushort PlanetSlowEndScale = 0x04a0;
        /// <summary>Fly-to-camera zoom threshold $0180 below which the ship turns toward its exit angle on every fourth cinematic update.</summary>
        public const ushort PlanetTurnStartScale = 0x0180;
        /// <summary>Fly-to-camera zoom threshold $0020; crossing below it restores gunship colors and begins the operation-success message.</summary>
        public const ushort PlanetExitScale = 0x0020;
        /// <summary>Initial low word $8000 of the fly-to-camera horizontal 16.16 velocity, paired with a zero integer word.</summary>
        public const ushort InitialAccelerationFraction = 0x8000;
        /// <summary>Horizontal velocity decrement per cinematic update in signed 16.16 pixels per update; $0100 equals 1/256 pixel per update squared.</summary>
        public const int AccelerationDelta16Point16 = 0x0000_0100;
        /// <summary>Credits vertical-scroll increment per update in unsigned 16.16 pixels: one-half pixel.</summary>
        public const uint CreditsScrollDelta16Point16 = 0x0000_8000;
        /// <summary>Native table-index angle $E0 approached one angle unit per update during the slow-spinning gunship emergence.</summary>
        public static SnesAngle PlanetSlowTargetAngle => SnesAngle.FromTableIndex(0xe0);
        /// <summary>Native table-index angle $10 approached during the final fly-to-camera turn, separate from the slow-spin target.</summary>
        public static SnesAngle PlanetExitTargetAngle => SnesAngle.FromTableIndex(0x10);
        /// <summary>$8B:DCDD masks EndingShipShakeIndex with0F, defining the sixteen-step fast domain.</summary>
        public const int PlanetFastPatternLength = 16;
        /// <summary>$8B:DD74 masks EndingShipShakeIndex with07, defining the eight-step slow domain.</summary>
        public const int PlanetSlowPatternLength = 8;
        /// <summary>$8B:DD02-DD41 and DDAD-DDCC each sum to this selected two-pixel cycle displacement; its exact cinematic translation choice is retained.</summary>
        private const int PlanetCycleDisplacement16Point16 = 2 << 16;
        /// <summary>$8B:DD02..DD11/DD1A..DD29: selected four-step positive fast runs in the reviewed cinematic translation composition.</summary>
        private const int FastPositiveRun = 4;
        /// <summary>$8B:DD12..DD19/DD2A..DD31/DD3A..DD41: selected two-step negative fast runs and terminal-negative policy in the cinematic translation composition.</summary>
        private const int FastNegativeRun = 2;
        /// <summary>$8B:DDAD..DDB8: selected first three positive slow steps in the cinematic translation composition.</summary>
        private const int SlowFirstPositiveRun = 3;
        /// <summary>$8B:DDB9..DDC0: selected following two negative slow steps in the cinematic translation composition.</summary>
        private const int SlowFirstNegativeRun = 2;
        /// <summary>$8B:DDC1..DDC8: selected following two positive slow steps before the terminal negative step.</summary>
        private const int SlowSecondPositiveRun = 2;

        private const int FastRunPeriod = FastPositiveRun + FastNegativeRun;
        private const int FastCompleteRunCount = PlanetFastPatternLength / FastRunPeriod;
        private const int FastFinalPositiveRun = PlanetFastPatternLength % FastRunPeriod - FastNegativeRun;
        private const int FastPositiveSteps = FastCompleteRunCount * FastPositiveRun + FastFinalPositiveRun;
        private const int SlowPositiveSteps = SlowFirstPositiveRun + SlowSecondPositiveRun;
        private const int PlanetFastShakeAmplitude = PlanetCycleDisplacement16Point16 / (2 * FastPositiveSteps - PlanetFastPatternLength);
        private const int PlanetSlowShakeAmplitude = PlanetCycleDisplacement16Point16 / (2 * SlowPositiveSteps - PlanetSlowPatternLength);
        /// <summary>$8B:DD02..DD41: repeated positive/negative shake runs and the shorter final positive run.</summary>
        /// <remarks>Repeated samples, fixed-point sign assembly and amplitudes from the common cycle displacement calculate. The
        /// exact directional schedules and shared displacement are selected cinematic path content:
        /// the sequential consumer has no geometry/state rule choosing reversal phases. Generating
        /// a different schedule changes that composition. Rotation, zoom and other paths are separate.</remarks>
        public static int PlanetFastDelta(int index)
        {
            if ((uint)index >= PlanetFastPatternLength) throw new IndexOutOfRangeException();
            int period = FastPositiveRun + FastNegativeRun;
            int completeRuns = PlanetFastPatternLength / period * period;
            int positiveRun = index < completeRuns ? FastPositiveRun : PlanetFastPatternLength - completeRuns - FastNegativeRun;
            return index % period < positiveRun ? PlanetFastShakeAmplitude : -PlanetFastShakeAmplitude;
        }

        /// <summary>$8B:DDAD..DDCC: two selected positive runs separated by negative shake phases.</summary>
        public static int PlanetSlowDelta(int index)
        {
            if ((uint)index >= PlanetSlowPatternLength) throw new IndexOutOfRangeException();
            int secondStart = SlowFirstPositiveRun + SlowFirstNegativeRun;
            bool positive = index < SlowFirstPositiveRun || index >= secondStart && index < secondStart + SlowSecondPositiveRun;
            return positive ? PlanetSlowShakeAmplitude : -PlanetSlowShakeAmplitude;
        }
    }

    /// <summary>Result-panel tile-word indexes, percentage glyphs and inventory masks, and clear-time sprite coordinates.</summary>
    public static class Text
    {
        /// <summary>Row-major tile-word index 288, column zero of row nine, where the 9-row result panel is placed.</summary>
        public const int ResultPanelDestination = 288;
        /// <summary>Number of tile words in the 32-column, 9-row result panel cleared before the copyright panel.</summary>
        public const int ResultPanelWords = 288;
        /// <summary>Row-major tile-word index 384, column zero of row twelve, where the copyright panel is placed.</summary>
        public const int CopyrightPanelDestination = 384;
        /// <summary>Row-major tile-word index 736, column zero of row twenty-three, where the Japanese subtitle region is drawn or cleared.</summary>
        public const int JapaneseSubtitleDestination = 736;
        /// <summary>Number of tile words in the two-row, 32-column Japanese subtitle region.</summary>
        public const int JapaneseSubtitleWords = 64;
        /// <summary>Tile-word index of the top half of the item-percentage hundreds digit; tens, units, and percent sign follow to its right.</summary>
        public const int PercentageHundredsTopIndex = 462;
        /// <summary>Packed BG tilemap word for the upper half of percentage digit zero; adding 0-9 selects the corresponding glyph.</summary>
        public const ushort DigitTopTile = 0x3860;
        /// <summary>Packed BG tilemap word for the lower half of percentage digit zero, placed one tilemap row below its upper half.</summary>
        public const ushort DigitBottomTile = 0x3870;
        /// <summary>Packed BG tilemap word for the upper half of the percentage sign, following the three numeric positions.</summary>
        public const ushort PercentTopTile = 0x386a;
        /// <summary>Packed BG tilemap word for the lower half of the percentage sign, placed one tilemap row below its upper half.</summary>
        public const ushort PercentBottomTile = 0x387a;
        /// <summary>Collected-item bit mask $F32F whose set bits contribute one each to the ending item percentage; ammo and energy pickups are counted separately.</summary>
        public const ushort CollectibleItemMask = 0xf32f;
        /// <summary>Collected-beam bit mask $100F whose set bits contribute one each to the ending item percentage.</summary>
        public const ushort CollectibleBeamMask = 0x100f;
        /// <summary>$8B:F064 clear-time hours tens-digit origin X=156 screen pixels.</summary>
        public const ushort HoursTensX = 0x009c;
        /// <summary>Clear-time hours units-digit origin X=164 screen pixels.</summary>
        public const ushort HoursUnitsX = 0x00a4;
        /// <summary>Clear-time minutes tens-digit origin X=180 screen pixels, to the right of the colon.</summary>
        public const ushort MinutesTensX = 0x00b4;
        /// <summary>$8B:F09A clear-time minutes units-digit origin X=188 screen pixels.</summary>
        public const ushort MinutesUnitsX = 0x00bc;
    }

    /// <summary>Ending actor constructor definitions with wrapped sixteen-bit pixel origins, OBJ palette attributes, and bank-$8B instruction-list pointers.</summary>
    public static class Sprites
    {
        /// <summary>OBJ palette two used by the operation-success text actors ($8B:F037 native palette word $0400).</summary>
        public static readonly SnesObjAttributeWord TextPalette = SnesObjPalettes.Index2;
        /// <summary>OBJ palette one used by the clear-time label, separate from digit actors' zero palette override.</summary>
        public static readonly SnesObjAttributeWord TimeLabelPalette = SnesObjPalettes.Index1;
        /// <summary>OBJ palette five used by atmospheric clouds, explosion overlays, and suitless reward actors.</summary>
        public static readonly SnesObjAttributeWord ScenePalette = SnesObjPalettes.Index5;
        /// <summary>OBJ palette six used by explosion afterglow and armored Samus reward actors.</summary>
        public static readonly SnesObjAttributeWord AlternatePalette = SnesObjPalettes.Index6;
        /// <summary>OBJ palette seven used by the exploding planet, glow, and star actors.</summary>
        public static readonly SnesObjAttributeWord PlanetPalette = SnesObjPalettes.Index7;
        /// <summary>$8B:EEEB / F0F9, yellow-cloud right actor: origin (320,192) pixels, OBJ palette five, instruction list $8B:ED0D.</summary>
        public static readonly EndingSpriteDefinition EscapeACloudRightTop =
            new(0x0140, 0x00c0, ScenePalette, 0xed0d);
        /// <summary>$8B:EEF1, yellow-cloud left actor: origin (-64,64) pixels encoded with wrapped X, OBJ palette five, instruction list $8B:ED15.</summary>
        public static readonly EndingSpriteDefinition EscapeACloudLeftTop =
            new(0xffc0, 0x0040, ScenePalette, 0xed15);
        /// <summary>$8B:EEEB / F0F9 with the alternate initialization parameter: right cloud at (320,448) pixels, palette five, list $8B:ED0D.</summary>
        public static readonly EndingSpriteDefinition EscapeACloudRightBottom =
            new(0x0140, 0x01c0, ScenePalette, 0xed0d);
        /// <summary>$8B:EEF1 with the alternate initialization parameter: left cloud at (-64,-192) pixels encoded as wrapped words, palette five, list $8B:ED15.</summary>
        public static readonly EndingSpriteDefinition EscapeACloudLeftBottom =
            new(0xffc0, 0xff40, ScenePalette, 0xed15);
        /// <summary>$8B:EED3 / F0B2: first upper cloud, parameter zero, origin (128,-96), list ECED.</summary>
        public static readonly EndingSpriteDefinition EscapeBCloudTopA =
            new(0x0080, 0xffa0, ScenePalette, 0xeced);
        /// <summary>$8B:EED9 / F0E1: second upper cloud, parameter zero, origin (128,-32), list ECF5.</summary>
        public static readonly EndingSpriteDefinition EscapeBCloudTopB =
            new(0x0080, 0xffe0, ScenePalette, 0xecf5);
        /// <summary>$8B:EEDF / F0E9: first lower cloud, parameter zero, origin (128,288), list ECFD.</summary>
        public static readonly EndingSpriteDefinition EscapeBCloudBottomA =
            new(0x0080, 0x0120, ScenePalette, 0xecfd);
        /// <summary>$8B:EEE5 / F0F1: second lower cloud, parameter zero, origin (128,352), list ED05.</summary>
        public static readonly EndingSpriteDefinition EscapeBCloudBottomB =
            new(0x0080, 0x0160, ScenePalette, 0xed05);
        /// <summary>$8B:EE9D, CinematicSpriteObjectDefinitions_ExplodingZebes_Zebes: planet actor at (128,128) pixels, palette seven, list $8B:EB0F.</summary>
        public static readonly EndingSpriteDefinition ExplodingZebes =
            new(0x0080, 0x0080, PlanetPalette, 0xeb0f);
        /// <summary>$8B:EEAF explosion palette-five actor: lava overlay at (128,128) pixels with instruction list $8B:EB59.</summary>
        public static readonly EndingSpriteDefinition ExplosionLava =
            new(0x0080, 0x0080, ScenePalette, 0xeb59);
        /// <summary>$8B:EEA3 purple-glow actor: origin (128,128) pixels, palette seven, instruction list $8B:EB3D.</summary>
        public static readonly EndingSpriteDefinition ExplosionGlow =
            new(0x0080, 0x0080, PlanetPalette, 0xeb3d);
        /// <summary>$8B:EEA9 explosion-stars actor: origin (128,128) pixels, palette seven, instruction list $8B:EB51.</summary>
        public static readonly EndingSpriteDefinition ExplosionStars =
            new(0x0080, 0x0080, PlanetPalette, 0xeb51);
        /// <summary>$8B:EEB5 palette-five explosion silhouette: origin (128,128) pixels with instruction list $8B:EB69.</summary>
        public static readonly EndingSpriteDefinition ExplosionSilhouette =
            new(0x0080, 0x0080, ScenePalette, 0xeb69);
        /// <summary>$8B:EEBB right-hand explosion-stars actor: origin (128,128) pixels, palette seven, instruction list $8B:EB71.</summary>
        public static readonly EndingSpriteDefinition ExplosionStarsRight =
            new(0x0080, 0x0080, PlanetPalette, 0xeb71);
        /// <summary>$8B:EEF7 / EFFD left-hand explosion-stars actor: origin (-128,128) pixels with wrapped X, palette seven, list $8B:EB81.</summary>
        public static readonly EndingSpriteDefinition ExplosionStarsLeft =
            new(0xff80, 0x0080, PlanetPalette, 0xeb81);
        /// <summary>$8B:EEC1 / F018, Zebes afterglow: origin (128,128) pixels, palette six, instruction list $8B:EB89.</summary>
        public static readonly EndingSpriteDefinition ExplosionAfterglow =
            new(0x0080, 0x0080, AlternatePalette, 0xeb89);
        /// <summary>$8B:EEC7 / F02B, THE OPERATION WAS actor: origin (128,96) pixels, palette two, instruction list $8B:EB91.</summary>
        public static readonly EndingSpriteDefinition OperationWasText =
            new(0x0080, 0x0060, TextPalette, 0xeb91);
        /// <summary>$8B:EECD / F02B, COMPLETED SUCCESSFULLY actor: origin (128,96) pixels, palette two, instruction list $8B:EBD7.</summary>
        public static readonly EndingSpriteDefinition CompletedSuccessfullyText =
            new(0x0080, 0x0060, TextPalette, 0xebd7);
        /// <summary>$8B:EEFD / F03E, CinematicSpriteObjectDefinitions_ClearTime: label actor at (128,160) pixels, palette one, instruction list $8B:EC35.</summary>
        public static readonly EndingSpriteDefinition ClearTimeText =
            new(0x0080, 0x00a0, TimeLabelPalette, 0xec35);
        /// <summary>$8B:EF0F / F07C clear-time colon actor at (172,160) pixels with no palette override and instruction list $8B:ECD1; separate from the numeric digit lists.</summary>
        public static readonly EndingSpriteDefinition ClearTimeColon =
            new(0x00ac, 0x00a0, new SnesObjAttributeWord(0), 0xecd1);
        /// <summary>Shared clear-time digit origin Y=160 screen pixels, aligned with the label and colon.</summary>
        public const ushort ClearTimeDigitY = 0x00a0;
        /// <summary>$8B:EC81, first clear-time digit instruction list, representing zero; digit selection adds the eight-byte list stride.</summary>
        public const ushort ClearTimeDigitInstructionBase = 0xec81;
        /// <summary>Byte stride between the ten bank-$8B clear-time digit instruction lists.</summary>
        public const int ClearTimeDigitInstructionStride = 8;
        /// <summary>$8B:EF27 suitless idle-body actor: origin (120,136) pixels, palette five, instruction list $8B:ED1D.</summary>
        public static readonly EndingSpriteDefinition SuitlessRewardBody =
            new(0x0078, 0x0088, ScenePalette, 0xed1d);
        /// <summary>$8B:EF2D, CinematicSpriteObjectDefinitions_SuitlessSamus_Idle_Legs: lower suitless composition at the shared origin (120,136) pixels, palette five, list $8B:ED25; the legacy Head name does not match its native legs role.</summary>
        public static readonly EndingSpriteDefinition SuitlessRewardHead =
            new(0x0078, 0x0088, ScenePalette, 0xed25);
        /// <summary>$8B:EF45 / F156 suited idle-body actor: origin (120,152) pixels, palette six, instruction list $8B:EDB1.</summary>
        public static readonly EndingSpriteDefinition ArmoredRewardBody =
            new(0x0078, 0x0098, AlternatePalette, 0xedb1);
        /// <summary>$8B:EF51 / F17C helmetless suited-head actor: origin (121,107) pixels, palette five, instruction list $8B:EDC1.</summary>
        public static readonly EndingSpriteDefinition HelmetlessRewardHead =
            new(0x0079, 0x006b, ScenePalette, 0xedc1);
        /// <summary>$8B:EF4B / F169 helmeted suited-head actor: origin (124,108) pixels, palette six, instruction list $8B:EDB9.</summary>
        public static readonly EndingSpriteDefinition ArmoredRewardHead =
            new(0x007c, 0x006c, AlternatePalette, 0xedb9);
    }
}

/// <summary>Immutable bank-$8B ending-sprite constructor data.</summary>
/// <param name="X">Horizontal origin in pixels as a native sixteen-bit word; negative offscreen positions use two's-complement wrapping.</param>
/// <param name="Y">Vertical origin in pixels as a native sixteen-bit word; negative offscreen positions use two's-complement wrapping.</param>
/// <param name="Attributes">OBJ attribute word providing the palette bits passed to the cinematic sprite constructor.</param>
/// <param name="InstructionPointer">Bank-relative pointer into bank $8B's cinematic sprite instruction lists, rather than a spritemap address.</param>
public readonly record struct EndingSpriteDefinition(
    ushort X,
    ushort Y,
    SnesObjAttributeWord Attributes,
    ushort InstructionPointer);
