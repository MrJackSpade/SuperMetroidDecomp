using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

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
    public static class Vram
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
        /// <summary>VRAM byte destination for background character graphics.</summary>
        public const int BackgroundCharacterDestinationByte = 0x0000;
        /// <summary>VRAM byte destination for the first font.</summary>
        public const int FontOneDestinationByte = 0x8000;
        /// <summary>VRAM byte destination for the Samus portrait tilemap.</summary>
        public const int SamusHeadTilemapDestinationByte = 0x9000;
        /// <summary>VRAM byte destination for the active narration tilemap.</summary>
        public const int NarrationTilemapDestinationByte = 0x9800;
        /// <summary>VRAM byte destination for the background-page tilemaps.</summary>
        public const int BackgroundPagesDestinationByte = 0xa000;
        /// <summary>VRAM byte destination for intro-specific object characters.</summary>
        public const int IntroObjectCharactersDestinationByte = 0xc000;
        /// <summary>VRAM byte destination for cinematic object characters.</summary>
        public const int CinematicObjectCharactersDestinationByte = 0xdc00;
        /// <summary>VRAM byte destination cleared for Japanese blank characters.</summary>
        public const int JapaneseBlankCharactersDestinationByte = 0x8300;
        /// <summary>Number of Japanese blank-character bytes cleared in VRAM.</summary>
        public const int JapaneseBlankCharactersByteCount = 0x0600;
    }

    /// <summary>BG tilemap and character bases selected by each intro layer.</summary>
    public static class Layers
    {
        /// <summary>BG screen-base word used for the portrait tilemap.</summary>
        public const ushort PortraitTilemapWord = 0x4800;
        /// <summary>BG screen-base word used for narration text.</summary>
        public const ushort NarrationTilemapWord = 0x4c00;
        /// <summary>BG1 screen-base word used by cinematic scenes.</summary>
        public const ushort SceneBg1TilemapWord = 0x5000;
        /// <summary>BG2 screen-base word used by cinematic scenes.</summary>
        public const ushort SceneBg2TilemapWord = 0x5400;
        /// <summary>BG screen-base word used by the scientist scene.</summary>
        public const ushort ScientistTilemapWord = 0x5800;
        /// <summary>BG character-base word used by the cinematic font.</summary>
        public const ushort FontCharacterBaseWord = 0x4000;
        /// <summary>Number of narration rows populated by the intro.</summary>
        public const int NarrationRowCount = 28;
        /// <summary>Word count of a complete 32-by-32 text tilemap.</summary>
        public const int TextTilemapWordCount = 0x0400;
        /// <summary>Word count copied for the visible portion of a text tilemap.</summary>
        public const int VisibleTextTransferWordCount = 0x03c0;
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
        /// <summary>Byte count copied for one Japanese blank glyph.</summary>
        public const int JapaneseBlankCharacterByteCount = 0x10;
        /// <summary>Tilemap-index delta from the upper to lower Japanese blank region.</summary>
        public const int JapaneseBlankBottomDelta = 896;
        /// <summary>First tilemap word replaced by the final text line.</summary>
        public const int FinalLineDestinationStart = 768;
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
        /// <summary>Bit that identifies a negative signed crossfade counter.</summary>
        public const ushort CounterSignBit = 0x8000;
        /// <summary>Mask that advances a palette step once every four updates.</summary>
        public const ushort StepEveryFourFramesMask = 0x0003;
        /// <summary>Palette spans faded for the gameplay flashback.</summary>
        public static Regions Gameplay { get; } = new(Scene.Gameplay);
        /// <summary>Palette spans cleared when gameplay flashback initialization begins.</summary>
        public static Regions GameplayClear { get; } = new(Scene.GameplayClear);
        /// <summary>Palette spans faded for narration screens.</summary>
        public static Regions Narration { get; } = new(Scene.Narration);
        /// <summary>Palette spans faded for the Metroid discovery scene.</summary>
        public static Regions Discovery { get; } = new(Scene.Discovery);

        /// <summary>Mutually exclusive intro palette-region selections.</summary>
        public enum Scene
        {
            /// <summary>The Mother Brain gameplay-flashback palette.</summary>
            Gameplay,
            /// <summary>The subset cleared before the gameplay flashback.</summary>
            GameplayClear,
            /// <summary>The narration-screen palette.</summary>
            Narration,
            /// <summary>The Metroid discovery-scene palette.</summary>
            Discovery,
        }

        /// <summary>Ordered scene operations, selected without a stored region table.</summary>
        public sealed class Regions : System.Collections.Generic.IReadOnlyList<IntroPaletteSpan>
        {
            /// <summary>Scene whose ordered palette ranges this view exposes.</summary>
            private readonly Scene scene;

            /// <summary>Creates a palette-range view for one intro scene.</summary>
            /// <param name="scene">Scene selection that determines the exposed spans.</param>
            internal Regions(Scene scene) => this.scene = scene;
            /// <summary>Gets the number of palette spans in the selected scene.</summary>
            public int Count => scene switch { Scene.Narration => 4, Scene.Discovery => 2, _ => 3 };
            /// <summary>Gets the palette span at the specified scene-relative index.</summary>
            public IntroPaletteSpan this[int index] => scene switch
            {
                Scene.Gameplay or Scene.GameplayClear => index switch
                {
                    0 => scene == Scene.Gameplay ? GameplayBackground : GameplayClearBackground,
                    1 => GameplayEnvironment,
                    2 => GameplaySamus,
                    _ => throw new IndexOutOfRangeException(),
                },
                Scene.Narration => index switch
                {
                    0 => NarrationText,
                    1 => NarrationBackground,
                    2 => NarrationPortrait,
                    3 => NarrationObjects,
                    _ => throw new IndexOutOfRangeException(),
                },
                Scene.Discovery => index switch
                {
                    0 => DiscoveryBackground,
                    1 => DiscoveryObjects,
                    _ => throw new IndexOutOfRangeException(),
                },
                _ => throw new InvalidOperationException(),
            };
            /// <summary>Returns an enumerator over the selected scene's palette spans.</summary>
            public System.Collections.Generic.IEnumerator<IntroPaletteSpan> GetEnumerator()
            {
                for (int index = 0; index < Count; index++) yield return this[index];
            }
            /// <summary>Returns a non-generic enumerator over this sequence.</summary>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>$8B:B258/B25B, CrossFadeToSamusGameplay: first background row and four colors of the next.</summary>
        private static IntroPaletteSpan GameplayBackground => new(0, 16 + 4);
        /// <summary>$8B:B3C8/B3CB: gameplay initialization clears only the first background row.</summary>
        private static IntroPaletteSpan GameplayClearBackground => new(0, 16);
        /// <summary>$8B:B261/B264: gameplay background row 3.</summary>
        private static IntroPaletteSpan GameplayEnvironment => new(3 * 32, 16);
        /// <summary>$8B:B26A/B26D: six colors at object row 6, color 9.</summary>
        private static IntroPaletteSpan GameplaySamus => new((8 + 6) * 32 + 9 * 2, 6);
        /// <summary>$8B:B273/B276: three narration colors at background row 1, color 4.</summary>
        private static IntroPaletteSpan NarrationText => new(32 + 4 * 2, 3);
        /// <summary>$8B:B27C/B27F: narration background row 7.</summary>
        private static IntroPaletteSpan NarrationBackground => new(7 * 32, 16);
        /// <summary>$8B:B285/B288: two narration object rows starting at row 4.</summary>
        private static IntroPaletteSpan NarrationPortrait => new((8 + 4) * 32, 2 * 16);
        /// <summary>$8B:B28E/B291: narration object row 7.</summary>
        private static IntroPaletteSpan NarrationObjects => new((8 + 7) * 32, 16);
        /// <summary>$8B:B2F5/B2F8, CrossFadeToScientistCutscene: background row 2.</summary>
        private static IntroPaletteSpan DiscoveryBackground => new(2 * 32, 16);
        /// <summary>$8B:B2FE/B301: nine scientist cutscene colors in object row 6.</summary>
        private static IntroPaletteSpan DiscoveryObjects => new((8 + 6) * 32, 9);
    }

    /// <summary>Demo records and fixed Mother Brain room payload.</summary>
    public static class Flashback
    {
        /// <summary>Native demo instruction expected when the Mother Brain flashback ends.</summary>
        public const ushort ExpectedEndInstruction = 0x8739;
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
        public static readonly SnesObjAttributeWord DiscoveryPalette = SnesObjPalettes.Index7;
        /// <summary>Object palette used by scientist actors.</summary>
        public static readonly SnesObjAttributeWord ScientistPalette = SnesObjPalettes.Index6;
        /// <summary>Object palette used by cinematic explosions.</summary>
        public static readonly SnesObjAttributeWord ExplosionPalette = SnesObjPalettes.Index5;
        /// <summary>Maximum sound effects the intro queues for one library.</summary>
        public const byte MaximumQueuedSounds = 6;
        /// <summary>First baby Metroid cry sound.</summary>
        public static readonly SoundEffectId BabyCry1 =
            new(SoundEffectLibrary.Library3, 0x23);
        /// <summary>Second baby Metroid cry sound.</summary>
        public static readonly SoundEffectId BabyCry2 =
            new(SoundEffectLibrary.Library3, 0x26);
        /// <summary>Third baby Metroid cry sound.</summary>
        public static readonly SoundEffectId BabyCry3 =
            new(SoundEffectLibrary.Library3, 0x27);
        /// <summary>Typewriter sound used while narration characters appear.</summary>
        public static readonly SoundEffectId Typewriter =
            new(SoundEffectLibrary.Library3, 0x0d);
        /// <summary>Sound played when the baby Metroid hatches.</summary>
        public static readonly SoundEffectId EggHatch =
            new(SoundEffectLibrary.Library2, 0x0b);
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
        /// <summary>Mask selecting the X coordinate from a packed position word.</summary>
        public const int PackedPositionXMask = 0x00ff;
        /// <summary>Width and height of one text character in pixels.</summary>
        public const int CharacterPixelSize = 8;
        /// <summary>Pixel offset from a character origin to its baseline.</summary>
        public const int CharacterBaselineOffset = 8;
        /// <summary>Bytes between a record's duration and packed position.</summary>
        public const int RecordDurationToPositionByteCount = 2;
        /// <summary>Bytes between a record's duration and data pointer.</summary>
        public const int RecordDurationToDataPointerByteCount = 4;
        /// <summary>Size in bytes of one background-object record.</summary>
        public const int BackgroundRecordByteCount = 6;
    }
}

/// <summary>One byte-indexed CGRAM span used by the intro palette fader.</summary>
/// <param name="ByteOffset">First byte of the palette range in the CGRAM image.</param>
/// <param name="ByteCount">Number of consecutive CGRAM bytes covered by the range.</param>
public readonly record struct IntroPaletteSpan(ushort ByteOffset, ushort ByteCount);
