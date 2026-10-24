namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Verified cartridge resources and immutable layout shared by the opening cinematic.
/// </summary>
public static class IntroCinematicRomData
{
    /// <summary>ROM banks that hold opening-cinematic data.</summary>
    public static class Banks
    {
        /// <summary>Bank containing the cinematic spritemap definitions.</summary>
        public const byte Spritemaps = 0x8c;
    }

    /// <summary>Compressed and fixed-bank resources loaded by the main intro owner.</summary>
    public static class Assets
    {
        /// <summary>SNES address of the compressed cinematic palette.</summary>
        public const int Palette = 0x8ce3e9;
        /// <summary>SNES address of the compressed background character graphics.</summary>
        public const int BackgroundCharacters = 0x95f90e;
        /// <summary>SNES address of the first cinematic font.</summary>
        public const int FontOne = 0x95d089;
        /// <summary>SNES address of the Samus portrait tilemap.</summary>
        public const int SamusHeadTilemap = 0x9788cc;
        /// <summary>SNES address of the background-page tilemaps.</summary>
        public const int BackgroundPageTilemaps = 0x96ff14;
        /// <summary>SNES address of the cinematic object character graphics.</summary>
        public const int ObjectCharacters = 0x95e4c2;
        /// <summary>SNES address of the first narration tilemap.</summary>
        public const int FirstNarrationTilemap = 0x978d12;
        /// <summary>SNES address of the intro-specific object character graphics.</summary>
        public const int IntroObjectCharacters = 0x9ad200;
        /// <summary>SNES address of the final cinematic text line.</summary>
        public const int FinalTextLine = 0x8ba72b;
    }

    /// <summary>Expected decompressed sizes and native VRAM byte destinations.</summary>
    public static class Transfers
    {
        /// <summary>Decompressed byte count of the background character graphics.</summary>
        public const int BackgroundCharacterBytes = 0x8000;
        /// <summary>Byte count of the first font's character graphics.</summary>
        public const int FontOneBytes = 0x0900;
        /// <summary>Byte count of the Samus portrait tilemap.</summary>
        public const int SamusHeadTilemapBytes = 0x0800;
        /// <summary>Decompressed byte count of the background-page tilemaps.</summary>
        public const int BackgroundPageTilemapBytes = 0x2000;
        /// <summary>Byte count of the cinematic object character graphics.</summary>
        public const int ObjectCharacterBytes = 0x2400;
        /// <summary>Byte count of one narration tilemap.</summary>
        public const int NarrationTilemapBytes = 0x0800;
        /// <summary>Number of Japanese blank-character bytes cleared in VRAM.</summary>
        public const int JapaneseBlankCharactersByteCount = 0x0600;
    }

    /// <summary>BG tilemap and character bases selected by each intro layer.</summary>
    public static class Layers
    {
        /// <summary>Word count of a complete 32-by-32 text tilemap.</summary>
        public const int TextTilemapWordCount = 0x0400;
        /// <summary>The cells of a text tilemap the narration shows: its first thirty rows.</summary>
        public const int VisibleTextCellCount = 0x03c0;
        /// <summary>Width of an intro tilemap in tiles.</summary>
        public const int TilemapWidth = 32;
    }

    /// <summary>Blank text words, language glyph sources, and final-line placement.</summary>
    public static class Text
    {
        /// <summary>Blank tile written into the gameplay text region.</summary>
        public static readonly SnesBgTilemapWord Blank = new(0x002f);
        /// <summary>Blank tile used by the Japanese narration layout.</summary>
        public static readonly SnesBgTilemapWord JapaneseBlank = new(0x3c29);
        /// <summary>Blank tile used around the final text line.</summary>
        public static readonly SnesBgTilemapWord FinalBlank = new(0x1c29);
        /// <summary>First word cleared in the gameplay text tilemap.</summary>
        public const int GameplayBlankStartIndex = 128;
        /// <summary>Number of gameplay text words cleared.</summary>
        public const int GameplayBlankWordCount = 640;
        /// <summary>Byte offset of the Japanese blank glyph in the font data.</summary>
        public const int JapaneseBlankSourceOffset = 0x0290;
        /// <summary>Tilemap-index delta from the upper to lower Japanese blank region.</summary>
        public const int JapaneseBlankBottomDelta = 896;
        /// <summary>First tilemap cell replaced by the final text line.</summary>
        public const int FinalLineFirstCell = 768;
        /// <summary>Number of words copied for the final text line.</summary>
        public const int FinalLineWordCount = 128;
        /// <summary>Tilemap index blanked immediately left of the final text.</summary>
        public const int FinalBlankLeftIndex = 911;
        /// <summary>Tilemap index blanked immediately right of the final text.</summary>
        public const int FinalBlankRightIndex = 912;
    }

    /// <summary>Palette slices used by the repeating flashback/narration crossfades.</summary>
    public static class Palette
    {
        /// <summary>Initial fixed-point counter used by cinematic palette crossfades.</summary>
        public const ushort CrossfadeInitialCounter = 0x007f;
        /// <summary>Mask that advances a palette step once every four updates.</summary>
        public const ushort StepEveryFourFramesMask = 0x0003;
        /// <summary>Palette spans faded for the gameplay flashback.</summary>
        public static IReadOnlyList<IntroPaletteSpan> Gameplay { get; } = [GameplayBackground, GameplayEnvironment, GameplaySamus];
        /// <summary>Palette spans cleared when gameplay flashback initialization begins.</summary>
        public static IReadOnlyList<IntroPaletteSpan> GameplayClear { get; } = [GameplayClearBackground, GameplayEnvironment, GameplaySamus];
        /// <summary>Palette spans faded for narration screens.</summary>
        public static IReadOnlyList<IntroPaletteSpan> Narration { get; } = [NarrationText, NarrationBackground, NarrationPortrait, NarrationObjects];
        /// <summary>Palette spans faded for the Metroid discovery scene.</summary>
        public static IReadOnlyList<IntroPaletteSpan> Discovery { get; } = [DiscoveryBackground, DiscoveryObjects];

        /// <summary>$8B:B258/B25B, CrossFadeToSamusGameplay: first background row and four colors of the next.</summary>
        private static IntroPaletteSpan GameplayBackground => new(0, 16 + 4);
        /// <summary>$8B:B3C8/B3CB: gameplay initialization clears only the first background row.</summary>
        private static IntroPaletteSpan GameplayClearBackground => new(0, 16);
        /// <summary>$8B:B261/B264: gameplay background row 3.</summary>
        private static IntroPaletteSpan GameplayEnvironment => new(3 * 16, 16);
        /// <summary>$8B:B26A/B26D: six colors at object row 6, color 9.</summary>
        private static IntroPaletteSpan GameplaySamus => new((8 + 6) * 16 + 9, 6);
        /// <summary>$8B:B273/B276: three narration colors at background row 1, color 4.</summary>
        private static IntroPaletteSpan NarrationText => new(16 + 4, 3);
        /// <summary>$8B:B27C/B27F: narration background row 7.</summary>
        private static IntroPaletteSpan NarrationBackground => new(7 * 16, 16);
        /// <summary>$8B:B285/B288: two narration object rows starting at row 4.</summary>
        private static IntroPaletteSpan NarrationPortrait => new((8 + 4) * 16, 2 * 16);
        /// <summary>$8B:B28E/B291: narration object row 7.</summary>
        private static IntroPaletteSpan NarrationObjects => new((8 + 7) * 16, 16);
        /// <summary>$8B:B2F5/B2F8, CrossFadeToScientistCutscene: background row 2.</summary>
        private static IntroPaletteSpan DiscoveryBackground => new(2 * 16, 16);
        /// <summary>$8B:B2FE/B301: nine scientist cutscene colors in object row 6.</summary>
        private static IntroPaletteSpan DiscoveryObjects => new((8 + 6) * 16, 9);
    }

    /// <summary>Music operations performed by the intro's major scene handoffs.</summary>
    public static class Music
    {
        /// <summary>Music-data index loaded for the opening narration.</summary>
        public const byte OpeningDataIndex = 0x3f;
        /// <summary>Music-data index loaded for the Mother Brain flashback.</summary>
        public const byte MotherBrainDataIndex = 0x42;
        /// <summary>Music-data index loaded for the Metroid discovery scene.</summary>
        public const byte DiscoveryDataIndex = 0x36;
        /// <summary>Track selected within each loaded cinematic music data set.</summary>
        public const byte SceneTrack = 5;
        /// <summary>Native delay argument used when the cinematic scene track starts.</summary>
        public const ushort SceneTrackDelayArgument = 0x000e;
        /// <summary>Initial countdown before discovery-scene music begins.</summary>
        public const int DiscoveryInitialTimer = 0x000e;
    }

    /// <summary>Screen-fade seeds stored to both $0723 and $0725 by the intro's handoffs.</summary>
    public static class Fade
    {
        /// <summary>
        /// $8B:A5B3, $8B:A659 and $8B:A837: the first narration's fade in and out and page
        /// one's fade in, advanced by AdvanceSlowScreenFadeIn/Out one level every two calls.
        /// </summary>
        public const ushort NarrationSlowFade = 0x0002;
        /// <summary>
        /// $8B:B246 Instruction_FinishIntro: the final fade, advanced by HandleFadingOut one
        /// level every second dispatch.
        /// </summary>
        public const ushort FinishFade = 0x0001;
    }

    /// <summary>Common object palette and sound definitions used by intro actors.</summary>
    public static class Objects
    {
        /// <summary>Object palette used by discovery-scene actors.</summary>
        public static readonly SnesObjAttributeWord DiscoveryPalette = SnesObjPalettes.Palette(7);
        /// <summary>Object palette used by scientist actors.</summary>
        public static readonly SnesObjAttributeWord ScientistPalette = SnesObjPalettes.Palette(6);
        /// <summary>Object palette used by cinematic explosions.</summary>
        public static readonly SnesObjAttributeWord ExplosionPalette = SnesObjPalettes.Palette(5);
        /// <summary>Maximum sound effects the intro queues for one library.</summary>
        public const byte MaximumQueuedSounds = 6;
        /// <summary>First baby Metroid cry sound.</summary>
        public static readonly SoundEffectId BabyCry1 =
            Library3Sound.BabyMetroidCry1;
        /// <summary>Typewriter sound used while narration characters appear.</summary>
        public static readonly SoundEffectId Typewriter =
            Library3Sound.IntroTypewriter;
        /// <summary>Sound played when the baby Metroid hatches.</summary>
        public static readonly SoundEffectId EggHatch =
            Library2Sound.ContactKill;
    }

    /// <summary>Record layout consumed by intro text/object streams.</summary>
    public static class ObjectSystem
    {
        /// <summary>Initial vertical caret position before narration begins.</summary>
        public const ushort CaretInitialY = 0x00f8;
        /// <summary>Leftmost horizontal caret position in pixels.</summary>
        public const ushort CaretLeftX = 8;
        /// <summary>Vertical caret position of the first narration line.</summary>
        public const ushort CaretFirstTextY = 24;
        /// <summary>Width and height of one text character in pixels.</summary>
        public const int CharacterPixelSize = 8;
        /// <summary>Pixel offset from a character origin to its baseline.</summary>
        public const int CharacterBaselineOffset = 8;
    }
}
