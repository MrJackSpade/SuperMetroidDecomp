namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the wide foreground part of the Zebes explosion.</summary>
/// <remarks>
/// Issue #842 / #625: the pinned NTSC J/U v1.0 ROM starts definition
/// <c>$8D:E1C8</c> at <c>$8D:CB3C</c>: <c>SetColorIndex($0002)</c>, sixteen
/// 34-byte records of <c>duration, colors[15], Wait</c>, then <c>Delete</c>
/// at <c>$CD60</c>. Durations are 4 for frames 0..2, 60 for frame 3, and 6
/// for frames 4..15: 144 frames total. All 35 control words match. For each
/// frame <c>f</c> (0..15), exactly the first <c>min(f + 1, 15)</c> colors are
/// nonzero; the rest are black, matching all 240 ROM positions (135 nonzero,
/// 105 black). Frames 0..5 fill that prefix uniformly with, respectively,
/// <c>7C00,7CA0,7DE0,7DE0,7E80,7F20</c>. Later frames have authored accents:
/// frame 6 starts <c>7FFD,7FE9</c>, frame 14 ends <c>6B40</c>, and frame 15
/// ends <c>7FF7</c>. Retain all 240 presentation words to preserve those
/// colors; the presentation compiler supplies them while this catalog supplies
/// the control words to the palette-FX runtime. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
{
    /// <summary>The palette-FX definition at <c>$8D:E1C8</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c8;

    /// <summary>The instruction-list entry at <c>$8D:CB3C</c>.</summary>
    public const ushort ProgramStart = 0xcb3c;

    /// <summary>The first timed record at <c>$8D:CB40</c>.</summary>
    public const ushort FirstFramePointer = 0xcb40;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:CD60</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xcd60;

    /// <summary>The foreground explosion writes CGRAM from byte index <c>$0002</c>.</summary>
    public const ushort ColorByteIndex = 0x0002;

    /// <summary>The one-shot foreground explosion contains sixteen timed records.</summary>
    public const int FrameCount = 16;

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 34;

    /// <summary>The complete one-shot foreground explosion lasts 144 frames.</summary>
    public const int CycleFrames = 144;

    private static readonly ushort[] Durations =
        [4, 4, 4, 60, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6];

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
                0 => Durations[frame],
                FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
