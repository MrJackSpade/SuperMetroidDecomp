using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verified cartridge assets and immutable layout for the title sequence.</summary>
public static class TitleSequenceRomData
{
    /// <summary>Compressed graphics and the complete title CGRAM image.</summary>
    public static class Assets
    {
        public const int PaletteAddress = 0x8ce1e9;
        public const int Mode7CharactersAddress = 0x94e000;
        public const int Mode7MapAddress = 0x96fc04;
        public const int ObjectCharactersAddress = 0x9580d8;
        public const int BabyMetroidCharactersAddress = 0x95a5e1;
    }

    /// <summary>Music data set and tracks selected by the normal and skip paths.</summary>
    public static class Music
    {
        public const byte DataIndex = 0x03;
        public const byte OpeningTrack = 5;
        public const byte ImmediateTitleTrack = 6;
    }

    /// <summary>Bank-$8B text-object lists, origins, and character offsets.</summary>
    public static class TextSequences
    {
        public static readonly TitleTextSequenceDefinition Year =
            new(TitleSequencePhase.YearText, 0x8ba03d, 129, 112, 0x0400);
        public static readonly TitleTextSequenceDefinition Nintendo =
            new(TitleSequencePhase.NintendoText, 0x8ba055, 129, 112, 0x0200);
        public static readonly TitleTextSequenceDefinition Presents =
            new(TitleSequencePhase.PresentsText, 0x8ba079, 129, 112, 0x0200);
        public static readonly TitleTextSequenceDefinition MetroidThree =
            new(TitleSequencePhase.MetroidThreeText, 0x8ba09d, 129, 112, 0x0200);
        public const int TimedEntryByteCount = 4;
        public const int InstructionWordByteCount = 2;
        public const ushort CommandBit = 0x8000;
    }

    /// <summary>Mode-7 scene starting transforms, motion, and completion thresholds.</summary>
    public static class Scenes
    {
        public static readonly TitleMode7SceneDefinition SceneZero =
            new(TitleSequencePhase.SceneZeroPan, 0x0048, 0x013b, 0x00e1);
        public static readonly TitleMode7SceneDefinition SceneOne =
            new(TitleSequencePhase.SceneOnePan, 0x0060, 0x002c, unchecked((short)0xff65));
        public static readonly TitleMode7SceneDefinition SceneTwo =
            new(TitleSequencePhase.SceneTwoPan, 0x0060,
                unchecked((short)0xff4f), unchecked((short)0xff60));
        public static readonly TitleMode7SceneDefinition SceneThree =
            new(TitleSequencePhase.SceneThreeZoom, 0x0043, 0, 0);
        public const int PanVelocity16Point16 = 0x0001_8000;
        public const int SceneZeroEndX = -7;
        public const int SceneOneEndX = -176;
        public const int SceneTwoEndY = 163;
        public const int IdentityScale = 0x0100;
        public static SnesAngle Rotation => SnesAngle.Zero;
    }

    /// <summary>Static title/copyright spritemaps and OBJ placement.</summary>
    public static class Sprites
    {
        public const byte Bank = 0x8c;
        public const ushort Blank = 0x0000;
        public const ushort SuperMetroidLogo = 0x879d;
        public const ushort NintendoCopyright = 0x8103;
        public const int LogoPointerAddress = 0x8ba0c7;
        public const ushort LogoX = 128;
        public const ushort LogoY = 48;
        public const ushort CopyrightX = 128;
        public const ushort CopyrightY = 196;
        public static readonly SnesObjAttributeWord TitleCharacterOffset = SnesObjPalettes.Index2;
        public static readonly SnesObjAttributeWord CopyrightPalette = SnesObjPalettes.Index4;
        public const byte ObjectSizeAndBaseSelector = 0x03;
    }

    /// <summary>VRAM ranges and page order used by title graphics DMA.</summary>
    public static class Vram
    {
        private static readonly byte[] BabySourcePages = [0, 1, 2, 1];

        public const int ObjectCharacterDestinationByte = 0xc000;
        public const int ObjectCharacterByteCount = 0x4000;
        public const int Mode7CharacterByteCount = 0x4000;
        public const int Mode7MapByteCount = 0x1000;
        public const byte Mode7InitialMapByte = 0xff;
        public const ushort BabyCharacterDestinationWord = 0x3800;
        public const int BabyCharacterPageByteCount = 0x0100;
        public const ushort Mode7MapLowByteMask = 0x00ff;
        public const int Mode7CharacterByteShift = 8;
        public static ReadOnlySpan<byte> BabyAnimationSourcePages => BabySourcePages;
    }

    /// <summary>Fixed palette replacements made only by the immediate-title skip path.</summary>
    public static class Palette
    {
        public const int CopyrightWhiteIndex = 201;
        public const ushort CopyrightWhite = 0x7fff;
        public const int CopyrightRedIndex = 202;
        public const ushort CopyrightRed = 0x7d80;
    }

    /// <summary>Frame counts, brightness rates, and accepted skip buttons.</summary>
    public static class Timing
    {
        public const int MaximumBrightness = 15;
        public const int SkipFadeBrightnessStep = 2;
        public const int LogoHoldFrames = 32;
        public const int CopyrightHoldFrames = 32;
        public const int TitleScreenNtscFrames = 900;
        public const int TitleFadeCadenceFrames = 2;
        public const int BabyFrameDuration = 10;
        public static SnesButton ConfirmButtons =>
            SnesButton.B | SnesButton.Start | SnesButton.A;
    }
}

/// <summary>One bank-$8B title-card object definition.</summary>
public readonly record struct TitleTextSequenceDefinition(
    TitleSequencePhase Phase,
    int InstructionAddress,
    ushort OriginX,
    ushort OriginY,
    ushort CharacterOffset);

/// <summary>Initial Mode-7 transform installed by one title-scene command.</summary>
public readonly record struct TitleMode7SceneDefinition(
    TitleSequencePhase Phase,
    int Scale,
    short HorizontalOffset,
    short VerticalOffset);
