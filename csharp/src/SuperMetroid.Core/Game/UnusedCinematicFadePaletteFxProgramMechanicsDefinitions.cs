namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the cartridge's unused cinematic fade.</summary>
/// <remarks>
/// Definition <c>$E1EC</c> runs eleven timed records and then deletes itself. Its 176
/// BGR555 words remain live presentation data; this catalog owns palette placement,
/// timing, waits, and termination.
/// </remarks>
public static class UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
{
    /// <summary>The unused palette-FX definition at <c>$8D:E1EC</c>.</summary>
    public const ushort DefinitionPointer = 0xe1ec;

    /// <summary>The instruction-list entry at <c>$8D:D9D0</c>.</summary>
    public const ushort ProgramStart = 0xd9d0;

    /// <summary>The first timed record at <c>$8D:D9D4</c>.</summary>
    public const ushort FirstFramePointer = 0xd9d4;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:DB60</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xdb60;

    /// <summary>The fade writes CGRAM from byte index <c>$00A0</c>.</summary>
    public const ushort ColorByteIndex = 0x00a0;

    /// <summary>The one-shot fade contains eleven timed records.</summary>
    public const int FrameCount = 11;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 36;

    /// <summary>Each record lasts two frames.</summary>
    public const ushort FrameDuration = 2;

    /// <summary>The complete unused fade lasts 22 frames.</summary>
    public const int CycleFrames = FrameCount * FrameDuration;

    /// <summary>Returns one timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one presentation-owned BGR555 word in a timed record.</summary>
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
            DeleteInstructionPointer => PaletteFxInstructionCodes.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0; frame < FrameCount; frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => FrameDuration,
                FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
