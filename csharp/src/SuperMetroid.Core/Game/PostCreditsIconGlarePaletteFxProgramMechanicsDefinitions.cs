namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the post-credits Super Metroid icon glare.</summary>
/// <remarks>
/// Definition <c>$E200</c> runs fourteen one-frame records and then deletes itself. Its
/// 224 BGR555 words remain live presentation data; this catalog owns palette placement,
/// timing, waits, and termination.
/// </remarks>
public static class PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions
{
    /// <summary>The palette-FX definition at <c>$8D:E200</c>.</summary>
    public const ushort DefinitionPointer = 0xe200;

    /// <summary>The instruction-list entry at <c>$8D:DF94</c>.</summary>
    public const ushort ProgramStart = 0xdf94;

    /// <summary>The first timed record at <c>$8D:DF98</c>.</summary>
    public const ushort FirstFramePointer = 0xdf98;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:E190</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xe190;

    /// <summary>The glare writes CGRAM from byte index <c>$01E0</c>.</summary>
    public const ushort ColorByteIndex = 0x01e0;

    /// <summary>The one-shot glare contains fourteen timed records.</summary>
    public const int FrameCount = 14;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 36;

    /// <summary>Each record lasts one frame.</summary>
    public const ushort FrameDuration = 1;

    /// <summary>The complete icon glare lasts fourteen frames.</summary>
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
