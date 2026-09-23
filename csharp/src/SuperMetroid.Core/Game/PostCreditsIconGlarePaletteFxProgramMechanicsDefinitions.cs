namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the post-credits Super Metroid icon glare.</summary>
/// <remarks>
/// Issue #851 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E200</c>
/// enters at <c>$8D:DF94</c>: <c>SetColorIndex($01E0)</c>, fourteen
/// 36-byte records of <c>1, colors[16], Wait</c>, then <c>Delete</c> at
/// <c>$E190</c> after 14 frames. All 31 control words match. Let
/// <c>base[c]</c> be the final record's sixteen authored BGR555 colors.
/// For record <c>f</c> (0..13), set <c>t = f + 1</c> for 0..6 and
/// <c>t = 13 - f</c> for 7..13. For each color column <c>c</c> and
/// five-bit channel <c>q</c> of <c>base[c]</c>, the output channel is
/// <c>floor(((7 - t)*q + 31*t)/7)</c>. Frame 6 is white, frames 7..12
/// mirror frames 5..0, and frame 13 restores the base. This rule matches
/// all 224 ROM colors exactly. They remain live presentation data supplied
/// by the presentation compiler. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
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
