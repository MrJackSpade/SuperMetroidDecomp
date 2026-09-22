namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the Zebes explosion finale.</summary>
/// <remarks>
/// Definition <c>$E1CC</c> runs 45 timed records and then deletes itself. Its 675
/// BGR555 words remain live presentation data; this catalog owns palette placement,
/// the two-phase duration schedule, waits, and termination.
/// </remarks>
public static class ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions
{
    /// <summary>The palette-FX definition at <c>$8D:E1CC</c>.</summary>
    public const ushort DefinitionPointer = 0xe1cc;

    /// <summary>The instruction-list entry at <c>$8D:CD62</c>.</summary>
    public const ushort ProgramStart = 0xcd62;

    /// <summary>The first timed record at <c>$8D:CD66</c>.</summary>
    public const ushort FirstFramePointer = 0xcd66;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:D360</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xd360;

    /// <summary>The finale writes CGRAM from byte index <c>$0002</c>.</summary>
    public const ushort ColorByteIndex = 0x0002;

    /// <summary>The finale contains 45 timed records.</summary>
    public const int FrameCount = 45;

    /// <summary>The first fifteen records form the fast gradient-fill phase.</summary>
    public const int FastPhaseFrameCount = 15;

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 34;

    /// <summary>Each fast-phase record lasts two frames.</summary>
    public const ushort FastPhaseFrameDuration = 2;

    /// <summary>Each rotation/restore record lasts nine frames.</summary>
    public const ushort SlowPhaseFrameDuration = 9;

    /// <summary>The complete one-shot finale lasts 300 frames.</summary>
    public const int CycleFrames = 300;

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

    /// <summary>Returns the cartridge-authored duration for one finale record.</summary>
    public static ushort FrameDuration(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return frame < FastPhaseFrameCount
            ? FastPhaseFrameDuration
            : SlowPhaseFrameDuration;
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
                0 => FrameDuration(frame),
                FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
