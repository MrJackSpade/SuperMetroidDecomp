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
/// Native $82:BC27..BD96 has three cry groups, preceded by 2, 4 and 3 idle
/// cycles respectively. Each idle cycle is a four-step triangular frame wave,
/// Closed/Middle/Open/Middle, held ten ticks per step. A cry has eight steps
/// of that wave; its duration is 2 + abs(4 - step), with step 0..7, and its
/// palette follows the frame plus one. These values are calculated on demand.
/// The first cry step has a two-byte sound callback after its six-byte record.
/// Get decodes this gap before checking alignment; callback bytes, fields within
/// records and every other unsupported ushort pointer retain their rejection.
/// The final record restarts at BC27 after the FFFF marker at BD95. There is no
/// generated instruction array or pointer dictionary. The 2/4/3 cycle counts
/// are named cry-stage cases, not an interpolated numeric curve.
/// </remarks>
public static class GameOverBabyAnimationDefinitions
{
    /// <summary>First native instruction at <c>$82:BC27</c>.</summary>
    public const ushort FirstPointer = 0xbc27;

    /// <summary>End marker immediately after the final record at <c>$82:BD95</c>.</summary>
    public const ushort EndMarkerPointer = 0xbd95;

    /// <summary>Sixty six-byte frame records plus three two-byte callbacks.</summary>
    public const int InstructionCount = (2 + 4 + 3) * 4 + 3 * 8;

    /// <summary>First cry group: two idle cycles, eight cry steps and one sound word.</summary>
    private const int FirstGroupBytes = (2 * 4 + 8) * 6 + 2;
    /// <summary>Second cry group: four idle cycles, eight cry steps and one sound word.</summary>
    private const int SecondGroupBytes = (4 * 4 + 8) * 6 + 2;

    public static IEnumerable<GameOverBabyInstruction> All
    {
        get
        {
            ushort pointer = FirstPointer;
            while (true)
            {
                GameOverBabyInstruction instruction = Get(pointer);
                yield return instruction;
                if (instruction.RestartAfter) yield break;
                pointer = instruction.NextPointer;
            }
        }
    }

    public static GameOverBabyInstruction Get(ushort pointer)
    {
        int offset = pointer - FirstPointer;
        if ((uint)offset >= EndMarkerPointer - FirstPointer) throw Unknown(pointer);
        int idleFrames;
        GameOverBabySound cry;
        if (offset < FirstGroupBytes)
        {
            idleFrames = 2 * 4;
            cry = GameOverBabySound.Cry23;
        }
        else if (offset < FirstGroupBytes + SecondGroupBytes)
        {
            offset -= FirstGroupBytes;
            idleFrames = 4 * 4;
            cry = GameOverBabySound.Cry26;
        }
        else
        {
            offset -= FirstGroupBytes + SecondGroupBytes;
            idleFrames = 3 * 4;
            cry = GameOverBabySound.Cry27;
        }
        int soundOffset = idleFrames * 6 + 6;
        if (offset == soundOffset || offset == soundOffset + 1) throw Unknown(pointer);
        if (offset > soundOffset) offset -= 2;
        if (offset % 6 != 0) throw Unknown(pointer);
        int step = offset / 6;
        int cryStep = step - idleFrames;
        int frame = 2 - Math.Abs(2 - (step & 3));
        ushort duration = (ushort)(cryStep < 0 ? 10 : 2 + Math.Abs(4 - cryStep));
        var palette = cryStep < 0 ? GameOverBabyPalette.Idle : (GameOverBabyPalette)(frame + 1);
        GameOverBabySound sound = cryStep == 0 ? cry : GameOverBabySound.None;
        bool restart = pointer == EndMarkerPointer - 6;
        ushort next = restart ? FirstPointer : (ushort)(pointer + (sound == GameOverBabySound.None ? 6 : 8));
        return new(pointer, duration, (GameOverBabyFrame)frame, palette, sound, next, restart);
    }

    private static InvalidDataException Unknown(ushort pointer) =>
        new($"Unknown compiled game-over Baby instruction $82:{pointer:X4}.");

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

}
