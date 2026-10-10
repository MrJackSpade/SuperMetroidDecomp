namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable mechanics for the shared wide-background and space-whiteout palette ramp.
/// </summary>
/// <remarks>
/// Issue #848 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E1E8</c>
/// enters at <c>$8D:D362</c>, sets color index <c>$0022</c>, and branches
/// to <c>$D36E</c>. Definition <c>$8D:E1D0</c> enters at <c>$8D:D36A</c>,
/// sets index <c>$0000</c>, and falls through to that same ramp. It has
/// fifteen six-byte records of <c>14, color, Wait</c>, then <c>Delete</c>
/// at <c>$D3C8</c> after 210 frames; all 37 control words match. For frame
/// <c>f</c> (0..14), grayscale channel <c>g</c> is
/// <c>floor((31*f + 6)/14)</c>, except frame 12 uses <c>g = 26</c> rather
/// than 27. The BGR555 color is <c>g * $0421</c>. This rule matches all
/// fifteen ROM colors exactly, including the authored one-unit step. The
/// presentation compiler still supplies those live colors while this catalog
/// supplies only controls to the palette-FX runtime. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
{

    /// <summary>The wide-background entry at <c>$8D:D362</c>.</summary>
    public const ushort WideExplosionBackgroundProgramStart = 0xd362;

    /// <summary>The wide-background entry writes CGRAM from byte index <c>$0022</c>.</summary>
    public const ushort WideExplosionBackgroundColorByteIndex = 0x0022;

    /// <summary>The space-whiteout entry at <c>$8D:D36A</c>.</summary>
    public const ushort SpaceWhiteoutProgramStart = 0xd36a;

    /// <summary>The space-whiteout entry writes CGRAM from byte index <c>$0000</c>.</summary>
    public const ushort SpaceWhiteoutColorByteIndex = 0x0000;

    /// <summary>The first shared timed record at <c>$8D:D36E</c>.</summary>
    public const ushort FirstFramePointer = 0xd36e;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:D3C8</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xd3c8;

    /// <summary>The shared one-shot ramp contains fifteen timed records.</summary>
    public const int FrameCount = 15;

    /// <summary>Each record writes one live BGR555 color.</summary>
    public const int ColorsPerFrame = 1;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 6;

    /// <summary>Each record lasts fourteen frames.</summary>
    public const ushort FrameDuration = 14;

    /// <summary>Returns one shared timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns the shared whiteout BGR555 color word in a timed record.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Fixed control words outside the timed records, by bank-<c>$8D</c> address.</summary>
    private enum MechanicsWord : ushort
    {
        /// <summary>Wide-background <c>SetColorIndex</c> opcode at <c>$8D:D362</c>.</summary>
        WideBackgroundSetColorIndex = WideExplosionBackgroundProgramStart,
        /// <summary>Wide-background color-index operand at <c>$8D:D364</c>.</summary>
        WideBackgroundColorIndexOperand = WideExplosionBackgroundProgramStart + 2,
        /// <summary>Wide-background <c>goto</c> opcode at <c>$8D:D366</c>.</summary>
        WideBackgroundGoto = WideExplosionBackgroundProgramStart + 4,
        /// <summary>Wide-background goto target operand at <c>$8D:D368</c>.</summary>
        WideBackgroundGotoTarget = WideExplosionBackgroundProgramStart + 6,
        /// <summary>Space-whiteout <c>SetColorIndex</c> opcode at <c>$8D:D36A</c>.</summary>
        SpaceWhiteoutSetColorIndex = SpaceWhiteoutProgramStart,
        /// <summary>Space-whiteout color-index operand at <c>$8D:D36C</c>; its value is zero.</summary>
        SpaceWhiteoutColorIndexOperand = SpaceWhiteoutProgramStart + 2,
        /// <summary>Terminal <c>delete</c> opcode at <c>$8D:D3C8</c>.</summary>
        Delete = DeleteInstructionPointer,
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (Enum.IsDefined((MechanicsWord)pointer))
        {
            value = (MechanicsWord)pointer switch
            {
                MechanicsWord.WideBackgroundSetColorIndex => PaletteFxInstructionCodes.SetColorIndex,
                MechanicsWord.WideBackgroundColorIndexOperand => WideExplosionBackgroundColorByteIndex,
                MechanicsWord.WideBackgroundGoto => PaletteFxInstructionCodes.Goto,
                MechanicsWord.WideBackgroundGotoTarget => FirstFramePointer,
                MechanicsWord.SpaceWhiteoutSetColorIndex => PaletteFxInstructionCodes.SetColorIndex,
                MechanicsWord.SpaceWhiteoutColorIndexOperand => SpaceWhiteoutColorByteIndex,
                MechanicsWord.Delete => PaletteFxInstructionCodes.Delete,
                _ => throw new InvalidOperationException($"Undefined Zebes whiteout mechanics word ${pointer:X4}."),
            };
            return true;
        }

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

        value = 0;
        return false;
    }
}
