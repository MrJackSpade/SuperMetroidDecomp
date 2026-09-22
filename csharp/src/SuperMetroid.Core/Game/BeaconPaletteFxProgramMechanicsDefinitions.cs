namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the shared Crateria/Brinstar beacon-flashing loop.</summary>
/// <remarks>
/// Definition <c>$F781</c> is used by pre-Tourian hall, Red Brinstar mainstreet, the
/// Red Brinstar elevator, and early Kraid rooms. Its forty BGR555 words and one sound-ID
/// byte remain live data; setup, timing, CGRAM skips, audio opcode, and loop flow compile.
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

    /// <summary>Live sound-ID byte at <c>$8D:F051</c>.</summary>
    public const ushort SoundOperandPointer = 0xf051;

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
}
