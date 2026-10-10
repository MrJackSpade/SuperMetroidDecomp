using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verified cartridge assets and immutable layout for the title sequence.</summary>
public static class TitleSequenceRomData
{
    /// <summary>Console-light objects spawned by title setup and skip reconstruction.</summary>
    public static class ConsolePaletteFx
    {
        /// <summary>$8D:E1A0, kPalfx_E1A0: ten-frame console colors at CGRAM 42–45.</summary>
        public const ushort SlowLights = 0xe1a0;

        /// <summary>$8D:E1A4, kPalfx_E1A4: alternating console colors at CGRAM 46–47.</summary>
        public const ushort FastLights = 0xe1a4;
    }

    /// <summary>Compressed graphics and the complete title CGRAM image.</summary>
    public static class Assets
    {
        /// <summary>$8C:E1E9, Palettes_TitleScreen: full 256-color title CGRAM image, separate from console-light animations and skip-only color replacements.</summary>
        public const int PaletteAddress = 0x8ce1e9;
        /// <summary>$94:E000, Tiles_Title_Background_Mode7: compressed title-background character lane transferred through the high-byte VRAM port.</summary>
        public const int Mode7CharactersAddress = 0x94e000;
        /// <summary>$96:FC04, Title_Mode7_Tilemap: compressed low-byte Mode 7 tilemap for the title background.</summary>
        public const int Mode7MapAddress = 0x96fc04;
        /// <summary>$95:80D8, Tiles_Title_Sprite: compressed OBJ character sheet for title text, logo, and copyright sprites.</summary>
        public const int ObjectCharactersAddress = 0x9580d8;
        /// <summary>$95:A5E1, Tiles_Baby_Metroid_Mode7: compressed animation character pages inserted into the background's high-byte Mode 7 lane.</summary>
        public const int BabyMetroidCharactersAddress = 0x95a5e1;
    }

    /// <summary>Music data set and tracks selected by the normal and skip paths.</summary>
    public static class Music
    {
        /// <summary>Music data selector $03 loaded at title-sequence setup.</summary>
        public const byte DataIndex = 0x03;
        /// <summary>Track command five queued after title music data for the normal opening sequence.</summary>
        public const byte OpeningTrack = 5;
        /// <summary>$8B:9A94 track command six queued by the skip transition before reconstructing the immediate title screen.</summary>
        public const byte ImmediateTitleTrack = 6;
    }

    /// <summary>Bank-$8B text-object lists, origins, and character offsets.</summary>
    public static class TextSequences
    {
        /// <summary>$8B:9CBC initializes the $8B:A03D Year text at (129,112) with OBJ palette 1 ($0200).</summary>
        public static readonly TitleTextSequenceDefinition Year =
            new(TitleSequencePhase.YearText, 0x8ba03d, 129, 112, SnesObjPalettes.Index1.Raw);
        /// <summary>$8B:A055 Nintendo scrolling-text list, initialized by $8B:9D4A at (129,112) screen pixels with OBJ palette one.</summary>
        public static readonly TitleTextSequenceDefinition Nintendo =
            new(TitleSequencePhase.NintendoText, 0x8ba055, 129, 112, 0x0200);
        /// <summary>$8B:A079 Presents scrolling-text list, initialized by $8B:9DC3 at (129,112) screen pixels with OBJ palette one.</summary>
        public static readonly TitleTextSequenceDefinition Presents =
            new(TitleSequencePhase.PresentsText, 0x8ba079, 129, 112, 0x0200);
        /// <summary>$8B:A09D Metroid 3 scrolling-text list, initialized by $8B:9E45 at (129,112) screen pixels with OBJ palette one.</summary>
        public static readonly TitleTextSequenceDefinition MetroidThree =
            new(TitleSequencePhase.MetroidThreeText, 0x8ba09d, 129, 112, 0x0200);
        /// <summary>Byte length of a title text-list timed entry: one duration word followed by one bank-$8C spritemap-pointer word.</summary>
        public const int TimedEntryByteCount = 4;
        /// <summary>Byte length of a command word in a bank-$8B title text instruction list.</summary>
        public const int InstructionWordByteCount = 2;
        /// <summary>High bit distinguishing a code-pointer command from a timed duration in title text lists.</summary>
        public const ushort CommandBit = 0x8000;
    }

    /// <summary>Mode-7 scene starting transforms, motion, and completion thresholds.</summary>
    public static class Scenes
    {
        /// <summary>$8B:9CE1, Instruction_TriggerTitleSequenceScene0: lower pan setup with scale $0048 and background offsets (315,225) pixels.</summary>
        public static readonly TitleMode7SceneDefinition SceneZero =
            new(TitleSequencePhase.SceneZeroPan, 0x0048, 0x013b, 0x00e1);
        /// <summary>$8B:9D5D upper scene-one pan setup: scale $0060 and signed background offsets (44,-155) pixels.</summary>
        public static readonly TitleMode7SceneDefinition SceneOne =
            new(TitleSequencePhase.SceneOnePan, 0x0060, 0x002c, unchecked((short)0xff65));
        /// <summary>$8B:9DD6 downward scene-two pan setup: scale $0060 and signed background offsets (-177,-160) pixels.</summary>
        public static readonly TitleMode7SceneDefinition SceneTwo =
            new(TitleSequencePhase.SceneTwoPan, 0x0060,
                unchecked((short)0xff4f), unchecked((short)0xff60));
        /// <summary>$8B:9E58 final scene-three zoom setup: scale $0043 with both background offsets zero.</summary>
        public static readonly TitleMode7SceneDefinition SceneThree =
            new(TitleSequencePhase.SceneThreeZoom, 0x0043, 0, 0);
        /// <summary>Pan-speed magnitude in signed 16.16 pixels per title update: 1.5 pixels; scenes zero and one negate it, while scene two pans downward.</summary>
        public const int PanVelocity16Point16 = 0x0001_8000;
        /// <summary>$8B:9D2A lower-pan signed X threshold -7 pixels; crossing below it starts the Nintendo title card.</summary>
        public const int SceneZeroEndX = -7;
        /// <summary>$8B:9DA3 upper-pan signed X threshold -176 pixels; crossing below it starts the Presents title card.</summary>
        public const int SceneOneEndX = -176;
        /// <summary>$8B:9E25 downward-pan Y threshold 163 pixels; reaching it starts the Metroid 3 title card.</summary>
        public const int SceneTwoEndY = 163;
        /// <summary>Unity Mode 7 transform scale $0100 in native 8.8 representation; reaching it completes the final zoom.</summary>
        public const int IdentityScale = 0x0100;
        /// <summary>Zero rotation shared by the title Mode 7 scenes; motion changes background offsets and scale only.</summary>
        public static SnesAngle Rotation => SnesAngle.Zero;
    }

    /// <summary>Static title/copyright spritemaps and OBJ placement.</summary>
    public static class Sprites
    {
        /// <summary>Bank $8C containing title-sequence spritemaps referenced by bank-relative list operands.</summary>
        public const byte Bank = 0x8c;
        /// <summary>Zero spritemap-pointer sentinel that suppresses the active title-card or logo composition.</summary>
        public const ushort Blank = 0x0000;
        /// <summary>$8C:879D, TitleSequenceSpritemaps_SuperMetroidTitleLogo: bank-relative pointer to the static logo composition.</summary>
        public const ushort SuperMetroidLogo = 0x879d;
        /// <summary>$8C:8103, TitleSequenceSpritemaps_NintendoCopyright: bank-relative pointer to the 1994 Nintendo copyright composition.</summary>
        public const ushort NintendoCopyright = 0x8103;
        /// <summary>$8B:A0C7 spritemap operand in the title-logo instruction list; stores the bank-$8C logo pointer rather than the spritemap itself.</summary>
        public const int LogoPointerAddress = 0x8ba0c7;
        /// <summary>Horizontal title-logo drawing origin in screen pixels.</summary>
        public const ushort LogoX = 128;
        /// <summary>Vertical title-logo drawing origin in screen pixels.</summary>
        public const ushort LogoY = 48;
        /// <summary>Horizontal Nintendo copyright drawing origin in screen pixels.</summary>
        public const ushort CopyrightX = 128;
        /// <summary>Vertical Nintendo copyright drawing origin in screen pixels, below the title logo.</summary>
        public const ushort CopyrightY = 196;
        /// <summary>OBJ palette-two attribute bits passed as the logo's legacy character-offset argument; this value is not a character count.</summary>
        public static readonly SnesObjAttributeWord TitleCharacterOffset = SnesObjPalettes.Index2;
        /// <summary>OBJ palette-four attribute bits applied when drawing the Nintendo copyright composition.</summary>
        public static readonly SnesObjAttributeWord CopyrightPalette = SnesObjPalettes.Index4;
        /// <summary>Native OBSEL value $03: title OBJ character base at VRAM byte $C000 with the small/large sprite-size selector zero.</summary>
        public const byte ObjectSizeAndBaseSelector = 0x03;
    }

    /// <summary>VRAM ranges and page order used by title graphics DMA.</summary>
    public static class Vram
    {
        /// <summary>Four-phase triangular source-page cycle in $8B:A131..A140.</summary>
        public readonly struct BabyPageSequence : IReadOnlyList<byte>
        {
            /// <summary>Four animation phases in the immutable source-page cycle 0,1,2,1.</summary>
            public int Count => 4;
            /// <summary>Animation phase count, equivalent to <see cref="Count"/>, for frame wrapping.</summary>
            public int Length => Count;
            /// <summary>Returns the source-page number for a zero-based animation phase in the cycle 0,1,2,1.</summary>
            /// <param name="frame">Animation phase 0-3, rather than an unrestricted elapsed-frame count.</param>
            /// <returns>Source-page index 0-2; multiplying it by the character-page byte count gives the source offset.</returns>
            /// <exception cref="IndexOutOfRangeException">The phase is outside 0-3.</exception>
            public byte this[int frame] => (uint)frame < Length
                ? (byte)Math.Min(frame, Length - frame)
                : throw new IndexOutOfRangeException();
            /// <summary>Enumerates the four source-page indexes in animation order without retaining a page array.</summary>
            /// <returns>An enumerator yielding 0,1,2,1.</returns>
            public IEnumerator<byte> GetEnumerator()
            {
                for (int frame = 0; frame < Count; frame++) yield return this[frame];
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>VRAM byte address $C000, corresponding to word $6000, for the title OBJ character sheet.</summary>
        public const int ObjectCharacterDestinationByte = 0xc000;
        /// <summary>Byte length of the complete title OBJ character upload: $4000 bytes.</summary>
        public const int ObjectCharacterByteCount = 0x4000;
        /// <summary>Decompressed byte extent required for the Baby's Mode 7 character resource, separate from its $0100-byte animation page uploads.</summary>
        public const int BabyCharacterByteCount = 0x0400;
        /// <summary>Byte length of the full title Mode 7 character lane.</summary>
        public const int Mode7CharacterByteCount = 0x4000;
        /// <summary>Byte length of the title Mode 7 map prefix uploaded into the low-byte lane.</summary>
        public const int Mode7MapByteCount = 0x1000;
        /// <summary>Blank map tile $FF filling the complete low-byte lane before the selected map prefix is uploaded.</summary>
        public const byte Mode7InitialMapByte = 0xff;
        /// <summary>VRAM word address $3800 where Baby animation replaces high-byte character data while preserving each low-byte map value.</summary>
        public const ushort BabyCharacterDestinationWord = 0x3800;
        /// <summary>Byte length of one Baby character source page; each byte replaces the high byte of a successive VRAM word.</summary>
        public const int BabyCharacterPageByteCount = 0x0100;
        /// <summary>Mask $00FF retaining a VRAM word's Mode 7 map byte during Baby character updates.</summary>
        public const ushort Mode7MapLowByteMask = 0x00ff;
        /// <summary>Left shift positioning a Baby character byte in the high-byte Mode 7 lane of a VRAM word.</summary>
        public const int Mode7CharacterByteShift = 8;
        /// <summary>Immutable four-phase Baby character-page cycle 0,1,2,1 derived from $8B:A131-A140.</summary>
        public static BabyPageSequence BabyAnimationSourcePages => default;
    }

    /// <summary>Fixed palette replacements made only by the immediate-title skip path.</summary>
    public static class Palette
    {
        /// <summary>CGRAM color-word index 201 replaced with the skip-path white copyright color.</summary>
        public const int CopyrightWhiteIndex = 201;
        /// <summary>Packed SNES BGR555 white $7FFF installed by the immediate-title skip path.</summary>
        public static Bgr555 CopyrightWhite => Bgr555.White;
        /// <summary>CGRAM color-word index 202 replaced with the skip-path red copyright color.</summary>
        public const int CopyrightRedIndex = 202;
        /// <summary>Packed SNES BGR555 word $7D80 for the skip-only copyright color, preserving the native payload despite the legacy Red name.</summary>
        public static Bgr555 CopyrightRed => Bgr555.FromWord(0x7d80);
    }

    /// <summary>Frame counts, brightness rates, and accepted skip buttons.</summary>
    public static class Timing
    {
        /// <summary>Maximum native INIDISP brightness level, used to clamp fades and scale rendered colors.</summary>
        public const int MaximumBrightness = 15;
        /// <summary>Brightness-level change per update for both the fast skip fade-out and immediate-title fade-in.</summary>
        public const int SkipFadeBrightnessStep = 2;
        /// <summary>Title updates holding the logo stage before the copyright stage.</summary>
        public const int LogoHoldFrames = 32;
        /// <summary>Title updates holding the copyright stage before starting the idle title timeout.</summary>
        public const int CopyrightHoldFrames = 32;
        /// <summary>Idle title-screen timeout in NTSC title updates; expiration selects the demo before a confirmation edge on that update.</summary>
        public const int TitleScreenNtscFrames = 900;
        /// <summary>Title updates between one-level brightness decrements when leaving the idle title screen.</summary>
        public const int TitleFadeCadenceFrames = 2;
        /// <summary>Title updates per Baby animation phase, using the four-phase source-page sequence 0,1,2,1.</summary>
        public const int BabyFrameDuration = 10;
        /// <summary>$8B:9A52 accepted B, Start, and A buttons; newly pressed edges skip the opening or leave the idle title screen.</summary>
        public static SnesButton ConfirmButtons =>
            SnesButton.B | SnesButton.Start | SnesButton.A;
    }
}

/// <summary>One bank-$8B title-card object definition.</summary>
/// <param name="Phase">Runtime title-card phase entered when this instruction list starts.</param>
/// <param name="InstructionAddress">Full 24-bit address of the bank-$8B text-object instruction list.</param>
/// <param name="OriginX">Horizontal drawing origin in screen pixels.</param>
/// <param name="OriginY">Vertical drawing origin in screen pixels.</param>
/// <param name="CharacterOffset">Native OBJ attribute/palette word applied to the composition; the legacy name does not denote a tile-count offset.</param>
public readonly record struct TitleTextSequenceDefinition(
    TitleSequencePhase Phase,
    int InstructionAddress,
    ushort OriginX,
    ushort OriginY,
    ushort CharacterOffset);

/// <summary>Initial Mode-7 transform installed by one title-scene command.</summary>
/// <param name="Phase">Pan or zoom phase entered by the scene-trigger command.</param>
/// <param name="Scale">Initial native 8.8 transform scale, with $0100 representing unity.</param>
/// <param name="HorizontalOffset">Signed background horizontal position in pixels, before fixed-point pan updates.</param>
/// <param name="VerticalOffset">Signed background vertical position in pixels, before fixed-point pan updates.</param>
public readonly record struct TitleMode7SceneDefinition(
    TitleSequencePhase Phase,
    int Scale,
    short HorizontalOffset,
    short VerticalOffset);
