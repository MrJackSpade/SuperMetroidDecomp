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
/// all 224 ROM colors exactly. LogoGlarePaletteColorDefinitions evaluates matching
/// presentation colors from the final supplied row; independent edits remain explicit.
/// The presentation compiler owns the base colors and overrides. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions
{

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

    /// <summary>Frames0..13 occupy36-byte records atDF98: duration,16 colors, wait.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Color0..15 begins two bytes after its frame duration and advances by two.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>The program's header and terminal control-word addresses.</summary>
    private enum ControlWord : ushort
    {
        /// <summary>The <c>SetColorIndex</c> opcode at the program entry.</summary>
        SetColorIndex = ProgramStart,
        /// <summary>The CGRAM byte-index operand of <c>SetColorIndex</c>.</summary>
        ColorIndexOperand = ProgramStart + 2,
        /// <summary>The terminal <c>Delete</c> opcode.</summary>
        Delete = DeleteInstructionPointer,
    }

    /// <summary>Named header/delete operations and duration/wait at offsets0/34 of
    /// each36-byte record. Exact finite pointer ownership excludes colors and odd bytes;
    /// independently checked by decoding the original stream through its delete.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = 0;
        var control = (ControlWord)pointer;
        if (Enum.IsDefined(control))
        {
            value = control switch
            {
                ControlWord.SetColorIndex => PaletteFxInstructionCodes.SetColorIndex,
                ControlWord.ColorIndexOperand => ColorByteIndex,
                ControlWord.Delete => PaletteFxInstructionCodes.Delete,
                _ => throw new InvalidOperationException($"Undefined ControlWord {control}."),
            };
            return true;
        }

        int offset = pointer - FirstFramePointer;
        if ((uint)offset >= FrameCount * FrameByteCount) return false;
        value = (offset % FrameByteCount) switch
        {
            0 => FrameDuration,
            FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
            _ => 0,
        };
        return value != 0;
    }
}
