namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for fading in the Super Metroid title logo.</summary>
/// <remarks>
/// Issue #852 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E194</c>
/// enters at <c>$8D:C696</c>: <c>SetColorIndex($0142)</c>, eight 34-byte
/// records of <c>3, colors[15], Wait</c>, then <c>Delete</c> at
/// <c>$C7AA</c> after 24 frames. All 19 control words match. Let
/// <c>final[c]</c> be color column <c>c</c> (0..14) of the eighth record.
/// For record <c>f</c> (0..7), each five-bit BGR555 channel <c>q</c> is
/// <c>floor(q(final[c]) * f / 7)</c>. This exact channel rule matches all
/// 120 ROM colors; frame 0 is black and frame 7 is the authored final
/// palette. The presentation compiler still supplies all live colors while
/// this catalog supplies only controls to the palette-FX runtime. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
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
