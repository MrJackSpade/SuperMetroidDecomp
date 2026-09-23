using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verified cartridge streams, opcodes, and layout for the game-over menu.</summary>
public static class GameOverRomData
{
    public const int TextBank = 0x810000;
    public const int SpriteBank = 0x820000;
    public const int TilemapWidth = 32;
    public const int TilemapHeight = 32;
    public const int TilemapRowByteCount = TilemapWidth * sizeof(ushort);
    public const ushort TextEnd = 0xffff;
    public const ushort TextNextLine = 0xfffe;
    public const int MaximumBrightness = 15;
    public const byte MaximumQueuedSounds = 6;

    /// <summary>Localized text streams and their byte destinations in BG1.</summary>
    /// <remarks>
    /// Issues #625 and #969: GameOverMenu_1_Init at $81:9206..9230 loads five
    /// bank-$81 sources and BG1 byte destinations in this order. Pinned NTSC
    /// J/U v1.0 ROM and bank_81.asm match all ten immediates. The source words
    /// occupy $92DC..9303, $9304..9333, $9334..934B, $934C..939F, and
    /// $93A0..93E7; each has one final $FFFF, and streams 0, 3, and 4 contain
    /// a $FFFE next-line command. Destinations $0156, $038A, $0414, $04CE,
    /// and $05CE encode screen columns/rows (11,5), (5,14), (10,16),
    /// (7,19), and (7,23) as 2*x+64*y. The loader and asset extractor both
    /// walk these five bounded command streams. Their text lengths and placements
    /// are authored presentation data; retaining explicit source/destination
    /// records is clearer than hiding lengths in a prefix-sum generator.
    /// </remarks>
    public static class Text
    {
        private static readonly GameOverTextStream[] Streams =
        [
            new(0x0156, 0x92dc, "GAME OVER"),
            new(0x038a, 0x9304, "FIND THE METROID LARVA"),
            new(0x0414, 0x9334, "TRY AGAIN"),
            new(0x04ce, 0x934c, "YES - RETURN TO GAME"),
            new(0x05ce, 0x93a0, "NO - GO TO TITLE"),
        ];

        public static ReadOnlySpan<GameOverTextStream> All => Streams;
    }

    /// <summary>Baby Metroid animation record layout and control words.</summary>
    /// <remarks>
    /// Issues #625 and #973: the four 16-word BGR555 Baby palettes at
    /// $82:BD97..BE16 match all 64 words in pinned NTSC J/U v1.0 ROM and
    /// bank_82.asm. The 60 reachable frame records select only these four
    /// blocks, and rendering copies exactly 16 colors to CGRAM $C0..CF.
    /// Color slot 0 stays $3800, while all other 15 slots change across
    /// phases. Fourteen channel sequences are nonmonotonic, and the first
    /// phase change has seven distinct RGB delta vectors across those slots.
    /// This is authored illustration color rather than a uniform fade or
    /// common tint rule; retain the 64 source colors as presentation data.
    /// </remarks>
    public static class BabyAnimation
    {
        public const ushort FirstInstruction = 0xbc27;
        public const ushort InitialFrameDuration = 10;
        public const ushort AcceptedAnswerHoldDuration = 180;
        public const int DurationOffset = 0;
        public const int SpritemapOffset = 2;
        public const int PalettePointerOffset = 4;
        public const int NextInstructionOffset = 6;
        public const int FrameByteCount = 6;
        public const int SoundInstructionByteCount = 8;
        public const ushort End = 0xffff;
        public const ushort CryOpcode23 = 0xbc0c;
        public const ushort CryOpcode26 = 0xbc15;
        public const ushort CryOpcode27 = 0xbc1e;
        /// <summary>First Baby frame, ID $65 in the bounded $65+frame sequence.</summary>
        /// <remarks>Issues #625 and #971: GameOverBabyAnimationDefinitions.NativeSpritemap
        /// documents all 60 native frame records and the three spritemap pointers.</remarks>
        public const ushort InitialSpritemap = 0x65;
        public const int PaletteDestinationIndex = 0xc0;
        /// <summary>Sixteen colors per game-over Baby palette source.</summary>
        /// <remarks>Issues #625 and #972: four contiguous 16-word sources at
        /// $82:BD97+$20*p are selected by GameOverBabyAnimationDefinitions.NativePalettePointer
        /// for bounded phase p=0..3; all 60 native frame records stay in that domain.</remarks>
        public const int PaletteColorCount = 16;

        public static readonly SoundEffectId Cry23 =
            new(SoundEffectLibrary.Library3, 0x23);
        public static readonly SoundEffectId Cry26 =
            new(SoundEffectLibrary.Library3, 0x26);
        public static readonly SoundEffectId Cry27 =
            new(SoundEffectLibrary.Library3, 0x27);

        /// <summary>Maps a bank-$82 animation opcode to its library-qualified cry.</summary>
        /// <remarks>
        /// Issues #625 and #970: pinned NTSC J/U v1.0 ROM and bank_82.asm match
        /// all three native instructions at $82:BC0C+9*i for i=0..2. Each loads
        /// library-3 effect $23, $26, or $27 and calls the same sound queue.
        /// The game-over Baby stream references those opcodes once each at
        /// $82:BC5D, BCEF, and BD69. Opcode spacing is regular, but the effect
        /// identities are authored; retain the explicit bounded selector and
        /// reject unknown opcodes.
        /// </remarks>
        public static SoundEffectId ResolveCry(ushort opcode) => opcode switch
        {
            CryOpcode23 => Cry23,
            CryOpcode26 => Cry26,
            CryOpcode27 => Cry27,
            _ => throw new InvalidDataException(
                $"Unknown game-over Baby instruction $82:{opcode:X4}."),
        };
    }

    /// <summary>Game-over music data set and track selected by the state machine.</summary>
    public static class Music
    {
        public const byte DataIndex = 0x03;
        public const byte TrackIndex = 4;
    }

    /// <summary>Fixed OBJ identities, positions, palettes, and animation timing.</summary>
    public static class Sprites
    {
        private static readonly ushort[] MissileFrames = [0x37, 0x36, 0x35, 0x34];

        public const ushort BabyX = 0x7c;
        public const ushort BabyY = 0x50;
        public static readonly SnesObjAttributeWord BabyPalette = SnesObjPalettes.Index4;
        public const ushort EggSpritemap = 0x64;
        public static readonly SnesObjAttributeWord EggPalette = SnesObjPalettes.Index5;
        public const ushort MissileX = 40;
        public const ushort YesMissileY = 160;
        public const ushort NoMissileY = 192;
        /// <summary>All four $82:BAAA menu missile timer words equal eight calls.</summary>
        /// <remarks>Issues #625 and #955: pinned NTSC J/U v1.0 ROM and bank_82.asm
        /// match 4/4. The timer reload is shared with file select and options; the
        /// adjacent $82:BAB2 spritemap IDs are a separate table. Game-over animation
        /// may use an editable presentation override.</remarks>
        public const int MissileFrameDuration = 8;
        /// <summary>Shared $82:BAB2 menu missile IDs, exactly $0037-frame for frame 0..3.</summary>
        /// <remarks>Issues #625 and #954: game-over animation wraps modulo this four-word
        /// span; file select and options expose the same native table. All four words match
        /// pinned NTSC J/U v1.0 ROM and bank_82.asm.</remarks>
        public static ReadOnlySpan<ushort> MissileFrameIds => MissileFrames;
    }

    /// <summary>Blank tile used before the text command streams populate BG1.</summary>
    public static readonly SnesBgTilemapWord BlankTile = new(0x000f);
}

/// <summary>One bank-$81 text command stream and its BG1 byte destination.</summary>
public readonly record struct GameOverTextStream(
    int DestinationByteOffset,
    ushort SourcePointer,
    string Description);
