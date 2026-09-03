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
        public const ushort InitialSpritemap = 0x65;
        public const int PaletteDestinationIndex = 0xc0;
        public const int PaletteColorCount = 16;

        public static readonly SoundEffectId Cry23 =
            new(SoundEffectLibrary.Library3, 0x23);
        public static readonly SoundEffectId Cry26 =
            new(SoundEffectLibrary.Library3, 0x26);
        public static readonly SoundEffectId Cry27 =
            new(SoundEffectLibrary.Library3, 0x27);

        /// <summary>Maps a bank-$82 animation opcode to its library-qualified cry.</summary>
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
        public static readonly SnesObjAttributeWord BabyPalette = new(0x0800);
        public const ushort EggSpritemap = 0x64;
        public static readonly SnesObjAttributeWord EggPalette = new(0x0a00);
        public const ushort MissileX = 40;
        public const ushort YesMissileY = 160;
        public const ushort NoMissileY = 192;
        public const int MissileFrameDuration = 8;
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
