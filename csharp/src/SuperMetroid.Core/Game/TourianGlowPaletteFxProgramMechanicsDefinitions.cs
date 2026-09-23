namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for Tourian's glowing block and red-orb palette loop.</summary>
/// <remarks>
/// Definitions <c>$F7A1</c> and <c>$F7A5</c> enter one shared eleven-record program.
/// Its 88 BGR555 colors remain presentation data; this catalog owns entry setup, the
/// slot-sensitive pre-instruction, inline CGRAM skips, timing, waits, and loop control.
/// Clone entry $8D:F62A selects CGRAM byte $00E8 and jumps to $F636;
/// live entry $F632 selects the same index and falls through. $F636
/// installs pre-instruction $F621, which deletes the owner when two later
/// palette-FX slots exist. Records f=0..10 begin at $F63A + 22*f and
/// last ten frames each: one live color, $C5A2 skip of three CGRAM
/// colors, seven live colors, then $C595 wait. The $C61E goto at $F72C
/// returns to $F63A after a 110-frame cycle; f=11 reaches control.
/// All 43 mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class TourianGlowPaletteFxProgramMechanicsDefinitions
{
    /// <summary>The live Tourian 2 definition.</summary>
    public const ushort LiveDefinitionPointer = 0xf7a1;

    /// <summary>The unused Tourian 4 clone definition.</summary>
    public const ushort CloneDefinitionPointer = 0xf7a5;

    /// <summary>Clone entry at <c>$8D:F62A</c>.</summary>
    public const ushort CloneProgramStart = 0xf62a;

    /// <summary>Live entry at <c>$8D:F632</c>.</summary>
    public const ushort LiveProgramStart = 0xf632;

    /// <summary>Shared pre-instruction setup at <c>$8D:F636</c>.</summary>
    public const ushort SharedProgramStart = 0xf636;

    /// <summary>First timed record at <c>$8D:F63A</c>.</summary>
    public const ushort FirstFramePointer = 0xf63a;

    /// <summary>Terminal <c>goto</c> at <c>$8D:F72C</c>.</summary>
    public const ushort LoopInstructionPointer = 0xf72c;

    /// <summary>Eleven ten-frame records form one complete glow cycle.</summary>
    public const int FrameCount = 11;

    /// <summary>Eight live BGR555 colors are written by each record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 22;

    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 110;

    /// <summary>The first destination byte in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x00e8;

    /// <summary>Returns the timed-record pointer for one zero-based cycle frame.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one split presentation-color address within a frame.</summary>
    /// <remarks>
    /// For frame f=0..10, color c=0..7, and d=min(f,11-f), decode
    /// base BGR555 words ($5294,$0019,$0012,$5C00,$4000,$1084,
    /// $197F,$7FFF) into red, green, and blue channels. Subtract
    /// step[c]*d from each channel independently, clamp each at zero,
    /// then re-encode BGR555. Steps are (2,3,3,3,3,0,3,3).
    /// Color 0 is at $F63A + 22*f + 2; colors 1..7 begin at offset 6
    /// after the inline CGRAM skip. All 88 words match the pinned NTSC
    /// J/U v1.0 ROM, including clamped channels. Frame eleven reaches
    /// loop control; both entries read the colors live.
    /// </remarks>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        int offset = color == 0
            ? sizeof(ushort)
            : 6 + (color - 1) * sizeof(ushort);
        return unchecked((ushort)(FramePointer(frame) + offset));
    }

    /// <summary>Resolves one compiled mechanics word across both entries and shared code.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            CloneProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            CloneProgramStart + 2 => ColorByteIndex,
            CloneProgramStart + 4 => PaletteFxInstructionCodes.Goto,
            CloneProgramStart + 6 => SharedProgramStart,
            LiveProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            LiveProgramStart + 2 => ColorByteIndex,
            SharedProgramStart => PaletteFxInstructionCodes.SetPreInstruction,
            SharedProgramStart + 2 => PaletteFxPreInstructionCodes.InspectAdjacentSlot,
            LoopInstructionPointer => PaletteFxInstructionCodes.Goto,
            LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        int frameOffset = pointer - FirstFramePointer;
        if (frameOffset >= 0 && frameOffset < FrameCount * FrameByteCount)
        {
            int inFrame = frameOffset % FrameByteCount;
            value = inFrame switch
            {
                0 => 10,
                4 => PaletteFxInstructionCodes.ColorPlus3,
                FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
