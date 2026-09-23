namespace SuperMetroid.Core.Game;

/// <summary>Immutable control words for Red Brinstar's background-glow palette loop.</summary>
/// <remarks>
/// Palette-FX definition <c>$F77D</c> owns fourteen timed records at $8D:EED7-$EFF6.
/// Its 112 BGR555 colors remain live presentation data; this catalog owns only color-index
/// setup, durations, waits, and loop control.
/// </remarks>
public static class RedBrinstarGlowPaletteFxProgramMechanicsDefinitions
{
    /// <summary>Red Brinstar background-glow palette-FX definition at $8D:F77D.</summary>
    public const ushort DefinitionPointer = 0xf77d;

    /// <summary><c>InstList_PaletteFXObject_Brinstar2_0</c> at $8D:EED7.</summary>
    /// <remarks>
    /// Definition $8D:F77D enters a bounded fourteen-record loop. $EED7
    /// selects CGRAM byte $00C8; record i=0..13 starts at $EEDB + 20 * i,
    /// lasts ten frames, writes eight live BGR555 colors, and ends in
    /// $C595 wait. The $C61E goto at $EFF3 returns to $EEDB, making a
    /// 140-frame cycle; index fourteen reaches control rather than data.
    /// All 32 mechanics words match the pinned NTSC J/U v1.0 ROM.
    /// </remarks>
    public const ushort ProgramStart = 0xeed7;

    /// <summary>First timed record, <c>InstList_PaletteFXObject_Brinstar2_1</c>.</summary>
    public const ushort FirstFramePointer = 0xeedb;

    /// <summary>Terminal <c>goto</c> command at $8D:EFF3.</summary>
    public const ushort LoopInstructionPointer = 0xeff3;

    /// <summary>Fourteen ten-frame records form one complete glow cycle.</summary>
    public const int FrameCount = 14;

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 20;

    /// <summary>The byte index of the first Red Brinstar glow color in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x00c8;

    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 140;

    /// <summary>Reads one fixed mechanics word while excluding BGR555 presentation words.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (pointer == ProgramStart)
        {
            value = PaletteFxInstructionCodes.SetColorIndex;
            return true;
        }
        if (pointer == unchecked((ushort)(ProgramStart + sizeof(ushort))))
        {
            value = ColorByteIndex;
            return true;
        }
        if (pointer == LoopInstructionPointer)
        {
            value = PaletteFxInstructionCodes.Goto;
            return true;
        }
        if (pointer == unchecked((ushort)(LoopInstructionPointer + sizeof(ushort))))
        {
            value = FirstFramePointer;
            return true;
        }

        int frameOffset = pointer - FirstFramePointer;
        if (frameOffset >= 0 && frameOffset < FrameCount * FrameByteCount)
        {
            int inFrame = frameOffset % FrameByteCount;
            if (inFrame == 0)
            {
                value = 10;
                return true;
            }
            if (inFrame == FrameByteCount - sizeof(ushort))
            {
                value = PaletteFxInstructionCodes.Wait;
                return true;
            }
        }

        value = 0;
        return false;
    }

    /// <summary>Returns the timed-record pointer for one zero-based glow frame.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one contiguous presentation-color address within a frame.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
