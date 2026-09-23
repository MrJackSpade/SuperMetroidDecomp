namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Nintendo-logo fade entry.</summary>
public enum NintendoLogoFadePaletteFxProgramOwner
{
    /// <summary>The unused Nintendo boot-logo fade.</summary>
    BootLogo,
    /// <summary>The Nintendo copyright fade.</summary>
    Copyright,
}

/// <summary>Immutable mechanics shared by the Nintendo boot-logo and copyright fades.</summary>
/// <remarks>
/// Issue #850 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E198</c>
/// sets color index <c>$0132</c> at <c>$8D:C7AC</c> and falls through to
/// the shared body at <c>$C7B0</c>. Definition <c>$8D:E19C</c> sets index
/// <c>$0192</c> at <c>$8D:C7F2</c> and branches there from <c>$C7F6</c>.
/// Eight records of <c>3, colors[2], Wait</c> end in <c>Delete</c> at
/// <c>$C7F0</c> after 24 frames; all 23 control words match. For record
/// <c>f</c> (0..7), let <c>L = 4*f + 3</c>. The first BGR555 color is
/// <c>L*$0421</c> (equal red, green, blue). The second has red zero, blue
/// <c>L</c>, and green <c>floor((3*L + 4)/8)</c>. This rule matches all
/// sixteen shared ROM colors exactly. They remain live presentation data
/// supplied by the presentation compiler. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class NintendoLogoFadePaletteFxProgramMechanicsDefinitions
{
    /// <summary>The unused boot-logo palette-FX definition at <c>$8D:E198</c>.</summary>
    public const ushort BootLogoDefinitionPointer = 0xe198;

    /// <summary>The copyright palette-FX definition at <c>$8D:E19C</c>.</summary>
    public const ushort CopyrightDefinitionPointer = 0xe19c;

    /// <summary>The unused boot-logo entry at <c>$8D:C7AC</c>.</summary>
    public const ushort BootLogoEntry = 0xc7ac;

    /// <summary>The shared first timed record at <c>$8D:C7B0</c>.</summary>
    public const ushort FirstFramePointer = 0xc7b0;

    /// <summary>The shared terminal <c>delete</c> command at <c>$8D:C7F0</c>.</summary>
    public const ushort DeleteInstructionPointer = 0xc7f0;

    /// <summary>The copyright entry at <c>$8D:C7F2</c>.</summary>
    public const ushort CopyrightEntry = 0xc7f2;

    /// <summary>The copyright branch command at <c>$8D:C7F6</c>.</summary>
    public const ushort CopyrightGotoInstructionPointer = 0xc7f6;

    /// <summary>The boot logo writes CGRAM from byte index <c>$0132</c>.</summary>
    public const ushort BootLogoColorByteIndex = 0x0132;

    /// <summary>The copyright text writes CGRAM from byte index <c>$0192</c>.</summary>
    public const ushort CopyrightColorByteIndex = 0x0192;

    /// <summary>The shared one-shot fade contains eight timed records.</summary>
    public const int FrameCount = 8;

    /// <summary>Each record writes two live BGR555 colors.</summary>
    public const int ColorsPerFrame = 2;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 8;

    /// <summary>Each record lasts three frames.</summary>
    public const ushort FrameDuration = 3;

    /// <summary>Either entry runs the shared fade for 24 frames.</summary>
    public const int CycleFrames = FrameCount * FrameDuration;

    private static readonly NintendoLogoFadePaletteFxProgramDefinition[] Definitions =
    [
        new(
            NintendoLogoFadePaletteFxProgramOwner.BootLogo,
            BootLogoDefinitionPointer,
            BootLogoEntry,
            BootLogoColorByteIndex,
            BranchesToSharedBody: false),
        new(
            NintendoLogoFadePaletteFxProgramOwner.Copyright,
            CopyrightDefinitionPointer,
            CopyrightEntry,
            CopyrightColorByteIndex,
            BranchesToSharedBody: true),
    ];
    private static readonly IReadOnlyList<NintendoLogoFadePaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The boot-logo and copyright entries in definition order.</summary>
    public static IReadOnlyList<NintendoLogoFadePaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Returns one shared timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one presentation-owned word in the shared fade body.</summary>
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
            BootLogoEntry => PaletteFxInstructionCodes.SetColorIndex,
            BootLogoEntry + 2 => BootLogoColorByteIndex,
            DeleteInstructionPointer => PaletteFxInstructionCodes.Delete,
            CopyrightEntry => PaletteFxInstructionCodes.SetColorIndex,
            CopyrightEntry + 2 => CopyrightColorByteIndex,
            CopyrightGotoInstructionPointer => PaletteFxInstructionCodes.Goto,
            CopyrightGotoInstructionPointer + 2 => FirstFramePointer,
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

/// <summary>One entry into the shared Nintendo-logo fade body.</summary>
public sealed record NintendoLogoFadePaletteFxProgramDefinition(
    NintendoLogoFadePaletteFxProgramOwner Owner,
    ushort DefinitionPointer,
    ushort ProgramStart,
    ushort ColorByteIndex,
    bool BranchesToSharedBody);
