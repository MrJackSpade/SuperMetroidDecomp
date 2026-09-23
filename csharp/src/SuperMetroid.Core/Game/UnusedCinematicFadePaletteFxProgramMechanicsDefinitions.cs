namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the cartridge's unused cinematic fade.</summary>
/// <remarks>
/// Issue #849 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E1EC</c>
/// enters at <c>$8D:D9D0</c>: <c>SetColorIndex($00A0)</c>, eleven 36-byte
/// records of <c>2, colors[16], Wait</c>, then <c>Delete</c> at
/// <c>$DB60</c> after 22 frames. All 25 control words match. One exact
/// ROM-equivalent factorization starts from white and uses this possible
/// 16-color BGR555 anchor <c>Q</c>:
/// <c>[0000,1529,00C8,0023,0000,0508,00C7,0085,0064,1120,04C0,
/// 0460,1484,0C21,0421,2529]</c>. For frame <c>f</c> (0..10), column
/// <c>c</c>, and each five-bit channel <c>q</c> of <c>Q[c]</c>, the
/// output channel is <c>floor((31*(15 - f) + q*f)/15)</c>. Anchors selected
/// from frames 0..8 predict frames 9 and 10 and reproduce all 176 ROM
/// words. Nine anchor components admit more than one value over this
/// truncated fade, so this is a bounded data relationship, not a claim that
/// the native runtime interpolates. All 176 colors remain live presentation
/// data supplied by the presentation compiler. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
{
    /// <summary>The unused palette-FX definition at <c>$8D:E1EC</c>.</summary>
    public const ushort DefinitionPointer = 0xe1ec;

    /// <summary>The instruction-list entry at <c>$8D:D9D0</c>.</summary>
    public const ushort ProgramStart = 0xd9d0;

    /// <summary>The first timed record at <c>$8D:D9D4</c>.</summary>
    public const ushort FirstFramePointer = 0xd9d4;

    /// <summary>The terminal <c>delete</c> command at <c>$8D:DB60</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xdb60;

    /// <summary>The fade writes CGRAM from byte index <c>$00A0</c>.</summary>
    public const ushort ColorByteIndex = 0x00a0;

    /// <summary>The one-shot fade contains eleven timed records.</summary>
    public const int FrameCount = 11;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 36;

    /// <summary>Each record lasts two frames.</summary>
    public const ushort FrameDuration = 2;

    /// <summary>The complete unused fade lasts 22 frames.</summary>
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
