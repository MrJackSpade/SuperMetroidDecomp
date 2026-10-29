namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One entry into Tourian's shared escape red-flash loop.</summary>
public sealed class TourianEscapeSharedRedFlashPaletteFxColorDefinition
{
    /// <summary>Captures the setup and branch policy for one definition entering the shared red-flash loop.</summary>
    internal TourianEscapeSharedRedFlashPaletteFxColorDefinition(
        TourianEscapeSharedRedFlashPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        bool usesGoto)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        UsesGoto = usesGoto;
    }

    /// <summary>The mutually exclusive entry owner.</summary>
    public TourianEscapeSharedRedFlashPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this entry.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }

    /// <summary>Whether this entry branches over the adjacent entry before the shared loop.</summary>
    public bool UsesGoto { get; }
}
