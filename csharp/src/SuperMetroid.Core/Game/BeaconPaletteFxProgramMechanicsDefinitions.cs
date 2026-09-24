namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the shared Crateria/Brinstar beacon-flashing loop.</summary>
/// <remarks>
/// Definition <c>$F781</c> is used by pre-Tourian hall, Red Brinstar mainstreet, the
/// Red Brinstar elevator, and early Kraid rooms. Its sound-ID byte is a fixed
/// library-two selection; setup, timing, CGRAM skips, audio command, and loop
/// flow compile. The forty BGR555 colors remain presentation data.
/// $8D:EFF7 selects CGRAM byte $00E2. Ten records last ten frames each:
/// frames i=0..5 begin at $EFFB + 14*i; frames i=6..9 begin at
/// $F052 + 14*(i-6), after the $C673 library-two sound opcode at $F04F
/// consumes the compiled one-byte ID $18 at $F051. Each record writes three
/// colors, runs $C5BD to skip nine CGRAM colors, writes one more color,
/// then waits at $C595. The $C61E goto at $F08A returns to $EFFB;
/// frame ten reaches that control after a 100-frame cycle. All 35
/// mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class BeaconPaletteFxProgramMechanicsDefinitions
{
    /// <summary>The room palette-FX definition at <c>$8D:F781</c>.</summary>
    public const ushort DefinitionPointer = 0xf781;

    /// <summary>Color-index setup at <c>$8D:EFF7</c>.</summary>
    public const ushort ProgramStart = 0xeff7;

    /// <summary>First timed record at <c>$8D:EFFB</c>.</summary>
    public const ushort FirstFramePointer = 0xeffb;

    /// <summary>Library-two sound opcode at <c>$8D:F04F</c>.</summary>
    public const ushort SoundInstructionPointer = 0xf04f;

    /// <summary>Sound-ID byte at <c>$8D:F051</c>.</summary>
    public const ushort SoundOperandPointer = 0xf051;

    /// <summary>Library-two sound ID <c>$18</c> selected at <c>$8D:F051</c>.</summary>
    public const byte SoundId = 0x18;

    /// <summary>First timed record after the byte-sized sound command.</summary>
    public const ushort PostSoundFramePointer = 0xf052;

    /// <summary>Terminal <c>goto</c> command at <c>$8D:F08A</c>.</summary>
    public const ushort LoopInstructionPointer = 0xf08a;

    /// <summary>Ten ten-frame records form one complete flash cycle.</summary>
    public const int FrameCount = 10;

    /// <summary>Four live BGR555 colors are written by each timed record.</summary>
    public const int ColorsPerFrame = 4;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 14;

    /// <summary>Frames before the mid-cycle sound command.</summary>
    public const int PreSoundFrameCount = 6;

    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 100;

    /// <summary>Three colors precede each record's inline CGRAM-index skip.</summary>
    private const int ColorsBeforeIndexSkip = 3;

    /// <summary>The fourth color follows the inline CGRAM-index skip at byte ten.</summary>
    private const int PostSkipColorOffset = 10;

    /// <summary>The first destination byte in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x00e2;

    /// <summary>Returns the timed-record pointer for one zero-based cycle frame.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame < PreSoundFrameCount)
            return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
        return unchecked((ushort)(
            PostSoundFramePointer + (frame - PreSoundFrameCount) * FrameByteCount));
    }

    /// <summary>Returns one live BGR555 word, skipping the inline CGRAM-index command.</summary>
    /// <remarks>
    /// The valid frame range is 0..9, with four colors each: offsets 2, 4,
    /// 6, and 10 from FramePointer. Frames 0..5 contain authored BGR555
    /// rows ($02BF,$017F,$0015,$7FFF), ($023B,$00FB,$0011,$739C),
    /// ($01D8,$0098,$000E,$5AD6), ($0154,$0055,$000B,$4E73),
    /// ($00D0,$0010,$0007,$4631), ($00AA,$000B,$0004,$3DEF).
    /// Frames 6..9 mirror rows 4..1. This schedule matches all 40 words
    /// in the pinned NTSC J/U v1.0 ROM. The six irregular color rows are
    /// retained as live presentation data rather than generated at runtime.
    /// </remarks>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        int offset = color < ColorsBeforeIndexSkip
            ? sizeof(ushort) + color * sizeof(ushort)
            : PostSkipColorOffset;
        return unchecked((ushort)(FramePointer(frame) + offset));
    }

    /// <summary>Resolves one compiled mechanics word while retaining color/audio data.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            ProgramStart + 2 => ColorByteIndex,
            SoundInstructionPointer => PaletteFxInstructionCodes.QueueSfx2,
            LoopInstructionPointer => PaletteFxInstructionCodes.Goto,
            LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0; frame < FrameCount; frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => 10,
                8 => PaletteFxInstructionCodes.ColorPlus9,
                12 => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }

    /// <summary>Resolves the fixed sound operand without reading bank <c>$8D</c>.</summary>
    public static bool TryReadMechanicsByte(ushort pointer, out byte value)
    {
        value = pointer == SoundOperandPointer ? SoundId : (byte)0;
        return pointer == SoundOperandPointer;
    }
}
