namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the gunship emerging from the Zebes explosion.</summary>
/// <remarks>
/// Definition <c>$E1E4</c> runs sixteen timed records and then deletes itself. Its 256
/// BGR555 words remain live presentation data; this catalog owns palette placement,
/// timing, waits, and termination.
/// </remarks>
public static class ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions
{
    /// <summary>The palette-FX definition at <c>$8D:E1E4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1e4;

    /// <summary>The instruction-list entry at <c>$8D:D6BA</c>.</summary>
    public const ushort ProgramStart = 0xd6ba;

    /// <summary>The first timed record at <c>$8D:D6BE</c>.</summary>
    public const ushort FirstFramePointer = 0xd6be;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:D8FE</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xd8fe;

    /// <summary>The reveal writes CGRAM from byte index <c>$00A0</c>.</summary>
    public const ushort ColorByteIndex = 0x00a0;

    /// <summary>The one-shot reveal contains sixteen timed records.</summary>
    public const int FrameCount = 16;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 36;

    /// <summary>Each record lasts 24 frames.</summary>
    public const ushort FrameDuration = 0x18;

    /// <summary>The complete gunship reveal lasts 384 frames.</summary>
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
