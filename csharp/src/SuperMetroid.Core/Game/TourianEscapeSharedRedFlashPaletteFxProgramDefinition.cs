namespace SuperMetroid.Core.Game;

/// <summary>One entry into Tourian's shared escape red-flash loop.</summary>
public sealed class TourianEscapeSharedRedFlashPaletteFxProgramDefinition
{
    /// <summary>Captures the setup and branch policy for one definition entering the shared red-flash loop.</summary>
    internal TourianEscapeSharedRedFlashPaletteFxProgramDefinition(
        TourianEscapeSharedRedFlashPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        bool usesGoto)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        UsesGoto = usesGoto;
    }

    /// <summary>The mutually exclusive entry owner.</summary>
    public TourianEscapeSharedRedFlashPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this entry.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>Whether this entry branches over the adjacent entry before the shared loop.</summary>
    public bool UsesGoto { get; }

    /// <summary>Recognizes this entry's color-index setup and optional branch into the common frame loop.</summary>
    internal bool TryReadEntryWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => (ushort)PaletteFxInstruction.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when UsesGoto && item == ProgramStart + 4 =>
                (ushort)PaletteFxInstruction.Goto,
            var item when UsesGoto && item == ProgramStart + 6 =>
                TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FirstFramePointer,
            _ => 0,
        };
        return value != 0;
    }
}
