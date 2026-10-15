namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxColorDefinition
{
    /// <summary>Records one accent destination and its independent bank 8D setup list.</summary>
    internal OldTourianEscapeAccentPaletteFxColorDefinition(
        OldTourianEscapeAccentPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
    }

    /// <summary>The mutually exclusive escape-accent owner.</summary>
    public OldTourianEscapeAccentPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(
        FirstFramePointer +
        OldTourianEscapeAccentPaletteFxColorDefinitions.FrameCount *
        OldTourianEscapeAccentPaletteFxColorDefinitions.FrameByteCount));

    /// <summary>Locates one of fifteen timed records after this entry's color-index setup.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >=
            OldTourianEscapeAccentPaletteFxColorDefinitions.FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frame));
        }
        return unchecked((ushort)(FirstFramePointer + frame *
            OldTourianEscapeAccentPaletteFxColorDefinitions.FrameByteCount));
    }

    /// <summary>Locates one of three live BGR555 words immediately after a record's duration.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= OldTourianEscapeAccentPaletteFxColorDefinitions
                .ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
