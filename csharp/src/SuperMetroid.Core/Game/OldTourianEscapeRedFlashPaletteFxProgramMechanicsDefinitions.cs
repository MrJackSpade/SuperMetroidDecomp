namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the old-Tourian escape-shaft red-flash loop.</summary>
/// <remarks>
/// Definition <c>$FFD9</c> enters one fourteen-record program. Its 112 BGR555 words
/// remain live presentation data; this catalog owns palette placement, timing, both
/// inline CGRAM skips, waits, and loop control.
/// </remarks>
public static class OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions
{
    /// <summary><c>PalFxDef_Crateria8</c> at <c>$8D:FFD9</c>.</summary>
    public const ushort DefinitionPointer = 0xffd9;

    /// <summary><c>PalFxInstList_Crateria8</c> at <c>$8D:FA69</c>.</summary>
    public const ushort ProgramStart = 0xfa69;

    /// <summary>The first timed record at <c>$8D:FA6D</c>.</summary>
    public const ushort FirstFramePointer = 0xfa6d;

    /// <summary>The terminal <c>goto</c> at <c>$8D:FBBD</c>.</summary>
    public const ushort LoopInstructionPointer = 0xfbbd;

    /// <summary>The loop contains fourteen timed records.</summary>
    public const int FrameCount = 14;

    /// <summary>Each record writes eight live BGR555 colors.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 24;

    /// <summary>The complete loop lasts 42 frames.</summary>
    public const int CycleFrames = 42;

    /// <summary>Returns one timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            ProgramStart + 2 => 0x00a2,
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
                0 => 3,
                8 => PaletteFxInstructionCodes.ColorPlus4,
                18 => PaletteFxInstructionCodes.ColorPlus2,
                FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
