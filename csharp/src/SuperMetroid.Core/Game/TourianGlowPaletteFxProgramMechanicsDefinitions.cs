namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for Tourian's glowing block and red-orb palette loop.</summary>
/// <remarks>
/// Definitions <c>$F7A1</c> and <c>$F7A5</c> enter one shared eleven-record program.
/// Its 88 BGR555 colors remain presentation data; this catalog owns entry setup, the
/// slot-sensitive pre-instruction, inline CGRAM skips, timing, waits, and loop control.
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
