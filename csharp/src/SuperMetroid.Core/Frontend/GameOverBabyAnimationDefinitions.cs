namespace SuperMetroid.Core.Frontend;

/// <summary>Semantic Baby Metroid frames used by the game-over animation.</summary>
public enum GameOverBabyFrame
{
    Closed,
    Middle,
    Open,
}

/// <summary>Semantic Baby Metroid palette phases used by the game-over animation.</summary>
public enum GameOverBabyPalette
{
    Idle,
    ClosedCry,
    MiddleCry,
    OpenCry,
}

/// <summary>Named sound handoffs embedded between native game-over animation records.</summary>
public enum GameOverBabySound
{
    None,
    Cry23,
    Cry26,
    Cry27,
}

/// <summary>One compiled native animation record, independent of cartridge memory.</summary>
public readonly record struct GameOverBabyInstruction(
    ushort Pointer,
    ushort Duration,
    GameOverBabyFrame Frame,
    GameOverBabyPalette Palette,
    GameOverBabySound SoundAfter,
    ushort NextPointer,
    bool RestartAfter);

/// <summary>
/// Complete cartridge game-over Baby instruction list from <c>$82:BC27-$82:BD96</c>.
/// Native pointers remain the serialized/debugger identity; timing, frame selection and
/// sound dispatch are compiled application mechanics rather than editable visual data.
/// </summary>
/// <remarks>
/// Issues #625 and #974: pinned NTSC J/U v1.0 ROM and bank_82.asm agree with
/// all 60 records and every control word at $82:BC27..BD96. Build emits 2, 4,
/// then 3 idle cycles, each with four 10-tick frames (Closed, Middle, Open,
/// Middle) on the Idle palette. Each group ends with the same eight-frame cry:
/// durations 6,5,4,3,2,3,4,5; frames Closed,Middle,Open,Middle,Closed,
/// Middle,Open,Middle; matching cry palettes 1,2,3,2,1,2,3,2. The first cry
/// frame is followed by a distinct opcode at $82:BC5D, BCEF, or BD69, so its
/// successor is eight bytes away; all other successors are six bytes away.
/// The last record starts at $82:BD8F, followed by $FFFF at BD95, which loops
/// to BC27. An independent ROM walk matched all 180 record words, three cry
/// words, and the terminator; the extractor also checks compiled records
/// against cartridge reads. This bounded generator is smaller and clearer
/// than storing the serialized instruction table. Unknown pointers fail in Get.
/// </remarks>
public static class GameOverBabyAnimationDefinitions
{
    /// <summary>First native instruction at <c>$82:BC27</c>.</summary>
    public const ushort FirstPointer = 0xbc27;

    /// <summary>End marker immediately after the final record at <c>$82:BD95</c>.</summary>
    public const ushort EndMarkerPointer = 0xbd95;

    private static readonly GameOverBabyInstruction[] instructions = Build();
    private static readonly Dictionary<ushort, GameOverBabyInstruction> byPointer =
        instructions.ToDictionary(instruction => instruction.Pointer);

    public static ReadOnlySpan<GameOverBabyInstruction> All => instructions;

    public static GameOverBabyInstruction Get(ushort pointer) =>
        byPointer.TryGetValue(pointer, out GameOverBabyInstruction instruction)
            ? instruction
            : throw new InvalidDataException(
                $"Unknown compiled game-over Baby instruction $82:{pointer:X4}.");

    /// <summary>Native menu-spritemap identity used only for extraction parity.</summary>
    /// <remarks>
    /// Issues #625 and #971: for semantic frame f=0..2 (Closed, Middle,
    /// Open), native spritemap ID is $65+f. A pinned NTSC J/U v1.0 ROM walk of
    /// the complete $82:BC27..BD95 Baby instruction stream found 60 positive
    /// frame records, using IDs $65/$66/$67 exactly 15/30/15 times, with three
    /// separate cry words. Menu spritemap pointer-table entries $82:C633,
    /// C635, and C637 resolve to $82:CFF6, CFFD, and D004 as bank_82.asm says.
    /// The enum domain is only these three frames; unknown values fail.
    /// </remarks>
    public static ushort NativeSpritemap(GameOverBabyFrame frame) => frame switch
    {
        GameOverBabyFrame.Closed => 0x65,
        GameOverBabyFrame.Middle => 0x66,
        GameOverBabyFrame.Open => 0x67,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Native bank-$82 palette pointer used only for extraction parity.</summary>
    /// <remarks>
    /// Issues #625 and #972: for semantic palette phase p=0..3 (Idle,
    /// ClosedCry, MiddleCry, OpenCry), native source is $82:BD97+$20*p.
    /// Each contiguous block has 16 little-endian color words; four blocks
    /// occupy $82:BD97..BE16. A pinned NTSC J/U v1.0 ROM walk of all 60
    /// positive Baby frame records found phase counts 36, 6, 12, and 6,
    /// with no pointer outside these four sources. bank_82.asm names the same
    /// blocks. Unknown semantic phases fail rather than extrapolate.
    /// </remarks>
    public static ushort NativePalettePointer(GameOverBabyPalette palette) => palette switch
    {
        GameOverBabyPalette.Idle => 0xbd97,
        GameOverBabyPalette.ClosedCry => 0xbdb7,
        GameOverBabyPalette.MiddleCry => 0xbdd7,
        GameOverBabyPalette.OpenCry => 0xbdf7,
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    };

    /// <summary>Native bank-$82 sound callback identity used only for extraction parity.</summary>
    /// <remarks>Issues #625 and #970: this is the inverse view of
    /// GameOverRomData.BabyAnimation.ResolveCry for the three native cry
    /// instructions at $82:BC0C, BC15, and BC1E. Their effect IDs and stream
    /// references were checked against pinned NTSC J/U v1.0 ROM.</remarks>
    public static ushort NativeSoundOpcode(GameOverBabySound sound) => sound switch
    {
        GameOverBabySound.Cry23 => GameOverRomData.BabyAnimation.CryOpcode23,
        GameOverBabySound.Cry26 => GameOverRomData.BabyAnimation.CryOpcode26,
        GameOverBabySound.Cry27 => GameOverRomData.BabyAnimation.CryOpcode27,
        _ => throw new ArgumentOutOfRangeException(nameof(sound)),
    };

    private static GameOverBabyInstruction[] Build()
    {
        var seeds = new List<Seed>();
        ushort pointer = FirstPointer;

        AddIdleCycles(2);
        AddCry(GameOverBabySound.Cry23);
        AddIdleCycles(4);
        AddCry(GameOverBabySound.Cry26);
        AddIdleCycles(3);
        AddCry(GameOverBabySound.Cry27);

        if (pointer != EndMarkerPointer)
            throw new InvalidDataException(
                $"Compiled game-over Baby list ended at $82:{pointer:X4}, expected $82:{EndMarkerPointer:X4}.");

        var result = new GameOverBabyInstruction[seeds.Count];
        for (int index = 0; index < seeds.Count; index++)
        {
            Seed seed = seeds[index];
            bool restart = index == seeds.Count - 1;
            ushort next = restart ? FirstPointer : seeds[index + 1].Pointer;
            result[index] = new(seed.Pointer, seed.Duration, seed.Frame, seed.Palette,
                seed.SoundAfter, next, restart);
        }
        return result;

        void AddIdleCycles(int count)
        {
            for (int cycle = 0; cycle < count; cycle++)
            {
                Add(10, GameOverBabyFrame.Closed, GameOverBabyPalette.Idle);
                Add(10, GameOverBabyFrame.Middle, GameOverBabyPalette.Idle);
                Add(10, GameOverBabyFrame.Open, GameOverBabyPalette.Idle);
                Add(10, GameOverBabyFrame.Middle, GameOverBabyPalette.Idle);
            }
        }

        void AddCry(GameOverBabySound sound)
        {
            Add(6, GameOverBabyFrame.Closed, GameOverBabyPalette.ClosedCry, sound);
            Add(5, GameOverBabyFrame.Middle, GameOverBabyPalette.MiddleCry);
            Add(4, GameOverBabyFrame.Open, GameOverBabyPalette.OpenCry);
            Add(3, GameOverBabyFrame.Middle, GameOverBabyPalette.MiddleCry);
            Add(2, GameOverBabyFrame.Closed, GameOverBabyPalette.ClosedCry);
            Add(3, GameOverBabyFrame.Middle, GameOverBabyPalette.MiddleCry);
            Add(4, GameOverBabyFrame.Open, GameOverBabyPalette.OpenCry);
            Add(5, GameOverBabyFrame.Middle, GameOverBabyPalette.MiddleCry);
        }

        void Add(ushort duration, GameOverBabyFrame frame, GameOverBabyPalette palette,
            GameOverBabySound soundAfter = GameOverBabySound.None)
        {
            seeds.Add(new(pointer, duration, frame, palette, soundAfter));
            pointer = unchecked((ushort)(pointer +
                (soundAfter == GameOverBabySound.None
                    ? GameOverRomData.BabyAnimation.FrameByteCount
                    : GameOverRomData.BabyAnimation.SoundInstructionByteCount)));
        }
    }

    private readonly record struct Seed(
        ushort Pointer,
        ushort Duration,
        GameOverBabyFrame Frame,
        GameOverBabyPalette Palette,
        GameOverBabySound SoundAfter);
}
