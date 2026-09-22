namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive owner of the shared Zebes-explosion whiteout ramp.</summary>
public enum ZebesExplosionWhiteoutPaletteFxProgramOwner
{
    /// <summary>The wide explosion's background color.</summary>
    WideExplosionBackground,
    /// <summary>The space backdrop whiteout.</summary>
    SpaceWhiteout,
}

/// <summary>
/// Immutable mechanics for the shared wide-background and space-whiteout palette ramp.
/// </summary>
/// <remarks>
/// Definitions <c>$E1E8</c> and <c>$E1D0</c> retain separate native entry points but
/// converge on the same fifteen-record program at <c>$8D:D36E</c>. Its fifteen BGR555
/// words remain live presentation data; this catalog owns placement, timing, the entry
/// branch, waits, and termination.
/// </remarks>
public static class ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
{
    /// <summary>The wide-background definition at <c>$8D:E1E8</c>.</summary>
    public const ushort WideExplosionBackgroundDefinitionPointer = 0xe1e8;

    /// <summary>The wide-background entry at <c>$8D:D362</c>.</summary>
    public const ushort WideExplosionBackgroundProgramStart = 0xd362;

    /// <summary>The wide-background entry writes CGRAM from byte index <c>$0022</c>.</summary>
    public const ushort WideExplosionBackgroundColorByteIndex = 0x0022;

    /// <summary>The space-whiteout definition at <c>$8D:E1D0</c>.</summary>
    public const ushort SpaceWhiteoutDefinitionPointer = 0xe1d0;

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

    /// <summary>The complete one-shot whiteout lasts 210 frames.</summary>
    public const int CycleFrames = 210;

    /// <summary>All native definitions whose mechanics are owned by this catalog.</summary>
    public static IReadOnlyList<ZebesExplosionWhiteoutPaletteFxProgramDefinition> All { get; } =
    [
        new(
            ZebesExplosionWhiteoutPaletteFxProgramOwner.WideExplosionBackground,
            WideExplosionBackgroundDefinitionPointer,
            WideExplosionBackgroundProgramStart),
        new(
            ZebesExplosionWhiteoutPaletteFxProgramOwner.SpaceWhiteout,
            SpaceWhiteoutDefinitionPointer,
            SpaceWhiteoutProgramStart),
    ];

    /// <summary>Returns one shared timed-record pointer.</summary>
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
            WideExplosionBackgroundProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            WideExplosionBackgroundProgramStart + 2 =>
                WideExplosionBackgroundColorByteIndex,
            WideExplosionBackgroundProgramStart + 4 => PaletteFxInstructionCodes.Goto,
            WideExplosionBackgroundProgramStart + 6 => FirstFramePointer,
            SpaceWhiteoutProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            SpaceWhiteoutProgramStart + 2 => SpaceWhiteoutColorByteIndex,
            DeleteInstructionPointer => PaletteFxInstructionCodes.Delete,
            _ => 0,
        };
        if (value != 0 || pointer == SpaceWhiteoutProgramStart + 2)
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

/// <summary>One native entry into the shared Zebes-explosion whiteout program.</summary>
public readonly record struct ZebesExplosionWhiteoutPaletteFxProgramDefinition(
    ZebesExplosionWhiteoutPaletteFxProgramOwner Owner,
    ushort DefinitionPointer,
    ushort ProgramStart);
