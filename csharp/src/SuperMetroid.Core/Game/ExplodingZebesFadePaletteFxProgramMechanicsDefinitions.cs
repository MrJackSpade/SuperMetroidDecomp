namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the zoomed-out exploding-Zebes fade-out.</summary>
/// <remarks>
/// Issue #841 / #625: in the pinned NTSC J/U v1.0 ROM, definition <c>$8D:E1C4</c>
/// enters at <c>$8D:CAAA</c>: <c>SetColorIndex($01E0)</c>, seven 20-byte
/// records of <c>8, colors[8], Wait</c>, then <c>Delete</c> at <c>$CB3A</c>
/// after 56 frames. All 17 control words match. The first color row is
/// <c>2003,0E9A,05F9,0596,0133,008E,0009,0005</c>. For frame <c>f</c>
/// (0..6), columns 1..7 equal that column's first BGR555 color with
/// <c>max(0, component - 5*f)</c> applied separately to red, green, and blue;
/// column 3's red loses one additional unit when <c>f &gt; 0</c>. Column 0
/// stays <c>$2003</c> for frames 0 and 1, then becomes zero. This bounded
/// relationship matches all 56 ROM colors exactly; it describes the authored
/// data, not runtime color arithmetic. The presentation compiler still supplies
/// all 56 live colors, and this catalog supplies only the controls. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ExplodingZebesFadePaletteFxProgramMechanicsDefinitions
{
    /// <summary>The palette-FX definition at <c>$8D:E1C4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c4;

    /// <summary>The instruction-list entry at <c>$8D:CAAA</c>.</summary>
    public const ushort ProgramStart = 0xcaaa;

    /// <summary>The first timed record at <c>$8D:CAAE</c>.</summary>
    public const ushort FirstFramePointer = 0xcaae;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:CB3A</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xcb3a;

    /// <summary>The fade writes CGRAM from byte index <c>$01E0</c>.</summary>
    public const ushort ColorByteIndex = 0x01e0;

    /// <summary>The one-shot fade contains seven timed records.</summary>
    public const int FrameCount = 7;

    /// <summary>Each record writes eight live BGR555 colors.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 20;

    /// <summary>Each record lasts eight frames.</summary>
    public const ushort FrameDuration = 8;

    /// <summary>The complete one-shot fade lasts 56 frames.</summary>
    public const int CycleFrames = 56;

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
