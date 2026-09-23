namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive PLANET ZEBES text-fade palette program.</summary>
public enum PlanetZebesTextPaletteFxProgramOwner
{
    /// <summary>The opening cinematic's text fade-in.</summary>
    FadeIn,
    /// <summary>The opening cinematic's text fade-out.</summary>
    FadeOut,
}

/// <summary>Immutable mechanics for the cinematic PLANET ZEBES text fades.</summary>
/// <remarks>
/// Issue #840 / #625: the pinned NTSC J/U v1.0 ROM encodes both definitions as
/// <c>SetColorIndex($0102)</c>, eight ten-byte records of
/// <c>3, colors[3], Wait</c>, then <c>Delete</c>. Fade-in starts at
/// <c>$8D:C90E</c> (first record <c>$C912</c>, delete <c>$C962</c>);
/// fade-out starts at <c>$8D:C964</c> (first record <c>$C968</c>, delete
/// <c>$C9B8</c>). Each one-shot lasts 24 frames. All 38 control words match
/// the ROM. For frame <c>i</c> and color <c>c</c>, the ROM's fade-out color
/// equals fade-in color <c>(7 - i, c)</c> at all 24 positions. The eight
/// authored color rows and their reverse remain 48 live presentation words;
/// the presentation compiler supplies them while this catalog supplies only
/// control words to the palette-FX runtime. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class PlanetZebesTextPaletteFxProgramMechanicsDefinitions
{
    /// <summary>The text-fade programs each contain eight timed records.</summary>
    public const int FrameCount = 8;

    /// <summary>Each record writes three live BGR555 colors.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 10;

    /// <summary>Each text-fade record lasts three frames.</summary>
    public const ushort FrameDuration = 3;

    /// <summary>Each complete one-shot fade lasts 24 frames.</summary>
    public const int CycleFrames = 24;

    private static readonly PlanetZebesTextPaletteFxProgramDefinition[] Definitions =
    [
        new(
            PlanetZebesTextPaletteFxProgramOwner.FadeIn,
            definitionPointer: 0xe1b0,
            programStart: 0xc90e,
            colorByteIndex: 0x0102),
        new(
            PlanetZebesTextPaletteFxProgramOwner.FadeOut,
            definitionPointer: 0xe1b4,
            programStart: 0xc964,
            colorByteIndex: 0x0102),
    ];
    private static readonly IReadOnlyList<PlanetZebesTextPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The fade-in and fade-out programs in definition order.</summary>
    public static IReadOnlyList<PlanetZebesTextPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (PlanetZebesTextPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete cinematic PLANET ZEBES text-fade control program.</summary>
public sealed class PlanetZebesTextPaletteFxProgramDefinition
{
    internal PlanetZebesTextPaletteFxProgramDefinition(
        PlanetZebesTextPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
    }

    /// <summary>The mutually exclusive text-fade owner.</summary>
    public PlanetZebesTextPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks>
    /// <c>$8D:E1B0</c> is the fade-in definition and <c>$8D:E1B4</c> is fade-out.
    /// </remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks>
    /// <c>$8D:C90E</c> fades in and <c>$8D:C964</c> fades out.
    /// </remarks>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM, authored as <c>$0102</c>.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>delete</c> command after all timed records.</summary>
    public ushort DeleteInstructionPointer => unchecked((ushort)(FirstFramePointer +
        PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount *
        PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame *
            PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one presentation-owned BGR555 word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == DeleteInstructionPointer => PaletteFxInstructionCodes.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameDuration,
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
