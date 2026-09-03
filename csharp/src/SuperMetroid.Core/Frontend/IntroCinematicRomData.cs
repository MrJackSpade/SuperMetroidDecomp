using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Verified cartridge resources and immutable layout shared by the opening cinematic.
/// </summary>
public static class IntroCinematicRomData
{
    public static class Banks
    {
        public const int CinematicCode = 0x8b0000;
        public const byte Spritemaps = 0x8c;
    }

    /// <summary>Compressed and fixed-bank resources loaded by the main intro owner.</summary>
    public static class Assets
    {
        public const int Palette = 0x8ce3e9;
        public const int BackgroundCharacters = 0x95f90e;
        public const int FontOne = 0x95d089;
        public const int SamusHeadTilemap = 0x9788cc;
        public const int BackgroundPageTilemaps = 0x96ff14;
        public const int ObjectCharacters = 0x95e4c2;
        public const int FirstNarrationTilemap = 0x978d12;
        public const int JapaneseFontTwo = 0x95d713;
        public const int MotherBrainLevelData = 0x8cbec3;
        public const int IntroObjectCharacters = 0x9ad200;
        public const int FinalTextLine = 0x8ba72b;
    }

    /// <summary>Expected decompressed sizes and native VRAM byte destinations.</summary>
    public static class Vram
    {
        public const int BackgroundCharacterBytes = 0x8000;
        public const int FontOneBytes = 0x0900;
        public const int SamusHeadTilemapBytes = 0x0800;
        public const int BackgroundPageTilemapBytes = 0x2000;
        public const int ObjectCharacterBytes = 0x2400;
        public const int NarrationTilemapBytes = 0x0800;
        public const int JapaneseFontTwoBytes = 0x1200;
        public const int BackgroundCharacterDestinationByte = 0x0000;
        public const int FontOneDestinationByte = 0x8000;
        public const int SamusHeadTilemapDestinationByte = 0x9000;
        public const int NarrationTilemapDestinationByte = 0x9800;
        public const int BackgroundPagesDestinationByte = 0xa000;
        public const int IntroObjectCharactersDestinationByte = 0xc000;
        public const int CinematicObjectCharactersDestinationByte = 0xdc00;
        public const int JapaneseBlankCharactersDestinationByte = 0x8300;
        public const int JapaneseBlankCharactersByteCount = 0x0600;
    }

    /// <summary>BG tilemap and character bases selected by each intro layer.</summary>
    public static class Layers
    {
        public const ushort PortraitTilemapWord = 0x4800;
        public const ushort NarrationTilemapWord = 0x4c00;
        public const ushort SceneBg1TilemapWord = 0x5000;
        public const ushort SceneBg2TilemapWord = 0x5400;
        public const ushort ScientistTilemapWord = 0x5800;
        public const ushort ScientistAlternateTilemapWord = 0x5c00;
        public const ushort FontCharacterBaseWord = 0x4000;
        public const int NarrationRowCount = 28;
        public const int TextTilemapWordCount = 0x0400;
        public const int VisibleTextTransferWordCount = 0x03c0;
        public const int TilemapWidth = 32;
    }

    /// <summary>Blank text words, language glyph sources, and final-line placement.</summary>
    public static class Text
    {
        public static readonly SnesBgTilemapWord Blank = new(0x002f);
        public static readonly SnesBgTilemapWord JapaneseBlank = new(0x3c29);
        public static readonly SnesBgTilemapWord FinalBlank = new(0x1c29);
        public const int GameplayBlankStartIndex = 128;
        public const int GameplayBlankWordCount = 640;
        public const int JapaneseBlankSourceOffset = 0x0290;
        public const int JapaneseBlankCharacterByteCount = 0x10;
        public const int JapaneseBlankTopStart = 0;
        public const int JapaneseBlankBottomDelta = 896;
        public const int FinalLineDestinationStart = 768;
        public const int FinalLineWordCount = 128;
        public const int FinalBlankLeftIndex = 911;
        public const int FinalBlankRightIndex = 912;
    }

    /// <summary>Palette slices used by the repeating flashback/narration crossfades.</summary>
    public static class Palette
    {
        private static readonly IntroPaletteSpan[] GameplayRegions =
        [
            new(0x0000, 0x0014),
            new(0x0060, 0x0010),
            new(0x01d2, 0x0006),
        ];
        private static readonly IntroPaletteSpan[] GameplayClearRegions =
        [
            new(0x0000, 0x0010),
            new(0x0060, 0x0010),
            new(0x01d2, 0x0006),
        ];
        private static readonly IntroPaletteSpan[] NarrationRegions =
        [
            new(0x0028, 0x0003),
            new(0x00e0, 0x0010),
            new(0x0180, 0x0020),
            new(0x01e0, 0x0010),
        ];
        private static readonly IntroPaletteSpan[] DiscoveryRegions =
        [
            new(0x0040, 0x0010),
            new(0x01c0, 0x0009),
        ];

        public const ushort CrossfadeInitialCounter = 0x007f;
        public const ushort CounterSignBit = 0x8000;
        public const ushort StepEveryFourFramesMask = 0x0003;
        public static ReadOnlySpan<IntroPaletteSpan> Gameplay => GameplayRegions;
        public static ReadOnlySpan<IntroPaletteSpan> GameplayClear => GameplayClearRegions;
        public static ReadOnlySpan<IntroPaletteSpan> Narration => NarrationRegions;
        public static ReadOnlySpan<IntroPaletteSpan> Discovery => DiscoveryRegions;
    }

    /// <summary>Demo records and fixed Mother Brain room payload.</summary>
    public static class Flashback
    {
        public const int MotherBrainLevelByteCount = 448;
        public const ushort DemoInputObject = 0x8784;
        public const ushort ExpectedEndInstruction = 0x8739;
    }

    /// <summary>Music operations performed by the intro's major scene handoffs.</summary>
    public static class Music
    {
        public const byte OpeningDataIndex = 0x3f;
        public const byte MotherBrainDataIndex = 0x42;
        public const byte DiscoveryDataIndex = 0x36;
        public const byte SceneTrack = 5;
        public const ushort SceneTrackDelayArgument = 0x000e;
        public const int DiscoveryInitialTimer = 0x000e;
    }

    /// <summary>Common object palette and sound definitions used by intro actors.</summary>
    public static class Objects
    {
        public static readonly SnesObjAttributeWord DiscoveryPalette = SnesObjPalettes.Index7;
        public static readonly SnesObjAttributeWord ScientistPalette = SnesObjPalettes.Index6;
        public static readonly SnesObjAttributeWord ExplosionPalette = SnesObjPalettes.Index5;
        public const byte MaximumQueuedSounds = 6;
        public static readonly SoundEffectId BabyCry1 =
            new(SoundEffectLibrary.Library3, 0x23);
        public static readonly SoundEffectId BabyCry2 =
            new(SoundEffectLibrary.Library3, 0x26);
        public static readonly SoundEffectId BabyCry3 =
            new(SoundEffectLibrary.Library3, 0x27);
        public static readonly SoundEffectId Typewriter =
            new(SoundEffectLibrary.Library3, 0x0d);
        public static readonly SoundEffectId EggHatch =
            new(SoundEffectLibrary.Library2, 0x0b);
    }

    /// <summary>Record layout consumed by intro text/object streams.</summary>
    public static class ObjectSystem
    {
        public const ushort CaretInitialY = 0x00f8;
        public const ushort CaretLeftX = 8;
        public const ushort CaretFirstTextY = 24;
        public const int PackedPositionXMask = 0x00ff;
        public const int CharacterPixelSize = 8;
        public const int CharacterBaselineOffset = 8;
        public const int RecordDurationToPositionByteCount = 2;
        public const int RecordDurationToDataPointerByteCount = 4;
        public const int BackgroundRecordByteCount = 6;
        public const int NextRecordPositionXByteOffset = 8;
        public const int NextRecordPositionYByteOffset = 9;
    }
}

/// <summary>One byte-indexed CGRAM span used by the intro palette fader.</summary>
public readonly record struct IntroPaletteSpan(ushort ByteOffset, ushort ByteCount);
