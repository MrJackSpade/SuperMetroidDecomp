namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the old-Tourian escape-shaft red-flash loop.</summary>
/// <remarks>
/// Definition <c>$FFD9</c> enters one fourteen-record program. Its 112 BGR555 words
/// remain live presentation data; this catalog owns palette placement, timing, both
/// inline CGRAM skips, waits, and loop control.
/// </remarks>
public static class OldTourianEscapeRedFlashPaletteFxColorDefinitions
{

    /// <summary><c>PalFxInstList_Crateria8</c> at <c>$8D:FA69</c>.</summary>
    public const ushort ProgramStart = 0xfa69;

    /// <summary>The first CGRAM destination byte for the old-Tourian red flash.</summary>
    public const ushort ColorByteIndex = 0x00a2;

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

    /// <summary>Returns one timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one color word, skipping the two inline CGRAM-index instructions.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        // Each preceding inline command occupies one word without supplying a color.
        int skippedCommands = (color >= 3 ? 1 : 0) + (color >= 7 ? 1 : 0);
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) *
            (1 + color + skippedCommands)));
    }

    /// <summary>The program's header and loop control-word addresses.</summary>
    private enum ControlWord : ushort
    {
        /// <summary>The <c>SetColorIndex</c> opcode at the program entry.</summary>
        SetColorIndex = ProgramStart,
        /// <summary>The CGRAM byte-index operand of <c>SetColorIndex</c>.</summary>
        ColorIndexOperand = ProgramStart + 2,
        /// <summary>The terminal <c>Goto</c> opcode.</summary>
        Goto = LoopInstructionPointer,
        /// <summary>The <c>Goto</c> target operand.</summary>
        GotoTarget = LoopInstructionPointer + 2,
    }
}
