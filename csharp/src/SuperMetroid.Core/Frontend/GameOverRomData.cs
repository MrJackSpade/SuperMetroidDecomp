using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verified cartridge streams, opcodes, and layout for the game-over menu.</summary>
public static class GameOverRomData
{
    /// <summary>Bank-$81 base for expanding the sixteen-bit localized text-stream pointers into cartridge byte addresses during extraction.</summary>
    public const int TextBank = 0x810000;
    /// <summary>Bank-$82 base containing the Baby animation, palettes, and menu spritemaps; combined with native sixteen-bit resource pointers.</summary>
    public const int SpriteBank = 0x820000;
    /// <summary>Number of BG1 tile entries per game-over row; text destinations encode their column as twice the tile X.</summary>
    public const int TilemapWidth = 32;
    /// <summary>Number of tile rows in the complete game-over BG1 page, including rows outside the visible viewport.</summary>
    public const int TilemapHeight = 32;
    /// <summary>Byte stride between consecutive BG1 rows: 32 native sixteen-bit entries; next-line text commands advance by this amount.</summary>
    public const int TilemapRowByteCount = TilemapWidth * sizeof(ushort);
    /// <summary>Native $FFFF command ending one bank-$81 text stream without writing a tile.</summary>
    public const ushort TextEnd = 0xffff;
    /// <summary>Native $FFFE text command moving the destination to the next BG1 row at the stream's original starting column.</summary>
    public const ushort TextNextLine = 0xfffe;
    /// <summary>Full intensity of the four-bit SNES brightness field; native fade-in at $81:90DE enters the interactive menu at this value.</summary>
    public const int MaximumBrightness = 15;
    /// <summary>Maximum-six queue policy used by the native selection-missile sound and all three Baby cry requests.</summary>
    public const byte MaximumQueuedSounds = 6;

    /// <summary>Localized text streams and their byte destinations in BG1.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: GameOverMenu_1_Init at $81:9206..9230 loads five
    /// bank-$81 sources and BG1 byte destinations in this order. Pinned NTSC
    /// J/U v1.0 ROM and bank_81.asm match all ten immediates. The source words
    /// occupy $92DC..9303, $9304..9333, $9334..934B, $934C..939F, and
    /// $93A0..93E7; each has one final $FFFF, and streams 0, 3, and 4 contain
    /// a $FFFE next-line command. Destinations $0156, $038A, $0414, $04CE,
    /// and $05CE encode screen columns/rows (11,5), (5,14), (10,16),
    /// (7,19), and (7,23) as 2*x+64*y. The loader and asset extractor both
    /// walk these five bounded command streams. Named screen-element cases select
    /// the resource and destination directly, without persistent record storage.
    /// This is element selection, not a numerical curve or fitted stream-length rule.
    /// </remarks>
    public static class Text
    {
        /// <summary>Five bounded text elements loaded in native initialization order: title, objective, prompt, Yes, and No.</summary>
        public const int Count = 5;

        /// <summary>Selects one native text element; unsupported selectors retain span-style rejection.</summary>
        public static GameOverTextStream Get(GameOverTextElement element) => element switch
        {
            GameOverTextElement.Title => new(0x0156, 0x92dc, "GAME OVER"),
            GameOverTextElement.Objective => new(0x038a, 0x9304, "FIND THE METROID LARVA"),
            GameOverTextElement.Prompt => new(0x0414, 0x9334, "TRY AGAIN"),
            GameOverTextElement.ReturnToGame => new(0x04ce, 0x934c, "YES - RETURN TO GAME"),
            GameOverTextElement.ReturnToTitle => new(0x05ce, 0x93a0, "NO - GO TO TITLE"),
            _ => throw new IndexOutOfRangeException(),
        };

        /// <summary>Enumerates the five native load calls in their original order, without a stored table.</summary>
        public static IEnumerable<GameOverTextStream> All
        {
            get
            {
                for (int index = 0; index < Count; index++)
                    yield return Get((GameOverTextElement)index);
            }
        }
    }

    /// <summary>Baby Metroid animation record layout and control words.</summary>
    /// <remarks>
    /// The four 16-word BGR555 Baby palettes are at $82:BD97..BE16.
    /// The animation selects these four blocks and rendering copies sixteen colors
    /// to CGRAM $C0..CF. Palette payload review is separate from instruction layout.
    /// </remarks>
    public static class BabyAnimation
    {
        /// <summary>Gameplay/menu updates assigned to the current Baby frame at $81:914B when either answer is accepted, holding that frame during fade-out.</summary>
        public const ushort AcceptedAnswerHoldDuration = 180;
        /// <summary>Native $FFFF animation sentinel, which restarts the stream at $82:BC27 rather than deleting the Baby or drawing a frame.</summary>
        public const ushort End = 0xffff;
        /// <summary><c>$82:BC0C Instruction_Queue_BabyMetroid_Cry1_SoundEffect</c>: queues library-three sound $23, then advances past the two-byte opcode.</summary>
        public const ushort CryOpcode23 = 0xbc0c;
        /// <summary><c>$82:BC15 Instruction_Queue_BabyMetroid_Cry2_SoundEffect</c>: queues library-three sound $26, then advances past the two-byte opcode.</summary>
        public const ushort CryOpcode26 = 0xbc15;
        /// <summary><c>$82:BC1E Instruction_Queue_BabyMetroid_Cry3_SoundEffect</c>: queues library-three sound $27, then advances past the two-byte opcode.</summary>
        public const ushort CryOpcode27 = 0xbc1e;
        /// <summary>First Baby frame, ID $65 in the bounded $65+frame sequence.</summary>
        /// <remarks>Issues #625 and #971: GameOverBabyAnimationDefinitions.NativeSpritemap
        /// documents all 60 native frame records and the three spritemap pointers.</remarks>
        public const ushort InitialSpritemap = 0x65;
        /// <summary>CGRAM color-entry index $C0, the start of OBJ palette four; each Baby frame copies sixteen colors here at $82:BBA9.</summary>
        public const int PaletteDestinationIndex = 0xc0;
        /// <summary>Sixteen colors per game-over Baby palette source.</summary>
        /// <remarks>Issues #625 and #972: four contiguous 16-word sources at
        /// $82:BD97+$20*p are selected by GameOverBabyAnimationDefinitions.NativePalettePointer
        /// for bounded phase p=0..3; all 60 native frame records stay in that domain.</remarks>
        public const int PaletteColorCount = 16;

        /// <summary>Typed library-three sound $23 emitted by the first cry opcode, distinct from its bank-$82 instruction address.</summary>
        public static readonly SoundEffectId Cry23 =
            new(SoundEffectLibrary.Library3, 0x23);
        /// <summary>Typed library-three sound $26 emitted by the second cry opcode, distinct from its bank-$82 instruction address.</summary>
        public static readonly SoundEffectId Cry26 =
            new(SoundEffectLibrary.Library3, 0x26);
        /// <summary>Typed library-three sound $27 emitted by the third cry opcode, distinct from its bank-$82 instruction address.</summary>
        public static readonly SoundEffectId Cry27 =
            new(SoundEffectLibrary.Library3, 0x27);
    }

    /// <summary>Game-over music data set and track selected by the state machine.</summary>
    public static class Music
    {
        /// <summary>Music data-set selector three loaded through a delayed-eight command during game-over initialization, after requesting that current music stop.</summary>
        public const byte DataIndex = 0x03;
        /// <summary>Track selector four, the pre-statue-hall music requested once the initial music-command queue drains at $81:93E8.</summary>
        public const byte TrackIndex = 4;
    }

    /// <summary>Fixed OBJ identities, positions, palettes, and animation timing.</summary>
    public static class Sprites
    {
        /// <summary>Fixed screen-pixel X anchor $7C used for both Baby and container spritemaps by $82:BBBF/$BBD1.</summary>
        public const ushort BabyX = 0x7c;
        /// <summary>Fixed screen-pixel Y anchor $50 used for both Baby and container spritemaps by $82:BBC2/$BBD4; animation changes art, not this position.</summary>
        public const ushort BabyY = 0x50;
        /// <summary>Packed OBJ palette-four attribute bits $0800 selected at $82:BBB7, matching the animated colors copied to CGRAM $C0..CF.</summary>
        public static readonly SnesObjAttributeWord BabyPalette = SnesObjPalettes.Index4;
        /// <summary>Menu spritemap ID $64 selected at $82:BBCE; table entry $82:C631 resolves to <c>TitleMenuSpritemaps_64_GameOverBabyMetroidContainer</c> at $82:CFE0.</summary>
        public const ushort EggSpritemap = 0x64;
        /// <summary>Packed OBJ palette-five attribute bits $0A00 selected at $82:BBC9 for the stationary Baby container.</summary>
        public static readonly SnesObjAttributeWord EggPalette = SnesObjPalettes.Index5;
        /// <summary>Screen-pixel X anchor 40 for the selection missile, shared by the Yes and No choices.</summary>
        public const ushort MissileX = 40;
        /// <summary>Screen-pixel missile Y anchor 160 for selection zero, Yes/return to game.</summary>
        public const ushort YesMissileY = 160;
        /// <summary>Screen-pixel missile Y anchor 192 for selection one, No/return to title.</summary>
        public const ushort NoMissileY = 192;
    }

    /// <summary>Blank tile used before the text command streams populate BG1.</summary>
    public static readonly SnesBgTilemapWord BlankTile = new(0x000f);
}

/// <summary>Mutually exclusive game-over text elements in native initialization order.</summary>
public enum GameOverTextElement
{
    /// <summary>$81:9206 loads GAME OVER from $81:92DC to BG1 byte $0156.</summary>
    Title,
    /// <summary>$81:920F loads FIND THE METROID LARVA from $81:9304 to BG1 byte $038A.</summary>
    Objective,
    /// <summary>$81:9218 loads TRY AGAIN from $81:9334 to BG1 byte $0414.</summary>
    Prompt,
    /// <summary>$81:9221 loads YES - RETURN TO GAME from $81:934C to BG1 byte $04CE.</summary>
    ReturnToGame,
    /// <summary>$81:922A loads NO - GO TO TITLE from $81:93A0 to BG1 byte $05CE.</summary>
    ReturnToTitle,
}

/// <summary>One bank-$81 text command stream and its BG1 byte destination.</summary>
/// <param name="DestinationByteOffset">BG1-page-relative byte offset of the initial tile; row and column are encoded as 64 times Y plus twice X.</param>
/// <param name="SourcePointer">Sixteen-bit bank-$81 source pointer to tile words and the $FFFE/$FFFF text commands.</param>
/// <param name="Description">Human-readable text-element identity for extraction diagnostics, not the encoded tile-word payload.</param>
public readonly record struct GameOverTextStream(
    int DestinationByteOffset,
    ushort SourcePointer,
    string Description);
