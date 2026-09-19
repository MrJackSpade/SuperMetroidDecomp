namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control skeleton for Hyper Beam palette-FX program <c>$8D:D900</c>.
/// The ten eight-color payloads remain presentation data; this catalog owns the fixed
/// entry command, timing, record terminators, loop command, and valid pointer domain.
/// </summary>
internal static class HyperBeamPaletteFxProgramDefinitions
{
    /// <summary>Native address of the destination-selection command at <c>$8D:D900</c>.</summary>
    public const int NativeEntryControlAddress = 0x8dd900;

    /// <summary>Native address of the first frame timer at <c>$8D:D904</c>.</summary>
    public const int NativeFirstFrameTimerAddress = 0x8dd904;

    /// <summary>Native address of the terminal loop command at <c>$8D:D9CC</c>.</summary>
    public const int NativeLoopControlAddress = 0x8dd9cc;

    /// <summary>Bank-local instruction pointer installed by the object definition.</summary>
    public const ushort InitialInstructionPointer = 0xd900;

    /// <summary>Bank-local instruction pointer of the first timed color record.</summary>
    public const ushort FirstFramePointer = 0xd904;

    /// <summary>Bank-local instruction pointer of the terminal loop command.</summary>
    public const ushort LoopInstructionPointer = 0xd9cc;

    /// <summary>Number of authored timed color records.</summary>
    public const int FrameCount = 10;

    /// <summary>Number of BGR555 colors in each presentation payload.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes occupied by one timer, eight colors, and the done command.</summary>
    public const ushort FrameByteCount = 20;

    /// <summary>Fixed duration loaded by every authored color record.</summary>
    public const ushort FrameDuration = 2;

    /// <summary>
    /// Resolves an entry, timed-frame, or loop pointer to one typed frame. Restored
    /// pointers outside the native program fail rather than entering adjacent bank data.
    /// </summary>
    public static HyperBeamPaletteFxFrame ResolveFrame(
        ushort instructionPointer,
        out bool completedCycle)
    {
        completedCycle = instructionPointer == LoopInstructionPointer;
        ushort framePointer = instructionPointer switch
        {
            InitialInstructionPointer or LoopInstructionPointer => FirstFramePointer,
            _ => instructionPointer,
        };

        int relative = framePointer - FirstFramePointer;
        if (relative < 0 ||
            relative % FrameByteCount != 0 ||
            relative >= FrameCount * FrameByteCount)
        {
            throw new InvalidDataException(
                $"Hyper Beam palette-FX instruction pointer $8D:{instructionPointer:X4} " +
                "is outside the compiled control program.");
        }

        int frameIndex = relative / FrameByteCount;
        return new HyperBeamPaletteFxFrame(
            frameIndex,
            FrameDuration,
            unchecked((ushort)(framePointer + sizeof(ushort))),
            unchecked((ushort)(framePointer + FrameByteCount)));
    }
}

/// <summary>One resolved Hyper Beam palette frame and its fixed control metadata.</summary>
/// <param name="Index">Zero-based frame number.</param>
/// <param name="Duration">Number of handler calls for which the frame remains active.</param>
/// <param name="FirstColorPointer">Bank-$8D pointer to the editable eight-color payload.</param>
/// <param name="NextInstructionPointer">Bank-$8D pointer reached after the done command.</param>
internal readonly record struct HyperBeamPaletteFxFrame(
    int Index,
    ushort Duration,
    ushort FirstColorPointer,
    ushort NextInstructionPointer);
