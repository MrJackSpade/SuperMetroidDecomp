namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for fading in the Super Metroid title logo.</summary>
/// <remarks>
/// Definition <c>$E194</c> runs eight three-frame records and then deletes itself. Its
/// 120 BGR555 words remain live presentation data; this catalog owns palette placement,
/// timing, waits, and termination.
/// </remarks>
public static class TitleLogoFadePaletteFxProgramMechanicsDefinitions
{
    /// <summary>The palette-FX definition at <c>$8D:E194</c>.</summary>
    public const ushort DefinitionPointer = 0xe194;

    /// <summary>The instruction-list entry at <c>$8D:C696</c>.</summary>
    public const ushort ProgramStart = 0xc696;

    /// <summary>The first timed record at <c>$8D:C69A</c>.</summary>
    public const ushort FirstFramePointer = 0xc69a;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:C7AA</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xc7aa;

    /// <summary>The fade writes CGRAM from byte index <c>$0142</c>.</summary>
    public const ushort ColorByteIndex = 0x0142;

    /// <summary>The one-shot fade contains eight timed records.</summary>
    public const int FrameCount = 8;

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 34;

    /// <summary>Each record lasts three frames.</summary>
    public const ushort FrameDuration = 3;

    /// <summary>The complete title-logo fade lasts 24 frames.</summary>
    public const int CycleFrames = FrameCount * FrameDuration;

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
