namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for upper Crateria's escape red-flash loop.</summary>
/// <remarks>
/// Definition <c>$FFE5</c> enters one fourteen-record program. Its 98 BGR555 words
/// remain live presentation data; this catalog owns palette placement, timing, waits,
/// and loop control.
/// </remarks>
public static class UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions
{
    /// <summary><c>PalFxDef_Crateria2</c> at <c>$8D:FFE5</c>.</summary>
    public const ushort DefinitionPointer = 0xffe5;
    /// <summary><c>PalFxInstList_Crateria2</c> at <c>$8D:FCFD</c>.</summary>
    public const ushort ProgramStart = 0xfcfd;
    /// <summary>The first CGRAM destination byte for upper Crateria's red flash.</summary>
    public const ushort ColorByteIndex = 0x0082;
    /// <summary>The first timed record at <c>$8D:FD01</c>.</summary>
    public const ushort FirstFramePointer = 0xfd01;
    /// <summary>The terminal <c>goto</c> at <c>$8D:FDFD</c>.</summary>
    public const ushort LoopInstructionPointer = 0xfdfd;
    /// <summary>The loop contains fourteen timed records.</summary>
    public const int FrameCount = 14;
    /// <summary>Each record writes seven live BGR555 colors.</summary>
    public const int ColorsPerFrame = 7;
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 18;
    /// <summary>The complete loop lasts 63 frames.</summary>
    public const int CycleFrames = 63;

    /// <summary>Fourteen timed red-flash records in their native order.</summary>
    /// <remarks>
    /// For the only valid frame indices 0..13, duration is
    /// abs(7 - frame) + 1. The unsigned words at $8D:FD01 + 18 * frame
    /// all match this integer triangle in the pinned NTSC J/U v1.0 ROM.
    /// Their sum is 63 frames. The $C61E goto at $FDFD returns to $FD01;
    /// frame fourteen is control rather than another duration. Seven
    /// interleaved BGR555 colors per frame remain live presentation data.
    /// </remarks>
    private static readonly ushort[] Durations = [8, 7, 6, 5, 4, 3, 2, 1, 2, 3, 4, 5, 6, 7];

    /// <summary>Returns one timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            ProgramStart + 2 => ColorByteIndex,
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
                0 => Durations[frame],
                FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
